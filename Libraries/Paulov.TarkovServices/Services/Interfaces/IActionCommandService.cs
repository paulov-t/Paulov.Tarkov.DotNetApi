using Newtonsoft.Json.Linq;
using static Paulov.TarkovServices.Services.ActionCommandService;

namespace Paulov.TarkovServices.Services.Interfaces
{
    public interface IActionCommandService
    {
        public Task<ExecuteCommandResult> ExecuteCommandAsync(JArray commands, string sessionId);
    }
}
