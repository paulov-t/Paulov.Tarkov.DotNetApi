using Newtonsoft.Json.Linq;

namespace Paulov.TarkovServices.Services.Interfaces
{
    public interface ILootGenerationService
    {
        public JArray GenerateLootForLocation(JObject location);
    }
}
