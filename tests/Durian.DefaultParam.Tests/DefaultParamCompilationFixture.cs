using System.Diagnostics;
using Durian.DefaultParam;

namespace Durian.Tests.DefaultParam
{
	[DebuggerDisplay("{Compilation}")]
	public sealed class DefaultParamCompilationFixture
	{
		public TestableCompilationData Compilation { get; }

		public DefaultParamCompilationFixture()
		{
			Compilation = TestableCompilationData.Create();
			Compilation.UpdateCompilation(DPMethodGenConvention.GetText());
			Compilation.UpdateCompilation(DPTypeGenConvention.GetText());
			Compilation.UpdateCompilation(DefaultParamAttribute.GetText());
			Compilation.UpdateCompilation(DefaultParamConfigurationAttribute.GetText());
		}
	}
}