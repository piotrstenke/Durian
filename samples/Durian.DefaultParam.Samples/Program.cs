using System;
using Durian.Configuration;

namespace Durian.DefaultParam.Samples
{
	internal partial class Parent
	{
	}

	internal partial class Program : Parent
	{
		private static void Main()
		{
			Console.WriteLine("Hello World!");
		}

		[DefaultParamConfiguration(MethodConvention = DPMethodGenConvention.Call)]
		public static void Method<[DefaultParam(typeof(string))]T>() where T : class
		{

		}
	}
}
