using Xunit;

namespace Durian.Tests.AnalysisCore.MemberData
{
	public sealed class Validate : CompilationTest
	{
		[Fact]
		public void ReturnsTrueByDefault()
		{
			Generator.Data.MemberData data = GetMember("class Test { }");
			Assert.True(data.Validate(RoslynUtilities.CreateExecutionContext()));
		}
	}
}
