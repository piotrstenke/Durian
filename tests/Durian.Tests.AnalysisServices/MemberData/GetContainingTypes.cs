using System.Linq;
using Durian.Generator.Data;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Durian.Tests.AnalysisServices.MemberData
{
	public sealed class GetContainingTypes : CompilationTest
	{
		[Fact]
		public void ReturnsEmpty_When_IsNotNestedType()
		{
			Generator.Data.MemberData data = GetMember("class Test { }");
			ITypeData[] containingTypes = data.GetContainingTypes().ToArray();
			Assert.True(containingTypes.Length == 0);
		}

		[Fact]
		public void CanReturnSingleType()
		{
			Generator.Data.MemberData data = GetMember("class Parent { class Child { } }", 1);
			ITypeData[] containingTypes = data.GetContainingTypes().ToArray();
			Assert.True(containingTypes.Length == 1 && containingTypes[0].Symbol.Name == "Parent");
		}

		[Fact]
		public void CanReturnMultipleTypes()
		{
			Generator.Data.MemberData data = GetMember("class Test { class Parent { class Child { } } }", 2);
			ITypeData[] containingTypes = data.GetContainingTypes().ToArray();
			Assert.True(containingTypes.Length == 2 && containingTypes.Any(t => t.Symbol.Name == "Parent") && containingTypes.Any(t => t.Symbol.Name == "Test"));
		}

		[Fact]
		public void ReturnsTypesInParentFirstOrder()
		{
			Generator.Data.MemberData data = GetMember("class Test { class Parent { class Child { } } }", 2);
			ITypeData[] containingTypes = data.GetContainingTypes().ToArray();
			Assert.True(containingTypes.Length == 2 && containingTypes[0].Symbol.Name == "Test" && containingTypes[1].Symbol.Name == "Parent");
		}
	}
}
