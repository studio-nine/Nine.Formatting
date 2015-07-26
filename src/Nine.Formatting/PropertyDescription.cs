namespace Nine.Formatting
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class PropertyDescription : ICustomAttributeProvider
    {
        private readonly ICustomAttributeProvider _icap;

        public readonly string Name;
        public readonly int Ordinal;
        public readonly Type Type;
        public readonly PropertyType PropertyType;

        public readonly bool IsArray;
        public readonly bool IsNumber;
        public readonly bool IsReadOnly; // TODO:

        public PropertyDescription(PropertyInfo property, int ordinal)
            : this(property, property.Name, ordinal, property.PropertyType)
        {
            if (property.GetIndexParameters().Length > 0 || !property.CanRead)
            {
                throw new ArgumentException(nameof(property));
            }
        }

        public PropertyDescription(FieldInfo field, int ordinal)
            : this(field, field.Name, ordinal, field.FieldType)
        { }

        public PropertyDescription(ParameterInfo parameter, int ordinal)
            : this(parameter, parameter.Name, ordinal, parameter.ParameterType)
        { }

        private PropertyDescription(ICustomAttributeProvider icap, string name, int ordinal, Type type)
        {
            if (icap == null)
            {
                throw new ArgumentNullException(nameof(icap));
            }

            var elementType = UnwrapArrayType(type);

            this._icap = icap;
            this.Ordinal = ordinal;
            this.Name = name;
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

            if (type == typeof(bool)) return PropertyType.Boolean;
            if (type == typeof(DateTime)) return PropertyType.DateTime;
            if (type == typeof(DateTimeOffset)) return PropertyType.DateTimeOffset;
            if (type == typeof(TimeSpan)) return PropertyType.TimeSpan;
            if (type == typeof(Guid)) return PropertyType.Guid;
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

            return PropertyType.String;
        }

        public object[] GetCustomAttributes(Type attributeType, bool inherit)
            => _icap.GetCustomAttributes(attributeType, inherit);

        public object[] GetCustomAttributes(bool inherit)
            => _icap.GetCustomAttributes(inherit);

        public bool IsDefined(Type attributeType, bool inherit)
            => _icap.IsDefined(attributeType, inherit);
    }
}
