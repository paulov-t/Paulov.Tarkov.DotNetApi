using Paulov.TarkovModels;
using Paulov.TarkovServices.Providers.Interfaces;

namespace WebApiTests
{
    internal class SimpleSaveProvider : ISaveProvider
    {
        Account Account { get; set; } = new Account()
        {
            AccountId = "00000000000000000000001",
            Username = "TestUser",
            Password = "Password123",
            Edition = "Edge_Of_Darkness",
            CurrentMode = "PVE",
            Modes = new AccountProfileModes()
            {

            }
        };

        public SimpleSaveProvider()
        {

        }

        public string CreateAccount(Dictionary<string, object> parameters)
        {
            return Account.AccountId; // Return a dummy account ID
        }

        public AccountProfileMode GetAccountProfileMode(Account account)
        {
            return Account.Modes.PVE;
        }

        public AccountProfileMode GetAccountProfileMode(string sessionId)
        {
            return Account.Modes.PVE;

        }

        public AccountProfileCharacter GetPmcProfile(string sessionId)
        {
            return Account.Modes.PVE.Characters.PMC;

        }

        public AccountProfileCharacter GetPmcProfile(Account account)
        {
            return Account.Modes.PVE.Characters.PMC;
        }

        public AccountProfileCharacter GetScavProfile(string sessionId)
        {
            return Account.Modes.PVE.Characters.Scav;
        }

        public AccountProfileCharacter GetScavProfile(Account account)
        {
            return Account.Modes.PVE.Characters.Scav;
        }

        public Account LoadProfile(string sessionId)
        {
            return Account;
        }

        public void SaveProfile(string sessionId, Account profileModel = null)
        {
            Account = profileModel ?? Account; // Save the provided profile or keep the existing one if null
        }

        public Dictionary<string, Account> GetProfiles()
        {
            return new Dictionary<string, Account> { { Account.AccountId, Account } }; // Return a list containing the single account
        }
    }
}
