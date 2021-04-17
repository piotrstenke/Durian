using System;
using System.Linq;
using Durian.Data;
using Durian.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Tests.AnalysisCore.SymbolExtensions
{
	public sealed class GetContainingTypes : CompilationTest
	{
		[Fact]
		public void ThrowsArgumentNullException_When_SymbolIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => ((INamedTypeSymbol)null!).GetContainingTypes(Compilation));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_CompilationIsNull()
		{
			INamedTypeSymbol type = GetSymbol("class Test { }");
			Assert.Throws<ArgumentNullException>(() => type.GetContainingTypes(null!));
		}

		[Fact]
		public void ReturnsEmpty_When_IsNotNestedType()
		{
			INamedTypeSymbol type = GetSymbol("class Test { }");
			ITypeData[] containingTypes = type.GetContainingTypes(Compilation).ToArray();
			Assert.True(containingTypes.Length == 0);
		}

		[Fact]
		public void CanReturnSingleType()
		{
			INamedTypeSymbol type = GetSymbol("class Parent { class Child { } }", 1);
			ITypeData[] containingTypes = type.GetContainingTypes(Compilation).ToArray();
			Assert.True(containingTypes.Length == 1 && containingTypes[0].Symbol.Name == "Parent");
		}

		[Fact]
		public void CanReturnMultipleTypes()
		{
			INamedTypeSymbol type = GetSymbol("class Test { class Parent { class Child { } } }", 2);
			ITypeData[] containingTypes = type.GetContainingTypes(Compilation).ToArray();
			Assert.True(containingTypes.Length == 2 && containingTypes.Any(t => t.Symbol.Name == "Parent") && containingTypes.Any(t => t.Symbol.Name == "Test"));
		}

		[Fact]
		public void ReturnsTypesInParentFirstOrder()
		{
			INamedTypeSymbol type = GetSymbol("class Test { class Parent { class Child { } } }", 2);
			ITypeData[] containingTypes = type.GetContainingTypes(Compilation).ToArray();
			Assert.True(containingTypes.Length == 2 && containingTypes[0].Symbol.Name == "Test" && containingTypes[1].Symbol.Name == "Parent");
		}

		private INamedTypeSymbol GetSymbol(string source, int index = 0)
		{
			return GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>(source, index);
		}
	}
}
