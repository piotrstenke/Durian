using System;
using Durian.Configuration;

namespace Durian.DefaultParam.Samples
{
	internal class Parent
	{
		[Durian.Generator.DurianGenerated]
		public delegate void Del<T>();
	}

	internal partial class Program : Parent
	{
		[DefaultParamConfiguration(ApplyNewModifierWhenPossible = true)]
		public delegate void Del<U, [DefaultParam(typeof(string))]T>();

		public delegate void Del();
		private static void Main()
		{
			Console.WriteLine("Hello World!");
		}
	}
}
