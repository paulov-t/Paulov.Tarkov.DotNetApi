using Microsoft.Extensions.Configuration;
using Paulov.TarkovServices.Models;
using Paulov.TarkovServices.Providers.DatabaseProviders.FileDatabaseProviders;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Services;

namespace Paulov.TarkovServices.Tests
{
    public class BotGenerationServiceTests
    {

        IConfiguration configuration;
        private readonly IDatabaseProvider _databaseProvider;

        public BotGenerationServiceTests()
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            _databaseProvider = new JsonFileCollectionDatabaseProvider();
            _databaseProvider.Connect(AppContext.BaseDirectory);
            new DatabaseService(configuration, _databaseProvider);
        }

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GenerateBotTest()
        {
            BotGenerationService botGenerationService = new BotGenerationService(new TestsGlobalsService(), new InventoryService());
            botGenerationService.GenerateBot(new GenerateBotConditionModel(1, "assault", "normal"));
        }

    }
}