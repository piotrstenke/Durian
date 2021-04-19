[assembly: Durian.Configuration.DefaultParamConfiguration(ApplyNewToGeneratedMembersWithEquivalentSignature = true)]
namespace Durian.Samples.DefaultParam
{
	internal class Parent
	{
		public static void Method(int value)
		{

		}
	}

	internal partial class Program : Parent
	{
		public delegate void D<T>(T value);

		private static void Main()
		{
			Logger<int>.Log(12);
		}

		public static void Method<[DefaultParam(typeof(int))]U>(U value)
		{
		}
	}
}
