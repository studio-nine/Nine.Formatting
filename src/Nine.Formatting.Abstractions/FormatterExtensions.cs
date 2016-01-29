namespace Nine.Formatting
{
    using System.IO;
    using System.Text;

    public static class FormatterExtensions
    {
        private static readonly Encoding _encoding = new UTF8Encoding(false, true);

        public static T ReadFrom<T>(this IFormatter formatter, Stream stream)
        {
            return (T)formatter.ReadFrom(typeof(T), stream);
        }

        public static T ReadFrom<T>(this ITextFormatter formatter, Stream stream)
        {
            using (var reader = new StreamReader(stream, _encoding, true, 1024, leaveOpen: true))
            {
                return (T)formatter.ReadFrom(typeof(T), new StreamReader(stream));
            }
        }

        public static void WriteTo<T>(this ITextFormatter formatter, T value, Stream stream)
        {
            using (var writer = new StreamWriter(stream, _encoding, 1024, leaveOpen: true))
            {
                formatter.WriteTo(value, writer);
            }
        }

        public static T FromText<T>(this ITextFormatter formatter, string text)
        {
            return (T)formatter.ReadFrom(typeof(T), new StringReader(text));
        }

        public static string ToText(this ITextFormatter formatter, object value)
        {
            var sb = StringBuilderCache.Acquire(256);
            formatter.WriteTo(value, new StringWriter(sb));
            return StringBuilderCache.GetStringAndRelease(sb);
        }
    }
}
