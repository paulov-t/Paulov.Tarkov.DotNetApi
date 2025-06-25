using Newtonsoft.Json.Linq;

namespace Paulov.TarkovServices.Services.Interfaces
{
    public interface IActionCommandService
    {
        public Task<JObject> ExecuteCommandAsync(JArray commands, string sessionId);
    }
}
