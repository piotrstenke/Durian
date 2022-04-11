// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Analysis.Tests.EventData
{
	public sealed class Constructor_EventFieldDeclarationSyntax : CompilationTest
	{
		[Fact]
		public void AsFieldIsSet_And_AsPropertyIsNull_When_EventIsField()
		{
			EventFieldDeclarationSyntax field = GetNode<EventFieldDeclarationSyntax>("class Test { event System.Action e; }")!;
			Data.EventData data = new(field, Compilation);

			Assert.True(data.AsField is not null && data.AsField.IsEquivalentTo(field) && data.AsProperty is null);
		}

		[Fact]
		public void AsPropertyIsSet_And_AsFieldIsNull_When_EventIsProperty()
		{
			EventDeclarationSyntax decl = GetNode<EventDeclarationSyntax>("class Test { event System.Action e { add { } remove { } }")!;
			;
			Data.EventData data = new(decl, Compilation);

			Assert.True(data.AsProperty is not null && data.AsProperty.IsEquivalentTo(decl) && data.AsField is null);
		}

		[Fact]
		public void CanHandleDeclarationWithMultipleFields()
		{
			EventFieldDeclarationSyntax field = GetNode<EventFieldDeclarationSyntax>("class Test { event System.Action e1, e2; }")!;
			Data.EventData data = new(field, Compilation, 1);

			Assert.True(data.Symbol is not null && data.Declaration is not null);
		}

		[Fact]
		public void CanHandleDeclarationWithSingleField()
		{
			EventFieldDeclarationSyntax field = GetNode<EventFieldDeclarationSyntax>("class Test { event System.Action e; }")!;
			Data.EventData data = new(field, Compilation);

			Assert.True(data.Symbol is not null && data.Declaration is not null);
		}

		[Fact]
		public void IndexIs0_And_VariableIsNull_When_EventIsProperty()
		{
			EventDeclarationSyntax decl = GetNode<EventDeclarationSyntax>("class Test { event System.Action e { add { } remove { } } }")!;
			Data.EventData data = new(decl, Compilation);

			Assert.True(data.Index == 0 && data.Variable is null);
		}

		[Fact]
		public void IndexIsSetThroughConstructor_When_EventIsField()
		{
			EventFieldDeclarationSyntax field = GetNode<EventFieldDeclarationSyntax>("class Test { event System.Action e1, e2; }")!;
			Data.EventData data = new(field, Compilation, 1);

			Assert.True(data.Index == 1);
		}

		[Fact]
		public void InternalConstructorSetsAllNeededData_When_EventIsField()
		{
			EventFieldDeclarationSyntax decl = GetNode<EventFieldDeclarationSyntax>("class Test { event System.Action e1, e2 }")!;
			const int index = 1;
			VariableDeclaratorSyntax var = decl.Declaration.Variables[index];
			SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(decl.SyntaxTree);
			IEventSymbol symbol = (semanticModel.GetDeclaredSymbol(var) as IEventSymbol)!;
			Data.EventData data = new(symbol, Compilation);
			Location location = decl.GetLocation();

			Assert.True(
				data.SemanticModel is not null &&
				data.SemanticModel.SyntaxTree.IsEquivalentTo(semanticModel.SyntaxTree) &&
				data.Symbol is not null &&
				SymbolEqualityComparer.Default.Equals(data.Symbol, symbol) &&
				data.Location is not null &&
				data.Location == location &&
				data.Declaration is not null &&
				data.Declaration.IsEquivalentTo(decl) &&
				data.ParentCompilation is not null &&
				data.ParentCompilation == Compilation &&
				data.AsProperty is null &&
				data.AsField is not null &&
				data.AsField.IsEquivalentTo(decl) &&
				data.Variable is not null &&
				data.Variable.IsEquivalentTo(var) &&
				data.Index == index
			);
		}

		[Fact]
		public void InternalConstructorSetsAllNeededData_When_EventIsProperty()
		{
			EventDeclarationSyntax decl = GetNode<EventDeclarationSyntax>("class Test { event System.Action e { add { } remove { } }")!;
			SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(decl.SyntaxTree);
			IEventSymbol symbol = semanticModel.GetDeclaredSymbol(decl)!;
			Data.EventData data = new(symbol, Compilation);
			Location location = decl.GetLocation();

			Assert.True(
				data.SemanticModel is not null &&
				data.SemanticModel.SyntaxTree.IsEquivalentTo(semanticModel.SyntaxTree) &&
				data.Symbol is not null &&
				SymbolEqualityComparer.Default.Equals(data.Symbol, symbol) &&
				data.Location is not null &&
				data.Location == location &&
				data.Declaration is not null &&
				data.Declaration.IsEquivalentTo(decl) &&
				data.ParentCompilation is not null &&
				data.ParentCompilation == Compilation &&
				data.AsProperty is not null &&
				data.AsProperty.IsEquivalentTo(decl) &&
				data.AsField is null &&
				data.Variable is null &&
				data.Index == 0
			);
		}

		[Fact]
		public void ThrowsIndexOutOfRangeException_When_IndexIsNotWithinTheRangeOfDeclaredFields()
		{
			EventFieldDeclarationSyntax field = GetNode<EventFieldDeclarationSyntax>("class Test { event System.Action e; }")!;
			Assert.Throws<IndexOutOfRangeException>(() => new Data.EventData(field, Compilation, 1));
		}

		[Fact]
		public void VariableIsSetThroughConstructor_When_EventIsField()
		{
			EventFieldDeclarationSyntax field = GetNode<EventFieldDeclarationSyntax>("class Test { event System.Action e; }")!;
			Data.EventData data = new(field, Compilation);

			Assert.True(data.Variable is not null && data.Variable.IsEquivalentTo(field.Declaration.Variables[0]));
		}

		[Fact]
		public void VariableIsSetThroughConstructor_When_EventIsField_And_CalledConstructorWithIndex()
		{
			EventFieldDeclarationSyntax field = GetNode<EventFieldDeclarationSyntax>("class Test { event System.Action e1, e2; }")!;
			Data.EventData data = new(field, Compilation, 1);

			Assert.True(data.Variable is not null && data.Variable.IsEquivalentTo(field.Declaration.Variables[1]));
		}
	}
}
