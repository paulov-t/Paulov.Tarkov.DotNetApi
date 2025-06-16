using NUnit.Framework.Internal;
using Paulov.TarkovServices.Providers.SaveProviders;

namespace Paulov.TarkovServices.Tests
{
    public class JsonFileSaveProviderTests
    {
        private readonly JsonFileSaveProvider _saveProvider;

        public JsonFileSaveProviderTests()
        {
            _saveProvider = new JsonFileSaveProvider();
        }

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        [Order(1)]
        public void CreateProfileTest()
        {
            var userId = new Randomizer().NextLong().ToString();
            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
            {
                { "username", userId.ToString() },
                { "password", "password123" },
                { "accountId", userId.ToString() },
                { "edition", "Standard" },
                { "currentMode", "PvE" }
            };
            _saveProvider.CreateAccount(keyValuePairs);
        }

        [Test]
        [Order(2)]
        public void LoadProfilesTest()
        {
            Assert.That(_saveProvider.GetProfiles().Count > 0);
        }
    }
}