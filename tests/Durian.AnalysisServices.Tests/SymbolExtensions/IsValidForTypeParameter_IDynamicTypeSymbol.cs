// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Extensions;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;
using System;
using System.Collections.Immutable;
using Xunit;

namespace Durian.Analysis.Tests.SymbolExtensions
{
    public sealed class IsValidForTypeParameter_IDynamicTypeSymbol : CompilationTest
    {
        [Fact]
        public void ReturnsFalse_When_ConstrainedToAnyType()
        {
            IDynamicTypeSymbol type = GetSymbol();
            Mock<ITypeParameterSymbol> parameter = new();

            parameter.SetupGet(p => p.Name).Returns("T");
            parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>(Compilation.CurrentCompilation.GetTypeByMetadataName("System.Collections.IList")!));

            Assert.False(type.IsValidForTypeParameter(parameter.Object));
        }

        [Fact]
        public void ReturnsFalse_When_HasStructConstraint()
        {
            IDynamicTypeSymbol type = GetSymbol();
            Mock<ITypeParameterSymbol> parameter = new();
            parameter.SetupGet(p => p.Name).Returns("T");
            parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
            parameter.SetupGet(p => p.HasValueTypeConstraint).Returns(true);

            Assert.False(type.IsValidForTypeParameter(parameter.Object));
        }

        [Fact]
        public void ReturnsFalse_When_HasUnmanagedConstraint()
        {
            IDynamicTypeSymbol type = GetSymbol();
            Mock<ITypeParameterSymbol> parameter = new();
            parameter.SetupGet(p => p.Name).Returns("T");
            parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
            parameter.SetupGet(p => p.HasUnmanagedTypeConstraint).Returns(true);

            Assert.False(type.IsValidForTypeParameter(parameter.Object));
        }

        [Fact]
        public void ReturnsTrue_When_HasNewConstraint()
        {
            IDynamicTypeSymbol type = GetSymbol();
            Mock<ITypeParameterSymbol> parameter = new();
            parameter.SetupGet(p => p.Name).Returns("T");
            parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
            parameter.SetupGet(p => p.HasConstructorConstraint).Returns(true);

            Assert.True(type.IsValidForTypeParameter(parameter.Object));
        }

        [Fact]
        public void ReturnsTrue_When_HasNoConstraints()
        {
            IDynamicTypeSymbol type = GetSymbol();
            Mock<ITypeParameterSymbol> parameter = new();
            parameter.SetupGet(p => p.Name).Returns("T");
            parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());

            Assert.True(type.IsValidForTypeParameter(parameter.Object));
        }

        [Fact]
        public void ReturnsTrue_When_HasNotNullConstraint()
        {
            IDynamicTypeSymbol type = GetSymbol();
            Mock<ITypeParameterSymbol> parameter = new();
            parameter.SetupGet(p => p.Name).Returns("T");
            parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
            parameter.SetupGet(p => p.HasNotNullConstraint).Returns(true);

            Assert.True(type.IsValidForTypeParameter(parameter.Object));
        }

        [Fact]
        public void ReturnsTrue_When_HasNotNullConstraint_And_DynamicIsNullable()
        {
            NullableTypeSyntax id = GetNode<NullableTypeSyntax>("class Parent { void Method() { dynamic? test = 2)}); } }")!;
            SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(id.SyntaxTree);
            TypeInfo info = semanticModel.GetTypeInfo(id);

            Mock<ITypeParameterSymbol> parameter = new();
            parameter.SetupGet(p => p.Name).Returns("T");
            parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
            parameter.SetupGet(p => p.HasNotNullConstraint).Returns(true);

            Assert.True((info.Type as IDynamicTypeSymbol)!.IsValidForTypeParameter(parameter.Object));
        }

        [Fact]
        public void ReturnsTrue_When_HasNullableReferenceTypeConstraint()
        {
            IDynamicTypeSymbol type = GetSymbol();
            Mock<ITypeParameterSymbol> parameter = new();
            parameter.SetupGet(p => p.Name).Returns("T");
            parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
            parameter.SetupGet(p => p.HasReferenceTypeConstraint).Returns(true);
            parameter.SetupGet(p => p.ReferenceTypeConstraintNullableAnnotation).Returns(NullableAnnotation.Annotated);

            Assert.True(type.IsValidForTypeParameter(parameter.Object));
        }

        [Fact]
        public void ReturnsTrue_When_HasNullableReferenceTypeConstraint_And_DynamicIsNullable()
        {
            NullableTypeSyntax id = GetNode<NullableTypeSyntax>("class Parent { void Method() { dynamic? test = 2; } }")!;
            SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(id.SyntaxTree);
            TypeInfo info = semanticModel.GetTypeInfo(id);

            Mock<ITypeParameterSymbol> parameter = new();
            parameter.SetupGet(p => p.Name).Returns("T");
            parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
            parameter.SetupGet(p => p.HasReferenceTypeConstraint).Returns(true);
            parameter.SetupGet(p => p.ReferenceTypeConstraintNullableAnnotation).Returns(NullableAnnotation.Annotated);

            Assert.True((info.Type as IDynamicTypeSymbol)!.IsValidForTypeParameter(parameter.Object));
        }

        [Fact]
        public void ReturnsTrue_When_HasReferenceConstraint()
        {
            IDynamicTypeSymbol type = GetSymbol();
            Mock<ITypeParameterSymbol> parameter = new();
            parameter.SetupGet(p => p.Name).Returns("T");
            parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
            parameter.SetupGet(p => p.HasReferenceTypeConstraint).Returns(true);

            Assert.True(type.IsValidForTypeParameter(parameter.Object));
        }

        [Fact]
        public void ThrowsArgumentNullException_When_DynamicTypelIsNull()
        {
            IDynamicTypeSymbol type = null!;
            Mock<ITypeParameterSymbol> parameter = new();
            parameter.SetupGet(p => p.Name).Returns("T");
            parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());

            Assert.Throws<ArgumentNullException>(() => type.IsValidForTypeParameter(parameter.Object));
        }

        [Fact]
        public void ThrowsArgumentNullException_When_ParameterIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => GetSymbol().IsValidForTypeParameter(null!));
        }

        private IDynamicTypeSymbol GetSymbol()
        {
            return (Compilation.CurrentCompilation.DynamicType as IDynamicTypeSymbol)!;
        }
    }
}