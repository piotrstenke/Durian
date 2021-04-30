[assembly: Durian.Configuration.DefaultParamConfiguration(ApplyNewToGeneratedMembersWithEquivalentSignature = true)]

namespace Durian.Samples.DefaultParam
{
	internal partial class Program
	{
		private static void Main()
		{

		}

		public void Method<U, [DefaultParam(typeof(int))] T>(T value)
		{
		}

		public void Method<[DefaultParam(typeof(string))]T>(T value)
		{

		}
	}
}
