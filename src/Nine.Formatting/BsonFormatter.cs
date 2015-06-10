namespace Nine.Formatting
{
    using System;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Bson;
    using Newtonsoft.Json.Serialization;

    public class BsonFormatter : IFormatter
    {
        private readonly Encoding encoding = new UTF8Encoding(false);
        private readonly JsonSerializer json;

        public BsonFormatter(params JsonConverter[] defaultConverters)
        {
            json = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };

            if (defaultConverters != null)
            {
                foreach (var converter in defaultConverters)
                {
                    json.Converters.Add(converter);
                }
            }
            json.Converters.Add(new TimeSpanConverter());
            json.Converters.Add(new DateTimeConverter());
        }

        public object FromBytes(Type type, byte[] bytes, int index, int count)
        {
            using (var reader = new BsonReader(new MemoryStream(bytes, index, count)))
            {
                return json.Deserialize(reader, type);
            }
        }

        public byte[] ToBytes(object value)
        {
            var ms = new MemoryStream(256);
            using (var writer = new BsonWriter(ms))
            {
                writer.Formatting = json.Formatting;
                json.Serialize(writer, value);
            }
            return ms.ToArray();
        }

        class TimeSpanConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(TimeSpan);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(((TimeSpan)value).Ticks);
            }

            public override object ReadJson(JsonReader reader, Type type, object value, JsonSerializer serializer)
            {
                return TimeSpan.FromTicks((long)reader.Value);
            }
        }

        class DateTimeConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(DateTime);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(((DateTime)value).Ticks);
            }

            public override object ReadJson(JsonReader reader, Type type, object value, JsonSerializer serializer)
            {
                return new DateTime((long)reader.Value, DateTimeKind.Utc);
            }
        }
    }
}
