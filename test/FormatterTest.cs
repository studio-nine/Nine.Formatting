namespace Nine.Formatting
{
    using System.Collections.Generic;
    
    public class FormatterTest : FormatterSpec<FormatterTest>
    {
        public override IEnumerable<IFormatter> GetData()
        {
            return new IFormatter[]
            {
                new ProtoFormatter(),
                new JsonFormatter(),
                new BsonFormatter(),
                new UriFormatter(),
            };
        }
    }
}
