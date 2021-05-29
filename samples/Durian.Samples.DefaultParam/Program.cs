using System;
using Durian.Configuration;

namespace Durian.DefaultParam.Samples
{
	public partial class Parent
	{
	}

	[DefaultParamScopedConfiguration(TypeConvention = DPTypeConvention.Inherit)]
	internal partial class Program : Parent
	{
		public static void Main()
		{
		}

		[Durian.Configuration.DefaultParamConfiguration(TypeConvention = Durian.Configuration.DPTypeConvention.Copy)]
		public struct Test<[DefaultParam(typeof(string))]T>
		{

		}
	}
}
