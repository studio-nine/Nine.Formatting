namespace Nine.Formatting
{
    using System;
    using System.Collections.Generic;
    
    public interface IPropertyFormatter
    {
        PropertyElement[] ToProperties(Type type, object obj);

        object FromProperties(Type type, IEnumerable<PropertyElement> properties);
    }
}
