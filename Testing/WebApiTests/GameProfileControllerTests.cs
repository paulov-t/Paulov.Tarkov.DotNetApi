using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Paulov.Tarkov.Web.Api.Controllers;
using Paulov.TarkovServices.Providers.DatabaseProviders.FileDatabaseProviders;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Services;
using Paulov.TarkovServices.Services.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace WebApiTests
{
    public sealed class GameProfileControllerTests
    {
        private readonly ISaveProvider _saveProvider;
        private readonly IConfiguration configuration;
        private readonly IGlobalsService _globalsService;
        private readonly IDatabaseProvider databaseProvider;
        private readonly IDatabaseService databaseService;

        public GameProfileControllerTests()
        {
            databaseProvider = new JsonFileCollectionDatabaseProvider();
            databaseProvider.Connect(AppContext.BaseDirectory);
            _saveProvider = new SimpleSaveProvider();
            configuration = new ConfigurationBuilder().Build();
            databaseService = new DatabaseService(configuration, databaseProvider);
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
        public void ProfileCreateTest()
        {
            // Arrange: Create a JSON object with the required fields for profile creation (This comes from the client)
            var data = new JObject();
            data.Add("side", "usec");
            data.Add("nickname", "Tests");
            data.Add("headId", "60a6aaad42fd2735e4589978");
            data.Add("voiceId", "5fc615110b735e7b024c76ea");
            var requestBodyStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(data.ToJson()));

            // Set up the HttpContext with the request body and session
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = requestBodyStream;
            httpContext.Request.ContentLength = requestBodyStream.Length;
            httpContext.Session = new SessionForGameProfileControllerTests();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };

            // Act: Create an instance of the GameProfileController and call the ProfileCreate method
            var controller = new GameProfileController(_saveProvider, new TestsGlobalsService())
            {
                ControllerContext = controllerContext
            };
            var result = controller.ProfileCreate().Result;
            _ = result;

            // TODO: Add assertions to verify the profile creation logic
        }
    }

    public class SessionForGameProfileControllerTests : Microsoft.AspNetCore.Http.ISession
    {
        public bool IsAvailable => throw new NotImplementedException();

        public string Id => throw new NotImplementedException();

        public IEnumerable<string> Keys => throw new NotImplementedException();

        public void Clear()
        {
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
        }

        public void Set(string key, byte[] value)
        {
        }

        public bool TryGetValue(string key, [NotNullWhen(true)] out byte[] value)
        {
            value = Encoding.UTF8.GetBytes("60a6aaad42fd2735e4589978");
            return true;
        }
    }
}
