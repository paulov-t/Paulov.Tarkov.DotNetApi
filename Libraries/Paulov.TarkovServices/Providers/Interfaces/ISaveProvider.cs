using Paulov.TarkovModels;

namespace Paulov.TarkovServices.Providers.Interfaces
{
    public interface ISaveProvider
    {
        public string CreateAccount(Dictionary<string, object> parameters);

        public void SaveProfile(string sessionId, Account profileModel = null);

        public Account LoadProfile(string sessionId);

        public AccountProfileMode GetAccountProfileMode(Account account);
        public AccountProfileMode GetAccountProfileMode(string sessionId);

        public AccountProfileCharacter GetPmcProfile(string sessionId);

        public AccountProfileCharacter GetScavProfile(string sessionId);

    }
}
