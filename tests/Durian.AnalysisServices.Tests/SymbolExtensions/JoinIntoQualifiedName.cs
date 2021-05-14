using System;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;
using Xunit;

namespace Durian.Tests.AnalysisCore.SymbolExtensions
{
	public sealed class JoinIntoQualifiedName
	{
		[Fact]
		public void ThrowsArgumentNullException_When_NamespacesIsIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => ((INamespaceSymbol[])null!).JoinIntoQualifiedName());
		}

		[Fact]
		public void ReturnsNull_When_ThereWereLessThan2Namespaces()
		{
			Mock<INamespaceSymbol> n = new();
			n.SetupGet(n => n.Name).Returns("System");

			Assert.True(new INamespaceSymbol[] { n.Object }.JoinIntoQualifiedName() is null);
		}

		[Fact]
		public void ReturnsQualifiedName_When_ThereWere2OrMoreNames()
		{
			Mock<INamespaceSymbol> n1 = new();
			n1.SetupGet(n => n.Name).Returns("System");

			Mock<INamespaceSymbol> n2 = new();
			n2.SetupGet(n => n.Name).Returns("Collections");

			Mock<INamespaceSymbol> n3 = new();
			n3.SetupGet(n => n.Name).Returns("Generic");

			QualifiedNameSyntax? syntax = new INamespaceSymbol[] { n1.Object, n2.Object, n3.Object }.JoinIntoQualifiedName();
			Assert.True(syntax is not null && syntax.ToFullString() == "System.Collections.Generic");
		}
	}
}
