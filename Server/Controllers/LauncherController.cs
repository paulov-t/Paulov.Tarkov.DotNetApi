//using Microsoft.AspNetCore.Mvc;
//using Paulov.Tarkov.WebServer.DOTNET.Middleware;
//using Paulov.Tarkov.WebServer.DOTNET.Providers;
//using Paulov.Tarkov.WebServer.DOTNET.ResponseModels;
//using System.Text;

//namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
//{
//    /// <summary>
//    /// Provides endpoints for user authentication, registration, and server connectivity in the launcher.
//    /// </summary>
//    /// <remarks>This controller includes methods for logging in, registering a new user, and interacting with
//    /// the server. It handles HTTP POST requests and produces JSON responses. Use this controller to manage user
//    /// profiles and establish server connections.</remarks>
//    [ApiController]
//    [Produces("application/json")]
//    public class LauncherController : ControllerBase
//    {
//        private SaveProvider saveProvider { get; } = new SaveProvider();

//        /// <summary>
//        /// Login to the Server
//        /// </summary>
//        /// <returns></returns>
//        [Route("launcher/profile/login", Name = "LauncherLogin")]
//        [Route("launcher/profile/login/{username}", Name = "LauncherLoginWithUsername")]
//        [HttpPost]
//        public async void Login()
//        {

//            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

//            var resolvedUserName = "";
//            if (requestBody != null && requestBody.ContainsKey("username"))
//            {
//                resolvedUserName = requestBody["username"].ToString();
//            }
//            else if (Request.RouteValues.ContainsKey("username"))
//            {
//                resolvedUserName = Request.RouteValues["username"].ToString();
//            }
//            if (string.IsNullOrEmpty(resolvedUserName))
//            {
//                Response.StatusCode = 401; // unauthorized
//                return;
//            }

//            if (Request.Cookies.ContainsKey("PHPSESSID"))
//            {
//                Response.Cookies.Delete("PHPSESSID");
//            }

//            if (saveProvider.ProfileExists(resolvedUserName, out var sessionId))
//            {
//                Response.Cookies.Append("PHPSESSID", sessionId);

//                HttpContext.Session.Set("SessionId", Encoding.UTF8.GetBytes(sessionId));
//                var profile = saveProvider.LoadProfile(sessionId);
//                //int aid = int.Parse(profile.AccountId);
//                //HttpContext.Session.SetInt32("AccountId", aid);

//                await HttpBodyConverters.CompressStringIntoResponseBody(sessionId, Request, Response);
//            }
//            else
//                await HttpBodyConverters.CompressStringIntoResponseBody("FAILED", Request, Response);
//        }

//        /// <summary>
//        /// Register to Server
//        /// </summary>
//        /// <returns></returns>
//        //[HttpPost(Name = "login")]
//        [Route("launcher/profile/register", Name = "LauncherRegister")]
//        [HttpPost]
//        public async void Register()
//        {
//            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);
//            var sessionId = saveProvider.CreateAccount(requestBody);
//            if (sessionId == null)
//                return;
//            await HttpBodyConverters.CompressStringIntoResponseBody(sessionId, Request, Response);
//        }


//        [Route("/launcher/server/connect", Name = "LauncherConnect")]
//        [HttpPost]
//        public async Task<IActionResult> LauncherServerConnect()
//        {
//            return new BSGSuccessBodyResult(true);
//        }

//        [Route("/launcher/ping", Name = "LauncherPing")]
//        [HttpPost]
//        public IActionResult LauncherServerPing()
//        {
//            return new BSGSuccessBodyResult(true);
//        }



//    }
//}
