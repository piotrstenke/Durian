// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Linq;
using Durian.Analysis.Data;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Durian.Analysis.Tests.TypeData
{
	public sealed class GetContainingTypes : CompilationTest
	{
		[Fact]
		public void CanReturnMultipleTypes()
		{
			Analysis.Data.TypeData data = GetType("class Test { class Parent { class Child { } } }", 2);
			ITypeData[] containingTypes = data.GetContainingTypes(false).ToArray();
			Assert.True(containingTypes.Length == 2 && containingTypes.Any(t => t.Symbol.Name == "Parent") && containingTypes.Any(t => t.Symbol.Name == "Test"));
		}

		[Fact]
		public void CanReturnSingleType()
		{
			Analysis.Data.TypeData data = GetType("class Parent { class Child { } }", 1);
			ITypeData[] containingTypes = data.GetContainingTypes(false).ToArray();
			Assert.True(containingTypes.Length == 1 && containingTypes[0].Symbol.Name == "Parent");
		}

		[Fact]
		public void ReturnsEmpty_When_IsNotNestedType()
		{
			Analysis.Data.TypeData data = GetType("class Test { }");
			ITypeData[] containingTypes = data.GetContainingTypes(false).ToArray();
			Assert.True(containingTypes.Length == 0);
		}

		[Fact]
		public void ReturnsOnlyItself_When_IsNotNestedType_And_IncludeSelfIsTrue()
		{
			Analysis.Data.TypeData data = GetType("class Test { }");
			ITypeData[] containingTypes = data.GetContainingTypes(true).ToArray();
			Assert.True(containingTypes.Length == 1 && data == containingTypes[0]);
		}

		[Fact]
		public void ReturnsTypesInParentFirstOrder()
		{
			Analysis.Data.TypeData data = GetType("class Test { class Parent { class Child { } } }", 2);
			ITypeData[] containingTypes = data.GetContainingTypes(false).ToArray();
			Assert.True(containingTypes.Length == 2 && containingTypes[0].Symbol.Name == "Test" && containingTypes[1].Symbol.Name == "Parent");
		}

		[Fact]
		public void ReturnsTypesInParentFirstOrderWithItselfLast_When_IncludeSelfIsTrue()
		{
			Analysis.Data.TypeData data = GetType("class Test { class Parent { class Child { } } }", 2);
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
