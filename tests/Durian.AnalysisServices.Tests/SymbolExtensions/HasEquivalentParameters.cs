// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Analysis.Extensions;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Analysis.Tests.SymbolExtensions
{
	public sealed class HasEquivalentParameters : CompilationTest
	{
		[Fact]
		public void False_When_BothMethodsHaveTheSameAmountOfParameters_And_ParamatersAreInDifferentOrder()
		{
			IMethodSymbol first = GetSymbol("class Test { void Method(int a, string b) { } }");
			IMethodSymbol second = GetSymbol("class T { void M(string a, int b) { } }");
			Assert.False(first.HasEquivalentParameters(second));
		}

		[Fact]
		public void False_When_BothMethodsHaveTheSameAmountOfParameters_And_ParamatersHaveDifferentTypes()
		{
			IMethodSymbol first = GetSymbol("class Test { void Method(int a, string b) { } }");
			IMethodSymbol second = GetSymbol("class T { void M(int a, float b) { } }");
			Assert.False(first.HasEquivalentParameters(second));
		}

		[Fact]
		public void False_When_MethodsHaveSameParameterTypes_And_OneMethodHasInParameter()
		{
			IMethodSymbol first = GetSymbol("class Test { void Method(int a) { } }");
			IMethodSymbol second = GetSymbol("class T { void M(in int a) { } }");
			Assert.False(first.HasEquivalentParameters(second));
		}

		[Fact]
		public void False_When_MethodsHaveSameParameterTypes_And_OneMethodHasOutParameter()
		{
			IMethodSymbol first = GetSymbol("class Test { void Method(int a) { } }");
			IMethodSymbol second = GetSymbol("class T { void M(out int a) { } }");
			Assert.False(first.HasEquivalentParameters(second));
		}

		[Fact]
		public void False_When_MethodsHaveSameParameterTypes_And_OneMethodHasRefParameter()
		{
			IMethodSymbol first = GetSymbol("class Test { void Method(int a) { } }");
			IMethodSymbol second = GetSymbol("class T { void M(ref int a) { } }");
			Assert.False(first.HasEquivalentParameters(second));
		}

		[Fact]
		public void False_When_OneMethodHasMoreParameters()
		{
			IMethodSymbol first = GetSymbol("class Test { void Method(int a, int b) { } }");
			IMethodSymbol second = GetSymbol("class T { void M(int a) { } }");
			Assert.False(first.HasEquivalentParameters(second));
		}

		[Fact]
		public void False_When_OneMethodIsParameterless()
		{
			IMethodSymbol first = GetSymbol("class Test { void Method() { } }");
			IMethodSymbol second = GetSymbol("class T { void M(int a) { } }");
			Assert.False(first.HasEquivalentParameters(second));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_FirstIsNull()
		{
			IMethodSymbol first = null!;
			IMethodSymbol second = GetSymbol("class Test { void Method(int value) { }");
			Assert.Throws<ArgumentNullException>(() => first.HasEquivalentParameters(second));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SecondIsNull()
		{
			IMethodSymbol first = GetSymbol("class Test { void Method(int value) { }");
			IMethodSymbol second = null!;
			Assert.Throws<ArgumentNullException>(() => first.HasEquivalentParameters(second));
		}

		[Fact]
		public void True_When_BothMethodsAreParameterless()
		{
			IMethodSymbol first = GetSymbol("class Test { void Method() { } }");
			IMethodSymbol second = GetSymbol("class T { void M() { } }");
			Assert.True(first.HasEquivalentParameters(second));
		}

		[Fact]
		public void True_When_BothMethodsHaveTheSameParameterTypes()
		{
			IMethodSymbol first = GetSymbol("class Test { void Method(int a) { } }");
			IMethodSymbol second = GetSymbol("class T { void M(int b) { } }");
			Assert.True(first.HasEquivalentParameters(second));
		}

		[Fact]
		public void True_When_MethodsHaveSameParameterTypes_And_OneMethodHasOptionalParameter()
		{
			IMethodSymbol first = GetSymbol("class Test { void Method(int a) { } }");
			IMethodSymbol second = GetSymbol("class T { void M(int a = 0) { } }");
			Assert.True(first.HasEquivalentParameters(second));
		}

		[Fact]
		public void True_When_MethodsHaveSameParameterTypes_And_OneMethodHasParams()
		{
			IMethodSymbol first = GetSymbol("class Test { void Method(int[] a) { } }");
			IMethodSymbol second = GetSymbol("class T { void M(params int[] a) { } }");
			Assert.True(first.HasEquivalentParameters(second));
		}

		[Fact]
		public void True_When_OneMethodIsOut_And_TheSecondIsIn()
		{
			IMethodSymbol first = GetSymbol("class Test { void Method(out int a) { } }");
			IMethodSymbol second = GetSymbol("class T { void M(in int a) { } }");
			Assert.True(first.HasEquivalentParameters(second));
		}

		[Fact]
		public void True_When_OneMethodIsRef_And_TheSecondIsIn()
		{
			IMethodSymbol first = GetSymbol("class Test { void Method(ref int a) { } }");
			IMethodSymbol second = GetSymbol("class T { void M(in int a) { } }");
			Assert.True(first.HasEquivalentParameters(second));
		}

		[Fact]
		public void True_When_OneMethodIsRef_And_TheSecondIsOut()
		{
			IMethodSymbol first = GetSymbol("class Test { void Method(ref int a) { } }");
			IMethodSymbol second = GetSymbol("class T { void M(out int a) { } }");
			Assert.True(first.HasEquivalentParameters(second));
		}

		private IMethodSymbol GetSymbol(string input)
		{
			return GetSymbol<IMethodSymbol, MethodDeclarationSyntax>(input);
		}
	}
}