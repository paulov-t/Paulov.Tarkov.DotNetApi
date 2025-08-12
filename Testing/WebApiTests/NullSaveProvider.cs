using Paulov.TarkovModels;
using Paulov.TarkovServices.Providers.Interfaces;

namespace WebApiTests
{
    internal class NullSaveProvider : ISaveProvider
    {
        public string CreateAccount(Dictionary<string, object> parameters)
        {
            return "00000000000000000000001"; // Return a dummy account ID
        }

        public string CreateAccount(AccountCreationModel creationModel)
        {
            return "00000000000000000000001"; // Return a dummy account ID
        }

        public AccountProfileMode GetAccountProfileMode(Account account)
        {
            return null;
        }

        public AccountProfileMode GetAccountProfileMode(string sessionId)
        {
            return null;
        }

        public AccountProfileCharacter GetPmcProfile(string sessionId)
        {
            return null;
        }

        public AccountProfileCharacter GetPmcProfile(Account account)
        {
            return null;
        }

        public Dictionary<string, Account> GetProfiles()
        {
            return new Dictionary<string, Account>
            {
                {
                    "00000000000000000000001",
                    new Account
                    {
                        AccountId = "00000000000000000000001",
                        Username = "TestUser",
                        Password = "TestPassword",
                        Modes = new AccountProfileModes()
                    }
                }
            };
        }

        public AccountProfileCharacter GetScavProfile(string sessionId)
        {
            return null;
        }

        public AccountProfileCharacter GetScavProfile(Account account)
        {
            return null;
        }

        public Account LoadProfile(string sessionId)
        {
            return null;
        }

        public void SaveProfile(string sessionId, Account profileModel = null)
        {
        }
    }
}
