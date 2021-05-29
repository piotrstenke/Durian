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

		public override void Method<[DefaultParam(typeof(Attribute))] T>() where T : class
		{
			base.Method<T>();
		}

		[Durian.Configuration.DefaultParamConfiguration(TypeConvention = Durian.Configuration.DPTypeConvention.Copy)]
		public struct Test<[DefaultParam(typeof(string))]T>
		{

		}
	}
}
