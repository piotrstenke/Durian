using System.Linq;
using Durian.Generator.Data;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Durian.Tests.AnalysisCore.TypeData
{
	public sealed class GetContainingTypes : CompilationTest
	{
		[Fact]
		public void ReturnsEmpty_When_IsNotNestedType()
		{
			Generator.Data.TypeData data = GetType("class Test { }");
			ITypeData[] containingTypes = data.GetContainingTypes(false).ToArray();
			Assert.True(containingTypes.Length == 0);
		}

		[Fact]
		public void ReturnsOnlyItself_When_IsNotNestedType_And_IncludeSelfIsTrue()
		{
			Generator.Data.TypeData data = GetType("class Test { }");
			ITypeData[] containingTypes = data.GetContainingTypes(true).ToArray();
			Assert.True(containingTypes.Length == 1 && data == containingTypes[0]);
		}

		[Fact]
		public void CanReturnSingleType()
		{
			Generator.Data.TypeData data = GetType("class Parent { class Child { } }", 1);
			ITypeData[] containingTypes = data.GetContainingTypes(false).ToArray();
			Assert.True(containingTypes.Length == 1 && containingTypes[0].Symbol.Name == "Parent");
		}

		[Fact]
		public void CanReturnMultipleTypes()
		{
			Generator.Data.TypeData data = GetType("class Test { class Parent { class Child { } } }", 2);
			ITypeData[] containingTypes = data.GetContainingTypes(false).ToArray();
			Assert.True(containingTypes.Length == 2 && containingTypes.Any(t => t.Symbol.Name == "Parent") && containingTypes.Any(t => t.Symbol.Name == "Test"));
		}

		[Fact]
		public void ReturnsTypesInParentFirstOrder()
		{
			Generator.Data.TypeData data = GetType("class Test { class Parent { class Child { } } }", 2);
			ITypeData[] containingTypes = data.GetContainingTypes(false).ToArray();
			Assert.True(containingTypes.Length == 2 && containingTypes[0].Symbol.Name == "Test" && containingTypes[1].Symbol.Name == "Parent");
		}

		[Fact]
		public void ReturnsTypesInParentFirstOrderWithItselfLast_When_IncludeSelfIsTrue()
		{
			Generator.Data.TypeData data = GetType("class Test { class Parent { class Child { } } }", 2);
			ITypeData[] containingTypes = data.GetContainingTypes(true).ToArray();
			Assert.True(
				containingTypes.Length == 3 &&
				containingTypes[0].Symbol.Name == "Test" &&
				containingTypes[1].Symbol.Name == "Parent" &&
				containingTypes[2] == data
			);
		}
	}
}