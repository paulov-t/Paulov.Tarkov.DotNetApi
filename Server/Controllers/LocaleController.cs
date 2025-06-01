using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.Tarkov.WebServer.DOTNET.Providers;

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
        public async void Languages(int? retry, bool? debug)
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            DatabaseProvider.TryLoadLanguages(out var languages);

            await HttpBodyConverters.CompressIntoResponseBodyBSG(
                languages
                , Request
                , Response);

            languages = null;
        }

        [Route("client/locale/{language}")]
        [HttpPost]
        public void Locale([FromRoute] string language, int? retry, bool? debug)
        {
            DatabaseProvider.TryLoadLocaleGlobalEn(out string globalEn);

            HttpBodyConverters.CompressIntoResponseBodyBSG(
                globalEn
                , Request
                , Response).Wait();

            globalEn = null;

        }
    }
}
