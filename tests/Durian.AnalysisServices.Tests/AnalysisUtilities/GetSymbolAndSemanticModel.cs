// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using Xunit;

namespace Durian.Analysis.Tests.AnalysisUtilities
{
	public sealed class GetSymbolAndSemanticModel : CompilationTest
	{
		[Fact]
		public void ReturnsSemanticModelAndSymbol_When_InputIsValid()
		{
			ClassDeclarationSyntax node = GetNode<ClassDeclarationSyntax>("class Test { }")!;
			SemanticModel oldSemanticModel = Compilation.CurrentCompilation.GetSemanticModel(node.SyntaxTree);
			ISymbol oldSymbol = oldSemanticModel.GetDeclaredSymbol(node)!;
			(SemanticModel newSemanticModel, ISymbol newSymbol) = Analysis.AnalysisUtilities.GetSymbolAndSemanticModel(node, Compilation);

			Assert.True(
				newSemanticModel is not null &&
				newSemanticModel.SyntaxTree.IsEquivalentTo(oldSemanticModel.SyntaxTree) &&
				newSymbol is not null &&
				SymbolEqualityComparer.Default.Equals(newSymbol, oldSymbol)
			);
		}

		[Fact]
		public void ThrowsArgumentException_When_SyntaxNodeDoesNotRepresentAnySymbols()
		{
			BlockSyntax node = GetNode<BlockSyntax>("class Test { void Method() { } }")!;
			Assert.Throws<ArgumentException>(() => Analysis.AnalysisUtilities.GetSymbolAndSemanticModel(node, Compilation));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_CompilationIsNull()
		{
			ClassDeclarationSyntax node = GetNode<ClassDeclarationSyntax>("class Test { }")!;
			Assert.Throws<ArgumentNullException>(() => Analysis.AnalysisUtilities.GetSymbolAndSemanticModel(node, null!));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SyntaxNodeIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => Analysis.AnalysisUtilities.GetSymbolAndSemanticModel(null!, Compilation));
		}
	}
}