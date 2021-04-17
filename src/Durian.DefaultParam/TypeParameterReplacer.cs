using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.DefaultParam
{
	public class TypeParameterReplacer : CSharpSyntaxRewriter
	{
		public ITypeParameterSymbol? ParameterToReplace { get; set; }
		public IdentifierNameSyntax? Replacement { get; set; }
		public List<ISymbol?> InputSymbols { get; }
		public int Count => InputSymbols.Count;
		public bool HasChangedConstraints => ChangedConstraintIndices.Count > 0;
		public List<int> ChangedConstraintIndices { get; }

		private int _identifierCounter;
		private int _constraintCounter;

		public TypeParameterReplacer(List<ISymbol?> inputSymbols)
		{
			InputSymbols = inputSymbols;
			ChangedConstraintIndices = new List<int>();
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
			if (Count == 0)
			{
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
		}

		public void ResetCounter()
		{
			_identifierCounter = 0;
			_constraintCounter = 0;
			ChangedConstraintIndices.Clear();
		}
	}
}
