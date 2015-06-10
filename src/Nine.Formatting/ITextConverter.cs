namespace Nine.Formatting
{
    using System;
    using System.Collections.Generic;
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
            return GetConverter(type) != null;
        }

        private IConverter GetConverter(Type type)
        {
            IConverter converter = null;
            while (type != null)
            {
                if (converters.TryGetValue(type, out converter)) break;
                type = type.GetTypeInfo().BaseType;
            }
            return converter;
        }

        public bool ToText(object value, out string result)
        {
            if (value == null)
            {
                result = null;
                return false;
            }

            var converter = GetConverter(value.GetType());
            if (converter != null)
            {
                result = converter.ToText(value);
                return true;
            }

            result = null;
            return false;
        }

        public string ToText(object value)
        {
            string result;
            return ToText(value, out result) ? result : value?.ToString();
        }

        public bool FromText(Type type, string text, out object value)
        {
            var converter = GetConverter(type);
            if (converter != null)
            {
                value = converter.FromText(text);
                return true;
            }

            value = null;
            return false;
        }

        public object FromText(Type type, string text)
        {
            object result;
            return FromText(type, text, out result) ? result : null;
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
