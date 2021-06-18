// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;
using Durian.Analysis.Extensions;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;
using Xunit;

namespace Durian.Analysis.Tests.SymbolExtensions
{
	public sealed class InheritsOrImplementsFrom : CompilationTest
	{
		[Fact]
		public void ReturnsFalse_When_DoesNotInheritFromParent()
		{
			INamedTypeSymbol parent = GetClass("class Test { }");
			INamedTypeSymbol child = GetClass("class Child { }");

			Assert.False(child.InheritsOrImplementsFrom(parent));
		}

		[Fact]
		public void ReturnsFalse_When_ParentAndChildAreTheSameSymbol_And_ToReturnIfSameIsFalse()
		{
			INamedTypeSymbol type = GetClass("class Test { }");
			Assert.False(type.InheritsOrImplementsFrom(type, false));
		}

		[Fact]
		public void ReturnsTrue_When_DirectlyInheritsFromParent()
		{
			INamedTypeSymbol parent = GetClass("class Test { }");

			Mock<INamedTypeSymbol> child = new();
			child.SetupGet(t => t.AllInterfaces).Returns(ImmutableArray.Create<INamedTypeSymbol>());
			child.SetupGet(t => t.BaseType).Returns(parent);
			child.SetupGet(t => t.TypeKind).Returns(TypeKind.Class);

			Assert.True(child.Object.InheritsOrImplementsFrom(parent));
		}

		[Fact]
		public void ReturnsTrue_When_IndirectlyInheritsFromParent()
		{
			INamedTypeSymbol parent = GetClass("class Test { }");

			Mock<INamedTypeSymbol> child = new();
			child.SetupGet(t => t.AllInterfaces).Returns(ImmutableArray.Create<INamedTypeSymbol>());
			child.SetupGet(t => t.BaseType).Returns(parent);
			child.SetupGet(t => t.TypeKind).Returns(TypeKind.Class);

			Mock<INamedTypeSymbol> childOfChild = new();
			childOfChild.SetupGet(t => t.AllInterfaces).Returns(ImmutableArray.Create<INamedTypeSymbol>());
			childOfChild.SetupGet(t => t.BaseType).Returns(child.Object);
			childOfChild.SetupGet(t => t.TypeKind).Returns(TypeKind.Class);

			Assert.True(childOfChild.Object.InheritsOrImplementsFrom(parent));
		}

		[Fact]
		public void ReturnsTrue_When_ParentAndChildAreTheSameSymbol()
		{
			INamedTypeSymbol type = GetClass("class Test { }");
			Assert.True(type.InheritsOrImplementsFrom(type));
		}

		[Fact]
		public void ReturnsTrue_When_ParentIsInterface()
		{
			INamedTypeSymbol parent = GetInterface("interface ITest { }");

			Mock<INamedTypeSymbol> child = new();
			child.SetupGet(t => t.AllInterfaces).Returns(ImmutableArray.Create(parent));
			child.SetupGet(t => t.TypeKind).Returns(TypeKind.Class);

			Assert.True(child.Object.InheritsOrImplementsFrom(parent));
		}

		[Fact]
		public void ReturnsTrue_When_ParentIsInterface_And_ParentIsImplementedByBaseClass()
		{
			INamedTypeSymbol intf = GetInterface("interface ITest { }");

			Mock<INamedTypeSymbol> parent = new();
			parent.SetupGet(t => t.AllInterfaces).Returns(ImmutableArray.Create(intf));
			parent.SetupGet(t => t.TypeKind).Returns(TypeKind.Class);

			Mock<INamedTypeSymbol> child = new();
			child.SetupGet(t => t.AllInterfaces).Returns(ImmutableArray.Create(intf));
			child.SetupGet(t => t.TypeKind).Returns(TypeKind.Class);
			child.SetupGet(t => t.BaseType).Returns(parent.Object);

			Assert.True(child.Object.InheritsOrImplementsFrom(intf));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_BaseTypeIsNull()
		{
			INamedTypeSymbol child = null!;
			INamedTypeSymbol parent = GetClass("class Test { }");
			Assert.Throws<ArgumentNullException>(() => child.InheritsOrImplementsFrom(parent));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_TypeIsNull()
		{
			INamedTypeSymbol child = GetClass("class Test { }");
			INamedTypeSymbol parent = null!;
			Assert.Throws<ArgumentNullException>(() => child.InheritsOrImplementsFrom(parent));
		}

		private INamedTypeSymbol GetClass(string source)
		{
			return GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>(source);
		}

		private INamedTypeSymbol GetInterface(string source)
		{
			return GetSymbol<INamedTypeSymbol, InterfaceDeclarationSyntax>(source);
		}
	}
}
