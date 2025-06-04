using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovServices;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class CustomizationController : ControllerBase
    {
        [Route("client/customization")]
        [HttpPost]
        public async void Customization(int? retry, bool? debug)
        {
            DatabaseProvider.TryLoadCustomization(out var items);

            await HttpBodyConverters.CompressIntoResponseBodyBSG(items, Request, Response);
        }

        [Route("client/customization/storage")]
        [HttpPost]
        public IActionResult Storage(int? retry, bool? debug)
        {
            DatabaseProvider.TryLoadDatabaseFile("database/templates/customisationStorage.json", out string file);

            return new BSGSuccessBodyResult(file);
        }
    }
}
