using Paulov.TarkovModels;

namespace Paulov.TarkovServices.Providers.Interfaces
{
    public interface ISaveProvider
    {
        public string CreateAccount(Dictionary<string, object> parameters);

        public void SaveProfile(string sessionId, Account profileModel = null);

        public Account LoadProfile(string sessionId);

        public AccountProfileMode GetAccountProfileMode(Account account);

        public AccountProfileCharacter GetPmcProfile(Account account);

        public AccountProfileCharacter GetScavProfile(Account account);
    }
}
