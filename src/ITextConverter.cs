namespace Nine.Formatting
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    public interface ITextConverter { }
    public interface ITextConverter<T> : ITextConverter
    {
        string ToText(T value);
        T FromText(string text);
    }

    public class TextConverter
    {
        private readonly Dictionary<Type, IConverter> converters;

        public TextConverter(params ITextConverter[] converters)
        {
            this.converters = converters
                .GroupByGenericTypeArgument(typeof(ITextConverter<>), false)
                .ToDictionary(p => p.Key, p => (IConverter)Activator.CreateInstance(typeof(Converter<>).MakeGenericType(p.Key), p.Value));
        }

        public bool CanConvert(Type type)
        {
            return converters.ContainsKey(type);
        }

        public string ToText(object value)
        {
            IConverter converter = null;
            if (value == null) return null;
            if (converters.TryGetValue(value.GetType(), out converter)) return converter.ToText(value);
            return value.ToString();
        }

        public object FromText(Type type, string text)
        {
            IConverter converter = null;
            if (type == null) return null;
            if (converters.TryGetValue(type, out converter)) return converter.FromText(text);
            return null;
        }

        interface IConverter
        {
            string ToText(object value);
            object FromText(string text);
        }

        class Converter<T> : IConverter
        {
            private readonly ITextConverter<T> converter;

            public Converter(ITextConverter<T> converter)
            {
                this.converter = converter;
            }

            public object FromText(string text)
            {
                return converter.FromText(text);
            }

            public string ToText(object value)
            {
                return converter.ToText((T)value);
            }
        }
    }
}
