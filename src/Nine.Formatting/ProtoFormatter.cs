namespace Nine.Formatting
{
    using System;
    using System.Reflection;
    using System.IO;
    using System.Linq;
    using ProtoBuf;
    using System.Collections.Concurrent;

    public class ProtoFormatter : IFormatter
    {
        public object ReadFrom(Type type, Stream stream)
        {
            var result = Serializer.Deserialize(type, stream);
            MakeDateTimeUtc(result);
            return result;
        }

        public void WriteTo(object value, Stream stream)
        {
            Serializer.Serialize(stream, value);
        }

        private void MakeDateTimeUtc(object result)
        {
            var converter = convertDateTime.GetOrAdd(result.GetType(), x => new Lazy<Action<object>>(() => Go(x)));
            converter.Value(result);
        }

        private static readonly ConcurrentDictionary<Type, Lazy<Action<object>>> convertDateTime = new ConcurrentDictionary<Type, Lazy<Action<object>>>();

        private static Action<object> Go(Type type)
        {
            var properties = (
                from property in type.GetTypeInfo().DeclaredProperties
                where property.PropertyType == typeof(DateTime) &&
                      property.GetMethod != null && property.GetMethod.IsPublic &&
                      property.SetMethod != null && property.SetMethod.IsPublic &&
                      property.GetIndexParameters().Length <= 0
                select property).ToArray();

            var fields = (
                from field in type.GetTypeInfo().DeclaredFields
                where field.IsPublic && field.FieldType == typeof(DateTime)
                select field).ToArray();

            return (object obj) =>
            {
                foreach (var prop in properties)
                {
                    prop.SetValue(obj, ToUtc((DateTime)prop.GetValue(obj)));
                }
                foreach (var field in fields)
                {
                    field.SetValue(obj, ToUtc((DateTime)field.GetValue(obj)));
                }
            };
        }

        private static DateTime ToUtc(DateTime value)
        {
            return DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
    }
}
