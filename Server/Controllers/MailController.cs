using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Paulov.TarkovServices;
using Paulov.TarkovServices.Providers.Interfaces;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class MailController : ControllerBase
    {
        private SaveProvider _saveProvider;
        public MailController(ISaveProvider saveProvider)
        {
            _saveProvider = saveProvider as SaveProvider;
        }

        [Route("client/mail/dialog/view")]
        [HttpPost]
        public async Task<IActionResult> DialogView()
        {
            return new BSGSuccessBodyResult(new JObject());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="retry"></param>
        /// <param name="debug"></param>
        [Route("client/mail/dialog/list")]
        [HttpPost]
        public async Task<IActionResult> MailDialogList(int? retry, bool? debug)
        {
            return new BSGSuccessBodyResult(new JArray());
        }

    }
}
