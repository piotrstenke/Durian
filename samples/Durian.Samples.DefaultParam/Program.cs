[assembly: Durian.Configuration.DefaultParamConfiguration(CallInsteadOfCopying = true)]
namespace Durian.Samples
{
	internal class Parent
	{

	}

	internal partial class Program
	{
		public delegate void D<T>(T value);

		private static void Main()
		{

		}

		public static void Method<[Durian.DefaultParam(typeof(int))]T>(T value)
		{
			// test
			int a = 2;
		}

	}
}
