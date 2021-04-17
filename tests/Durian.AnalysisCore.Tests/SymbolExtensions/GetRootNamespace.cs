using System;
using Durian.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Tests.AnalysisCore.SymbolExtensions
{
	public sealed class GetRootNamespace : CompilationTest
	{
		[Fact]
		public void ThrowsArgumentNullException_When_SymbolIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => ((INamedTypeSymbol)null!).GetRootNamespace());
		}

		[Fact]
		public void ReturnsNull_When_IsNotInNamespace()
		{
			INamedTypeSymbol type = GetSymbol("class Test { }");
			Assert.True(type.GetRootNamespace() is null);
		}

		[Fact]
		public void ReturnsGlobalNamespace_When_IsNotInNamespace_And_IncludeGlobalIsTrue()
		{
			INamedTypeSymbol type = GetSymbol("class Test { }");
			INamespaceSymbol? n = type.GetRootNamespace(true);
			Assert.True(n is not null && SymbolEqualityComparer.Default.Equals(n, Compilation.CurrentCompilation.Assembly.GlobalNamespace));
		}

		[Fact]
		public void ReturnsRootNamespace_When_IsInRootNamespace()
		{
			INamedTypeSymbol type = GetSymbol("namespace N1 { class Test { } }");
			INamespaceSymbol? n = type.GetRootNamespace();
			Assert.True(n is not null && n.Name == "N1");
		}

		[Fact]
		public void ReturnsRootNamespace_When_IsInNestedNamespace()
		{
			INamedTypeSymbol type = GetSymbol("namespace N1.N2 { class Test { } }");
			INamespaceSymbol? n = type.GetRootNamespace();
			Assert.True(n is not null && n.Name == "N1");
		}

		[Fact]
		public void ReturnsGlobalNamespace_When_IsInNamespace_And_IncludeGlobalIsTrue()
		{
			INamedTypeSymbol type = GetSymbol("namespace N1.N2 { class Test { } }");
			INamespaceSymbol? n = type.GetRootNamespace(true);
			Assert.True(n is not null && SymbolEqualityComparer.Default.Equals(n, Compilation.CurrentCompilation.Assembly.GlobalNamespace));
		}

		private INamedTypeSymbol GetSymbol(string source)
		{
			return GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>(source);
		}
	}
}
