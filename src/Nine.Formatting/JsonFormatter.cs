namespace Nine.Formatting
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    public class JsonFormatter : IFormatter, ITextFormatter
    {
        private readonly Encoding encoding = new UTF8Encoding(false);
        private readonly JsonSerializer json;
        
        public JsonFormatter(TextConverter textConverter = null, params JsonConverter[] converters)
        {
            json = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };

            if (converters != null)
            {
                foreach (var converter in converters)
                {
                    json.Converters.Add(converter);
                }
            }

            if (textConverter != null)
            {
                json.Converters.Add(new TextConverterJsonConverter(textConverter));
            }

            json.Converters.Add(new NoThrowStringEnumConverter());
        }

        public object FromBytes(Type type, byte[] bytes, int index, int count)
        {
            return FromText(type, encoding.GetString(bytes, index, count));
        }

        public byte[] ToBytes(object value)
        {
            return encoding.GetBytes(ToText(value));
        }

        public string ToText(object value)
        {
            var sb = StringBuilderCache.Acquire(250);
            var stringWriter = new StringWriter(sb, CultureInfo.InvariantCulture);
            using (var writer = new JsonTextWriter(stringWriter))
            {
                writer.Formatting = json.Formatting;

                json.Serialize(writer, value);
            }
            return StringBuilderCache.GetStringAndRelease(sb);
        }

        public object FromText(Type type, string text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            using (var reader = new JsonTextReader(new StringReader(text)))
            {
                return json.Deserialize(reader, type);
            }
        }

        class NoThrowStringEnumConverter : StringEnumConverter
        {
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                try
                {
                    return base.ReadJson(reader, objectType, existingValue, serializer);
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        class TextConverterJsonConverter : JsonConverter
        {
            private readonly TextConverter converter;

            public TextConverterJsonConverter(TextConverter converter)
            {
                this.converter = converter;
            }

            public override bool CanConvert(Type objectType)
            {
                return converter.CanConvert(objectType);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return converter.FromText(objectType, reader.Value.ToString());
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(converter.ToText(value));
            }
        }
    }
}
