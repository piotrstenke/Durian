// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Analysis.Extensions;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Analysis.Tests.SymbolExtensions
{
	public sealed class GetGenericName_INamedTypeSymbol : CompilationTest
	{
		[Fact]
		public void CanReturnNameWithMultipleTypeArguments()
		{
			INamedTypeSymbol symbol = GetSymbolForArgument("class Test<T, U> { } class New : Test<int, string> { }");
			Assert.True(symbol.GetGenericName(GenericSubstitution.TypeArguments) == "Test<int, string>");
		}

		[Fact]
		public void CanReturnNameWithMultipleTypeParameters()
		{
			INamedTypeSymbol symbol = GetSymbolForParameter("class Test<T, U> { }");
			Assert.True(symbol.GetGenericName() == "Test<T, U>");
		}

		[Fact]
		public void CanReturnNameWithSingleTypeArgument()
		{
			INamedTypeSymbol symbol = GetSymbolForArgument("class Test<T> { } class New : Test<int> { }");
			Assert.True(symbol.GetGenericName(GenericSubstitution.TypeArguments) == "Test<int>");
		}

		[Fact]
		public void CanReturnNameWithSingleTypeParameter()
		{
			INamedTypeSymbol symbol = GetSymbolForParameter("class Test<T> { }");
			Assert.True(symbol.GetGenericName() == "Test<T>");
		}

		[Fact]
		public void ReturnsOnlyName_When_HasNoTypeParameters()
		{
			INamedTypeSymbol symbol = GetSymbolForParameter("class Test { }");
			Assert.True(symbol.GetGenericName() == "Test");
		}

		[Fact]
		public void ThrowsArgumentNullException_When_MethodIsNull()
		{
			INamedTypeSymbol symbol = null!;
			Assert.Throws<ArgumentNullException>(() => symbol.GetGenericName());
		}

		private INamedTypeSymbol GetSymbolForArgument(string source)
		{
			BaseTypeSyntax b = GetNode<BaseTypeSyntax>(source)!;
			SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(b.SyntaxTree);
			TypeInfo info = semanticModel.GetTypeInfo(b.Type);
			return (info.Type as INamedTypeSymbol)!;
		}

		private INamedTypeSymbol GetSymbolForParameter(string source)
		{
			return GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>(source)!;
		}
	}
}
