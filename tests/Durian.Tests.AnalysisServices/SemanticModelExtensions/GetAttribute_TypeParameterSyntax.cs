// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Tests.AnalysisServices.SemanticModelExtensions
{
	public sealed class GetAttribute_TypeParameterSyntax : CompilationTest
	{
		private INamedTypeSymbol AttributeSymbol => Compilation.CurrentCompilation.GetTypeByMetadataName("TestAttribute")!;

		public GetAttribute_TypeParameterSyntax() : base(Utilities.TestAttribute, Utilities.OtherAttribute)
		{
		}

		[Fact]
		public void ReturnsAttribute_WhenHasAttribute()
		{
			Assert.True(Execute("class Test<[Test]T> { }")!.ToString() == "Test");
		}

		[Fact]
		public void ReturnsFirstAttribute_When_HasMultipleAttributesOfTheSameType()
		{
			Assert.True(Execute("class Test<[Test(\"name\")][Test]T> { }")!.ToString() == "Test(\"name\")");
		}

		[Fact]
		public void ReturnsNull_When_HasNoAttributes()
		{
			Assert.True(Execute("class Test<T> { }") is null);
		}

		[Fact]
		public void ReturnsNull_When_HasOtherAttribute()
		{
			Assert.True(Execute("class Test<[Other]T> { }") is null);
		}

		[Fact]
		public void ReturnsValidAttribute_When_FirstAttributeIsOfOtherType()
		{
			Assert.True(Execute("class Test<[Other][Test(\"name\")][Test]T> { }")!.ToString() == "Test(\"name\")");
		}

		[Fact]
		public void ThrowsArgumentNullException_When_AttributeSymbolIsNull()
		{
			(TypeParameterSyntax parameter, SemanticModel semanticModel) = GetTypeParameter("class Test<[Test]T> { }");
			Assert.Throws<ArgumentNullException>(() => semanticModel.GetAttribute(parameter, null!));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SemanticModelIsNull()
		{
			TypeParameterSyntax parameter = GetNode<TypeParameterSyntax>("class Test<[Test]T> { }");
			SemanticModel? semanticModel = null;
			Assert.Throws<ArgumentNullException>(() => semanticModel!.GetAttribute(parameter, AttributeSymbol));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SyntaxNodelIsNull()
		{
			(_, SemanticModel semanticModel) = GetTypeParameter("class Test<[Test]T> { }");
			Assert.Throws<ArgumentNullException>(() => semanticModel.GetAttribute((TypeParameterSyntax)null!, AttributeSymbol));
		}

		private AttributeSyntax? Execute(string src)
		{
			(TypeParameterSyntax parameter, SemanticModel semanticModel) = GetTypeParameter(src);
			return semanticModel.GetAttribute(parameter, AttributeSymbol);
		}

		private (TypeParameterSyntax syntax, SemanticModel semanticModel) GetTypeParameter(string src)
		{
			TypeParameterSyntax parameter = GetNode<TypeParameterSyntax>(src);
			SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(parameter.SyntaxTree, true);
			return (parameter, semanticModel);
		}
	}
}
