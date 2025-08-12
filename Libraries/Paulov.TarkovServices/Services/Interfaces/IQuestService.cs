using Paulov.TarkovModels;

namespace Paulov.TarkovServices.Services.Interfaces
{
    public interface IQuestService
    {
        public List<RawQuestClass> GetQuestsForAccount(Account account);

    }
}
