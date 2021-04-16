using System;

namespace Durian.Samples.DefaultParam
{
	public static class Logger<T>
	{
		public static void Log(T message)
		{
			Console.WriteLine(message);
		}

		public static void Error(T message)
		{
			Console.WriteLine($"Error: {message}");
		}

		public static void Warning(T message)
		{
			Console.WriteLine($"Warning: {message}");
		}
	}
}
