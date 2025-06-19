using Paulov.TarkovServices.Models;
using Paulov.TarkovServices.Providers.DatabaseProviders.ZipDatabaseProviders;
using Paulov.TarkovServices.Services;

namespace Paulov.TarkovServices.Tests
{
    public class BotGenerationServiceTests
    {

        public BotGenerationServiceTests()
        {
        }

        [SetUp]
        public void Setup()
        {
            new DatabaseService(null);
        }

        [Test]
        public void GenerateBotTest()
        {
            BotGenerationService botGenerationService = new BotGenerationService(new GlobalsService(new MicrosoftCompressionZipDatabaseProvider()), new InventoryService());
            botGenerationService.GenerateBot(new GenerateBotConditionModel(1, "assault", "normal"));
        }

    }
}