using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using NUnit.Framework.Internal;
using Paulov.Tarkov.WebServer.DOTNET.Controllers;
using Paulov.TarkovServices.Providers.DatabaseProviders.FileDatabaseProviders;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Services;
using Paulov.TarkovServices.Services.Interfaces;

namespace WebApiTests
{
    public sealed class GameControllerTest
    {
        private readonly GameController _controller;
        private readonly ISaveProvider _saveProvider;
        private readonly IConfiguration configuration;
        private readonly IGlobalsService _globalsService;
        private readonly IDatabaseProvider databaseProvider;
        private readonly IDatabaseService databaseService;

        public GameControllerTest()
        {
            databaseProvider = new JsonFileCollectionDatabaseProvider();
            databaseProvider.Connect(AppContext.BaseDirectory);
            _saveProvider = new SimpleSaveProvider();
            configuration = new ConfigurationBuilder().Build();
            databaseService = new DatabaseService(configuration, databaseProvider);
            _controller = new GameController(_saveProvider, configuration, new TestsGlobalsService(), new InventoryService());
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
            var result = _controller.Start().Result;
            CommonTest_MustBeBSGSuccessBodyResult(result);
        }

        [Test]
        public void GameMode_ResponseTest()
        {
            var result = _controller.GameMode().Result;
            CommonTest_MustBeBSGSuccessBodyResult(result);
        }

        [Test]
        public void GameConfig_ResponseTest()
        {
            var result = _controller.GameConfig().Result;
            CommonTest_MustBeBSGSuccessBodyResult(result);
        }

        [Test]
        public void TemplateItems_ResponseTest()
        {
            var result = _controller.TemplateItems(-1, -1).Result;
            CommonTest_MustBeBSGSuccessBodyResult(result);
        }

        [Test]
        public void Globals_ResponseTest()
        {
            var result = _controller.Globals();
            CommonTest_MustBeBSGSuccessBodyResult(result);
        }

        [Test]
        public void ItemsMoving_ResponseTest()
        {
            var data = new JObject();
            data.Add("data", new JArray
            {
                new JObject
                {
                    ["Action"] = "Move",
                    ["Item"] = "5c0f2b1d86f7744a9c0e8b3d", // Example item ID
                    ["From"] = "main", // Example source location
                    ["To"] = "stash" // Example destination location
                }
            });
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(data.ToJson()));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = stream;
            httpContext.Request.ContentLength = stream.Length;

            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };

            var controller = new GameController(_saveProvider, configuration, new TestsGlobalsService(), new InventoryService())
            {
                ControllerContext = controllerContext
            };
            var result = controller.ItemsMoving().Result;

            CommonTest_MustBeBSGSuccessBodyResult(result);
        }

    }
}