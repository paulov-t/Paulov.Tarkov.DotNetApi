using BSGHelperLibrary.ResponseModels;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Paulov.Tarkov.WebServer.DOTNET.Controllers;
using Paulov.TarkovServices.Providers.Interfaces;

namespace WebApiTests
{
    public sealed class GameControllerTest
    {
        private readonly GameController controller;
        private readonly ISaveProvider saveProvider;
        private readonly IConfiguration configuration;

        public GameControllerTest()
        {
            saveProvider = new NullSaveProvider();
            configuration = new ConfigurationBuilder().Build();
            controller = new GameController(saveProvider, configuration);
        }

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Globals_ResponseTest()
        {
            var result = controller.Globals();
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

    }
}