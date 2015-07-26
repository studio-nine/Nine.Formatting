namespace Nine.Formatting
{
    public struct PropertyElement
    {
        public readonly PropertyDescription Description;
        public readonly object Value;
        public readonly string Text;

        public PropertyElement(PropertyDescription description, object value, string text)
        {
            this.Description = description;
            this.Value = value;
            this.Text = text;
        }
    }
}
