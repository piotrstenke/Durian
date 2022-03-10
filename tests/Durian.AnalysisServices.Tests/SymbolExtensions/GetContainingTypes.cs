// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using Xunit;

namespace Durian.Analysis.Tests.SymbolExtensions
{
	public sealed class GetContainingTypes : CompilationTest
	{
		[Fact]
		public void CanReturnMultipleTypes()
		{
			INamedTypeSymbol type = GetSymbol("class Test { class Parent { class Child { } } }", 2);
			ITypeData[] containingTypes = type.GetContainingTypesAsData(Compilation).ToArray();
			Assert.True(containingTypes.Length == 2 && containingTypes.Any(t => t.Symbol.Name == "Parent") && containingTypes.Any(t => t.Symbol.Name == "Test"));
		}

		[Fact]
		public void CanReturnSingleType()
		{
			INamedTypeSymbol type = GetSymbol("class Parent { class Child { } }", 1);
			ITypeData[] containingTypes = type.GetContainingTypesAsData(Compilation).ToArray();
			Assert.True(containingTypes.Length == 1 && containingTypes[0].Symbol.Name == "Parent");
		}

		[Fact]
		public void ReturnsEmpty_When_IsNotNestedType()
		{
			INamedTypeSymbol type = GetSymbol("class Test { }");
			ITypeData[] containingTypes = type.GetContainingTypesAsData(Compilation).ToArray();
			Assert.True(containingTypes.Length == 0);
		}

		[Fact]
		public void ReturnsTypesInParentFirstOrder()
		{
			INamedTypeSymbol type = GetSymbol("class Test { class Parent { class Child { } } }", 2);
			ITypeData[] containingTypes = type.GetContainingTypesAsData(Compilation).ToArray();
			Assert.True(containingTypes.Length == 2 && containingTypes[0].Symbol.Name == "Test" && containingTypes[1].Symbol.Name == "Parent");
		}

		[Fact]
		public void ThrowsArgumentNullException_When_CompilationIsNull()
		{
			INamedTypeSymbol type = GetSymbol("class Test { }");
			Assert.Throws<ArgumentNullException>(() => type.GetContainingTypesAsData(null!));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SymbolIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => ((INamedTypeSymbol)null!).GetContainingTypesAsData(Compilation));
		}

		private INamedTypeSymbol GetSymbol(string source, int index = 0)
		{
			return GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>(source, index)!;
		}
	}
}