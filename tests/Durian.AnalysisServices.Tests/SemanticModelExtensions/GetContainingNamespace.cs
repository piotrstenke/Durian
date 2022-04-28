// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Durian.Analysis.Tests.SemanticModelExtensions
{
	public sealed class GetContainingNamespace : CompilationTest
	{
		[Fact]
		public void ReturnsGlobalNamespace_When_IsInGlobalNamespace()
		{
			Assert.True(SymbolEqualityComparer.Default.Equals(
				Execute("class Test { }"),
				Compilation.CurrentCompilation.Assembly.GlobalNamespace
			));
		}

		[Fact]
		public void ReturnsParentNamespace_When_HasParentNamespace()
		{
			Assert.True(Execute("namespace N { class Test { } }").Name == "N");
		}

		[Fact]
		public void ReturnsParentNamespace_When_IsInNestedNamespace()
		{
			Assert.True(Execute("namespace N1.N2 { class Test { } }").Name == "N2");
		}

		[Fact]
		public void ReturnsParentNamespace_When_IsInsideType()
		{
			Assert.True(Execute("namespace N1.N2 { class Parent { class Child { } } }", 1).Name == "N2");
		}

		private INamespaceSymbol Execute(string src, int index = 0)
		{
			IMemberData member = GetClass(src, index)!;
			return member.SemanticModel.GetContainingNamespace(member.Declaration, Compilation);
		}
	}
}
