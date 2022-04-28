// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
	}
}
