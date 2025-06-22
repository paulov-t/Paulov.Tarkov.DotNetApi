using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovServices.Helpers;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Providers.SaveProviders;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class LocaleController : ControllerBase
    {
        private JsonFileSaveProvider _saveProvider;
        public LocaleController(ISaveProvider saveProvider)
        {
            _saveProvider = saveProvider as JsonFileSaveProvider;
        }

        [Route("client/menu/locale/{language}")]
        [HttpPost]
        public async void MenuLocale([FromRoute] string language)
        {
            DatabaseHelpers.TryLoadLocales(out var locales, out var localesDict, out var languages);

            await HttpBodyConverters.CompressIntoResponseBodyBSG(localesDict.GetValue("menu_en").ToObject<JObject>()
                , Request, Response);

            locales = null;
            localesDict = null;
            languages = null;
        }

        [Route("client/languages")]
        [HttpPost]
        public async Task<IActionResult> Languages()
        {
            DatabaseHelpers.TryLoadLanguages(out var languages);
            return new BSGSuccessBodyResult(languages);
        }

        [Route("client/locale/{language}")]
        [HttpPost]
        public async Task<IActionResult> Locale([FromRoute] string language)
        {
            DatabaseHelpers.TryLoadLocaleGlobalEn(out string globalEn);

            return new BSGSuccessBodyResult(globalEn);
        }
    }
}
