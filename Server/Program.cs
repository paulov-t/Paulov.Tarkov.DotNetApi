using Microsoft.AspNetCore.Mvc;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Providers.SaveProviders;
using Paulov.TarkovServices.Services;
using Paulov.TarkovServices.Services.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Paulov.Tarkov.WebServer.DOTNET
{
    public class Program
    {
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

            app.UseWebSockets(new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromMinutes(2)
            });

            app.UseMiddleware<WebSocketMiddleware>(builder.Services);
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
            // This is for logging what configuration settings are loaded
            foreach (var c in builder.Configuration.AsEnumerable())
            {
                Console.WriteLine(c.Key + " = " + c.Value);
            }


            var services = builder.Services;


            //MVC building
            Console.WriteLine("Loading Mods:");
            IMvcBuilder mvcBuilder = services.AddMvc().AddSessionStateTempDataProvider();
            const string modAssemblyFolderName = "Mods";
            DirectoryInfo modAssemblyDirectory = new(Path.Combine(AppContext.BaseDirectory, modAssemblyFolderName));
            IEnumerable<Assembly> modAssemblies =
                modAssemblyDirectory.EnumerateFiles("*.dll").Select(x => Assembly.LoadFile(x.FullName));
            foreach (Assembly assembly in modAssemblies)
            {
                if (!assembly.GetTypes().Any(x => x.IsSubclassOf(typeof(ControllerBase))))
                    continue;

                Console.WriteLine($" - {assembly.GetName().Name}");
                mvcBuilder.AddApplicationPart(assembly);
            }

            // Add controllers to the MVC builder
            services.AddControllers();

            // Get the database provider from configuration and register it
            IDatabaseProvider dbProvider = DatabaseService.GetDatabaseProviderByConfiguration(builder.Configuration);
            Console.WriteLine($"Loading DbProvider: {dbProvider.GetType().Name}");
            services.AddSingleton(typeof(IDatabaseProvider), dbProvider);

            // Register the GlobalsService and DatabaseService as singletons
            Console.WriteLine($"Loading GlobalsService. This can take a few seconds...");
            services.AddSingleton(typeof(IGlobalsService), new GlobalsService(dbProvider));
            Console.WriteLine($"Loading DatabaseService");
            services.AddSingleton(typeof(IDatabaseService), (new DatabaseService(builder.Configuration, dbProvider)));

            Console.WriteLine($"Loading QuestService");
            services.AddSingleton(typeof(IQuestService), new QuestService(dbProvider, new JsonFileSaveProvider()));

            services
                .AddSwaggerGen(ConfigureSwaggerGen)
                .AddDistributedMemoryCache()
                .AddSession()
                .AddSingleton<ISaveProvider, JsonFileSaveProvider>()
                .AddSingleton<IInventoryService, InventoryService>()
                .AddSingleton<IPasswordService, PasswordService>();

            Console.WriteLine($"Loading AccountService");
            services.AddSingleton<IAccountService, AccountService>();

            Console.WriteLine($"Loading FriendshipService");
            services.AddSingleton<IFriendshipService, FriendshipService>();

            Console.WriteLine($"Loading WebSocketService");
            services.AddSingleton(typeof(IWebSocketService), new WebSocketService());

            Console.WriteLine($"Loading MatchingService");
            services.AddSingleton<IMatchingService, MatchingService>();

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