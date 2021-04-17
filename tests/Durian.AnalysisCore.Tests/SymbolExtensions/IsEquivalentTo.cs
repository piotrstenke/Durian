using System;
using Durian.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Tests.AnalysisCore.SymbolExtensions
{
	public sealed class IsEquivalentTo : CompilationTest
	{
		[Fact]
		public void ThrowsArgumentNullException_When_FirstIsNull()
		{
			IParameterSymbol first = null!;
			IParameterSymbol second = GetSymbol("class Test { void Method(int value) { }");
			Assert.Throws<ArgumentNullException>(() => first.IsEquivalentTo(second));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SecondIsNull()
		{
			IParameterSymbol first = GetSymbol("class Test { void Method(int value) { }");
			IParameterSymbol second = null!;
			Assert.Throws<ArgumentNullException>(() => first.IsEquivalentTo(second));
		}

		[Fact]
		public void True_When_BothParametersHaveTheSameType()
		{
			IParameterSymbol first = GetSymbol("class Test { void Method(int a) { } }");
			IParameterSymbol second = GetSymbol("class T { void M(int a) { } }");
			Assert.True(first.IsEquivalentTo(second));
		}

		[Fact]
		public void True_When_BothParametersHaveTheSameType_And_OneParameterIsRef_And_SecondParameterIsIn()
		{
			IParameterSymbol first = GetSymbol("class Test { void Method(ref int a) { } }");
			IParameterSymbol second = GetSymbol("class T { void M(in int a) { } }");
			Assert.True(first.IsEquivalentTo(second));
		}

		[Fact]
		public void True_When_BothParametersHaveTheSameType_And_OneParameterIsRef_And_SecondParameterIsOut()
		{
			IParameterSymbol first = GetSymbol("class Test { void Method(ref int a) { } }");
			IParameterSymbol second = GetSymbol("class T { void M(out int a) { } }");
			Assert.True(first.IsEquivalentTo(second));
		}

		[Fact]
		public void True_When_BothParametersHaveTheSameType_And_OneParameterIsOut_And_SecondParameterIsIn()
		{
			IParameterSymbol first = GetSymbol("class Test { void Method(out int a) { } }");
			IParameterSymbol second = GetSymbol("class T { void M(in int a) { } }");
			Assert.True(first.IsEquivalentTo(second));
		}

		[Fact]
		public void False_When_BothParametersHaveTheSameType_And_OneParameterIsRef()
		{
			IParameterSymbol first = GetSymbol("class Test { void Method(int a) { } }");
			IParameterSymbol second = GetSymbol("class T { void M(ref int a) { } }");
			Assert.False(first.IsEquivalentTo(second));
		}

		[Fact]
		public void False_When_BothParametersHaveTheSameType_And_OneParameterIsIn()
		{
			IParameterSymbol first = GetSymbol("class Test { void Method(int a) { } }");
			IParameterSymbol second = GetSymbol("class T { void M(in int a) { } }");
			Assert.False(first.IsEquivalentTo(second));
		}

		[Fact]
		public void False_When_BothParametersHaveTheSameType_And_OneParameterIsOut()
		{
			IParameterSymbol first = GetSymbol("class Test { void Method(int a) { } }");
			IParameterSymbol second = GetSymbol("class T { void M(out int a) { } }");
			Assert.False(first.IsEquivalentTo(second));
		}

		[Fact]
		public void True_When_BothParametersHaveTheSameType_And_OneParameterIsParams()
		{
			IParameterSymbol first = GetSymbol("class Test { void Method(int[] a) { } }");
			IParameterSymbol second = GetSymbol("class T { void M(params int[] a) { } }");
			Assert.True(first.IsEquivalentTo(second));
		}

		[Fact]
		public void True_When_BothParametersHaveTheSameType_And_OneParameterIsOptional()
		{
			IParameterSymbol first = GetSymbol("class Test { void Method(int a) { } }");
			IParameterSymbol second = GetSymbol("class T { void M(int a = 2) { } }");
			Assert.True(first.IsEquivalentTo(second));
		}

		private IParameterSymbol GetSymbol(string input)
		{
			return GetSymbol<IParameterSymbol, ParameterSyntax>(input);
		}
	}
}
