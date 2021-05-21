using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Tests.AnalysisServices.AnalysisUtilities
{
	public sealed class GetSymbolAndSemanticModel : CompilationTest
	{
		[Fact]
		public void ThrowsArgumentNullException_When_SyntaxNodeIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => Durian.Generator.AnalysisUtilities.GetSymbolAndSemanticModel(null!, Compilation));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_CompilationIsNull()
		{
			ClassDeclarationSyntax node = GetNode<ClassDeclarationSyntax>("class Test { }");
			Assert.Throws<ArgumentNullException>(() => Durian.Generator.AnalysisUtilities.GetSymbolAndSemanticModel(node, null!));
		}

		[Fact]
		public void ThrowsArgumentException_When_SyntaxNodeDoesNotRepresentAnySymbols()
		{
			BlockSyntax node = GetNode<BlockSyntax>("class Test { void Method() { } }");
			Assert.Throws<ArgumentException>(() => Durian.Generator.AnalysisUtilities.GetSymbolAndSemanticModel(node, Compilation));
		}

		[Fact]
		public void ReturnsSemanticModelAndSymbol_When_InputIsValid()
		{
			ClassDeclarationSyntax node = GetNode<ClassDeclarationSyntax>("class Test { }");
			SemanticModel oldSemanticModel = Compilation.CurrentCompilation.GetSemanticModel(node.SyntaxTree);
			ISymbol oldSymbol = oldSemanticModel.GetDeclaredSymbol(node)!;
			(SemanticModel newSemanticModel, ISymbol newSymbol) = Durian.Generator.AnalysisUtilities.GetSymbolAndSemanticModel(node, Compilation);

			Assert.True(
				newSemanticModel is not null &&
				newSemanticModel.SyntaxTree.IsEquivalentTo(oldSemanticModel.SyntaxTree) &&
				newSymbol is not null &&
				SymbolEqualityComparer.Default.Equals(newSymbol, oldSymbol)
			);
		}
	}
}
