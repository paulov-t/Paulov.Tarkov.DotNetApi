using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Providers.SaveProviders;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class MailController : ControllerBase
    {
        private JsonFileSaveProvider _saveProvider;
        public MailController(ISaveProvider saveProvider)
        {
            _saveProvider = saveProvider as JsonFileSaveProvider;
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
