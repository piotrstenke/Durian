// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Linq;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Durian.Analysis.Tests.MemberData
{
	public sealed class GetContainingNamespaces : CompilationTest
	{
		[Fact]
		public void CanReturnMultipleNamespaces()
		{
			Data.MemberData data = GetMember("namespace N1.N2 { class Parent { } }", 1)!;
			INamespaceSymbol[] containingNamespaces = data.GetContainingNamespaces().ToArray();
			Assert.True(containingNamespaces.Length == 2 && containingNamespaces.Any(t => t.Name == "N1") && containingNamespaces.Any(t => t.Name == "N2"));
		}

		[Fact]
		public void CanReturnSingleNamespace()
		{
			Data.MemberData data = GetMember("namespace N1 { class Parent { } }", 1)!;
			INamespaceSymbol[] containingNamespaces = data.GetContainingNamespaces().ToArray();
			Assert.True(containingNamespaces.Length == 1 && containingNamespaces[0].Name == "N1");
		}

		[Fact]
		public void ReturnsEmpty_When_IsNotInNamespace()
		{
			Data.MemberData data = GetMember("class Test { }")!;
			INamespaceSymbol[] containingNamespaces = data.GetContainingNamespaces().ToArray();
			Assert.True(containingNamespaces.Length == 0);
		}

		[Fact]
		public void ReturnsNamespacesInParentFirstOrder()
		{
			Data.MemberData data = GetMember("namespace N1.N2 { class Parent { } }", 1)!;
			INamespaceSymbol[] containingNamespaces = data.GetContainingNamespaces().ToArray();
			Assert.True(containingNamespaces.Length == 2 && containingNamespaces[0].Name == "N1" && containingNamespaces[1].Name == "N2");
		}
	}
}