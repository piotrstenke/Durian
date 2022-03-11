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
	public sealed class GetContainingNamespacesAndTypes : CompilationTest
	{
		[Fact]
		public void ReturnsAllContainingTypes_When_IsNotInNamespace_And_IsNestedType()
		{
			INamedTypeSymbol type = GetSymbol("class Parent { class Test { class Child { } } }", 2);
			INamespaceOrTypeSymbol[] n = type.GetContainingNamespacesAndTypes().ToArray();
			Assert.True(n.Length == 2 && n.Any(n => n.Name == "Parent") && n.Any(n => n.Name == "Test"));
		}

		[Fact]
		public void ReturnsAllContainingTypesAndNamespaces()
		{
			INamedTypeSymbol type = GetSymbol("namespace N1.N2 { class Parent { class Test { class Child { } } } }", 2);
			INamespaceOrTypeSymbol[] n = type.GetContainingNamespacesAndTypes().ToArray();
			Assert.True(
				n.Length == 4 &&
				n.Any(n => n.Name == "Parent") &&
				n.Any(n => n.Name == "Test") &&
				n.Any(n => n.Name == "N1") &&
				n.Any(n => n.Name == "N2")
			);
		}

		[Fact]
		public void ReturnsAllContainingTypesAndNamespacesAndGlobalNamespace_When_IncludeGlobalIsTrue()
		{
			INamedTypeSymbol type = GetSymbol("namespace N1.N2 { class Parent { class Test { class Child { } } } }", 2);
			INamespaceOrTypeSymbol[] n = type.GetContainingNamespacesAndTypes(true).ToArray();
			Assert.True(
				n.Length == 5 &&
				n.Any(n => n.Name == "Parent") &&
				n.Any(n => n.Name == "Test") &&
				n.Any(n => n.Name == "N1") &&
				n.Any(n => n.Name == "N2") &&
				n.Any(n => SymbolEqualityComparer.Default.Equals(n, Compilation.CurrentCompilation.Assembly.GlobalNamespace))
			);
		}

		[Fact]
		public void ReturnsAllNamespaces_When_IsInNestedNamespace()
		{
			INamedTypeSymbol type = GetSymbol("namespace N1.N2 { class Test { } }");
			INamespaceOrTypeSymbol[] n = type.GetContainingNamespacesAndTypes().ToArray();
			Assert.True(n.Length == 2 && n.Any(n => n.Name == "N1") && n.Any(n => n.Name == "N2"));
		}

		[Fact]
		public void ReturnsEmpty_When_IsNotInNamespace_And_IsNotNestedType()
		{
			INamedTypeSymbol type = GetSymbol("class Test { }");
			INamespaceOrTypeSymbol[] n = type.GetContainingNamespacesAndTypes().ToArray();
			Assert.True(n.Length == 0);
		}

		[Fact]
		public void ReturnsNamespacesAndTypesByNamespacesFirst()
		{
			INamedTypeSymbol type = GetSymbol("namespace N1.N2 { class Parent { class Test { class Child { }} } }", 2);
			INamespaceOrTypeSymbol[] n = type.GetContainingNamespacesAndTypes().ToArray();
			Assert.True(
				n.Length == 4 &&
				n[0].Name == "N1" &&
				n[1].Name == "N2" &&
				n[2].Name == "Parent" &&
				n[3].Name == "Test"
			);
		}

		[Fact]
		public void ReturnsNamespacesAndTypesByNamespacesFirstWithGlobalNamespaceFirst_When_IncludeGlobalIsTrue()
		{
			INamedTypeSymbol type = GetSymbol("namespace N1.N2 { class Parent { class Test { class Child { } } } }", 2);
			INamespaceOrTypeSymbol[] n = type.GetContainingNamespacesAndTypes(true).ToArray();
			Assert.True(
				n.Length == 5 &&
				SymbolEqualityComparer.Default.Equals(n[0], Compilation.CurrentCompilation.Assembly.GlobalNamespace) &&
				n[1].Name == "N1" &&
				n[2].Name == "N2" &&
				n[3].Name == "Parent" &&
				n[4].Name == "Test"
			);
		}

		[Fact]
		public void ReturnsNamespacesByParentFirst()
		{
			INamedTypeSymbol type = GetSymbol("namespace N1.N2 { class Test { } }");
			INamespaceOrTypeSymbol[] n = type.GetContainingNamespacesAndTypes().ToArray();
			Assert.True(n.Length == 2 && n[0].Name == "N1" && n[1].Name == "N2");
		}

		[Fact]
		public void ReturnsNamespacesByParentFirstWithGlobalNamespaceFirst_When_IncludeGlobalIsTrue()
		{
			INamedTypeSymbol type = GetSymbol("namespace N1.N2 { class Test { } }");
			INamespaceOrTypeSymbol[] n = type.GetContainingNamespacesAndTypes(true).ToArray();
			Assert.True(
				n.Length == 3 &&
				SymbolEqualityComparer.Default.Equals(n[0], Compilation.CurrentCompilation.Assembly.GlobalNamespace) &&
				n[1].Name == "N1" &&
				n[2].Name == "N2"
			);
		}

		[Fact]
		public void ReturnsOnlyGlobalNamespace_When_IsNotInNamespace_And_IsNotNestedType_AndIncludeGlobalIsTrue()
		{
			INamedTypeSymbol type = GetSymbol("class Test { }");
			INamespaceOrTypeSymbol[] n = type.GetContainingNamespacesAndTypes(true).ToArray();
			Assert.True(n.Length == 1 && SymbolEqualityComparer.Default.Equals(n[0], Compilation.CurrentCompilation.Assembly.GlobalNamespace));
		}

		[Fact]
		public void ReturnsOnlyRootNamespace_When_IsInRootNamespace_And_IsNotNestedType()
		{
			INamedTypeSymbol type = GetSymbol("namespace N1 { class Test { } }");
			INamespaceOrTypeSymbol[] n = type.GetContainingNamespacesAndTypes().ToArray();
			Assert.True(n.Length == 1 && n[0].Name == "N1");
		}

		[Fact]
		public void ReturnsTypesByParentFirst()
		{
			INamedTypeSymbol type = GetSymbol("class Parent { class Test { class Child { } } }", 2);
			INamespaceOrTypeSymbol[] n = type.GetContainingNamespacesAndTypes().ToArray();
			Assert.True(n.Length == 2 && n[0].Name == "Parent" && n[1].Name == "Test");
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SymbolIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => ((INamedTypeSymbol)null!).GetContainingNamespacesAndTypes());
		}

		private INamedTypeSymbol GetSymbol(string source, int index = 0)
		{
			return GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>(source, index)!;
		}
	}
}
