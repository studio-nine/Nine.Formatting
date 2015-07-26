namespace Nine.Formatting
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    public class PropertyFormatter : IPropertyFormatter
    {
        struct TypeDescrition
        {
            public ConstructorInfo Constructor;
            public Dictionary<string, PropertyDescription> PropertiesByName;
            public PropertyDescription[] PropertiesByOrdinal;
        }

        private static readonly ConcurrentDictionary<Type, TypeDescrition> _descriptions =
                            new ConcurrentDictionary<Type, TypeDescrition>();

        private readonly TextConverter _textConverter;
        private readonly Func<Type, object> _activator;

        public PropertyFormatter(TextConverter textConverter = null, Func<Type, object> activator = null)
        {
            _textConverter = textConverter;
            _activator = activator;
        }

        public IReadOnlyDictionary<string, PropertyDescription> GetPropertiesByName(Type type)
            => GetDescriptionForType(type).PropertiesByName;

        public IReadOnlyList<PropertyDescription> GetPropertiesByOrdinal(Type type)
            => GetDescriptionForType(type).PropertiesByOrdinal;

        public IEnumerable<PropertyElement> ToProperties(Type type, object obj)
        {
            var descriptions = GetDescriptionForType(type).PropertiesByName;
            var elements = new PropertyElement[descriptions.Count];

            foreach (var desc in descriptions.Values)
            {
                var value = desc.GetValue(obj);
                var text = ToText(value, desc.Type, desc.PropertyType);
                elements[desc.Ordinal] = new PropertyElement(desc, value, text);
            }

            return elements;
        }

        public object FromProperties(Type type, IEnumerable<PropertyElement> properties)
        {
            var descriptions = GetDescriptionForType(type);
            if (descriptions.Constructor != null)
            {

            }

            var result = _activator != null ? _activator(type) : Activator.CreateInstance(type);

            foreach (var property in properties)
            {
                if (!property.Description.IsReadOnly)
                {
                    PropertyDescription desc;

                    if (descriptions.PropertiesByName.TryGetValue(property.Description.Name, out desc))
                    {
                        if (!desc.IsReadOnly)
                        {
                            desc.SetValue(result, property.Value);
                        }
                    }
                }
            }

            return result;
        }

        private string ToText(object value, Type type, PropertyType propertyType)
        {
            if (value == null) return null;

            switch (propertyType)
            {
                case PropertyType.String: return (string)value;
                case PropertyType.Boolean: return value.ToString();
                case PropertyType.Byte: return ((Byte)value).ToString(CultureInfo.InvariantCulture);
                case PropertyType.SByte: return ((SByte)value).ToString(CultureInfo.InvariantCulture);
                case PropertyType.Int16: return ((Int16)value).ToString(CultureInfo.InvariantCulture);
                case PropertyType.UInt16: return ((UInt16)value).ToString(CultureInfo.InvariantCulture);
                case PropertyType.Int32: return ((Int32)value).ToString(CultureInfo.InvariantCulture);
                case PropertyType.UInt32: return ((UInt32)value).ToString(CultureInfo.InvariantCulture);
                case PropertyType.Int64: return ((Int64)value).ToString(CultureInfo.InvariantCulture);
                case PropertyType.UInt64: return ((UInt64)value).ToString(CultureInfo.InvariantCulture);
                case PropertyType.Single: return ((Single)value).ToString(CultureInfo.InvariantCulture);
                case PropertyType.Double: return ((Double)value).ToString(CultureInfo.InvariantCulture);
                case PropertyType.Char: return ((Char)value).ToString(CultureInfo.InvariantCulture);
                case PropertyType.Decimal: return ((Decimal)value).ToString(CultureInfo.InvariantCulture);
                case PropertyType.DateTime: return ((DateTime)value).ToString("o", CultureInfo.InvariantCulture);
                case PropertyType.DateTimeOffset: return ((DateTimeOffset)value).ToString("o", CultureInfo.InvariantCulture);
                case PropertyType.TimeSpan: return ((TimeSpan)value).ToString(null, CultureInfo.InvariantCulture);
            }

            var formattable = value as IFormattable;
            if (formattable != null) return formattable.ToString(null, CultureInfo.InvariantCulture);

            if (_textConverter != null && _textConverter.CanConvert(type)) return _textConverter.ToText(value);

            return value.ToString();
        }

        private TypeDescrition GetDescriptionForType(Type type) => _descriptions.GetOrAdd(type, CreateDescriptionForType);
        private TypeDescrition CreateDescriptionForType(Type type)
        {
            var ordinal = 0;
            var constructor = (ConstructorInfo)null;
            var properties = new List<PropertyDescription>();
            var ti = type.GetTypeInfo();

            var isImmutable = ti.DeclaredFields.Where(f => f.IsPublic).All(f => f.IsInitOnly) &&
                              ti.DeclaredProperties.Where(p => p.SetMethod != null).All(p => !p.SetMethod.IsPublic);

            if (isImmutable)
            {
                constructor = ti.DeclaredConstructors.Where(c => c.IsPublic)
                                                         .OrderByDescending(c => c.GetParameters().Length)
                                                         .FirstOrDefault();
                if (constructor != null)
                {
                    foreach (var parameter in constructor.GetParameters())
                    {
                        properties.Add(new PropertyDescription(parameter, ordinal++));
                    }
                }
            }

            var propertiesByName = properties.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var member in ti.DeclaredMembers)
            {
                var property = member as PropertyInfo;
                if (property != null &&
                    property.GetMethod != null && property.GetMethod.IsPublic &&
                    property.GetIndexParameters().Length == 0)
                {
                    if (!propertiesByName.ContainsKey(property.Name))
                    {
                        var desc = new PropertyDescription(property, ordinal++);

                        properties.Add(desc);
                        propertiesByName.Add(property.Name, desc);
                    }
                }

                var field = member as FieldInfo;
                if (field != null && field.IsPublic)
                {
                    if (!propertiesByName.ContainsKey(field.Name))
                    {
                        var desc = new PropertyDescription(field, ordinal++);

                        properties.Add(desc);
                        propertiesByName.Add(field.Name, desc);
                    }
                }
            }

            return new TypeDescrition
            {
                Constructor = constructor,
                PropertiesByOrdinal = properties.ToArray(),
                PropertiesByName = propertiesByName,
            };
        }
    }
}
