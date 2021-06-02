using System;
using System.Text;
using System.IO;

namespace Durian.Samples.DefaultParam
{
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

		public void Error(T message)
		{
			_builder.Append("Error: ").AppendLine(message.ToString());
		}

		public void Warning(T message)
		{
			_builder.Append("Warning: ").AppendLine(message.ToString());
		}

		public void Info(T message)
		{
			_builder.Append("Info: ").AppendLine(message.ToString());
		}

		public override string ToString()
		{
			return _builder.ToString();
		}

		public void Save(string path)
		{
			File.WriteAllText(path, _builder.ToString());
		}

		public void Clear()
		{
			_builder.Clear();
		}
	}
}
