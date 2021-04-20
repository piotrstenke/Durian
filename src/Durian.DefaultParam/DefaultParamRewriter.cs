using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.DefaultParam
{
	public class DefaultParamRewriter
	{
		private readonly List<TypeParameterConstraintClauseSyntax> _includedConstraints;
		private readonly List<ITypeParameterSymbol> _includedConstraintSymbols;
		private readonly TypeParameterReplacer _replacer;
		private readonly TypeParameterIdentifierCollector _collector;
		private int _numParameters;
		private int _numConstraints;

		public CSharpSyntaxNode CurrentNode => DeclarationBuilder.CurrentNode;
		public CSharpSyntaxNode OriginalNode => DeclarationBuilder.OriginalNode;
		public SemanticModel SemanticModel => DeclarationBuilder.SemanticModel;
		public IDefaultParamDeclarationBuilder DeclarationBuilder { get; private set; }
		public DefaultParamCompilationData? ParentCompilation
		{
			get => _collector.ParentCompilation;
			set => _collector.ParentCompilation = value;
		}

		public DefaultParamRewriter() : this(null, null)
		{
		}

		public DefaultParamRewriter(DefaultParamCompilationData? compilation) : this(compilation, null)
		{
		}

		public DefaultParamRewriter(DefaultParamCompilationData? compilation, IDefaultParamDeclarationBuilder? wrapper)
		{
			_includedConstraints = new List<TypeParameterConstraintClauseSyntax>();
			_includedConstraintSymbols = new List<ITypeParameterSymbol>();
			DeclarationBuilder = wrapper!;
			_collector = new(compilation);
			_replacer = new(_collector.OutputSymbols);
		}

		public void Reset()
		{
			DeclarationBuilder = null!;
			_numParameters = 0;
			_includedConstraints.Clear();
			_includedConstraintSymbols.Clear();
			_numConstraints = 0;
			_replacer.Reset();
			// The collector.Reset() method only clears the list of symbols, and replacer.Reset() already does that
			//_collector.Reset();
		}

		public void Emplace(CSharpSyntaxNode node)
		{
			DeclarationBuilder.Emplace(node);
		}

		public void Acquire(IDefaultParamDeclarationBuilder declBuilder)
		{
			_includedConstraints.Clear();
			_includedConstraintSymbols.Clear();

			DeclarationBuilder = declBuilder;
			_numParameters = declBuilder.GetOriginalTypeParameterCount();

			SyntaxList<TypeParameterConstraintClauseSyntax> constraints = declBuilder.GetOriginalConstraintClauses();
			_numConstraints = constraints.Count;

			for (int i = 0; i < _numConstraints; i++)
			{
				TypeParameterConstraintClauseSyntax constraint = constraints[i];
				ISymbol? symbol = SemanticModel.GetSymbolInfo(constraint.Name).Symbol;

				_includedConstraints.Add(constraint);
				_includedConstraintSymbols.Add((symbol as ITypeParameterSymbol)!);
			}

			_replacer.Reset();
			_replacer.VisitDeclarationBody = declBuilder.VisitDeclarationBody;
			_collector.VisitDeclarationBody = declBuilder.VisitDeclarationBody;
			_collector.SemanticModel = declBuilder.SemanticModel;
			_collector.Visit(declBuilder.OriginalNode);
		}

		public void ReplaceType(ITypeParameterSymbol oldType, string newType)
		{
			_replacer.ParameterToReplace = oldType;
			_replacer.Replacement = SyntaxFactory.IdentifierName(newType ?? string.Empty);
			_replacer.ResetCounter();

			DeclarationBuilder.AcceptTypeParameterReplacer(_replacer);

			if (_replacer.HasChangedConstraints)
			{
				int length = _replacer.ChangedConstraintIndices.Count;

				for (int i = 0; i < length; i++)
				{
					_includedConstraints[i] = DeclarationBuilder.GetCurrentConstraintClause(_replacer.ChangedConstraintIndices[i]);
				}
			}
		}

		public void RemoveConstraintsOf(ITypeParameterSymbol symbol)
		{
			if (_numConstraints < 1)
			{
				return;
			}

			int length = _includedConstraints.Count;

			for (int i = 0; i < length; i++)
			{
				if (SymbolEqualityComparer.Default.Equals(_includedConstraintSymbols[i], symbol))
				{
					_includedConstraints.RemoveAt(i);
					_includedConstraintSymbols.RemoveAt(i);
					break;
				}
			}

			DeclarationBuilder.WithConstraintClauses(_includedConstraints);
		}

		public void RemoveLastParameter()
		{
			if (_numParameters < 0)
			{
				return;
			}

			_numParameters--;
			DeclarationBuilder.WithTypeParameters(_numParameters);
		}
	}
}
