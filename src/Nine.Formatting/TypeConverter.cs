namespace Nine.Formatting
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TypeConverter : ITextConverter<Type>
    {
        private readonly Dictionary<string, Type> _supportedTypes;

        public TypeConverter(params Type[] supportedTypes)
        {
            _supportedTypes = supportedTypes.ToDictionary(type => type.Name, StringComparer.OrdinalIgnoreCase);
        }

        public Type FromText(string text)
        {
            Type result;
            return _supportedTypes.TryGetValue(text, out result) ? result : null;
        }

        public string ToText(Type value)
        {
            return value.Name;
        }
    }
}
