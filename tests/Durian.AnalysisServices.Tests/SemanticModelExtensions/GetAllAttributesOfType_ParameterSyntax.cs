// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Extensions;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using Xunit;

namespace Durian.Analysis.Tests.SemanticModelExtensions
{
	public sealed class GetAllAttributesOfType_ParameterSyntax : CompilationTest
	{
		private INamedTypeSymbol AttributeSymbol => Compilation.CurrentCompilation.GetTypeByMetadataName("TestAttribute")!;

		public GetAllAttributesOfType_ParameterSyntax() : base(Utilities.TestAttribute, Utilities.OtherAttribute)
		{
		}

		[Fact]
		public void ReturnsAttributesFromLeftToRight()
		{
			AttributeSyntax[] attrs = Execute("class Test { void Method([Test(\"name\")][Test]int a) { } }");
			Assert.True(
				attrs.Length == 2 &&
				attrs[0].ToString() == "Test(\"name\")" &&
				attrs[1].ToString() == "Test"
			);
		}

		[Fact]
		public void ReturnsEmpty_When_HasNoAttributes()
		{
			Assert.True(Execute("class Test { void Method(int a) { } }").Length == 0);
		}

		[Fact]
		public void ReturnsEmpty_When_HasOtherAttribute()
		{
			Assert.True(Execute("class Test { void Method([Other]int a) { } }").Length == 0);
		}

		[Fact]
		public void ReturnsMultipleAttributes_WhenHasMultipleAttributesOfTheSameType()
		{
			AttributeSyntax[] attrs = Execute("class Test { void Method([Test(\"name\")][Test]int a) { } }");
			Assert.True(
				attrs.Length == 2 &&
				attrs.Any(n => n.ToString() == "Test") &&
				attrs.Any(n => n.ToString() == "Test(\"name\")")
			);
		}

		[Fact]
		public void ReturnsOneAttribute_WhenHasOneAttribute()
		{
			AttributeSyntax[] attrs = Execute("class Test { void Method([Test]int a) { } }");
			Assert.True(attrs.Length == 1 && attrs[0].ToString() == "Test");
		}

		[Fact]
		public void SkipsAttributesOfOtherTypes()
		{
			AttributeSyntax[] attrs = Execute("class Test { void Method([Test(\"name\")][Other][Test]int a) { } }");
			Assert.True(
				attrs.Length == 2 &&
				attrs.Any(n => n.ToString() == "Test") &&
				attrs.Any(n => n.ToString() == "Test(\"name\")")
			);
		}

		[Fact]
		public void ThrowsArgumentNullException_When_AttributeSymbolIsNull()
		{
			(ParameterSyntax parameter, SemanticModel semanticModel) = GetParameter("class Test { void Method([Test]int a) { } }");
			Assert.Throws<ArgumentNullException>(() => semanticModel.GetAttributes(parameter, null!));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SemanticModelIsNull()
		{
			ParameterSyntax parameter = GetNode<ParameterSyntax>("class Test { void Method([Test]int a) { } }")!;
			SemanticModel? semanticModel = null;
			Assert.Throws<ArgumentNullException>(() => semanticModel!.GetAttributes(parameter, AttributeSymbol));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SyntaxNodelIsNull()
		{
			(_, SemanticModel semanticModel) = GetParameter("class Test { void Method([Test]int a) { } }");
			Assert.Throws<ArgumentNullException>(() => semanticModel.GetAttributes((ParameterSyntax)null!, AttributeSymbol));
		}

		private AttributeSyntax[] Execute(string src)
		{
			(ParameterSyntax parameter, SemanticModel semanticModel) = GetParameter(src);
			return semanticModel.GetAttributes(parameter, AttributeSymbol).ToArray();
		}

		private (ParameterSyntax syntax, SemanticModel semanticModel) GetParameter(string src)
		{
			ParameterSyntax parameter = GetNode<ParameterSyntax>(src)!;
			SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(parameter.SyntaxTree, true);
			return (parameter, semanticModel);
		}
	}
}