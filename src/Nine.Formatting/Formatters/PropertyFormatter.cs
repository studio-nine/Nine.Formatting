namespace Nine.Formatting
{
    using System;
    using System.Collections.Generic;

    public class PropertyFormatter : IPropertyFormatter
    {
        public PropertyElement[] ToProperties(Type type, object obj)
        {
            throw new NotImplementedException();
        }

        public object FromProperties(Type type, IEnumerable<PropertyElement> properties)
        {
            throw new NotImplementedException();
        }
    }
}
