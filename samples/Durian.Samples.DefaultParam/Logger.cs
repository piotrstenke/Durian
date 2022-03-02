// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Configuration;
using System;
using System.IO;
using System.Text;

namespace Durian.Samples.DefaultParam
{
    [DefaultParamConfiguration(TypeConvention = DPTypeConvention.Copy)]
    public class Logger<[DefaultParam(typeof(string))]T> : ILogger<T> where T : IEquatable<string>
    {
        private readonly StringBuilder _builder;

        public Logger()
        {
            _builder = new(1024);
        }

        public Logger(StringBuilder builder)
        {
            _builder = builder;
        }

        public void Clear()
        {
            _builder.Clear();
        }

        public void Error(T message)
        {
            _builder.Append("Error: ").AppendLine(message.ToString());
        }

        public void Info(T message)
        {
            _builder.Append("Info: ").AppendLine(message.ToString());
        }

        public void Save(string path)
        {
            File.WriteAllText(path, _builder.ToString());
        }

        public override string ToString()
        {
            return _builder.ToString();
        }

        public void Warning(T message)
        {
            _builder.Append("Warning: ").AppendLine(message.ToString());
        }
    }
}