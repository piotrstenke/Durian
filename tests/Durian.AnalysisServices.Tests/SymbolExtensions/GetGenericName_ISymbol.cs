// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Extensions;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using Xunit;

namespace Durian.Analysis.Tests.SymbolExtensions
{
	public sealed class GetGenericName_ISymbol : CompilationTest
	{
		[Fact]
		public void CanReturnNameWithMultipleTypeArguments_When_IsNamedTypeSymbol()
		{
			INamedTypeSymbol symbol = GetTypeSymbolForArgument("class Test<T, U> { } class New : Test<int, string> { }");
			Assert.True(symbol.GetGenericName(false) == "Test<int, string>");
		}

		[Fact]
		public void CanReturnNameWithmultipleTypeArgumentsAndParameters_When_IsMethodSymbol_And_IncludeParametersIsTrue()
		{
			IMethodSymbol symbol = GetMethodSymbolForArgument("class Test { void Method<T, U>(int a) { } void Caller() { Method<int, string>(2); } }");
			Assert.True(symbol.GetGenericName(false, true) == "Method<int, string>(int)");
		}

		[Fact]
		public void CanReturnNameWithMultipleTypeArgumentsIsMethodSymbol()
		{
			IMethodSymbol symbol = GetMethodSymbolForArgument("class Test { void Method<T, U>() { } void Caller() { Method<int, string>(); } }");
			Assert.True(symbol.GetGenericName(false) == "Method<int, string>");
		}

		[Fact]
		public void CanReturnNameWithMultipleTypeParameters_When_IsNamedTypeSymbol()
		{
			INamedTypeSymbol symbol = GetTypeSymbolForParameter("class Test<T, U> { }");
			Assert.True(symbol.GetGenericName(true) == "Test<T, U>");
		}

		[Fact]
		public void CanReturnNameWithmultipleTypeParametersAndParameters_When_IsMethodSymbol_And_IncludeParametersIsTrue()
		{
			IMethodSymbol symbol = GetMethodSymbolForParameter("class Test { void Method<T, U>(int a) { } }");
			Assert.True(symbol.GetGenericName(true, true) == "Method<T, U>(int)");
		}

		[Fact]
		public void CanReturnNameWithMultipleTypeParametersIsMethodSymbol()
		{
			IMethodSymbol symbol = GetMethodSymbolForParameter("class Test { void Method<T, U>() { } }");
			Assert.True(symbol.GetGenericName(true) == "Method<T, U>");
		}

		[Fact]
		public void CanReturnNameWithSingleTypeArgument_When_IsMethodSymbol()
		{
			IMethodSymbol symbol = GetMethodSymbolForArgument("class Test { void Method<T>() { } void Caller() { Method<int>(); } }");
			Assert.True(symbol.GetGenericName(false) == "Method<int>");
		}

		[Fact]
		public void CanReturnNameWithSingleTypeArgument_When_IsNamedTypeSymbol()
		{
			INamedTypeSymbol symbol = GetTypeSymbolForArgument("class Test<T> { } class New : Test<int> { }");
			Assert.True(symbol.GetGenericName(false) == "Test<int>");
		}

		[Fact]
		public void CanReturnNameWithSingleTypeArgumentAndParameters_When_IsMethodSymbol_And_IncludeParametersIsTrue()
		{
			IMethodSymbol symbol = GetMethodSymbolForArgument("class Test { void Method<T>(int a) { } void Caller() { Method<int>(2); } }");
			Assert.True(symbol.GetGenericName(false, true) == "Method<int>(int)");
		}

		[Fact]
		public void CanReturnNameWithSingleTypeParameter_When_IsMethodSymbol()
		{
			IMethodSymbol symbol = GetMethodSymbolForParameter("class Test { void Method<T>() { } }");
			Assert.True(symbol.GetGenericName(true) == "Method<T>");
		}

		[Fact]
		public void CanReturnNameWithSingleTypeParameter_When_IsNamedTypeSymbol()
		{
			INamedTypeSymbol symbol = GetTypeSymbolForParameter("class Test<T> { }");
			Assert.True(symbol.GetGenericName(true) == "Test<T>");
		}

		[Fact]
		public void CanReturnNameWithSingleTypeParameterAndParameters_When_IsMethodSymbol_And_IncludeParametersIsTrue()
		{
			IMethodSymbol symbol = GetMethodSymbolForParameter("class Test { void Method<T>(int a) { } }");
			Assert.True(symbol.GetGenericName(true, true) == "Method<T>(int)");
		}

		[Fact]
		public void ReturnsNameWithParameters_When_IsMethodSymbol_And_HasNoTypeParameters_And_IncludeParametersIsTrue()
		{
			IMethodSymbol symbol = GetMethodSymbolForParameter("class Test { void Method(int a) { } }");
			Assert.True(symbol.GetGenericName(true, true) == "Method(int)");
		}

		[Fact]
		public void ReturnsOnlyName_When_IsMethodSymbol_And_HasNoTypeParameters()
		{
			IMethodSymbol symbol = GetMethodSymbolForParameter("class Test { void Method() { } }");
			Assert.True(symbol.GetGenericName(true) == "Method");
		}

		[Fact]
		public void ReturnsOnlyName_When_IsNamedTypeSymbol_And_HasNoTypeParameters()
		{
			INamedTypeSymbol symbol = GetTypeSymbolForParameter("class Test { }");
			Assert.True(symbol.GetGenericName(true) == "Test");
		}

		[Fact]
		public void ReturnsSymbolName_When_SymbolIsNotMethodOrNamedType()
		{
			ISymbol symbol = GetSymbol<IPropertySymbol, PropertyDeclarationSyntax>("class Test { string Name { get; } }")!;
			Assert.True(symbol.GetGenericName(true) == "Name");
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SymbolIsNull()
		{
			ISymbol symbol = null!;
			Assert.Throws<ArgumentNullException>(() => symbol.GetGenericName(true));
		}

		private IMethodSymbol GetMethodSymbolForArgument(string source)
		{
			InvocationExpressionSyntax inv = GetNode<InvocationExpressionSyntax>(source)!;
			SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(inv.SyntaxTree);
			SymbolInfo info = semanticModel.GetSymbolInfo(inv);
			return (info.Symbol as IMethodSymbol)!;
		}

		private IMethodSymbol GetMethodSymbolForParameter(string source)
		{
			return GetSymbol<IMethodSymbol, MethodDeclarationSyntax>(source)!;
		}

		private INamedTypeSymbol GetTypeSymbolForArgument(string source)
		{
			BaseTypeSyntax b = GetNode<BaseTypeSyntax>(source)!;
			SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(b.SyntaxTree);
			TypeInfo info = semanticModel.GetTypeInfo(b.Type);
			return (info.Type as INamedTypeSymbol)!;
		}

		private INamedTypeSymbol GetTypeSymbolForParameter(string source)
		{
			return GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>(source)!;
		}
	}
}