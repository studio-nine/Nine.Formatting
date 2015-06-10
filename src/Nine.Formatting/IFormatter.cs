namespace Nine.Formatting
{
    using System;
    using System.Text;
    using System.ComponentModel;

    public interface IFormatter
    {
        byte[] ToBytes(object value);

        object FromBytes(Type type, byte[] bytes, int index, int count);
    }

    public interface ITextFormatter
    {
        string ToText(object value);

        object FromText(Type type, string text);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class FormatterExtensions
    {
        public static T FromBytes<T>(this IFormatter formatter, byte[] bytes, int index, int count)
        {
            return (T)formatter.FromBytes(typeof(T), bytes, index, count);
        }
        
        public static T FromBytes<T>(this IFormatter formatter, byte[] bytes)
        {
            return formatter.FromBytes<T>(bytes, 0, bytes.Length);
        }

        public static object FromText(this IFormatter formatter, Type type, string text)
        {
            var textFormatter = formatter as ITextFormatter;
            if (textFormatter != null) return textFormatter.FromText(type, text);
            var bytes = Encoding.UTF8.GetBytes(text);
            return formatter.FromBytes(type, bytes, 0, bytes.Length);
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
    }
}
