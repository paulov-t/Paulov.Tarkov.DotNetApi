using Microsoft.AspNetCore.Mvc;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovServices.Providers.DatabaseProviders.CloudDatabaseProviders;
using Paulov.TarkovServices.Providers.DatabaseProviders.ZipDatabaseProviders;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Providers.SaveProviders;
using Paulov.TarkovServices.Services;
using Paulov.TarkovServices.Services.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.WebSockets;
using System.Reflection;

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
            ConfigureServices(builder);

            var app = builder.Build();

            //foreach (var c in builder.Configuration.AsEnumerable())
            //{
            //    Console.WriteLine(c.Key + " = " + c.Value);
            //}


            app.UseWebSockets(new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromMinutes(2)
            });

            app.UseMiddleware<WebsocketMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthorization();
            app.UseSession(new SessionOptions() { IdleTimeout = new TimeSpan(1, 1, 1, 1) });
            app.UseWebSockets();

            app.MapControllers();


            app.Run();
        }

        /// <summary>
        /// Configures the <see cref="IServiceCollection"/> provided for use in the application
        /// </summary>
        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            foreach (var c in builder.Configuration.AsEnumerable())
            {
                Console.WriteLine(c.Key + " = " + c.Value);
            }


            var services = builder.Services;
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

            // Add controllers to the MVC builder
            services.AddControllers();

            // Get the database provider from configuration and register it
            IDatabaseProvider dbProvider;
            switch (builder.Configuration["DatabaseProvider"])
            {
                case "mongodb":
                    dbProvider = new MongoDatabaseProvider(builder.Configuration);
                    break;
                case "ms-zip":
                default:
                    dbProvider = new MicrosoftCompressionZipDatabaseProvider();
                    break;
            }
            services.AddSingleton(typeof(IDatabaseProvider), dbProvider);

            // Register the GlobalsService and DatabaseService as singletons
            services.AddSingleton(typeof(IGlobalsService), new GlobalsService(dbProvider));
            services.AddSingleton(typeof(IDatabaseService), (new DatabaseService(builder.Configuration)));

            services
                .AddSwaggerGen(ConfigureSwaggerGen)
                .AddDistributedMemoryCache()
                .AddSession()
                //.AddSingleton<IGlobalsService, GlobalsService>()
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