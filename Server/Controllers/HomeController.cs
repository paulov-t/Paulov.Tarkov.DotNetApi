using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    /// <summary>
    /// Represents the controller responsible for handling HTTP requests to the application's root and specific routes.
    /// </summary>
    /// <remarks>The <see cref="HomeController"/> provides actions to render views for the root URL, "/ammo",
    /// and "/items" routes. It uses the application's configuration settings to supply additional data to the views,
    /// such as the server mode.</remarks>
    public class HomeController : Controller
    {
        IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration settings. This is used to retrieve configuration values required by the
        /// controller.</param>
        public HomeController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Handles HTTP GET requests to the root URL and returns the default view for the application.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> representing the rendered view for the root URL.</returns>
        [Route("/")]
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.AppVersion = Assembly.GetAssembly(typeof(HomeController)).GetName().Version;
            ViewBag.ServerMode = this.configuration["ServerMode"].ToString();
            return View();
        }

        /// <summary>
        /// Handles HTTP GET requests to the "/ammo" route and returns the corresponding view.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> representing the rendered view for the "/ammo" route.</returns>
        [Route("/ammo")]
        [HttpGet]
        public IActionResult Ammo()
        {
            ViewBag.AppVersion = Assembly.GetAssembly(typeof(HomeController)).GetName().Version;
            ViewBag.ServerMode = this.configuration["ServerMode"].ToString();
            return View();
        }

        /// <summary>
        /// Handles HTTP GET requests to retrieve the items view.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> representing the rendered items view.</returns>
        [Route("/items")]
        [HttpGet]
        public IActionResult Items()
        {
            ViewBag.AppVersion = Assembly.GetAssembly(typeof(HomeController)).GetName().Version;
            ViewBag.ServerMode = this.configuration["ServerMode"].ToString();
            return View();
        }

        /// <summary>
        /// Handles HTTP GET requests to retrieve the traders view.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> representing the rendered items view.</returns>
        [Route("/traders")]
        [HttpGet]
        public IActionResult Traders()
        {
            return View();
        }
    }
}
