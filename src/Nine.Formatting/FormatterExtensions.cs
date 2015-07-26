namespace Nine.Formatting
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class FormatterExtensions
    {
        public static T FromBytes<T>(this IFormatter formatter, byte[] bytes, int index, int count)
            => (T)formatter.FromBytes(typeof(T), bytes, index, count);

        public static T FromBytes<T>(this IFormatter formatter, byte[] bytes)
            => formatter.FromBytes<T>(bytes, 0, bytes.Length);

        public static IEnumerable<PropertyElement> ToProperties<T>(this IPropertyFormatter formatter, T obj)
            => formatter.ToProperties(typeof(T), obj);

        public static T FromProperties<T>(this IPropertyFormatter formatter, IEnumerable<PropertyElement> properties)
            => (T)formatter.FromProperties(typeof(T), properties);

        public static T FromProperties<T>(this IPropertyFormatter formatter, IReadOnlyDictionary<string, string> properties)
        {
            var type = typeof(T);
            var descriptions = formatter.GetPropertiesByName(type);
            var elements = new List<PropertyElement>(properties.Count);

            foreach (var pair in properties)
            {
                PropertyDescription desc;

                if (descriptions.TryGetValue(pair.Key, out desc))
                {
                    elements.Add(new PropertyElement(desc, null, pair.Value));
                }
            }

            return (T)formatter.FromProperties(type, elements);
        }

        public static T FromText<T>(this IFormatter formatter, string text)
        {
            var textFormatter = formatter as ITextFormatter;
            if (textFormatter != null) return (T)textFormatter.FromText(typeof(T), text);
            return FromBytes<T>(formatter, Encoding.UTF8.GetBytes(text));
        }

        public static string ToText<T>(this IFormatter formatter, T value)
        {
            var textFormatter = formatter as ITextFormatter;
            if (textFormatter != null) return textFormatter.ToText(value);
            var bytes = formatter.ToBytes(value);
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        public static T Copy<T>(this IFormatter formatter, T value)
        {
            if (Equals(value, default(T))) return default(T);
            var type = typeof(T);
            var textFormatter = formatter as ITextFormatter;
            if (textFormatter != null) return (T)textFormatter.FromText(type, textFormatter.ToText(value));
            var bytes = formatter.ToBytes(value);
            return (T)formatter.FromBytes(type, bytes, 0, bytes.Length);
        }

        public static T Copy<T>(this IPropertyFormatter formatter, T value)
        {
            if (Equals(value, default(T))) return default(T);

            var properties = formatter.ToProperties(value);
            return formatter.FromProperties<T>(formatter.ToProperties(value));
        }
    }
}
