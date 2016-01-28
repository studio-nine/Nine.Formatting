namespace Nine.Formatting
{
    using System;
    using System.IO;
    using System.Text;

    public static class FormatterExtensions
    {
        public static byte[] ToBytes(this IFormatter formatter, object value)
        {
            var ms = new MemoryStream();
            formatter.WriteTo(value, ms);
            return ms.ToArray();
        }

        public static object FromBytes(this IFormatter formatter, Type type, byte[] bytes, int index, int count)
            => formatter.ReadFrom(type, new MemoryStream(bytes, index, count, writable: false));

        public static T FromBytes<T>(this IFormatter formatter, byte[] bytes, int index, int count)
            => (T)formatter.FromBytes(typeof(T), bytes, index, count);

        public static T FromBytes<T>(this IFormatter formatter, byte[] bytes)
            => formatter.FromBytes<T>(bytes, 0, bytes.Length);
        
        public static T FromText<T>(this IFormatter formatter, string text)
        {
            return FromBytes<T>(formatter, Encoding.UTF8.GetBytes(text));
        }

        public static string ToText<T>(this IFormatter formatter, T value)
        {
            var bytes = formatter.ToBytes(value);
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        public static T Copy<T>(this IFormatter formatter, T value)
        {
            if (Equals(value, default(T))) return default(T);
            var type = typeof(T);
            var ms = new MemoryStream();
            formatter.WriteTo(value, ms);
            ms.Seek(0, SeekOrigin.Begin);
            return (T)formatter.ReadFrom(type, ms);
        }
    }
}
