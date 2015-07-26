namespace Nine.Formatting
{
    using System;

    public interface ITextFormatter
    {
        string ToText(object value);

        object FromText(Type type, string text);
    }
}
