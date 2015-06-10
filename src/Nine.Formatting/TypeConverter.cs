namespace Nine.Formatting
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TypeConverter : ITextConverter<Type>
    {
        private readonly Dictionary<string, Type> supportedTypes;

        public TypeConverter(params Type[] supportedTypes)
        {
            this.supportedTypes = supportedTypes.ToDictionary(type => type.Name, StringComparer.OrdinalIgnoreCase);
        }

        public Type FromText(string text)
        {
            Type result;
            return supportedTypes.TryGetValue(text, out result) ? result : null;
        }

        public string ToText(Type value)
        {
            return value.Name;
        }
    }
}
