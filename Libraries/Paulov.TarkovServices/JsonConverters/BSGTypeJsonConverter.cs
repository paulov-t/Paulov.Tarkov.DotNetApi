using Newtonsoft.Json;

namespace Paulov.TarkovServices.JsonConverters
{
    public class BSGTypeJsonConverter<T> : JsonConverter where T : struct, Enum
    {
        private static readonly Type type_0 = typeof(T);

        private readonly Dictionary<string, T> dictionary_0;

        private readonly Dictionary<T, string> dictionary_1;

        private readonly bool bool_0;

        public BSGTypeJsonConverter(bool caseSensitive = true)
        {
            bool_0 = caseSensitive;
            int count = GClass861<T>.Count;
            dictionary_0 = new Dictionary<string, T>(count);
            dictionary_1 = new Dictionary<T, string>(count);
            for (int num = 0; num < count; num++)
            {
                T val = GClass861<T>.Values[num];
                string text = GClass861<T>.Names[num];
                GAttribute21 obj = type_0.GetMember(text).First().GetCustomAttributes(typeof(GAttribute21), inherit: false)
                    .FirstOrDefault() as GAttribute21;
                object obj2;
                if (obj == null)
                {
                    obj2 = null;
                }
                else
                {
                    obj2 = obj.Name;
                    if (obj2 != null)
                    {
                        goto IL_0085;
                    }
                }
                obj2 = text;
                goto IL_0085;
            IL_0085:
                text = (string)obj2;
                dictionary_1.Add(val, text);
                if (!bool_0)
                {
                    text = text.ToLower();
                }
                dictionary_0.Add(text, val);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(dictionary_1[(T)value]);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string text = serializer.Deserialize<string>(reader);
            if (!bool_0)
            {
                text = text.ToLower();
            }

            T val = GClass861<T>.Values.First();
            if (text == null)
                return val;

            if (dictionary_0.TryGetValue(text, out var value))
                return value;
            return val;
        }

        public override bool CanConvert(Type objectType)
        {
            //Debug.WriteLine(objectType.Name);
            return objectType.Name == type_0.Name;
        }
    }
}
