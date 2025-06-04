using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Paulov.Tarkov.WebServer.DOTNET.ResponseModels.Survey
{
    public class SurveyResponseModel
    {
        // Token: 0x040097D3 RID: 38867
        [JsonProperty("locale")]
        public JObject localization;

        // Token: 0x040097D4 RID: 38868
        [JsonProperty("survey")]
        //public GClass1912 template;
        public object template;
    }
}
