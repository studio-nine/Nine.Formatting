namespace Nine.Formatting
{
    using System;
    
    public class ImmutableTypes
    {
        public readonly int Int;
        public readonly bool Boolean;
        public TimeSpan TimeSpan { get; private set; }
        public double Double { get; }

        public ImmutableTypes(
            int @int = 10, 
            TimeSpan? timeSpan = null, 
            bool _Boolean = true,
            double Double = 1.0)
        {
            this.Int = @int;
            this.TimeSpan = timeSpan ?? TimeSpan.FromSeconds(1);
            this.Boolean = _Boolean;
            this.Double = Double;
        }
    }
}
