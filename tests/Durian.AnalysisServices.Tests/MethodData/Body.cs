// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.TestServices;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Analysis.Tests.MethodData
{
	public sealed class Body : CompilationTest
	{
		[Fact]
		public void CanReturnBlockBody()
		{
			Data.MethodData data = GetMethod("class Test { void Method() { } }");
			Assert.True(data.Body is BlockSyntax);
		}

		[Fact]
		public void CanReturnExpressionBody()
		{
			Data.MethodData data = GetMethod("class Test { int Method() => 2; }");
			Assert.True(data.Body is ArrowExpressionClauseSyntax);
		}

		[Fact]
		public void ReturnsNull_When_HasNoBody()
		{
			Data.MethodData data = GetMethod("class Test { partial void Method(); }");
			Assert.True(data.Body is null);
		}
	}
}