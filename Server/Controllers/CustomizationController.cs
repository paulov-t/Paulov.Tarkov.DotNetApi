using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Providers.SaveProviders;
using Paulov.TarkovServices.Services;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class CustomizationController : ControllerBase
    {
        private JsonFileSaveProvider _saveProvider;
        public CustomizationController(ISaveProvider saveProvider)
        {
            _saveProvider = saveProvider as JsonFileSaveProvider;
        }

        [Route("client/customization")]
        [HttpPost]
        public async void Customization(int? retry, bool? debug)
        {
            DatabaseService.TryLoadCustomization(out var items);

            await HttpBodyConverters.CompressIntoResponseBodyBSG(items, Request, Response);
        }

        [Route("client/customization/storage")]
        [HttpPost]
        public IActionResult Storage(int? retry, bool? debug)
        {
            DatabaseService.TryLoadDatabaseFile("database/templates/customisationStorage.json", out string file);

            return new BSGSuccessBodyResult(file);
        }
    }
}
