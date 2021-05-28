using Durian.Configuration;
using System;

namespace Durian.DefaultParam.Samples
{
	public partial class Parent
	{
		public virtual void Method<[DefaultParam(typeof(System.Attribute))]T>()
		{

		}
	}

	[DefaultParamScopedConfiguration(TypeConvention = DPTypeConvention.Inherit)]
	internal partial class Program : Parent
	{
		public static void Main()
		{
		}

		public override void Method<[DefaultParam(typeof(Attribute))] T>()
		{
			base.Method<T>();
		}

		[Durian.Configuration.DefaultParamConfiguration(TypeConvention = Durian.Configuration.DPTypeConvention.Copy)]
		public struct Test<[DefaultParam(typeof(string))]T>
		{

		}
	}
}
