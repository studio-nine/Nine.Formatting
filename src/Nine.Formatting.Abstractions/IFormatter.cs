namespace Nine.Formatting
{
    using System;

    public interface IFormatter
    {
        byte[] ToBytes(object value);

        object FromBytes(Type type, byte[] bytes, int index, int count);
    }
}
