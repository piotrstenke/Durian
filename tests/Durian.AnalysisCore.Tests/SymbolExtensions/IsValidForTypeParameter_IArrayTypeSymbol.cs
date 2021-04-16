using System;
using System.Collections.Immutable;
using Durian.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;
using Xunit;

namespace Durian.Tests.CorePackage.SymbolExtensions
{
	public sealed class IsValidForTypeParameter_IArrayTypeSymbol : CompilationTest
	{
		[Fact]
		public void ThrowsArgumentNullException_When_ArrayTypelIsNull()
		{
			IArrayTypeSymbol type = null!;
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

		[Fact]
		public void ReturnsTrue_When_HasNoConstraints()
		{
			IArrayTypeSymbol type = GetSymbol();
			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());

			Assert.True(type.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsTrue_When_HasReferenceConstraint()
		{
			IArrayTypeSymbol type = GetSymbol();
			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
			parameter.SetupGet(p => p.HasReferenceTypeConstraint).Returns(true);

			Assert.True(type.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsFalse_When_HasUnmanagedConstraint()
		{
			IArrayTypeSymbol type = GetSymbol();
			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
			parameter.SetupGet(p => p.HasUnmanagedTypeConstraint).Returns(true);

			Assert.False(type.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsFalse_When_HasStructConstraint()
		{
			IArrayTypeSymbol type = GetSymbol();
			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
			parameter.SetupGet(p => p.HasValueTypeConstraint).Returns(true);

			Assert.False(type.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsFalse_When_HasNewConstraint()
		{
			IArrayTypeSymbol type = GetSymbol();
			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
			parameter.SetupGet(p => p.HasConstructorConstraint).Returns(true);

			Assert.False(type.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsTrue_When_HasNotNullConstraint()
		{
			IArrayTypeSymbol type = GetSymbol();
			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
			parameter.SetupGet(p => p.HasNotNullConstraint).Returns(true);

			Assert.True(type.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsTrue_When_HasNotNullConstraint_And_ArrayIsNullable()
		{
			NullableTypeSyntax id = GetNode<NullableTypeSyntax>("class Parent { void Method() { int[]? test = new { 1, 2 }); } }");
			SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(id.SyntaxTree);
			TypeInfo info = semanticModel.GetTypeInfo(id);

			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
			parameter.SetupGet(p => p.HasNotNullConstraint).Returns(true);

			Assert.True((info.Type as IArrayTypeSymbol)!.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsTrue_When_HasNullableReferenceTypeConstraint()
		{
			IArrayTypeSymbol type = GetSymbol();
			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
			parameter.SetupGet(p => p.HasReferenceTypeConstraint).Returns(true);
			parameter.SetupGet(p => p.ReferenceTypeConstraintNullableAnnotation).Returns(NullableAnnotation.Annotated);

			Assert.True(type.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsTrue_When_HasNullableReferenceTypeConstraint_And_ArrayIsNullable()
		{
			NullableTypeSyntax id = GetNode<NullableTypeSyntax>("class Parent { void Method() { int[]? test = { 0, 1 }; } }");
			SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(id.SyntaxTree);
			TypeInfo info = semanticModel.GetTypeInfo(id);

			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>());
			parameter.SetupGet(p => p.HasReferenceTypeConstraint).Returns(true);
			parameter.SetupGet(p => p.ReferenceTypeConstraintNullableAnnotation).Returns(NullableAnnotation.Annotated);

			Assert.True((info.Type as IArrayTypeSymbol)!.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsTrue_When_InheritsFromConstrainedType()
		{
			IArrayTypeSymbol type = GetSymbol();
			Mock<ITypeParameterSymbol> parameter = new();

			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>(Compilation.CurrentCompilation.GetTypeByMetadataName("System.Collections.IList")!));

			Assert.True(type.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsFalse_When_DoesNotInheritFromConstrainedType()
		{
			IArrayTypeSymbol type = GetSymbol();
			Mock<ITypeParameterSymbol> parameter = new();

			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>(Compilation.CurrentCompilation.GetTypeByMetadataName("System.Collections.ArrayList")!));

			Assert.False(type.IsValidForTypeParameter(parameter.Object));
		}

		[Fact]
		public void ReturnsTrue_When_InheritFromConstraintType_And_ParameterIsConstrainedToNullable()
		{
			IArrayTypeSymbol type = GetSymbol();

			Mock<ITypeParameterSymbol> parameter = new();
			parameter.SetupGet(p => p.Name).Returns("T");
			parameter.SetupGet(p => p.ConstraintTypes).Returns(ImmutableArray.Create<ITypeSymbol>(Compilation.CurrentCompilation.GetTypeByMetadataName("System.Collections.IList")!));
			parameter.SetupGet(p => p.ConstraintNullableAnnotations).Returns(ImmutableArray.Create(NullableAnnotation.Annotated));

			Assert.True(type.IsValidForTypeParameter(parameter.Object));
		}

		private IArrayTypeSymbol GetSymbol()
		{
			return GetSymbol<IArrayTypeSymbol, ArrayTypeSyntax>("class Test { void Method(int[] a) { } }");
		}
	}
}
