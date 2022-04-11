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
	public sealed class GetParameterList : CompilationTest
	{
		[Fact]
		public void CanHandleArray()
		{
			IMethodSymbol symbol = GetSymbol("class Test { void Method(int[] a) { } }");
			Assert.True(symbol.GetParameterList() == "(int[])");
		}

		[Fact]
		public void CanHandleArrayOfMultidimensionalArrays()
		{
			IMethodSymbol symbol = GetSymbol("class Test { void Method(int[][,] a) { } }");
			Assert.True(symbol.GetParameterList() == "(int[][,])");
		}

		[Fact]
		public void CanHandleArrayOfNullableValues()
		{
			IMethodSymbol symbol = GetSymbol("class Test { void Method(int?[] a) { } }");
			Assert.True(symbol.GetParameterList() == "(int?[])");
		}

		[Fact]
		public void CanHandleArraysOfGenericTypes()
		{
			IMethodSymbol symbol = GetSymbol("class Test { void Method(System.Collections.Generic.List<int>[] a) { } }");
			Assert.True(symbol.GetParameterList() == "(List<int>[])");
		}

		[Fact]
		public void CanHandleDynamic()
		{
			IMethodSymbol symbol = GetSymbol("class Test { void Method(dynamic a) { } }");
			Assert.True(symbol.GetParameterList() == "(dynamic)");
		}

		[Fact]
		public void CanHandleGenericTypes()
		{
			IMethodSymbol symbol = GetSymbol("class Test { void Method(System.Collections.Generic.List<int> a) { } }");
			Assert.True(symbol.GetParameterList() == "(List<int>)");
		}

		[Fact]
		public void CanHandleInParameter()
		{
			IMethodSymbol symbol = GetSymbol("class Test { void Method(in int a) { } }");
			Assert.True(symbol.GetParameterList() == "(in int)");
		}

		[Fact]
		public void CanHandleJaggedArrays()
		{
			IMethodSymbol symbol = GetSymbol("class Test { void Method(int[][] a) { } }");
			Assert.True(symbol.GetParameterList() == "(int[][])");
		}

		[Fact]
		public void CanHandleJaggedArraysOfNullableValues()
		{
			IMethodSymbol symbol = GetSymbol("class Test { void Method(int?[][] a) { } }");
			Assert.True(symbol.GetParameterList() == "(int?[][])");
		}

		[Fact]
		public void CanHandleMultidimensionalArrayOfArrays()
		{
			IMethodSymbol symbol = GetSymbol("class Test { void Method(int[,][] a) { } }");
			Assert.True(symbol.GetParameterList() == "(int[,][])");
		}

		[Fact]
		public void CanHandleMultidimensionalArrays()
		{
			IMethodSymbol symbol = GetSymbol("class Test { void Method(int[,] a) { } }");
			Assert.True(symbol.GetParameterList() == "(int[,])");
		}

		[Fact]
		public void CanHandleMultipleParameters()
		{
			IMethodSymbol symbol = GetSymbol("class Test { void Method(int a, int b) { } }");
			Assert.True(symbol.GetParameterList() == "(int, int)");
		}

		[Fact]
		public void CanHandleNullableGenericTypes()
		{
			IMethodSymbol symbol = GetSymbol("class Test { void Method(System.Collections.Generic.List<int>? a) { } }");
			Assert.True(symbol.GetParameterList() == "(List<int>?)");
		}

		[Fact]
		public void CanHandleNullableType()
		{
			IMethodSymbol symbol = GetSymbol("class Test { void Method(int? a) { } }");
			Assert.True(symbol.GetParameterList() == "(int?)");
		}

		[Fact]
		public void CanHandleNullableTypeAsDotNetType()
		{
			IMethodSymbol symbol = GetSymbol("class Test { void Method(System.Nullable<int> a { } }");
			Assert.True(symbol.GetParameterList() == "(int?)");
		}

		[Fact]
		public void CanHandleOneParameter()
		{
			IMethodSymbol symbol = GetSymbol("class Test { void Method(int a) { } }");
			Assert.True(symbol.GetParameterList() == "(int)");
		}

		[Fact]
		public void CanHandleOutParameter()
		{
			IMethodSymbol symbol = GetSymbol("class Test { void Method(out int a) { a = 2; } }");
			Assert.True(symbol.GetParameterList() == "(out int)");
		}

		[Fact]
		public void CanHandleParameteterLessMethod()
		{
			IMethodSymbol symbol = GetSymbol("class Test { void Method() { } }");
			Assert.True(symbol.GetParameterList() == "()");
		}

		[Fact]
		public void CanHandlePointer()
		{
			Compilation.OriginalCompilation = Compilation.OriginalCompilation!.WithOptions(Compilation.OriginalCompilation.Options.WithAllowUnsafe(true));
			IMethodSymbol symbol = GetSymbol("class Test { unsafe void Method(int* a) { } }");
			Assert.True(symbol.GetParameterList() == "(int*)");
		}

		[Fact]
		public void CanHandlePrimitiveTypeAsDotNetType()
		{
			IMethodSymbol symbol = GetSymbol("class Test { void Method(System.Int32 a) { } }");
			Assert.True(symbol.GetParameterList() == "(int)");
		}

		[Fact]
		public void CanHandleRefParameter()
		{
			IMethodSymbol symbol = GetSymbol("class Test { void Method(ref int a) { } }");
			Assert.True(symbol.GetParameterList() == "(ref int)");
		}

		[Fact]
		public void CanHandleVoidPointer()
		{
			Compilation.OriginalCompilation = Compilation.OriginalCompilation!.WithOptions(Compilation.OriginalCompilation.Options.WithAllowUnsafe(true));
			IMethodSymbol symbol = GetSymbol("class Test { unsafe void Method(void* a) { } }");
			Assert.True(symbol.GetParameterList() == "(void*)");
		}

		[Fact]
		public void ThrowsArgumentNullException_When_MethodIsNull()
		{
			IMethodSymbol symbol = null!;
			Assert.Throws<ArgumentNullException>(() => symbol.GetParameterList());
		}

		private IMethodSymbol GetSymbol(string source)
		{
			return GetSymbol<IMethodSymbol, MethodDeclarationSyntax>(source)!;
		}
	}
}
