// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using Durian.Analysis.Extensions;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Analysis.Tests.SymbolExtensions
{
	public sealed class ContainsSymbol : CompilationTest
	{
		[Fact]
		public void ReturnsFalse_When_ParentDoesNotContainChild()
		{
			INamedTypeSymbol parent = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("class Parent { }");
			INamedTypeSymbol child = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("class Child { }");

			Assert.False(parent.ContainsSymbol(child));
		}

		[Fact]
		public void ReturnsTrue_When_ParentContainsChild()
		{
			ClassDeclarationSyntax syntax = GetNode<ClassDeclarationSyntax>("class Parent { class Child { } }");
			INamedTypeSymbol parent = GetSymbol<INamedTypeSymbol>(syntax);
			INamedTypeSymbol child = parent.GetTypeMembers("Child").First();

			Assert.True(parent.ContainsSymbol(child));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_ChildIsNull()
		{
			INamedTypeSymbol child = null!;
			INamedTypeSymbol parent = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("class Parent { }");
			Assert.Throws<ArgumentNullException>(() => parent.ContainsSymbol(child));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_ParentIsNull()
		{
			INamedTypeSymbol child = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("class Child { }");
			INamedTypeSymbol parent = null!;
			Assert.Throws<ArgumentNullException>(() => parent.ContainsSymbol(child));
		}
	}
}