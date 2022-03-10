// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Extensions;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using Xunit;

namespace Durian.Analysis.Tests.SymbolExtensions
{
	public sealed class GetContainingTypeSymbols : CompilationTest
	{
		[Fact]
		public void CanReturnMultipleTypes()
		{
			INamedTypeSymbol type = GetSymbol("class Test { class Parent { class Child { } } }", 2);
			INamedTypeSymbol[] containingTypes = type.GetContainingTypes().ToArray();
			Assert.True(containingTypes.Length == 2 && containingTypes.Any(t => t.Name == "Parent") && containingTypes.Any(t => t.Name == "Test"));
		}

		[Fact]
		public void CanReturnSingleType()
		{
			INamedTypeSymbol type = GetSymbol("class Parent { class Child { } }", 1);
			INamedTypeSymbol[] containingTypes = type.GetContainingTypes().ToArray();
			Assert.True(containingTypes.Length == 1 && containingTypes[0].Name == "Parent");
		}

		[Fact]
		public void ReturnsEmpty_When_IsNotNestedType()
		{
			INamedTypeSymbol type = GetSymbol("class Test { }");
			INamedTypeSymbol[] containingTypes = type.GetContainingTypes().ToArray();
			Assert.True(containingTypes.Length == 0);
		}

		[Fact]
		public void ReturnsTypesInParentFirstOrder()
		{
			INamedTypeSymbol type = GetSymbol("class Test { class Parent { class Child { } } }", 2);
			INamedTypeSymbol[] containingTypes = type.GetContainingTypes().ToArray();
			Assert.True(containingTypes.Length == 2 && containingTypes[0].Name == "Test" && containingTypes[1].Name == "Parent");
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SymbolIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => ((INamedTypeSymbol)null!).GetContainingTypes());
		}

		private INamedTypeSymbol GetSymbol(string source, int index = 0)
		{
			return GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>(source, index)!;
		}
	}
}