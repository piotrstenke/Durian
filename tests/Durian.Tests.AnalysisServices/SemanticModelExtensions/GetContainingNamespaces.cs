// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using Durian.Generator.Data;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Durian.Tests.AnalysisServices.SemanticModelExtensions
{
	public sealed class GetContainingNamespaces : CompilationTest
	{
		private INamespaceSymbol GlobalNamespace => Compilation.CurrentCompilation.Assembly.GlobalNamespace;

		[Fact]
		public void IsSuccess_When_IsInsideType()
		{
			INamespaceSymbol[] namespaces = Execute("namespace N1.N2 { class Parent { class Child { } } }", false, 1);
			Assert.True(namespaces.Length == 2 && namespaces.Any(n => n.Name == "N1") && namespaces.Any(n => n.Name == "N2"));
		}

		[Fact]
		public void ReturnsEmpty_When_IsInGlobalNamespace_And_IncludeGlobalIsFalse()
		{
			Assert.True(Execute("class Test { }").Length == 0);
		}

		[Fact]
		public void ReturnsGlobalNamespace_When_IsInGlobalNamespace_And_IncludeGlobalIsTrue()
		{
			INamespaceSymbol[] namespaces = Execute("class Test { }", true);
			Assert.True(
				namespaces.Length == 1 &&
				SymbolEqualityComparer.Default.Equals(namespaces[0], GlobalNamespace)
			);
		}

		[Fact]
		public void ReturnsMultipleNamespaces_When_IsInNestedNamespace()
		{
			INamespaceSymbol[] namespaces = Execute("namespace N1.N2 { class Test { } }");
			Assert.True(namespaces.Length == 2 && namespaces.Any(n => n.Name == "N1") && namespaces.Any(n => n.Name == "N2"));
		}

		[Fact]
		public void ReturnsNamespacesFromLeftToRight()
		{
			INamespaceSymbol[] namespaces = Execute("namespace N1.N2 { class Test { } }");
			Assert.True(namespaces.Length == 2 && namespaces[0].Name == "N1" && namespaces[1].Name == "N2");
		}

		[Fact]
		public void ReturnsNamespacesFromLeftToRightAndGlobalNamespaceIsFirst_When_IncludeGlobalIsTrue()
		{
			INamespaceSymbol[] namespaces = Execute("namespace N1.N2 { class Test { } }", true);
			Assert.True(
				namespaces.Length == 3 &&
				SymbolEqualityComparer.Default.Equals(namespaces[0], GlobalNamespace) &&
				namespaces[1].Name == "N1" &&
				namespaces[2].Name == "N2"
			);
		}

		[Fact]
		public void ReturnsParentNamespace_When_HasParentNamespace()
		{
			INamespaceSymbol[] namespaces = Execute("namespace N { class Test { } }");
			Assert.True(namespaces.Length == 1 && namespaces[0].Name == "N");
		}

		[Fact]
		public void ReturnsParentNamespaceAndGlobalNamespace_When_HasParentNamespace_And_IncludeGlobalIsTrue()
		{
			INamespaceSymbol[] namespaces = Execute("namespace N { class Test { } }", true);
			Assert.True(
				namespaces.Length == 2 &&
				namespaces.Any(n => n.Name == "N") &&
				namespaces.Any(n => SymbolEqualityComparer.Default.Equals(n, GlobalNamespace))
			);
		}

		[Fact]
		public void ThrowsArgumentException_When_GlobalNamespaceIsNotActuallyGlobal()
		{
			IMemberData member = GetClass("namespace N { class Test { } }");
			Assert.Throws<ArgumentException>(() => member.SemanticModel.GetContainingNamespaces(member.Declaration, member.Symbol.ContainingNamespace));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_AssemblySymbolIsNull()
		{
			IMemberData member = GetClass("class Test { }");
			Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetContainingNamespaces(member.Declaration, assembly: null!));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_CompilationDataIsNull()
		{
			IMemberData member = GetClass("class Test { }");
			Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetContainingNamespaces(member.Declaration, compilationData: null!));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_CompilationIsNull()
		{
			IMemberData member = GetClass("class Test { }");
			Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetContainingNamespaces(member.Declaration, compilation: null!));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_GlobalNamespaceIsNull()
		{
			IMemberData member = GetClass("class Test { }");
			Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetContainingNamespaces(member.Declaration, globalNamespace: null!));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SemanticModelIsNull()
		{
			IMemberData member = GetClass("class Test { }");
			SemanticModel? semanticModel = null;
			Assert.Throws<ArgumentNullException>(() => semanticModel!.GetContainingNamespaces(member.Declaration, Compilation));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SyntaxNodeIsNull()
		{
			IMemberData member = GetClass("class Test { }");
			Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetContainingNamespaces(null!, Compilation));
		}

		private INamespaceSymbol[] Execute(string src, bool includeGlobal = false, int index = 0)
		{
			IMemberData member = GetClass(src, index);
			return member.SemanticModel.GetContainingNamespaces(member.Declaration, Compilation, includeGlobal).ToArray();
		}
	}
}
