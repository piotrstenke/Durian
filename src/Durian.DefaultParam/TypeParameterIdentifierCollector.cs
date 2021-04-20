using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.DefaultParam
{
	public class TypeParameterIdentifierCollector : CSharpSyntaxWalker
	{
		public List<ISymbol?> OutputSymbols { get; }
		public SemanticModel? SemanticModel { get; set; }
		public DefaultParamCompilationData? ParentCompilation { get; set; }
		public bool VisitDeclarationBody { get; set; } = true;

		public TypeParameterIdentifierCollector()
		{
			OutputSymbols = new List<ISymbol?>();
		}

		public TypeParameterIdentifierCollector(DefaultParamCompilationData? compilation)
		{
			OutputSymbols = new List<ISymbol?>();
			ParentCompilation = compilation;
		}

		public TypeParameterIdentifierCollector(SemanticModel? semanticModel)
		{
			OutputSymbols = new List<ISymbol?>();
			SemanticModel = semanticModel;
		}

		public TypeParameterIdentifierCollector(DefaultParamCompilationData? compilation, SemanticModel? semanticModel)
		{
			SemanticModel = semanticModel;
			ParentCompilation = compilation;
			OutputSymbols = new List<ISymbol?>();
		}

		public override void VisitBlock(BlockSyntax node)
		{
			if (VisitDeclarationBody || node.Parent is not MethodDeclarationSyntax)
			{
				base.VisitBlock(node);
			}
		}

		public override void VisitArrowExpressionClause(ArrowExpressionClauseSyntax node)
		{
			if (VisitDeclarationBody || node.Parent is not MethodDeclarationSyntax)
			{
				base.VisitArrowExpressionClause(node);
			}
		}

		public override void VisitTypeParameterConstraintClause(TypeParameterConstraintClauseSyntax node)
		{
			foreach (TypeParameterConstraintSyntax constraint in node.Constraints)
			{
				base.Visit(constraint);
			}
		}

		public override void VisitAttribute(AttributeSyntax node)
		{
			ISymbol? symbol = SemanticModel.GetSymbolInfo(node).Symbol;

			// The DefaultParam attributes will be removed later, so they don't need to be stored.
			if (SymbolEqualityComparer.Default.Equals(symbol, ParentCompilation?.AttributeConstructor) || SymbolEqualityComparer.Default.Equals(symbol, ParentCompilation?.MethodConfigurationConstructor))
			{
				return;
			}

			base.VisitAttribute(node);
		}

		public override void VisitIdentifierName(IdentifierNameSyntax node)
		{
			ISymbol? symbol = SemanticModel.GetSymbolInfo(node).Symbol;
			OutputSymbols.Add(symbol is ITypeParameterSymbol or IAliasSymbol ? symbol : null);
		}

		public void Reset()
		{
			OutputSymbols.Clear();
		}
	}
}
