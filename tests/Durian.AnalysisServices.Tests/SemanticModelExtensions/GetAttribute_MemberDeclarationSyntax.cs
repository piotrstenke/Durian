// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using Xunit;

namespace Durian.Analysis.Tests.SemanticModelExtensions
{
    public sealed class GetAttribute_MemberDeclarationSyntax : CompilationTest
    {
        private INamedTypeSymbol AttributeSymbol => Compilation.CurrentCompilation.GetTypeByMetadataName("TestAttribute")!;

        public GetAttribute_MemberDeclarationSyntax() : base(Utilities.TestAttribute, Utilities.OtherAttribute)
        {
        }

        [Fact]
        public void ReturnsAttribute_WhenHasAttribute()
        {
            Assert.True(Execute("[Test]class Test { }")!.ToString() == "Test");
        }

        [Fact]
        public void ReturnsFirstAttribute_When_HasMultipleAttributesOfTheSameType()
        {
            Assert.True(Execute("[Test(\"name\")][Test]class Test { }")!.ToString() == "Test(\"name\")");
        }

        [Fact]
        public void ReturnsNull_When_HasNoAttributes()
        {
            Assert.True(Execute("class Test { }") is null);
        }

        [Fact]
        public void ReturnsNull_When_HasOtherAttribute()
        {
            Assert.True(Execute("[Other]class Test { }") is null);
        }

        [Fact]
        public void ReturnsValidAttribute_When_FirstAttributeIsOfOtherType()
        {
            Assert.True(Execute("[Other][Test(\"name\")][Test]class Test { }")!.ToString() == "Test(\"name\")");
        }

        [Fact]
        public void ThrowsArgumentNullException_When_AttributeSymbolIsNull()
        {
            IMemberData member = GetClass("class Test { }")!;
            Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetAttribute(member.Declaration, null!));
        }

        [Fact]
        public void ThrowsArgumentNullException_When_SemanticModelIsNull()
        {
            IMemberData member = GetClass("class Test { }")!;
            SemanticModel? semanticModel = null;
            Assert.Throws<ArgumentNullException>(() => semanticModel!.GetAttribute(member.Declaration, AttributeSymbol));
        }

        [Fact]
        public void ThrowsArgumentNullException_When_SyntaxNodelIsNull()
        {
            IMemberData member = GetClass("class Test { }")!;
            Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetAttribute((MemberDeclarationSyntax)null!, AttributeSymbol));
        }

        private AttributeSyntax? Execute(string src)
        {
            IMemberData member = GetClass(src)!;
            return member.SemanticModel.GetAttribute(member.Declaration, AttributeSymbol);
        }
    }
}