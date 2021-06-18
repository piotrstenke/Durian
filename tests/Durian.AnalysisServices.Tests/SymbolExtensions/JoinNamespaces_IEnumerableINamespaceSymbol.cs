// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Moq;
using Xunit;

namespace Durian.Analysis.Tests.SymbolExtensions
{
	public sealed class JoinNamespaces_IEnumerableINamespaceSymbol
	{
		[Fact]
		public void ReturnsEmptyString_When_NamespacesIsEmpty()
		{
			Assert.True(Array.Empty<INamespaceSymbol>().JoinNamespaces() == string.Empty);
		}

		[Fact]
		public void ReturnsMultipleNamespaces_When_NamespacesContainsMultipleElements()
		{
			Mock<INamespaceSymbol> n1 = new();
			n1.SetupGet(n => n.Name).Returns("System");

			Mock<INamespaceSymbol> n2 = new();
			n2.SetupGet(n => n.Name).Returns("Collections");

			Mock<INamespaceSymbol> n3 = new();
			n3.SetupGet(n => n.Name).Returns("Generic");

			Assert.True(new INamespaceSymbol[] { n1.Object, n2.Object, n3.Object }.JoinNamespaces() == "System.Collections.Generic");
		}

		[Fact]
		public void ReturnsSingleNamespace_When_NamespacesContainsOneElement()
		{
			Mock<INamespaceSymbol> n = new();
			n.SetupGet(n => n.Name).Returns("System");

			Assert.True(new INamespaceSymbol[] { n.Object }.JoinNamespaces() == "System");
		}

		[Fact]
		public void ThrowsArgumentNullException_When_NamespacesIsIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => ((INamespaceSymbol[])null!).JoinNamespaces());
		}
	}
}
