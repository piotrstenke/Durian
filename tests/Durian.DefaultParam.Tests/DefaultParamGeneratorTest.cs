using Durian.Analysis.Logging;
using Durian.TestServices;

namespace Durian.Analysis.DefaultParam.Tests
{
	public abstract class DefaultParamGeneratorTest : DurianGeneratorTest<DefaultParamGenerator>
	{
		protected override ITestableGenerator CreateTestableGenerator(string testName)
		{
			return UnderlayingGenerator.CreateTestable(testName);
		}

		protected override DefaultParamGenerator CreateUnderlayingGenerator(LoggingConfiguration configuration)
		{
			return new DefaultParamGenerator(configuration);
		}
	}
}
