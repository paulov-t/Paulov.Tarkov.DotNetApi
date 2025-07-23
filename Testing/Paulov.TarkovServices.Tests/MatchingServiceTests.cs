using EFT;
using Microsoft.Extensions.Configuration;
using Paulov.TarkovServices.Providers.DatabaseProviders.FileDatabaseProviders;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Providers.SaveProviders;
using Paulov.TarkovServices.Services;

namespace Paulov.TarkovServices.Tests
{
    internal class MatchingServiceTests
    {
        private readonly JsonFileSaveProvider _saveProvider;
        private readonly MatchingService _matchingService;
        private readonly IDatabaseProvider _databaseProvider;
        IConfiguration configuration;
        private readonly string MySessionId = MongoID.Generate();
        private readonly string SessionId_Person2 = MongoID.Generate();
        private readonly string InviteRequestId = MongoID.Generate();

        public MatchingServiceTests()
        {
            configuration = new ConfigurationBuilder()
              .SetBasePath(AppContext.BaseDirectory)
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .Build();

            _saveProvider = new JsonFileSaveProvider();
            _databaseProvider = new JsonFileCollectionDatabaseProvider();
            _databaseProvider.Connect(AppContext.BaseDirectory);
            var databaseService = new DatabaseService(configuration, _databaseProvider);
            _matchingService = new MatchingService(new AccountService(_saveProvider, new InventoryService(), databaseService));
        }

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        [Order(1)]
        public void CreateMatchingGroupTest()
        {
            Assert.That(_matchingService.CreateMatchingGroupBySessionId(MySessionId) != null);
            Assert.That(_matchingService.MatchingGroups.Count > 0);
        }

        [Test]
        [Order(2)]
        public void GetMatchingGroupTest()
        {
            Assert.That(_matchingService.GetMatchingGroupBySessionId(MySessionId) != null);
        }

        [Test]
        [Order(3)]
        public void MatchingGroupSendInvite()
        {
            Assert.That(_matchingService.SendInviteToUser(requestId: InviteRequestId, MySessionId, SessionId_Person2) != null);
        }

        [Test]
        [Order(4)]
        public void MatchingGroupAcceptInvite()
        {
            Assert.That(_matchingService.AcceptInviteFromUser(SessionId_Person2) != null);
        }

        [Test]
        [Order(5)]
        public void MatchingGroupCancelInvite()
        {
            // Delete all traces of existing MatchingGroup
            _matchingService.DeleteMatchingGroupBySessionId(MySessionId);
            // Recreate the MatchingGroup and Send an Invite
            CreateMatchingGroupTest();
            MatchingGroupSendInvite();
            // Cancel the Invite with the requestId
            Assert.IsTrue(_matchingService.CancelInviteToUser(InviteRequestId));
        }
    }
}
