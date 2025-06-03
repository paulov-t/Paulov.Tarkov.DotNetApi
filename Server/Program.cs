using Comfort.Common;
using EFT.HealthSystem;
using Newtonsoft.Json.Linq;
using Paulov.Tarkov.WebServer.DOTNET.Providers;
using Paulov.Tarkov.WebServer.DOTNET.Services;
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

            // Load Mods
            foreach (var file in Directory.EnumerateFiles(Path.Combine(AppContext.BaseDirectory, "Mods")).Select(x => new FileInfo(x)))
            {
                if (file.Extension == ".dll")
                    assemblyMods.Add(Assembly.LoadFile(file.FullName));
            }

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddRequestDecompression(options =>
            {
                options.DecompressionProviders.Add("zlibdecompressionprovider", new ZLibDecompressionProvider());
            });


            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            //builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo());

                var filePath = Path.Combine(System.AppContext.BaseDirectory, "Paulov.Tarkov.WebServer.DOTNET.xml");
                c.IncludeXmlComments(filePath);
            });


            builder.Services.AddDistributedMemoryCache();
            var mvc = builder.Services.AddMvc().AddSessionStateTempDataProvider();

            // ---------------------------------------------------------------
            // Add Assembly Mods which use MVC Controllers to the MVC handler
            foreach (var assemblyMod in assemblyMods)
            {
                if (assemblyMod.GetTypes().Any(x => x.BaseType?.Name == "Controller"))
                {
                    mvc.AddApplicationPart(assemblyMod);
                }
            }
            //
            // ---------------------------------------------------------------
            builder.Services.AddSession();

            var app = builder.Build();

            //app.UseRequestDecompression();
            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseWebSockets();

            app.Use(async (context, next) =>
            {
                await next(context);

                if (context.Request.Path.ToString().StartsWith("/files/"))
                {
                    var stream = FMT.FileTools.EmbeddedResourceHelper.GetEmbeddedResourceByName("files.zip");
                    var fileAssetZipArchive = new ZipArchive(stream);
                    var path = context.Request.Path.ToString().Replace("/files/", "");
                    var fileEntry = fileAssetZipArchive.GetEntry(path);

                    if (fileEntry != null)
                    {
                        using var fileEntryStream = fileEntry.Open();
                        using var ms = new MemoryStream();
                        await fileEntryStream.CopyToAsync(ms);
                        context.Response.StatusCode = 200;
                        await context.Response.Body.WriteAsync(new ReadOnlyMemory<byte>(ms.ToArray()));
                    }
                }

            });

            app.Use(async (context, next) =>
            {
                JObject defaultNotificationPing = new();
                defaultNotificationPing.Add("type", "PING");
                defaultNotificationPing.Add("eventId", "ping");

                if (context.Request.Path.ToString().StartsWith("/notifierServer/getwebsocket"))
                {
                    var sessionId = context.Request.Path.ToString().Replace("/notifierServer/getwebsocket/", "");
                    if (!context.WebSockets.IsWebSocketRequest)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await next(context);
                        return;
                    }
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    if (webSocket == null)
                    {
                        return;
                    }

                    if (!WebSockets.ContainsKey(sessionId))
                        WebSockets.Add(sessionId, webSocket);

                    if (webSocket.State != WebSocketState.Open)
                        return;

                    await webSocket.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(defaultNotificationPing.ToJson())), System.Net.WebSockets.WebSocketMessageType.Binary, false, CancellationToken.None);

                    var buf = new byte[1024];
                    await webSocket.ReceiveAsync(new ArraySegment<byte>(buf), CancellationToken.None);

                    try
                    {
                        _ = Task.Run(async () =>
                        {
                            while (true)
                            {
                                foreach (var ws in WebSockets.Values)
                                {
                                    //{ type: NotificationEventType.PING, eventId: "ping" };

                                    var buf = new byte[1024];
                                    if (ws.State == WebSocketState.Open)
                                    {
                                        await ws.ReceiveAsync(new ArraySegment<byte>(buf), CancellationToken.None);
                                        await ws.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(defaultNotificationPing.ToJson())), System.Net.WebSockets.WebSocketMessageType.Binary, false, CancellationToken.None);
                                    }
                                }
                                await Task.Delay(1000);
                            }
                        });

                    }
                    catch
                    {

                    }
                    return;
                }

                await next(context);
            });

            app.UseMiddleware<RequestLoggingMiddleware>();

            app.UseAuthorization();
            app.UseSession(new SessionOptions() { IdleTimeout = new TimeSpan(1, 1, 1, 1) });

            app.MapControllers();

            GlobalsService.Instance.LoadGlobalsIntoComfortSingleton();
            // test the singleton
            var Temperature = Singleton<BackendConfigSettingsClass>.Instance.Health.ProfileHealthSettings.HealthFactorsSettings[EHealthFactorType.Temperature].ValueInfo;

            SaveProvider saveProvider = new();

            app.Run();



        }

        public class RequestLoggingMiddleware
        {
            private readonly RequestDelegate _next;

            public RequestLoggingMiddleware(RequestDelegate next)
            {
                _next = next;
            }

            public async Task InvokeAsync(HttpContext context)
            {
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