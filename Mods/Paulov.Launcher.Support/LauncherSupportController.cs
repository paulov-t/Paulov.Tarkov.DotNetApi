using Microsoft.AspNetCore.Mvc;

namespace Paulov.Launcher.Support
{
    public class LauncherSupportController : Controller
    {
        public LauncherSupportController()
        {
        }

        //[Route("/launcher/server/connect")]
        //[HttpPost]
        //public IActionResult LauncherServerConnect(int? retry, bool? debug)
        //{
        //    return new JsonResult("");
        //}

        //[Route("/launcher/ping")]
        //[HttpPost]
        //public IActionResult LauncherServerPing(int? retry, bool? debug)
        //{
        //    return new JsonResult("");
        //}

        //    //[Route("/launcher/profile/login")]
        //    //[HttpPost]
        //    //public async Task<IActionResult> LauncherServerProfileLogin(int? retry, bool? debug)
        //    //{
        //    //    return new JsonResult("");
        //    //}

        //    //[Route("/launcher/profile/register")]
        //    //[HttpPost]
        //    //public async Task<IActionResult> LauncherServerProfileRegister(int? retry, bool? debug)
        //    //{
        //    //    return new JsonResult("");
        //    //}


        [Route("/launcher/server/connect", Name = "LauncherConnect")]
        [HttpPost]
        public void LauncherServerConnect(int? retry, bool? debug)
        {
        }

        [Route("/launcher/ping", Name = "LauncherPing")]
        [HttpPost]
        public void LauncherServerPing(int? retry, bool? debug)
        {
        }

    }
}