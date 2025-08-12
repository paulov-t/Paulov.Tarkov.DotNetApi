using Microsoft.AspNetCore.Mvc;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Providers.SaveProviders;
using Paulov.TarkovServices.Services;
using Paulov.TarkovServices.Services.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Net.WebSockets;
using System.Reflection;

namespace Paulov.Tarkov.WebServer.DOTNET
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
            app.UseMiddleware<RequestLoggingMiddleware>();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthorization();
            app.UseSession(new SessionOptions() { IdleTimeout = new TimeSpan(1, 1, 1, 1) });

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
            IDatabaseProvider dbProvider = DatabaseService.GetDatabaseProviderByConfiguration(builder.Configuration);
            services.AddSingleton(typeof(IDatabaseProvider), dbProvider);

            // Register the GlobalsService and DatabaseService as singletons
            services.AddSingleton(typeof(IGlobalsService), new GlobalsService(dbProvider));
            services.AddSingleton(typeof(IDatabaseService), (new DatabaseService(builder.Configuration, dbProvider)));

            services.AddSingleton(typeof(IQuestService), new QuestService(dbProvider));

            services
                .AddSwaggerGen(ConfigureSwaggerGen)
                .AddDistributedMemoryCache()
                .AddSession()
                //.AddSingleton<IGlobalsService, GlobalsService>()
                .AddSingleton<ISaveProvider, JsonFileSaveProvider>()
                .AddSingleton<IInventoryService, InventoryService>()
                .AddSingleton<IPasswordService, PasswordService>();



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
    }
}