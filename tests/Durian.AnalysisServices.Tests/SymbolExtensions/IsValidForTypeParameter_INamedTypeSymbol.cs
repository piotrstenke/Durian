// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;
using Durian.Analysis.Extensions;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;
using Xunit;

namespace Durian.Analysis.Tests.SymbolExtensions
{
	public sealed class IsValidForTypeParameter_INamedTypeSymbol : CompilationTest
	{
		[Fact]
		public void ReturnsFalse_When_DoesNotInheritFromConstrainedType()
		{
			INamedTypeSymbol type = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("class Test { }");
			Mock<ITypeParameterSymbol> parameter = new();

			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>(Compilation.CurrentCompilation.GetTypeByMetadataName("System.Collections.IList")!));

			Assert.False(type.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsFalse_When_HasNewConstraint_And_IsReferenceType_And_HasNonPublicParameterlessConstructor()
		{
			INamedTypeSymbol type = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("class Test { private Test() { } }");
			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
			parameter.SetupGet(p => p.HasConstructorConstraint).Returns(true);

			Assert.False(type.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsFalse_When_HasNewConstraint_And_IsReferenceType_And_HasNoParameterlessConstructor()
		{
			INamedTypeSymbol type = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("class Test { public Test(int value) { } }");
			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
			parameter.SetupGet(p => p.HasConstructorConstraint).Returns(true);

			Assert.False(type.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsFalse_When_HasReferenceConstraint_And_IsNotReferenceType()
		{
			INamedTypeSymbol type = GetSymbol<INamedTypeSymbol, StructDeclarationSyntax>("struct Test { }");
			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
			parameter.SetupGet(p => p.HasReferenceTypeConstraint).Returns(true);

			Assert.False(type.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsFalse_When_HasStructConstraint_And_IsNotStruct()
		{
			INamedTypeSymbol type = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("class Test { }");
			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
			parameter.SetupGet(p => p.HasValueTypeConstraint).Returns(true);

			Assert.False(type.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsFalse_When_HasUnmanagedConstraint_And_IsNotUnmanagedType()
		{
			INamedTypeSymbol type = GetSymbol<INamedTypeSymbol, StructDeclarationSyntax>("struct Test { string value; }");
			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
			parameter.SetupGet(p => p.HasUnmanagedTypeConstraint).Returns(true);

			Assert.False(type.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsTrue_When_HasNewConstraint_And_IsReferenceType_And_HasPublicParameterlessConstructor()
		{
			INamedTypeSymbol type = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("class Test { }");
			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
			parameter.SetupGet(p => p.HasConstructorConstraint).Returns(true);

			Assert.True(type.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsTrue_When_HasNewConstraint_And_IsStruct()
		{
			INamedTypeSymbol type = GetSymbol<INamedTypeSymbol, StructDeclarationSyntax>("struct Test { }");
			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
			parameter.SetupGet(p => p.HasConstructorConstraint).Returns(true);

			Assert.True(type.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsTrue_When_HasNoConstraints()
		{
			INamedTypeSymbol type = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("class Test { }");
			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());

			Assert.True(type.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsTrue_When_HasNotNullConstraint_And_TypeIsNotNullable()
		{
			INamedTypeSymbol type = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("class Test { }");
			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
			parameter.SetupGet(p => p.HasNotNullConstraint).Returns(true);

			Assert.True(type.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsTrue_When_HasNotNullConstraint_And_TypeIsNullable()
		{
			Compilation.UpdateCompilation("class Test { }");
			NullableTypeSyntax id = GetNode<NullableTypeSyntax>("class Parent { void Method() { Test? test = new(); } }");
			SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(id.SyntaxTree);
			TypeInfo info = semanticModel.GetTypeInfo(id);

			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
			parameter.SetupGet(p => p.HasNotNullConstraint).Returns(true);

			Assert.True((info.Type as INamedTypeSymbol)!.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsTrue_When_HasNullableReferenceTypeConstraint_And_TypeIsNotNullable()
		{
			INamedTypeSymbol type = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("class Test { }");
			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
			parameter.SetupGet(p => p.HasReferenceTypeConstraint).Returns(true);
			parameter.SetupGet(p => p.ReferenceTypeConstraintNullableAnnotation).Returns(NullableAnnotation.Annotated);

			Assert.True(type.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsTrue_When_HasNullableReferenceTypeConstraint_And_TypeIsNullable()
		{
			Compilation.UpdateCompilation("class Test { }");
			NullableTypeSyntax id = GetNode<NullableTypeSyntax>("class Parent { void Method() { Test? test = new(); } }");
			SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(id.SyntaxTree);
			TypeInfo info = semanticModel.GetTypeInfo(id);

			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
			parameter.SetupGet(p => p.HasReferenceTypeConstraint).Returns(true);
			parameter.SetupGet(p => p.ReferenceTypeConstraintNullableAnnotation).Returns(NullableAnnotation.Annotated);

			Assert.True((info.Type as INamedTypeSymbol)!.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsTrue_When_HasReferenceConstraint_And_IsReferenceType()
		{
			INamedTypeSymbol type = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("class Test { }");
			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
			parameter.SetupGet(p => p.HasReferenceTypeConstraint).Returns(true);

			Assert.True(type.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsTrue_When_HasStructConstraint_And_IsStruct()
		{
			INamedTypeSymbol type = GetSymbol<INamedTypeSymbol, StructDeclarationSyntax>("struct Test { }");
			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
			parameter.SetupGet(p => p.HasValueTypeConstraint).Returns(true);

			Assert.True(type.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsTrue_When_HasUnmanagedConstraint_And_IsUnmanagedType()
		{
			INamedTypeSymbol type = GetSymbol<INamedTypeSymbol, StructDeclarationSyntax>("struct Test { int value; }");
			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
			parameter.SetupGet(p => p.HasUnmanagedTypeConstraint).Returns(true);

			Assert.True(type.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsTrue_When_InheritsFromConstrainedType()
		{
			INamedTypeSymbol type = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("class Test : System.Collections.IList { }");
			Mock<ITypeParameterSymbol> parameter = new();

			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>(Compilation.CurrentCompilation.GetTypeByMetadataName("System.Collections.IList")!));

			Assert.True(type.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsTrue_When_InheritsFromConstrainedType_And_ParameterIsConstrainedToNullable()
		{
			INamedTypeSymbol type = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("class Test : System.Collections.IList { }");
			Mock<ITypeParameterSymbol> parameter = new();

			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>(Compilation.CurrentCompilation.GetTypeByMetadataName("System.Collections.IList")!));
			parameter.SetupGet(p => p.ConstraintNullableAnnotations).Returns(ImmutableArray.Create(NullableAnnotation.Annotated));

			Assert.True(type.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_ParameterIsNull()
		{
			INamedTypeSymbol type = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("class Test { }");

			Assert.Throws<ArgumentNullException>(() => type.IsValidForTypeParameter(null!));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_TypelIsNull()
		{
			INamedTypeSymbol type = null!;
			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());

			Assert.Throws<ArgumentNullException>(() => type.IsValidForTypeParameter(parameter.Object));
		}
	}
}
