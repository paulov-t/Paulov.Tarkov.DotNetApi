using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Paulov.Tarkov.WebServer.DOTNET.Controllers;
using Paulov.TarkovServices.Providers.DatabaseProviders.FileDatabaseProviders;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Services;
using Paulov.TarkovServices.Services.Interfaces;

namespace WebApiTests
{
    public sealed class FriendControllerTests
    {
        private readonly ISaveProvider _saveProvider;
        private readonly IConfiguration configuration;
        private readonly IGlobalsService _globalsService;
        private readonly IDatabaseProvider databaseProvider;
        private readonly IDatabaseService databaseService;

        public FriendControllerTests()
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

        /// <summary>
        /// Tests the functionality of sending a friend request using the <see cref="FriendController"/>.
        /// </summary>
        /// <remarks>This test ensures that a friend request can be successfully sent between two
        /// profiles.  It verifies that the response is a valid JSON object containing the expected data
        /// structure.</remarks>
        [Test]
        public void FriendRequestTest()
        {
            new GameProfileControllerTests().ProfileCreateTest(); // Ensure the profile A is created before testing friend requests
            new GameProfileControllerTests().ProfileCreateTest(); // Ensure the profile B is created before testing friend requests

            // Arrange: Create a JSON object with the required fields for friend request (This comes from the client)
            var data = new JObject();
            data.Add("to", _saveProvider.GetProfiles().Last().Key); // Assuming the last profile is the one we want to send a request to
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

            // Create an instance of FriendController with the necessary dependencies
            var controller = new FriendController(_saveProvider, new FriendshipService(_saveProvider, new WebSocketService()), new WebSocketService())
            {
                ControllerContext = controllerContext
            };
            var result = controller.SendFriendRequest().Result;
            _ = result;

            // Assert: Check if the result is a BSGSuccessBodyResult
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
                Assert.Fail("Expected BSGSuccessBodyResult but got: " + result.GetType().Name);
            }

        }
    }
}
