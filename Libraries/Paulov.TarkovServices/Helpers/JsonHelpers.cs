using EFT;
using Newtonsoft.Json;

namespace Paulov.TarkovServices.Helpers
{
    public static class JsonHelpers
    {
        public static JsonSerializer GetNewtonsoftJsonSerializer()
        {
            var newtonSoftJsonSerializer = new Newtonsoft.Json.JsonSerializer
            {
            };

            foreach (var converter in GetNewtonsoftJsonSerializerConverters())
                newtonSoftJsonSerializer.Converters.Add(converter);

            return newtonSoftJsonSerializer;
        }

        public static JsonConverter[] GetNewtonsoftJsonSerializerConverters()
        {
            var tarkovTypes = typeof(TarkovApplication).Assembly.DefinedTypes;
            var convertersType = tarkovTypes.FirstOrDefault(x => x.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Any(p => p.Name == "Converters"));
            List<Newtonsoft.Json.JsonConverter> converters = new List<Newtonsoft.Json.JsonConverter>();
            if (convertersType != null)
                converters.AddRange((Newtonsoft.Json.JsonConverter[])convertersType.GetField("Converters", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).GetValue(null));
            converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            return converters.ToArray();
        }

        public static T DeserializeUsingNewtonsoft<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentNullException(nameof(json), "JSON string cannot be null or empty.");
            var newtonSoftJsonSerializer = GetNewtonsoftJsonSerializer();
            using (var stringReader = new StringReader(json))
            using (var jsonReader = new JsonTextReader(stringReader))
            {
                return newtonSoftJsonSerializer.Deserialize<T>(jsonReader);
            }
        }
    }
}