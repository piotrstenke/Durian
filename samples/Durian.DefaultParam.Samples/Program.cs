using System;
using Durian.Configuration;

namespace Durian.DefaultParam.Samples
{
	internal class Parent
	{
		public class Del
		{

		}
	}

	internal abstract partial class Program : Parent
	{
		[DefaultParamConfiguration(ApplyNewModifierWhenPossible = true)]
		public delegate void Del<U, [DefaultParam(typeof(string))]T>();

		private static void Main()
		{
			Console.WriteLine("Hello World!");
		}
	}
}
