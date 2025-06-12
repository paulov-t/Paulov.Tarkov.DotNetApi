using Paulov.TarkovModels;
using Paulov.TarkovServices.Providers.Interfaces;

namespace WebApiTests
{
    internal class NullSaveProvider : ISaveProvider
    {
        public string CreateAccount(Dictionary<string, object> parameters)
        {
            return null;
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

        public AccountProfileCharacter GetScavProfile(string sessionId)
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
