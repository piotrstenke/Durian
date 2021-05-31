using System;

namespace Durian.Samples.DefaultParam
{
	public interface ILoggable
	{

	}

	public readonly struct LoggableString
	{
		private readonly string _value;

		public LoggableString(string value)
		{
			_value = value;
		}

		public override bool Equals(object? obj)
		{
			if (obj is LoggableString other)
			{
				return other == this;
			}

			return false;
		}

		public bool Equals(string? value)
		{
			return _value == value;
		}

		public bool Equals(LoggableString value)
		{
			return value == this;
		}

		public override string ToString()
		{
			return _value;
		}

		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}

		public static bool operator ==(LoggableString a, LoggableString b)
		{
			return a._value == b._value;
		}

		public static bool operator !=(LoggableString a, LoggableString b)
		{
			return !(a == b);
		}
	}
}
