// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// Collects all references to a <see cref="ITypeParameterSymbol"/> in order they appear in the visited <see cref="CSharpSyntaxNode"/>.
	/// </summary>
	public class TypeParameterIdentifierCollector : CSharpSyntaxWalker
	{
		/// <summary>
		/// A <see cref="List{T}"/> of collected <see cref="ITypeParameterSymbol"/>s or <see cref="IAliasSymbol"/>s of <see cref="ITypeParameterSymbol"/>s in order they appear.
		/// </summary>
		public List<ISymbol?> OutputSymbols { get; }

		/// <summary>
		/// Parent <see cref="DefaultParamCompilationData"/> of the input <see cref="CSharpSyntaxNode"/>.
		/// </summary>
		public DefaultParamCompilationData? ParentCompilation { get; set; }

		/// <summary>
		/// <see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the input <see cref="CSharpSyntaxNode"/>.
		/// </summary>
		public SemanticModel? SemanticModel { get; set; }

		/// <summary>
		/// Determines whether to visit the declaration body of a <see cref="MethodDeclarationSyntax"/>. Defaults to <see langword="true"/>.
		/// </summary>
		public bool VisitDeclarationBody { get; set; } = true;

		/// <inheritdoc cref="TypeParameterIdentifierCollector(DefaultParamCompilationData, SemanticModel)"/>
		public TypeParameterIdentifierCollector()
		{
			OutputSymbols = new List<ISymbol?>();
		}

		/// <inheritdoc cref="TypeParameterIdentifierCollector(DefaultParamCompilationData, SemanticModel)"/>
		public TypeParameterIdentifierCollector(DefaultParamCompilationData? compilation)
		{
			OutputSymbols = new List<ISymbol?>();
			ParentCompilation = compilation;
		}

		/// <inheritdoc cref="TypeParameterIdentifierCollector(DefaultParamCompilationData, SemanticModel)"/>
		public TypeParameterIdentifierCollector(SemanticModel? semanticModel)
		{
			OutputSymbols = new List<ISymbol?>();
			SemanticModel = semanticModel;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeParameterIdentifierCollector"/> class.
		/// </summary>
		/// <param name="compilation">Parent <see cref="DefaultParamCompilationData"/> of the input <see cref="CSharpSyntaxNode"/>.</param>
		/// <param name="semanticModel"><see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the input <see cref="CSharpSyntaxNode"/>.</param>
		public TypeParameterIdentifierCollector(DefaultParamCompilationData? compilation, SemanticModel? semanticModel)
		{
			SemanticModel = semanticModel;
			ParentCompilation = compilation;
			OutputSymbols = new List<ISymbol?>();
		}

		/// <summary>
		/// Resets the collector.
		/// </summary>
		public void Reset()
		{
			OutputSymbols.Clear();
		}

		/// <inheritdoc/>
		public override void VisitArrowExpressionClause(ArrowExpressionClauseSyntax node)
		{
			if (VisitDeclarationBody || node.Parent is not MethodDeclarationSyntax)
			{
				base.VisitArrowExpressionClause(node);
			}
		}

		/// <inheritdoc/>
		public override void VisitAttribute(AttributeSyntax node)
		{
			ISymbol? symbol = SemanticModel.GetSymbolInfo(node).Symbol?.ContainingType;

			// The DefaultParam attributes will be removed later, so they don't need to be stored.
			if (SymbolEqualityComparer.Default.Equals(symbol, ParentCompilation?.DefaultParamAttribute) || SymbolEqualityComparer.Default.Equals(symbol, ParentCompilation?.DefaultParamConfigurationAttribute))
			{
				return;
			}

			base.VisitAttribute(node);
		}

		/// <inheritdoc/>
		public override void VisitBlock(BlockSyntax node)
		{
			if (VisitDeclarationBody || node.Parent is not MethodDeclarationSyntax)
			{
				base.VisitBlock(node);
			}
		}

		/// <inheritdoc/>
		public override void VisitIdentifierName(IdentifierNameSyntax node)
		{
			ISymbol? symbol = SemanticModel.GetSymbolInfo(node).Symbol;
			OutputSymbols.Add(symbol is ITypeParameterSymbol or IAliasSymbol ? symbol : null);
		}

		/// <inheritdoc/>
		public override void VisitTypeParameterConstraintClause(TypeParameterConstraintClauseSyntax node)
		{
			foreach (TypeParameterConstraintSyntax constraint in node.Constraints)
			{
				base.Visit(constraint);
			}
		}
	}
}