using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Paulov.Tarkov.WebServer.DOTNET.ResponseModels;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class MailController : ControllerBase
    {
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
