using Microsoft.AspNetCore.Mvc;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class MailController : ControllerBase
    {
        [Route("client/mail/dialog/view")]
        [HttpPost]
        public async void DialogView()
        {
        }
    }
}
