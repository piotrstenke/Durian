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
    public sealed class GetGenericName_IMethodSymbol : CompilationTest
    {
        [Fact]
        public void CanReturnNameWithMultipleTypeArguments()
        {
            IMethodSymbol symbol = GetSymbolForArgument("class Test { void Method<T, U>() { } void Caller() { Method<int, string>(); } }");
            Assert.True(symbol.GetGenericName(false) == "Method<int, string>");
        }

        [Fact]
        public void CanReturnNameWithmultipleTypeArgumentsAndParameters_When_IncludeParametersIsTrue()
        {
            IMethodSymbol symbol = GetSymbolForArgument("class Test { void Method<T, U>(int a) { } void Caller() { Method<int, string>(2); }  }");
            Assert.True(symbol.GetGenericName(false, true) == "Method<int, string>(int)");
        }

        [Fact]
        public void CanReturnNameWithMultipleTypeParameters()
        {
            IMethodSymbol symbol = GetSymbolForParameter("class Test { void Method<T, U>() { } }");
            Assert.True(symbol.GetGenericName(true) == "Method<T, U>");
        }

        [Fact]
        public void CanReturnNameWithmultipleTypeParametersAndParameters_When_IncludeParametersIsTrue()
        {
            IMethodSymbol symbol = GetSymbolForParameter("class Test { void Method<T, U>(int a) { } }");
            Assert.True(symbol.GetGenericName(true, true) == "Method<T, U>(int)");
        }

        [Fact]
        public void CanReturnNameWithSingleTypeArgument()
        {
            IMethodSymbol symbol = GetSymbolForArgument("class Test { void Method<T>() { } void Caller() { Method<int>(); } }");
            Assert.True(symbol.GetGenericName(false) == "Method<int>");
        }

        [Fact]
        public void CanReturnNameWithSingleTypeArgumentAndParameters_When_IncludeParametersIsTrue()
        {
            IMethodSymbol symbol = GetSymbolForArgument("class Test { void Method<T>(int a) { } void Caller() { Method<int>(2); } }");
            Assert.True(symbol.GetGenericName(false, true) == "Method<int>(int)");
        }

        [Fact]
        public void CanReturnNameWithSingleTypeParameter()
        {
            IMethodSymbol symbol = GetSymbolForParameter("class Test { void Method<T>() { } }");
            Assert.True(symbol.GetGenericName(true) == "Method<T>");
        }

        [Fact]
        public void CanReturnNameWithSingleTypeParameterAndParameters_When_IncludeParametersIsTrue()
        {
            IMethodSymbol symbol = GetSymbolForParameter("class Test { void Method<T>(int a) { } }");
            Assert.True(symbol.GetGenericName(true, true) == "Method<T>(int)");
        }

        [Fact]
        public void ReturnsNameWithParameters_When_HasNoTypeParameters_And_IncludeParametersIsTrue()
        {
            IMethodSymbol symbol = GetSymbolForParameter("class Test { void Method(int a) { } }");
            Assert.True(symbol.GetGenericName(true, true) == "Method(int)");
        }

        [Fact]
        public void ReturnsOnlyName_When_HasNoTypeParameters()
        {
            IMethodSymbol symbol = GetSymbolForParameter("class Test { void Method() { } }");
            Assert.True(symbol.GetGenericName(true) == "Method");
        }

        [Fact]
        public void ThrowsArgumentNullException_When_MethodIsNull()
        {
            IMethodSymbol symbol = null!;
            Assert.Throws<ArgumentNullException>(() => symbol.GetGenericName(true));
        }

        private IMethodSymbol GetSymbolForArgument(string source)
        {
            InvocationExpressionSyntax inv = GetNode<InvocationExpressionSyntax>(source)!;
            SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(inv.SyntaxTree);
            SymbolInfo info = semanticModel.GetSymbolInfo(inv);
            return (info.Symbol as IMethodSymbol)!;
        }

        private IMethodSymbol GetSymbolForParameter(string source)
        {
            return GetSymbol<IMethodSymbol, MethodDeclarationSyntax>(source)!;
        }
    }
}