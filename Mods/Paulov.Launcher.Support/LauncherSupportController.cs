using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovServices;
using Paulov.TarkovServices.Providers.Interfaces;
using System.Text;

namespace Paulov.Launcher.Support
{
    public class LauncherSupportController : ControllerBase
    {
        //private SaveProvider saveProvider { get; } = new SaveProvider();

        private SaveProvider _saveProvider;
        public LauncherSupportController(ISaveProvider saveProvider)
        {
            _saveProvider = saveProvider as SaveProvider;
        }

        /// <summary>
        /// Handles user login requests by validating the provided username and managing session cookies.
        /// </summary>
        /// <remarks>This method processes login requests by extracting the username from the request body
        /// or route values. If the username is valid and a corresponding profile exists, a session cookie is created and
        /// the user's profile is loaded. If the username is missing or invalid, the method returns an unauthorized
        /// response.</remarks>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the login operation. Returns a success response with
        /// the session ID if the login is successful, or a failure response if the profile does not exist.</returns>
        [Route("launcher/profile/login", Name = "LauncherLogin")]
        [Route("launcher/profile/login/{username}", Name = "LauncherLoginWithUsername")]
        [HttpPost]
        public async Task<IActionResult> Login()
        {

            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            var resolvedUserName = "";
            if (requestBody != null && requestBody.ContainsKey("username"))
            {
                resolvedUserName = requestBody["username"].ToString();
            }
            else if (Request.RouteValues.ContainsKey("username"))
            {
                resolvedUserName = Request.RouteValues["username"].ToString();
            }
            if (string.IsNullOrEmpty(resolvedUserName))
            {
                Response.StatusCode = 401; // unauthorized
                //return new BSGErrorBodyResult(401, "Unauthorized");
                //throw new Exception(401.ToString());
                return new EmptyResult();
            }

            if (Request.Cookies.ContainsKey("PHPSESSID"))
            {
                Response.Cookies.Delete("PHPSESSID");
            }

            if (_saveProvider.ProfileExists(resolvedUserName, out var sessionId))
            {
                Response.Cookies.Append("PHPSESSID", sessionId);

                HttpContext.Session.Set("SessionId", Encoding.UTF8.GetBytes(sessionId));
                var profile = _saveProvider.LoadProfile(sessionId);
                //int aid = int.Parse(profile.AccountId);
                //HttpContext.Session.SetInt32("AccountId", aid);

                return new BSGSuccessBodyResult(sessionId);
            }
            else
                return new BSGSuccessBodyResult("FAILED");
        }

        /// <summary>
        /// Register to Server
        /// </summary>
        /// <returns></returns>
        //[HttpPost(Name = "login")]
        [Route("launcher/profile/register", Name = "LauncherRegister")]
        [HttpPost]
        public async Task<IActionResult> Register()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);
            var sessionId = _saveProvider.CreateAccount(requestBody);
            if (sessionId == null)
                return new BSGSuccessBodyResult("FAILED");

            return new BSGSuccessBodyResult(sessionId);
        }


        [Route("/launcher/server/connect", Name = "LauncherConnect")]
        [HttpPost]
        public async Task<IActionResult> LauncherServerConnect()
        {
            return new BSGSuccessBodyResult(true.ToString());
        }

        [Route("/launcher/ping", Name = "LauncherPing")]
        [HttpPost]
        public IActionResult LauncherServerPing()
        {
            return new BSGSuccessBodyResult(true.ToString());
        }


    }
}