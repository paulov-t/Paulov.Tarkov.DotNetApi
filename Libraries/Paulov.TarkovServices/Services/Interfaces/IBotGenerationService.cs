using Paulov.TarkovModels;

namespace Paulov.TarkovServices.Services.Interfaces
{
    public interface IBotGenerationService
    {

        public List<AccountProfileCharacter> GenerateBots(List<WaveInfoClass> conditions);

        public AccountProfileCharacter GenerateBot(WaveInfoClass condition);
    }
}
