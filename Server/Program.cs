using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Providers.SaveProviders;
using Paulov.TarkovServices.Services;
using Paulov.TarkovServices.Services.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;

namespace SIT.WebServer
{
    public class Program
    {
        public static Dictionary<string, WebSocket> WebSockets { get; } = new Dictionary<string, WebSocket>();
        public static void Main(string[] args)
        {
            var assemblyMods = new List<Assembly>();
            // Create Mods Directory
            Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Mods"));
            // Create Directories
            Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "user"));
            Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "user", "profiles"));
            Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "v8", "mods"));

            var builder = WebApplication.CreateBuilder(args);
            ConfigureServices(builder.Services);

            var app = builder.Build();

            app.UseWebSockets(new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromMinutes(2)
            });

            // ----------------------------------------------------------------------------------------------------------------------------------------------------
            // Handle the WebSocket request
            // You can find useful information on WebSockets in .NET here https://learn.microsoft.com/en-us/aspnet/core/fundamentals/websockets?view=aspnetcore-9.0
            app.Use(async (context, next) =>
            {
                if (!context.WebSockets.IsWebSocketRequest)
                {
                    await next(context);
                    return;
                }

                var path = context.Request.Path.ToString();
                var lastIndexOfSlash = path.LastIndexOf('/');
                var sessionId = path.Substring(lastIndexOfSlash + 1);
                if (string.IsNullOrEmpty(sessionId))
                {
                    await next(context);
                    return;
                }

                JObject defaultNotificationPing = new();
                defaultNotificationPing.Add("type", "Ping");
                defaultNotificationPing.Add("eventId", "ping");

#if DEBUG
                Debug.WriteLine($"WebSocket: request received for {sessionId}");
#endif

                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                if (webSocket == null)
                    return;

#if DEBUG
                Debug.WriteLine($"WebSocket: request accepted for {sessionId}");
#endif

                if (!WebSockets.ContainsKey(sessionId))
                    WebSockets.Add(sessionId, webSocket);

                if (webSocket.State != WebSocketState.Open)
                    return;

#if DEBUG
                Debug.WriteLine($"WebSocket: sending default ping notification to {sessionId}");
#endif

                await webSocket.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(defaultNotificationPing.ToJson())), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);

                var socketFinishedTcs = new TaskCompletionSource<object>();
                // TODO: Handle receive of information and handle it in a background Task
                await socketFinishedTcs.Task;

            });
            // <-- End of Handle WebSocket

            app.UseMiddleware<RequestLoggingMiddleware>();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI();
            //}

            app.UseAuthorization();
            app.UseSession(new SessionOptions() { IdleTimeout = new TimeSpan(1, 1, 1, 1) });

            app.MapControllers();


            app.Run();
        }

        /// <summary>
        /// Configures the <see cref="IServiceCollection"/> provided for use in the application
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance to configure</param>
        private static void ConfigureServices(IServiceCollection services)
        {
            /*
            services.AddRequestDecompression(options =>
            {
                options.DecompressionProviders.Add("zlibdecompressionprovider", new ZLibDecompressionProvider());
            });
            */

            //MVC building
            IMvcBuilder mvcBuilder = services.AddMvc().AddSessionStateTempDataProvider();
            const string modAssemblyFolderName = "Mods";
            DirectoryInfo modAssemblyDirectory = new(Path.Combine(AppContext.BaseDirectory, modAssemblyFolderName));
            IEnumerable<Assembly> modAssemblies =
                modAssemblyDirectory.EnumerateFiles("*.dll").Select(x => Assembly.LoadFile(x.FullName));
            foreach (Assembly assembly in modAssemblies)
            {
                if (!assembly.GetTypes().Any(x => x.IsSubclassOf(typeof(ControllerBase)))) return;
                mvcBuilder.AddApplicationPart(assembly);
            }

            //Services
            services.AddControllers();
            services
                .AddSwaggerGen(ConfigureSwaggerGen)
                .AddDistributedMemoryCache()
                .AddSession()
                .AddSingleton<ISaveProvider, JsonFileSaveProvider>()
                .AddSingleton<IInventoryService, InventoryService>()
                .AddKeyedSingleton("fileAssets", (_, _) =>
                {
                    const string fileAssetArchiveResourceName = "files.zip";
                    Stream resourceStream = FMT.FileTools.EmbeddedResourceHelper.GetEmbeddedResourceByName(fileAssetArchiveResourceName);
                    return new ZipArchive(resourceStream);
                });
        }

        /// <summary>
        /// Configures Swagger API documentation genmeration 
        /// </summary>
        /// <param name="options">The <see cref="SwaggerGenOptions"/> to be configured</param>
        private static void ConfigureSwaggerGen(SwaggerGenOptions options)
        {
            const string swaggerDocVersion = "v1";
            const string swaggerCommentDocName = "Paulov.Tarkov.WebServer.DOTNET.xml";

            options.SwaggerDoc(swaggerDocVersion, new Microsoft.OpenApi.Models.OpenApiInfo());
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, swaggerCommentDocName));
        }

        /// <summary>
        /// Middleware that logs HTTP request and response details, including method, path, status code, and elapsed
        /// time.
        /// </summary>
        /// <remarks>This middleware logs information about incoming HTTP requests and their corresponding
        /// responses to the console and debug output. It logs the request method and path at the start of processing,
        /// and the response status code and elapsed time after processing. WebSocket requests are bypassed and not
        /// logged.</remarks>
        public class RequestLoggingMiddleware
        {
            private readonly RequestDelegate _next;

            /// <summary>
            /// Middleware that logs details about incoming HTTP requests and their responses.
            /// </summary>
            /// <remarks>This middleware captures information about each HTTP request and response,
            /// which can be used for debugging, monitoring, or auditing purposes. Ensure that this middleware is added
            /// to the pipeline in the correct order to avoid missing important request or response details.</remarks>
            /// <param name="next">The next middleware in the request pipeline. Cannot be null.</param>
            public RequestLoggingMiddleware(RequestDelegate next)
            {
                _next = next;
            }

            /// <summary>
            /// Processes an incoming HTTP request, logs request and response details, and forwards the request to the
            /// next middleware in the pipeline.
            /// </summary>
            /// <remarks>If the request is a WebSocket request, the method forwards the request to the
            /// next middleware without logging. For non-WebSocket requests, the method logs the HTTP method, request
            /// path, response status code, and the elapsed time for processing the request.</remarks>
            /// <param name="context">The <see cref="HttpContext"/> representing the current HTTP request and response.</param>
            /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
            public async Task InvokeAsync(HttpContext context)
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    await _next(context);
                    return;
                }

                var startTime = DateTime.Now;

                var logMessage = $"REQ {context.Request.Method} {context.Request.Path}";
                Console.WriteLine(logMessage);
                Debug.WriteLine(logMessage);

                await _next(context);

                var endTime = DateTime.Now;
                var elapsedTime = endTime - startTime;

                logMessage = $"{context.Request.Method} {context.Request.Path} {context.Response.StatusCode} {elapsedTime.TotalMilliseconds}ms";
                Console.WriteLine(logMessage);
                Debug.WriteLine(logMessage);
            }
        }


    }

}