namespace Nine.Formatting
{
    using Newtonsoft.Json;
    using Xunit;

    public class PropertyFormatterSpec
    {
        private static readonly IPropertyFormatter formatter = new PropertyFormatter();

        [Fact]
        public void round_trip()
        {
            RoundTrip(new BasicTypes());
            // RoundTrip(new ImmutableTypes());
        }

        private void RoundTrip<T>(T obj)
        {
            var obj1 = formatter.Copy(obj);

            Assert.Equal(JsonConvert.SerializeObject(obj),
                         JsonConvert.SerializeObject(obj1));
        }
    }
}
