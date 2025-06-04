using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.Tarkov.WebServer.DOTNET.Providers;
using Paulov.Tarkov.WebServer.DOTNET.ResponseModels;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class LocaleController : ControllerBase
    {
        private SaveProvider saveProvider { get; } = new SaveProvider();

        [Route("client/menu/locale/{language}")]
        [HttpPost]
        public async void MenuLocale([FromRoute] string language, int? retry, bool? debug)
        {
            DatabaseProvider.TryLoadLocales(out var locales, out var localesDict, out var languages);

            await HttpBodyConverters.CompressIntoResponseBodyBSG(localesDict.GetValue("menu_en").ToObject<JObject>()
                , Request, Response);

            locales = null;
            localesDict = null;
            languages = null;
        }

        [Route("client/languages")]
        [HttpPost]
        public async Task<IActionResult> Languages(int? retry, bool? debug)
        {
            DatabaseProvider.TryLoadLanguages(out var languages);
            return new BSGSuccessBodyResult(languages);
        }

        [Route("client/locale/{language}")]
        [HttpPost]
        public async Task<IActionResult> Locale([FromRoute] string language, int? retry, bool? debug)
        {
            DatabaseProvider.TryLoadLocaleGlobalEn(out string globalEn);

            return new BSGSuccessBodyResult(globalEn);
        }
    }
}
