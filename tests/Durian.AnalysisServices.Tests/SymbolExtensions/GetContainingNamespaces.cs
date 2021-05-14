using System;
using System.Linq;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Tests.AnalysisCore.SymbolExtensions
{
	public sealed class GetContainingNamespaces : CompilationTest
	{
		[Fact]
		public void ThrowsArgumentNullException_When_SymbolIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => ((INamedTypeSymbol)null!).GetContainingNamespaces());
		}

		[Fact]
		public void ReturnsEmpty_When_IsNotInNamespace()
		{
			INamedTypeSymbol type = GetSymbol("class Test { }");
			INamespaceSymbol[] containingNamespaces = type.GetContainingNamespaces().ToArray();
			Assert.True(containingNamespaces.Length == 0);
		}

		[Fact]
		public void ReturnsGlobalNamespace_When_IsNotInNamespace_And_IncludeGlobalIsTrue()
		{
			INamedTypeSymbol type = GetSymbol("class Test { }");
			INamespaceSymbol[] containingNamespaces = type.GetContainingNamespaces(true).ToArray();
			Assert.True(containingNamespaces.Length == 1 && SymbolEqualityComparer.Default.Equals(containingNamespaces[0], Compilation.CurrentCompilation.Assembly.GlobalNamespace));
		}

		[Fact]
		public void CanReturnSingleNamespace()
		{
			INamedTypeSymbol type = GetSymbol("namespace N1 { class Parent { } }");
			INamespaceSymbol[] containingNamespaces = type.GetContainingNamespaces().ToArray();
			Assert.True(containingNamespaces.Length == 1 && containingNamespaces[0].Name == "N1");
		}

		[Fact]
		public void CanReturnMultipleNamespaces()
		{
			INamedTypeSymbol type = GetSymbol("namespace N1.N2 { class Parent { } }");
			INamespaceSymbol[] containingNamespaces = type.GetContainingNamespaces().ToArray();
			Assert.True(containingNamespaces.Length == 2 && containingNamespaces.Any(t => t.Name == "N1") && containingNamespaces.Any(t => t.Name == "N2"));
		}

		[Fact]
		public void ReturnsNamespacesInParentFirstOrder()
		{
			INamedTypeSymbol type = GetSymbol("namespace N1.N2 { class Parent { } }");
			INamespaceSymbol[] containingNamespaces = type.GetContainingNamespaces().ToArray();
			Assert.True(containingNamespaces.Length == 2 && containingNamespaces[0].Name == "N1" && containingNamespaces[1].Name == "N2");
		}

		[Fact]
		public void ReturnsNamespacesInParentFirstOrderWithGlobalAsFirst_When_IncludeGlobalIsTrue()
		{
			INamedTypeSymbol type = GetSymbol("namespace N1.N2 { class Parent { } }");
			INamespaceSymbol[] containingNamespaces = type.GetContainingNamespaces(true).ToArray();

			Assert.True(
				containingNamespaces.Length == 3 &&
				SymbolEqualityComparer.Default.Equals(containingNamespaces[0], Compilation.CurrentCompilation.Assembly.GlobalNamespace) &&
				containingNamespaces[1].Name == "N1" &&
				containingNamespaces[2].Name == "N2"
			);
		}

		private INamedTypeSymbol GetSymbol(string source)
		{
			return GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>(source);
		}
	}
}
