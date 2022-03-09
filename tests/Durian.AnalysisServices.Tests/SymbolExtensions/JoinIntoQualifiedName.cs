// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;
using System;
using Xunit;

namespace Durian.Analysis.Tests.SymbolExtensions
{
	public sealed class JoinIntoQualifiedName
	{
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

		[Fact]
		public void ThrowsArgumentNullException_When_NamespacesIsIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => ((INamespaceSymbol[])null!).JoinIntoQualifiedName());
		}
	}
}