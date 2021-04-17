using Durian.DefaultParam;

namespace Durian.Tests.DefaultParam.Generator
{
	public abstract class DefaultParamGeneratorTest : GeneratorTest<DefaultParamGenerator>
	{
		public DefaultParamGeneratorTest() : base(new DefaultParamGenerator() { EnableSyntaxDiagnostics = true })
		{

		}

		public override SingletonGeneratorTestResult RunGenerator(string? input)
		{
			return RunGenerator(input, 2);
		}
	}
}