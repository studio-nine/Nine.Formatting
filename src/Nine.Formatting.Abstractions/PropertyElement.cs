namespace Nine.Formatting
{
    using System;

    public struct PropertyElement
    {
        public readonly PropertyDescription Description;
        public readonly object Value;
        public readonly string Text;

        public override string ToString() => $"{Description.Name}: {Text}";

        public PropertyElement(PropertyDescription description, object value, string text)
        {
            if (description == null) throw new ArgumentNullException(nameof(description));

            this.Description = description;
            this.Value = value;
            this.Text = text;
        }
    }
}
