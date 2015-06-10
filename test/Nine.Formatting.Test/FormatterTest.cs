namespace Nine.Formatting
{
    using System.Collections.Generic;
    
    public class FormatterTest : FormatterSpec<FormatterTest>
    {
        public override IEnumerable<IFormatter> GetData()
        {
            var converter = new TextConverter(new TypeConverter(typeof(BasicTypes)));

            return new IFormatter[]
            {
                new ProtoFormatter(),
                new JsonFormatter(converter),
                new BsonFormatter(),
                new UriFormatter(converter),
            };
        }
    }
}
