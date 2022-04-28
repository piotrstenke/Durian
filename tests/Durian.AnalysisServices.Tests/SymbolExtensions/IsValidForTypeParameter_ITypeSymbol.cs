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
	public sealed class IsValidForTypeParameter_ITypeSymbol : CompilationTest
	{
		[Fact]
		public void CanHandleArrayType()
		{
			ITypeSymbol type = GetSymbol<IArrayTypeSymbol, ArrayTypeSyntax>("class Test { void Method(int[] a) { } }")!;

			Assert.True(type.IsValidForTypeParameter(GetTypeParameter()));
		}

		[Fact]
		public void CanHandleDynamicType()
		{
			ITypeSymbol type = Compilation.CurrentCompilation.DynamicType;

			Assert.True(type.IsValidForTypeParameter(GetTypeParameter()));
		}

		[Fact]
		public void CanHandleNamedType()
		{
			ITypeSymbol type = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("class Test { }")!;

			Assert.True(type.IsValidForTypeParameter(GetTypeParameter()));
		}

		[Fact]
		public void CanHandleTypeParameter()
		{
			ITypeSymbol type = GetSymbol<ITypeParameterSymbol, TypeParameterSyntax>("class Test<T> { }")!;

			Assert.True(type.IsValidForTypeParameter(GetTypeParameter()));
		}

		private static ITypeParameterSymbol GetTypeParameter()
		{
			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());

			return parameter.Object;
		}
	}
}
