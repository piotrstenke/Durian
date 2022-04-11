// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Analysis.Tests.FieldData
{
	public sealed class Constructors : CompilationTest
	{
		[Fact]
		public void CanHandleDeclarationWithMultipleFields()
		{
			FieldDeclarationSyntax field = GetNode<FieldDeclarationSyntax>("class Test { int field1, field2; }")!;
			Data.FieldData data = new(field, Compilation, 1);

			Assert.True(data.Symbol is not null && data.Declaration is not null);
		}

		[Fact]
		public void CanHandleDeclarationWithSingleField()
		{
			FieldDeclarationSyntax field = GetNode<FieldDeclarationSyntax>("class Test { int field; }")!;
			Data.FieldData data = new(field, Compilation);

			Assert.True(data.Symbol is not null && data.Declaration is not null);
		}

		[Fact]
		public void IndexIsSetThroughConstructor()
		{
			FieldDeclarationSyntax field = GetNode<FieldDeclarationSyntax>("class Test { int field1, field2; }")!;
			Data.FieldData data = new(field, Compilation, 1);

			Assert.True(data.Index == 1);
		}

		[Fact]
		public void InternalConstructorSetsAllData()
		{
			FieldDeclarationSyntax field = GetNode<FieldDeclarationSyntax>("class Test { int field1, field2; }")!;
			const int index = 1;
			VariableDeclaratorSyntax decl = field.Declaration.Variables[index];
			SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(field.SyntaxTree);
			IFieldSymbol symbol = (semanticModel.GetDeclaredSymbol(decl) as IFieldSymbol)!;
			Data.FieldData data = new(symbol, Compilation);
			Location location = field.GetLocation();

			Assert.True(
				data.SemanticModel is not null &&
				data.SemanticModel.SyntaxTree.IsEquivalentTo(semanticModel.SyntaxTree) &&
				data.Symbol is not null &&
				SymbolEqualityComparer.Default.Equals(data.Symbol, symbol) &&
				data.Location is not null &&
				data.Location == location &&
				data.Declaration is not null &&
				data.Declaration.IsEquivalentTo(field) &&
				data.ParentCompilation is not null &&
				data.ParentCompilation == Compilation &&
				data.Variable is not null &&
				data.Variable.IsEquivalentTo(decl) &&
				data.Index == index
			);
		}

		[Fact]
		public void ThrowsIndexOutOfRangeException_When_IndexIsNotWithinTheRangeOfDeclaredFields()
		{
			FieldDeclarationSyntax field = GetNode<FieldDeclarationSyntax>("class Test { int field; }")!;
			Assert.Throws<IndexOutOfRangeException>(() => new Data.FieldData(field, Compilation, 1));
		}

		[Fact]
		public void VariableIsSetThroughConstructor()
		{
			FieldDeclarationSyntax field = GetNode<FieldDeclarationSyntax>("class Test { int field1; }")!;
			Data.FieldData data = new(field, Compilation);

			Assert.True(data.Variable is not null && data.Variable.IsEquivalentTo(field.Declaration.Variables[0]));
		}

		[Fact]
		public void VariableIsSetThroughConstructor_When_CalledConstructorWithIndex()
		{
			FieldDeclarationSyntax field = GetNode<FieldDeclarationSyntax>("class Test { int field1, field2; }")!;
			Data.FieldData data = new(field, Compilation, 1);

			Assert.True(data.Variable is not null && data.Variable.IsEquivalentTo(field.Declaration.Variables[1]));
		}
	}
}
