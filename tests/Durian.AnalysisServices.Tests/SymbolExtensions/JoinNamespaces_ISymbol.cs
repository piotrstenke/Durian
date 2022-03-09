// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Extensions;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using Xunit;

namespace Durian.Analysis.Tests.SymbolExtensions
{
	public sealed class JoinNamespaces_ISymbol : CompilationTest
	{
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

		[Fact]
		public void ThrowsArgumentNullException_When_MemberIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => ((INamedTypeSymbol)null!).JoinNamespaces());
		}

		private INamedTypeSymbol GetSymbol(string source)
		{
			return GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>(source)!;
		}
	}
}