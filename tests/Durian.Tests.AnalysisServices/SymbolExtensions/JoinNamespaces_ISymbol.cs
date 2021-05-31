using System;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Tests.AnalysisServices.SymbolExtensions
{
	public sealed class JoinNamespaces_ISymbol : CompilationTest
	{
		[Fact]
		public void ThrowsArgumentNullException_When_MemberIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => ((INamedTypeSymbol)null!).JoinNamespaces());
		}

		[Fact]
		public void ReturnsEmptyString_When_IsInGlobalNamespace()
		{
			Assert.True(GetSymbol("class Type { }").JoinNamespaces() == string.Empty);
		}

		[Fact]
		public void ReturnsParentNamespace_When_HasOneParentNamespace()
		{
			Assert.True(GetSymbol("namespace Test { class Type { } }").JoinNamespaces() == "Test");
		}

		[Fact]
		public void ReturnsParentNamespaces_When_IsInNestedNamespace()
		{
			Assert.True(GetSymbol("namespace N1.N2 { class Type { } }").JoinNamespaces() == "N1.N2");
		}

		[Fact]
		public void ReturnsParentNamespaces_When_IsInNestedNamespaces_AndNamespacesAreWrittenSeparately()
		{
			Assert.True(GetSymbol("namespace N1 { namespace N2 { class Type { } } }").JoinNamespaces() == "N1.N2");
		}

		private INamedTypeSymbol GetSymbol(string source)
		{
			return GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>(source);
		}
	}
}