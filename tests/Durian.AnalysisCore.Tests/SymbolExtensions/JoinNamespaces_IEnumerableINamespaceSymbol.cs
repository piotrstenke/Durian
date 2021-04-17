using System;
using Durian.Extensions;
using Microsoft.CodeAnalysis;
using Moq;
using Xunit;

namespace Durian.Tests.AnalysisCore.SymbolExtensions
{
	public sealed class JoinNamespaces_IEnumerableINamespaceSymbol
	{
		[Fact]
		public void ThrowsArgumentNullException_When_NamespacesIsIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => ((INamespaceSymbol[])null!).JoinNamespaces());
		}

		[Fact]
		public void ReturnsEmptyString_When_NamespacesIsEmpty()
		{
			Assert.True(Array.Empty<INamespaceSymbol>().JoinNamespaces() == string.Empty);
		}

		[Fact]
		public void ReturnsSingleNamespace_When_NamespacesContainsOneElement()
		{
			Mock<INamespaceSymbol> n = new();
			n.SetupGet(n => n.Name).Returns("System");

			Assert.True(new INamespaceSymbol[] { n.Object }.JoinNamespaces() == "System");
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
	}
}
