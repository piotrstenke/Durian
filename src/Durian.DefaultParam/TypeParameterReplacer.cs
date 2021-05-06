using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.DefaultParam
{
	public class TypeParameterReplacer : CSharpSyntaxRewriter
	{
		public ITypeParameterSymbol? ParameterToReplace { get; set; }
		public IdentifierNameSyntax? Replacement { get; set; }
		public List<ISymbol?> InputSymbols { get; set; }
		public int Count => InputSymbols.Count;
		public bool HasChangedConstraints => ChangedConstraintIndices.Count > 0;
		public bool VisitDeclarationBody { get; set; } = true;
		public List<int> ChangedConstraintIndices { get; }

		private int _identifierCounter;
		private int _constraintCounter;
		private bool _skip;

		public TypeParameterReplacer(List<ISymbol?> inputSymbols)
		{
			InputSymbols = inputSymbols;
			ChangedConstraintIndices = new();
		}

		public TypeParameterReplacer(IEnumerable<ISymbol> inputSymbols)
		{
			InputSymbols = inputSymbols?.ToList() ?? new();
			ChangedConstraintIndices = new();
		}

		public override SyntaxNode? VisitBlock(BlockSyntax node)
		{
			if (VisitDeclarationBody || node.Parent is not MethodDeclarationSyntax)
			{
				return base.VisitBlock(node);
			}
			else
			{
				_skip = true;
				SyntaxNode? n = base.VisitBlock(node);
				_skip = false;
				return n;
			}
		}

		public override SyntaxNode? VisitArrowExpressionClause(ArrowExpressionClauseSyntax node)
		{
			if (VisitDeclarationBody || node.Parent is not MethodDeclarationSyntax)
			{
				return base.VisitArrowExpressionClause(node);
			}
			else
			{
				_skip = true;
				SyntaxNode? n = base.VisitArrowExpressionClause(node);
				_skip = false;
				return n;
			}
		}

		public override SyntaxNode? VisitTypeParameterConstraintClause(TypeParameterConstraintClauseSyntax node)
		{
			bool changed = false;
			TypeParameterConstraintClauseSyntax clause = node;
			SeparatedSyntaxList<TypeParameterConstraintSyntax> originalConstraints = clause.Constraints;
			int length = originalConstraints.Count;
			TypeParameterConstraintSyntax[] constraints = new TypeParameterConstraintSyntax[length];

			for (int i = 0; i < length; i++)
			{
				TypeParameterConstraintSyntax current = originalConstraints[i];
				SyntaxNode? n = base.Visit(current);

				if (n != current)
				{
					changed = true;
					constraints[i] = (TypeParameterConstraintSyntax)n;
				}
				else
				{
					constraints[i] = current;
				}
			}

			if (changed)
			{
				ChangedConstraintIndices.Add(_constraintCounter);
				clause = clause.WithConstraints(SyntaxFactory.SeparatedList(constraints));
			}

			_constraintCounter++;

			return clause;
		}

		public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
		{
			if (_skip || Count == 0)
			{
				return base.VisitIdentifierName(node);
			}

			if(_identifierCounter >= InputSymbols.Count)
			{
				// _identifierCounter and InputSymbols.Count on rare occasions get out of sync when the user is editing the method/type/delegate.
				return base.VisitIdentifierName(node);
			}

			ISymbol? symbol = InputSymbols[_identifierCounter];
			SyntaxNode? toReturn;

			if (symbol is null || !SymbolEqualityComparer.Default.Equals(symbol, ParameterToReplace))
			{
				toReturn = base.VisitIdentifierName(node);
			}
			else if (symbol is ITypeParameterSymbol)
			{
				if (Replacement is null)
				{
					toReturn = base.VisitIdentifierName(node);
				}
				else
				{
					toReturn = Replacement
						.WithLeadingTrivia(node.GetLeadingTrivia())
						.WithTrailingTrivia(node.GetTrailingTrivia());
				}

				// The symbol has been replaced, so to avoid future equality comparison it is set to null.
				InputSymbols[_identifierCounter] = null;
			}
			else if (symbol is IAliasSymbol a)
			{
				toReturn = ((UsingDirectiveSyntax)a.DeclaringSyntaxReferences[0].GetSyntax()).Name;

				// The same as with ITypeParameterSymbol.
				InputSymbols[_identifierCounter] = null;
			}
			else
			{
				toReturn = base.VisitIdentifierName(node);
			}

			_identifierCounter++;

			return toReturn;
		}

		public void Reset()
		{
			ParameterToReplace = null!;
			Replacement = null!;
			InputSymbols.Clear();
			_identifierCounter = 0;
			VisitDeclarationBody = true;
		}

		public void ResetCounter()
		{
			_identifierCounter = 0;
			_constraintCounter = 0;
			ChangedConstraintIndices.Clear();
		}
	}
}
