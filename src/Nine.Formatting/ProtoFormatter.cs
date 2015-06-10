namespace Nine.Formatting
{
    using System;
    using System.Reflection;
    using System.IO;
    using System.Linq;
    using ProtoBuf;
    using System.Collections.Concurrent;

#if !PCL
    public class ProtoFormatter : IFormatter
    {
        private readonly MethodInfo deserialize = typeof(Serializer).GetRuntimeMethods().Single(m => m.Name == "Deserialize");

        public object FromBytes(Type type, byte[] bytes, int index, int count)
        {
            var result = deserialize.MakeGenericMethod(type).Invoke(null, new object[] { new MemoryStream(bytes, index, count) });
            MakeDateTimeUtc(result);
            return result;
        }

        public byte[] ToBytes(object value)
        {
            if (value == null) return new byte[0];

            var ms = new MemoryStream(256);
            Serializer.Serialize(ms, value);
            return ms.ToArray();
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
#endif
}
