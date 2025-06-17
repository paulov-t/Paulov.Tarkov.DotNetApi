using Paulov.TarkovServices.Models;
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

        }

        [Test]
        public void GenerateBotTest()
        {
            BotGenerationService botGenerationService = new BotGenerationService();
            botGenerationService.GenerateBot(new GenerateBotConditionModel(1, "assault", "normal"));
        }

    }
}