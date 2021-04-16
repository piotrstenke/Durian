using Durian.DefaultParam;

namespace Durian.Tests.DefaultParam
{
	public abstract class DefaultParamTestBase : GeneratorTest<DefaultParamGenerator>
	{
		public DefaultParamTestBase() : base(new DefaultParamGenerator() { EnableSyntaxDiagnostics = true })
		{

		}

		public override SingletonGeneratorTestResult RunGenerator(string? input)
		{
			return RunGenerator(input, 2);
		}
	}
}