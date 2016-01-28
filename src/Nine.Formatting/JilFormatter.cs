﻿namespace Nine.Formatting
{
    using System;
    using System.IO;
    using System.Text;
    using Jil;

    public class JilFormatter : IFormatter
    {
        private readonly Encoding _encoding = new UTF8Encoding(false, true);
        private readonly Options _options = Options.ISO8601ExcludeNullsIncludeInheritedUtcCamelCase;
        
        public void WriteTo(object value, Stream stream)
        {
            using (var writer = new StreamWriter(stream, _encoding, 1024, leaveOpen: true))
            {
                JSON.Serialize(value, writer, _options);
            }
        }

        public object ReadFrom(Type type, Stream stream)
        {
            using (var reader = new StreamReader(stream, _encoding, true, 1024, leaveOpen: true))
            {
                return JSON.Deserialize(reader, type, _options);
            }
        }
    }
}