// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Tests.AnalysisServices.MethodData
{
	public sealed class Body : CompilationTest
	{
		[Fact]
		public void CanReturnBlockBody()
		{
			Generator.Data.MethodData data = GetMethod("class Test { void Method() { } }");
			Assert.True(data.Body is BlockSyntax);
		}

		[Fact]
		public void CanReturnExpressionBody()
		{
			Generator.Data.MethodData data = GetMethod("class Test { int Method() => 2; }");
			Assert.True(data.Body is ArrowExpressionClauseSyntax);
		}

		[Fact]
		public void ReturnsNull_When_HasNoBody()
		{
			Generator.Data.MethodData data = GetMethod("class Test { partial void Method(); }");
			Assert.True(data.Body is null);
		}
	}
}
