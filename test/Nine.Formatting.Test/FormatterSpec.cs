namespace Nine.Formatting
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using Newtonsoft.Json;
    using Xunit;
    using ProtoBuf;

    public class FormatterSpec
    {
        public static TheoryData<IFormatter> Formatters => _formatters.Value;

        private static readonly Lazy<TheoryData<IFormatter>> _formatters = new Lazy<TheoryData<IFormatter>>(() =>
        {
            var converter = new TextConverter(new TypeConverter(typeof(BasicTypes)));

            return new TheoryData<IFormatter>
            {
                new ProtoFormatter(),
                new JsonFormatter(converter),
                new BsonFormatter(),
                new UriFormatter(converter),
            };
        });

        [Theory, MemberData(nameof(Formatters))]
        public void it_should_format_basic_types(IFormatter formatter)
        {
            var a = new BasicTypes();
            var b = PingPong(formatter, a, text =>
            {
                // Should respect type convert and not use full type name.
                if (formatter is JsonFormatter || formatter is UriFormatter)
                {
                    Assert.False(text.Contains("Nine.Formatting"));
                }
            });
            Assert.Equal(JsonConvert.SerializeObject(a), JsonConvert.SerializeObject(b));
        }

        [Theory, MemberData(nameof(Formatters))]
        public void it_should_not_format_private_members(IFormatter formatter)
        {
            var a = new BasicTypes();
            var b = PingPong(formatter, a,
                text => Assert.False(text.Contains("notSerialized")));
        }

        [Theory, MemberData(nameof(Formatters))]
        public void it_should_not_format_default_values(IFormatter formatter)
        {
            var a = new BasicTypes { DateTime = new DateTime() };
            var b = PingPong(formatter, a,
                text => Assert.False(text.Contains("0001-01")));
        }

        [Theory, MemberData(nameof(Formatters))]
        public void i_can_add_new_fields(IFormatter formatter)
        {
            var a = new BasicTypes();
            AddNewField b = PingPong<BasicTypes, AddNewField>(formatter, a);
            Assert.Equal(a.String, b.String);
        }

        [Theory, MemberData(nameof(Formatters))]
        public void i_can_remove_existing_fields(IFormatter formatter)
        {
            var a = new AddNewField();
            var b = PingPong<BasicTypes>(formatter, a);
            Assert.Equal(a.String, b.String);
        }

        [Theory, MemberData(nameof(Formatters))]
        public void it_should_fallback_to_default_if_an_enum_value_is_not_found(IFormatter formatter)
        {
            if (formatter is ProtoFormatter || formatter is BsonFormatter) return;

            var b = PingPong<EnumClassA, EnumClassB>(formatter, new EnumClassA { Comparison = "OrdinalIgnoreCase" });
            Assert.Equal(StringComparison.OrdinalIgnoreCase, b.Comparison);

            b = PingPong<EnumClassA, EnumClassB>(formatter, new EnumClassA { Comparison = "OrdinalIgnoreCase111" });
            Assert.Equal((StringComparison)0, b.Comparison);
        }

        [Theory, MemberData(nameof(Formatters))]
        public void i_can_turn_fields_into_nullable(IFormatter formatter)
        {
            var a = new NotNullable();
            var b = PingPong<NotNullable, Nullable>(formatter, a);
            Assert.Equal(a.Value, b.Value);
        }

        [Theory, MemberData(nameof(Formatters))]
        public void formatter_speed(IFormatter formatter)
        {
            var sw = Stopwatch.StartNew();
            var a = new BasicTypes();
            var iterations = 1000;
            for (int i = 0; i < iterations; i++)
            {
                formatter.FromBytes<BasicTypes>(formatter.ToBytes(a));
            }
            Debug.WriteLine(formatter + " time " + sw.Elapsed.TotalMilliseconds / iterations + "ms");
        }

        private static T PingPong<T>(IFormatter formatter, T value, Action<string> action = null)
        {
            var sw = Stopwatch.StartNew();
            var bytes = formatter.ToBytes(value);
            Debug.WriteLine("time " + sw.ElapsedMilliseconds + "ms");
            Debug.WriteLine("length " + bytes.Length);

            var text = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            Debug.WriteLine(text);

            if (action != null) action(text);

            sw.Restart();
            var result = formatter.FromBytes<T>(bytes);
            Debug.WriteLine("time " + sw.ElapsedMilliseconds + "ms");
            return result;
        }

        private static T2 PingPong<T1, T2>(IFormatter formatter, T1 value)
        {
            var bytes = formatter.ToBytes(value);
            Debug.WriteLine(Encoding.UTF8.GetString(bytes, 0, bytes.Length));
            return formatter.FromBytes<T2>(bytes);
        }

        [ProtoContract]
        public class AddNewField : BasicTypes
        {
            [ProtoMember(51)]
            public int? IWontBreakYou = int.MinValue;
        }

        [ProtoContract]
        public class NotNullable
        {
            [ProtoMember(1)]
            public long Value = long.MinValue;
        }

        [ProtoContract]
        public class Nullable
        {
            [ProtoMember(1)]
            public long? Value;
        }

        public class EnumClassA
        {
            public string Comparison { get; set; }
        }

        public class EnumClassB
        {
            public StringComparison Comparison { get; set; }
        }
    }
}
