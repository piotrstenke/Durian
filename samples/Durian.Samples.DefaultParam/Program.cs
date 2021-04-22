using Durian.Configuration;
[assembly: DefaultParamConfiguration(ApplyNewToGeneratedMembersWithEquivalentSignature = true, CallInsteadOfCopying = true)]

namespace Durian.Samples.DefaultParam
{
	internal partial class Program
	{
		public delegate void D<T>(T value);

		private static void Main()
		{
			Logger<int>.Log(12);
		}

		[DefaultParamMethodConfiguration(CallInsteadOfCopying = true)]
		public static void Method<[DefaultParam(typeof(int))]U>(U value)
		{

		}
	}
}
