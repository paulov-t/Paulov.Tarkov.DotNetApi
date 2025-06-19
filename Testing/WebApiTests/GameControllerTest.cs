using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Paulov.Tarkov.WebServer.DOTNET.Controllers;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Services;

namespace WebApiTests
{
    public sealed class GameControllerTest
    {
        private readonly GameController controller;
        private readonly ISaveProvider saveProvider;
        private readonly IConfiguration configuration;

        public GameControllerTest()
        {
            new DatabaseService(null);
            saveProvider = new NullSaveProvider();
            configuration = new ConfigurationBuilder().Build();
            controller = new GameController(saveProvider, configuration, new TestsGlobalsService());
        }

        [SetUp]
        public void Setup()
        {
        }

        private void CommonTest_MustBeBSGSuccessBodyResult(IActionResult result)
        {
            if (result is BSGSuccessBodyResult successBodyResult)
            {
                var responseBody = successBodyResult.CreateResponseBody();
                // We have a response
                Assert.IsTrue(responseBody.Length > 0);

                // Is a good Json Response?
                var jobj = JObject.Parse(responseBody);

                // Response must have data
                Assert.IsTrue(jobj.ContainsKey("data"));
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public void Start_ResponseTest()
        {
            var result = controller.Start().Result;
            CommonTest_MustBeBSGSuccessBodyResult(result);
        }

        [Test]
        public void GameMode_ResponseTest()
        {
            var result = controller.GameMode().Result;
            CommonTest_MustBeBSGSuccessBodyResult(result);
        }

        [Test]
        public void GameConfig_ResponseTest()
        {
            var result = controller.GameConfig().Result;
            CommonTest_MustBeBSGSuccessBodyResult(result);
        }

        [Test]
        public void TemplateItems_ResponseTest()
        {
            var result = controller.TemplateItems(-1, -1).Result;
            CommonTest_MustBeBSGSuccessBodyResult(result);
        }

        [Test]
        public void Globals_ResponseTest()
        {
            var result = controller.Globals();
            CommonTest_MustBeBSGSuccessBodyResult(result);
        }

    }
}