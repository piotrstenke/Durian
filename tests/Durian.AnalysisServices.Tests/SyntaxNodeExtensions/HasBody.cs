// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Analysis.Extensions;
using Durian.TestServices;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Analysis.Tests.SyntaxNodeExtensions
{
	public sealed class HasBody : CompilationTest
	{
		[Fact]
		public void ReturnsFalse_When_IsAbstract()
		{
			MethodDeclarationSyntax method = GetNode<MethodDeclarationSyntax>("class Test { abstract void Method(); }")!;
			Assert.False(method.HasBody());
		}

		[Fact]
		public void ReturnsFalse_When_IsPartial()
		{
			MethodDeclarationSyntax method = GetNode<MethodDeclarationSyntax>("class Test { partial void Method(); }")!;
			Assert.False(method.HasBody());
		}

		[Fact]
		public void ReturnsTrue_When_HasBlockBody()
		{
			MethodDeclarationSyntax method = GetNode<MethodDeclarationSyntax>("class Test { void Method() { } }")!;
			Assert.True(method.HasBody());
		}

		[Fact]
		public void ReturnsTrue_When_HasExpressionBody()
		{
			MethodDeclarationSyntax method = GetNode<MethodDeclarationSyntax>("class Test { int Method() => 1; }")!;
			Assert.True(method.HasBody());
		}
	}
}
