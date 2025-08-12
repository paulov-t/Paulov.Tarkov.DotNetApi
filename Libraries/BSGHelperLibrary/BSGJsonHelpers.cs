using EFT;
using Newtonsoft.Json;
using SIT.Core.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static SIT.Core.Misc.PaulovJsonConverters;

namespace SIT.BSGHelperLibrary
{
    public static class BSGJsonHelpers
    {
        public static Newtonsoft.Json.JsonConverter[] JsonConverterDefault { get; private set; }

        public static JsonConverter[] GetJsonConvertersBSG()
        {
            var tarkovTypes = typeof(TarkovApplication).Assembly.DefinedTypes;
            var convertersType = tarkovTypes.FirstOrDefault(x => x.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Any(p => p.Name == "Converters"));
            List<Newtonsoft.Json.JsonConverter> converters = new List<Newtonsoft.Json.JsonConverter>();
            if (convertersType != null)
                converters.AddRange((Newtonsoft.Json.JsonConverter[])convertersType.GetField("Converters", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).GetValue(null));
            converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            return converters.ToArray();
        }

        public static List<Newtonsoft.Json.JsonConverter> GetJsonConvertersPaulov()
        {
            var converters = new List<Newtonsoft.Json.JsonConverter>();
            converters.Add(new DateTimeOffsetJsonConverter());
            converters.Add(new SimpleCharacterControllerJsonConverter());
            converters.Add(new PlayerJsonConverter());
            converters.Add(new NotesJsonConverter());
            return converters;
        }

        private static List<Newtonsoft.Json.JsonConverter> SITSerializerConverters;

        public static JsonSerializerSettings GetJsonSerializerSettings()
        {
            if (SITSerializerConverters == null || SITSerializerConverters.Count == 0)
            {
                SITSerializerConverters = GetJsonConvertersPaulov();
                SITSerializerConverters.AddRange(GetJsonConvertersBSG().ToList());
            }

            return new JsonSerializerSettings()
            {
                Converters = SITSerializerConverters,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                FloatFormatHandling = FloatFormatHandling.DefaultValue,
                FloatParseHandling = FloatParseHandling.Double,
                Error = (serializer, err) =>
                {
                }
            };
        }
        public static JsonSerializerSettings GetJsonSerializerSettingsWithoutBSG()
        {
            var converters = GetJsonConvertersPaulov();

            return new JsonSerializerSettings()
            {
                Converters = converters,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Error = (serializer, err) =>
                {
                }
            };
        }

        public static string SITToJson(this object o)
        {


            return JsonConvert.SerializeObject(o
                    , GetJsonSerializerSettings()
                );
        }

        public static async Task<string> SITToJsonAsync(this object o)
        {
            return await Task.Run(() =>
            {
                return SITToJson(o);
            });
        }

        public static T SITParseJson<T>(this string str)
        {
            return JsonConvert.DeserializeObject<T>(str
                    , GetJsonSerializerSettings()
                    );
        }

        public static bool TrySITParseJson<T>(this string str, out T result)
        {
            try
            {
                result = SITParseJson<T>(str);
                return true;
            }
            catch (Exception)
            {
                result = default(T);
                return false;
            }
        }

    }
}
