namespace Nine.Formatting
{
    using System;
    using System.Collections.Generic;
    
    public interface IPropertyFormatter
    {
        IReadOnlyDictionary<string, PropertyDescription> GetPropertiesByName(Type type);

        IReadOnlyList<PropertyDescription> GetPropertiesByOrdinal(Type type);

        IEnumerable<PropertyElement> ToProperties(Type type, object obj);

        object FromProperties(Type type, IEnumerable<PropertyElement> properties);
    }
}
