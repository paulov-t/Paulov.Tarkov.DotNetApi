using Newtonsoft.Json.Linq;

namespace Paulov.TarkovServices.Services.Interfaces
{
    public interface ILocationService
    {
        public JObject LoadLocations();

    }
}
