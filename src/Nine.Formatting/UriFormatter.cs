namespace Nine.Formatting
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    public class UriFormatter : IFormatter, ITextFormatter
    {
        private static readonly ConcurrentDictionary<Type, object> defaultValues = new ConcurrentDictionary<Type, object>();
        private static readonly Encoding encoding = new UTF8Encoding(false, false);

        private readonly TextConverter converter;

        public UriFormatter() { }
        public UriFormatter(TextConverter converter) { this.converter = converter; }

        public object FromBytes(Type type, byte[] bytes, int index, int count)
        {
            return FromText(type, encoding.GetString(bytes, index, count));
        }

        public object FromText(Type type, string text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            var value = (string)null;
            var pairs = text.Split('&').Select(ToKeyValuePair).Where(p => p.Key != null).ToDictionary(p => p.Key, p => p.Value, StringComparer.OrdinalIgnoreCase);
            
            var result = Activator.CreateInstance(type);
            var typeInfo = type.GetTypeInfo();

            foreach (var property in typeInfo.DeclaredProperties)
            {
                if (property.GetMethod != null && property.GetMethod.IsPublic &&
                    property.SetMethod != null && property.SetMethod.IsPublic &&
                    pairs.TryGetValue(property.Name, out value))
                {
                    property.SetValue(result, ConvertBack(value, property.PropertyType));
                }
            }

            foreach (var field in typeInfo.DeclaredFields)
            {
                if (field.IsPublic && pairs.TryGetValue(field.Name, out value))
                {
                    field.SetValue(result, ConvertBack(value, field.FieldType));
                }
            }

            return result;
        }

        private KeyValuePair<string, string> ToKeyValuePair(string text)
        {
            if (string.IsNullOrEmpty(text)) return default(KeyValuePair<string, string>);
            var pair = text.Split('=');
            if (pair.Length <= 0) return default(KeyValuePair<string, string>);
            var key = UriQueryUtility.UrlDecode(pair[0].Trim());
            if (string.IsNullOrEmpty(key)) return default(KeyValuePair<string, string>);
            var value = pair.Length > 1 ? UriQueryUtility.UrlDecode(pair[1]) : null;
            return new KeyValuePair<string, string>(key, value);
        }

        private object ConvertBack(string text, Type type)
        {
            object result;
            if (converter != null && converter.FromText(type, text, out result)) return result;
            if (converter != null && converter.CanConvert(type)) return converter.FromText(type, text);
            if (string.IsNullOrEmpty(text)) return type.GetTypeInfo().IsValueType ? Activator.CreateInstance(type) : null;
            if (type == typeof(string)) return text;

            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsPrimitive) return Convert.ChangeType(text, type);
            if (typeInfo.IsEnum) return TryParseEnum(type, text);
            if (type == typeof(TimeSpan)) return TimeSpan.Parse(text);
            if (type == typeof(DateTime)) return DateTime.Parse(text).ToUniversalTime();
            if (type == typeof(DateTimeOffset)) return DateTimeOffset.Parse(text).ToUniversalTime();

            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null) return ConvertBack(text, underlyingType);

            throw new NotSupportedException(type.FullName + " is not supported by UrlFormatter");
        }

        public byte[] ToBytes(object value)
        {
            return encoding.GetBytes(ToText(value));
        }

        public string ToText(object value)
        {
            if (Equals(value, null)) return null;

            var typeInfo = value.GetType().GetTypeInfo();
            var properties =
                from property in typeInfo.DeclaredProperties
                where property.GetMethod != null && property.GetMethod.IsPublic &&
                      property.SetMethod != null && property.SetMethod.IsPublic
                select new KeyValuePair<string, object>(property.Name.ToLowerInvariant(), property.GetValue(value));

            var fields =
                from field in typeInfo.DeclaredFields
                where field.IsPublic
                select new KeyValuePair<string, object>(field.Name.ToLowerInvariant(), field.GetValue(value));

            var parameters = properties.Concat(fields).Where(p => !IsDefaultValue(p.Value));
            
            return string.Join("&", parameters.Select(EncodeParameter).ToArray());
        }

        private object EncodeParameter(KeyValuePair<string, object> parameter)
        {
            return parameter.Value == null
                ? string.Concat(UriQueryUtility.UrlEncode(parameter.Key), "=")
                : string.Concat(UriQueryUtility.UrlEncode(parameter.Key), "=", UriQueryUtility.UrlEncode(ConvertTo(parameter.Value)));
        }

        private string ConvertTo(object value)
        {
            string result;
            if (converter != null && converter.ToText(value, out result)) return result;
            if (value is DateTime) return ((DateTime)value).ToString("o");
            return value.ToString();
        }

        private static bool IsDefaultValue(object value)
        {
            if (value == null) return true;

            var type = value.GetType();
            if (type.GetTypeInfo().IsValueType)
            {
                return Equals(value, defaultValues.GetOrAdd(type, t => Activator.CreateInstance(t)));
            }
            return false;
        }
        
        private static object TryParseEnum(Type enumType, string text)
        {
            try
            {
                return Enum.Parse(enumType, text, true);
            }
            catch (ArgumentException)
            {
                return 0;
            }
        }
    }
}
