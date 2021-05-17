using System;
using System.Collections.Generic;
using Durian.Configuration;
using Durian.Generator;

namespace Durian.DefaultParam.Samples
{
	internal class Parent
	{
		public delegate void Del();
	}

	internal partial class Program : Parent
	{
		[DefaultParamConfiguration(ApplyNewModifierWhenPossible = true)]
		public delegate void Del<[DefaultParam(typeof(string))]T>();

		private static void Main()
		{
			List<EnableModuleAttribute> attr = new();
			Console.WriteLine("Hello World!");


		}
	}
}
