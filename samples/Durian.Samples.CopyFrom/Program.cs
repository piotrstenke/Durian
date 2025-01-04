using System;

namespace Durian.Samples.CopyFrom
{
	internal class Program
	{
		private static void Main()
		{
			// This method will print "Hello world!".
			Test.Source();

			// This method will print "Hello beautiful world!", even though it wasn't explicily written anywhere in the code!
			Test.Target();

			Console.ReadKey();
		}
	}

	public static partial class Test
	{
		[CopyFromMethod(nameof(Source)), Pattern("world", "beautiful world")]
		public static partial void Target(); 

		public static void Source()
		{
			Console.WriteLine("Hello world!");
		}
	}
}