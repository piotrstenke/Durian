using System;

namespace Durian.DefaultParam.Samples
{
	internal partial class Program
	{
		public delegate void Del<[DefaultParam(typeof(string))]T>();

		private static void Main()
		{
			Console.WriteLine("Hello World!");
		}
	}
}
