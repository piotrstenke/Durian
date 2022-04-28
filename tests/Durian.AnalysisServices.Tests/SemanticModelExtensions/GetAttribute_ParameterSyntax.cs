// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Analysis.Extensions;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Analysis.Tests.SemanticModelExtensions
{
	public sealed class GetAttribute_ParameterSyntax : CompilationTest
	{
		private INamedTypeSymbol AttributeSymbol => Compilation.CurrentCompilation.GetTypeByMetadataName("TestAttribute")!;

		public GetAttribute_ParameterSyntax() : base(Utilities.TestAttribute, Utilities.OtherAttribute)
		{
		}

		[Fact]
		public void ReturnsAttribute_WhenHasAttribute()
		{
			Assert.True(Execute("class Test { void Method([Test]int a) { } }")!.ToString() == "Test");
		}

		[Fact]
		public void ReturnsFirstAttribute_When_HasMultipleAttributesOfTheSameType()
		{
			Assert.True(Execute("class Test { void Method([Test(\"name\")][Test]int a) { } }")!.ToString() == "Test(\"name\")");
		}

		[Fact]
		public void ReturnsNull_When_HasNoAttributes()
		{
			Assert.True(Execute("class Test { void Method(int a) { } }") is null);
		}

		[Fact]
		public void ReturnsNull_When_HasOtherAttribute()
		{
			Assert.True(Execute("class Test { void Method([Other]int a) { } }") is null);
		}

		[Fact]
		public void ReturnsValidAttribute_When_FirstAttributeIsOfOtherType()
		{
			Assert.True(Execute("class Test { void Method([Other][Test(\"name\")][Test]int a) { } }")!.ToString() == "Test(\"name\")");
		}

		private AttributeSyntax? Execute(string src)
		{
			(ParameterSyntax parameter, SemanticModel semanticModel) = GetParameter(src);
			return semanticModel.GetAttribute(parameter, AttributeSymbol);
		}

		private (ParameterSyntax syntax, SemanticModel semanticModel) GetParameter(string src)
		{
			ParameterSyntax parameter = GetNode<ParameterSyntax>(src)!;
			SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(parameter.SyntaxTree, true);
			return (parameter, semanticModel);
		}
	}
}
