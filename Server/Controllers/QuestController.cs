using BSGHelperLibrary.ResponseModels;
using EFT;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Providers.SaveProviders;
using Paulov.TarkovServices.Services.Interfaces;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class QuestController : Controller
    {
        private JsonFileSaveProvider _saveProvider;
        private readonly IQuestService _questService;

        public QuestController(ISaveProvider saveProvider, IQuestService questService)
        {
            _saveProvider = saveProvider as JsonFileSaveProvider;
            _questService = questService ?? throw new ArgumentNullException(nameof(questService), "Quest service cannot be null.");
        }

        private string SessionId
        {
            get
            {
                return HttpSessionHelpers.GetSessionId(Request, HttpContext);
            }
        }

        [Route("client/quest/list")]
        [HttpPost]
        public async Task<IActionResult> QuestList()
        {
            var sessionId = SessionId;
#if !DEBUG
            if (string.IsNullOrEmpty(sessionId))
            {
                Response.StatusCode = 412; // Precondition
                return StatusCode(500);
            }
#else
            if (string.IsNullOrEmpty(sessionId))
                sessionId = _saveProvider?.GetProfiles().First().Key;
#endif

            var questList = _questService.GetQuestsForAccount(_saveProvider.LoadProfile(sessionId));


            var newtonSoftJsonSerializer = new Newtonsoft.Json.JsonSerializer
            {
            };
            var tarkovTypes = typeof(TarkovApplication).Assembly.DefinedTypes;
            var convertersType = tarkovTypes.FirstOrDefault(x => x.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Any(p => p.Name == "Converters"));
            List<Newtonsoft.Json.JsonConverter> converters = new List<Newtonsoft.Json.JsonConverter>();
            if (convertersType != null)
            {
                converters.AddRange((Newtonsoft.Json.JsonConverter[])convertersType.GetField("Converters", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).GetValue(null));
                // the GClass1669`1 converter is calling an ECall error because its using Unity Loggers...
                //foreach (var converter in converters.Where(x => x.GetType().Name != "GClass1669`1"))
                //    CachedSerializer.Converters.Add(converter);
            }
            converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            foreach (var converter in converters)
            {
                newtonSoftJsonSerializer.Converters.Add(converter);
            }


            var returnString = JsonConvert.SerializeObject(questList, Formatting.Indented, converters.ToArray());

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);

            //return new BSGSuccessBodyResult(returnString);
            // TODO: This breaks after leaving a raid. Needs fixing.
            return new BSGSuccessBodyResult("[]");
        }
    }
}
