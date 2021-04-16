using Xunit;

namespace Durian.Tests.CorePackage.MemberData
{
	public sealed class Validate : CompilationTest
	{
		[Fact]
		public void ReturnsTrueByDefault()
		{
			Data.MemberData data = GetMember("class Test { }");
			Assert.True(data.Validate(RoslynUtilities.CreateExecutionContext()));
		}
	}
}
