namespace Nine.Formatting
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class PropertyDescription : ICustomAttributeProvider
    {
        private readonly ICustomAttributeProvider _icap;
        private readonly Func<object, object> _getter;
        private readonly Action<object, object> _setter;

        public readonly string Name;
        public readonly int Ordinal;
        public readonly Type Type;
        public readonly PropertyType PropertyType;
        public readonly object DefaultValue;

        public readonly bool IsArray;
        public readonly bool IsNumber;
        public readonly bool IsReadOnly;

        public object GetValue(object obj) => _getter?.Invoke(obj);
        public void SetValue(object obj, object value) => _setter?.Invoke(obj, value);

        public override string ToString() => IsArray
            ? $"[{Ordinal}] {Name} ({Type.Name},{PropertyType})[] "
            : $"[{Ordinal}] {Name} ({Type.Name},{PropertyType})";

        public PropertyDescription(PropertyInfo property, int ordinal)
            : this(property.Name, property.PropertyType, ordinal,
                   !property.SetMethod?.IsPublic ?? true,
                   new Func<object, object>(property.GetValue),
                   new Action<object, object>(property.SetValue),
                   property)
        {
            if (property.GetIndexParameters().Length > 0 || !property.CanRead)
            {
                throw new ArgumentException(nameof(property));
            }
        }

        public PropertyDescription(FieldInfo field, int ordinal)
            : this(field.Name, field.FieldType, ordinal, field.IsInitOnly,
                   new Func<object, object>(field.GetValue),
                   new Action<object, object>(field.SetValue),
                   field)
        { }

        public PropertyDescription(ParameterInfo parameter, int ordinal)
            : this(parameter.Name, parameter.ParameterType,
                   ordinal, false, null, null,
                   parameter, parameter.DefaultValue)
        { }

        public PropertyDescription(
            string name, Type type, int ordinal, bool isReadOnly,
            Func<object, object> getter, Action<object, object> setter,
            ICustomAttributeProvider customAttributeProvider, object defaultValue = null)
        {
            if (customAttributeProvider == null) throw new ArgumentNullException(nameof(customAttributeProvider));
            if (string.IsNullOrEmpty(name)) throw new ArgumentException(nameof(name));

            var elementType = UnwrapArrayType(type);

            this._getter = getter;
            this._setter = setter;
            this._icap = customAttributeProvider;
            this.DefaultValue = defaultValue;
            this.Ordinal = ordinal;
            this.Name = name;
            this.IsReadOnly = isReadOnly;
            this.IsArray = (elementType != null);
            this.Type = elementType ?? type;
            this.PropertyType = ToPropertyType(type);
            this.IsNumber = PropertyType >= PropertyType.Byte && PropertyType <= PropertyType.Decimal;
        }

        private static Type UnwrapArrayType(Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }

            if (type.IsConstructedGenericType)
            {
                var definition = type.GetGenericTypeDefinition();
                if (definition == typeof(IEnumerable<>) ||
                    definition == typeof(IReadOnlyCollection<>) ||
                    definition == typeof(IReadOnlyList<>) ||
                    definition == typeof(IList<>))
                {
                    return type.GenericTypeArguments[0];
                }
            }

            return null;
        }

        private static PropertyType ToPropertyType(Type type)
        {
            if (type.IsConstructedGenericType && typeof(Nullable<>) == type.GetGenericTypeDefinition())
            {
                return ToPropertyType(Nullable.GetUnderlyingType(type));
            }

            if (type == typeof(string)) return PropertyType.String;
            if (type == typeof(bool)) return PropertyType.Boolean;
            if (type == typeof(DateTime)) return PropertyType.DateTime;
            if (type == typeof(DateTimeOffset)) return PropertyType.DateTimeOffset;
            if (type == typeof(TimeSpan)) return PropertyType.TimeSpan;
            if (type == typeof(byte)) return PropertyType.Byte;
            if (type == typeof(sbyte)) return PropertyType.SByte;
            if (type == typeof(char)) return PropertyType.Char;
            if (type == typeof(Int16)) return PropertyType.Int16;
            if (type == typeof(UInt16)) return PropertyType.UInt16;
            if (type == typeof(Int32)) return PropertyType.Int32;
            if (type == typeof(Int64)) return PropertyType.Int64;
            if (type == typeof(Single)) return PropertyType.Single;
            if (type == typeof(Double)) return PropertyType.Double;
            if (type == typeof(Decimal)) return PropertyType.Decimal;

            return PropertyType.Unknown;
        }

        public object[] GetCustomAttributes(Type attributeType, bool inherit)
            => _icap.GetCustomAttributes(attributeType, inherit);

        public object[] GetCustomAttributes(bool inherit)
            => _icap.GetCustomAttributes(inherit);

        public bool IsDefined(Type attributeType, bool inherit)
            => _icap.IsDefined(attributeType, inherit);
    }
}
