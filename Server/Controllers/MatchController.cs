using Microsoft.AspNetCore.Mvc;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovServices;
using Paulov.TarkovServices.Providers.Interfaces;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class MatchController : ControllerBase
    {
        private SaveProvider _saveProvider;
        public MatchController(ISaveProvider saveProvider)
        {
            _saveProvider = saveProvider as SaveProvider;
        }


        [Route("client/match/group/invite/cancel-all")]
        [HttpPost]
        public async void CancelAllGroupInvites(int? retry, bool? debug, string traderId)
        {
            await HttpBodyConverters.CompressIntoResponseBodyBSG(new { }, Request, Response);
        }
    }
}
