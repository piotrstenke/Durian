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
    public sealed class GetAllAttributesOfType_TypeParameterSyntax : CompilationTest
    {
        private INamedTypeSymbol AttributeSymbol => Compilation.CurrentCompilation.GetTypeByMetadataName("TestAttribute")!;

        public GetAllAttributesOfType_TypeParameterSyntax() : base(Utilities.TestAttribute, Utilities.OtherAttribute)
        {
        }

        [Fact]
        public void ReturnsAttributesFromLeftToRight()
        {
            AttributeSyntax[] attrs = Execute("class Test<[Test(\"name\")][Test]T> { }");
            Assert.True(
                attrs.Length == 2 &&
                attrs[0].ToString() == "Test(\"name\")" &&
                attrs[1].ToString() == "Test"
            );
        }

        [Fact]
        public void ReturnsEmpty_When_HasNoAttributes()
        {
            Assert.True(Execute("class Test<T> { }").Length == 0);
        }

        [Fact]
        public void ReturnsEmpty_When_HasOtherAttribute()
        {
            Assert.True(Execute("class Test<[Other]T> { }").Length == 0);
        }

        [Fact]
        public void ReturnsMultipleAttributes_WhenHasMultipleAttributesOfTheSameType()
        {
            AttributeSyntax[] attrs = Execute("class Test<[Test(\"name\")][Test]T> { }");
            Assert.True(
                attrs.Length == 2 &&
                attrs.Any(n => n.ToString() == "Test") &&
                attrs.Any(n => n.ToString() == "Test(\"name\")")
            );
        }

        [Fact]
        public void ReturnsOneAttribute_WhenHasOneAttribute()
        {
            AttributeSyntax[] attrs = Execute("class Test<[Test]T> { }");
            Assert.True(attrs.Length == 1 && attrs[0].ToString() == "Test");
        }

        [Fact]
        public void SkipsAttributesOfOtherTypes()
        {
            AttributeSyntax[] attrs = Execute("class Test<[Test(\"name\")][Other][Test]T> { }");
            Assert.True(
                attrs.Length == 2 &&
                attrs.Any(n => n.ToString() == "Test") &&
                attrs.Any(n => n.ToString() == "Test(\"name\")")
            );
        }

        [Fact]
        public void ThrowsArgumentNullException_When_AttributeSymbolIsNull()
        {
            (TypeParameterSyntax parameter, SemanticModel semanticModel) = GetTypeParameter("class Test<[Test]T> { }");
            Assert.Throws<ArgumentNullException>(() => semanticModel.GetAttributes(parameter, null!));
        }

        [Fact]
        public void ThrowsArgumentNullException_When_SemanticModelIsNull()
        {
            TypeParameterSyntax parameter = GetNode<TypeParameterSyntax>("class Test<[Test]T> { }")!;
            SemanticModel? semanticModel = null;
            Assert.Throws<ArgumentNullException>(() => semanticModel!.GetAttributes(parameter, AttributeSymbol));
        }

        [Fact]
        public void ThrowsArgumentNullException_When_SyntaxNodelIsNull()
        {
            (_, SemanticModel semanticModel) = GetTypeParameter("class Test<[Test]T> { }");
            Assert.Throws<ArgumentNullException>(() => semanticModel.GetAttributes((TypeParameterSyntax)null!, AttributeSymbol));
        }

        private AttributeSyntax[] Execute(string src)
        {
            (TypeParameterSyntax parameter, SemanticModel semanticModel) = GetTypeParameter(src);
            return semanticModel.GetAttributes(parameter, AttributeSymbol).ToArray();
        }

        private (TypeParameterSyntax syntax, SemanticModel semanticModel) GetTypeParameter(string src)
        {
            TypeParameterSyntax parameter = GetNode<TypeParameterSyntax>(src)!;
            SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(parameter.SyntaxTree, true);
            return (parameter, semanticModel);
        }
    }
}