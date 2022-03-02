// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Durian.Analysis.DefaultParam
{
    /// <summary>
    /// Replaces <see cref="CSharpSyntaxNode"/>s representing a specified <see cref="ITypeParameterSymbol"/>.
    /// </summary>
    public class TypeParameterReplacer : CSharpSyntaxRewriter
    {
        private int _constraintCounter;

        private int _identifierCounter;

        private bool _skip;

        /// <summary>
        /// A <see cref="List{T}"/> that contains the indexes of <see cref="TypeParameterConstraintClauseSyntax"/> that were modified during last visit.
        /// </summary>
        public List<int> ChangedConstraintIndices { get; }

        /// <summary>
        /// Number of <see cref="ISymbol"/> in the <see cref="List{T}"/> of <see cref="InputSymbols"/>.
        /// </summary>
        public int Count => InputSymbols.Count;

        /// <summary>
        /// Determines whether any <see cref="TypeParameterConstraintClauseSyntax"/> was modified during last visit.
        /// </summary>
        public bool HasModifiedConstraints => ChangedConstraintIndices.Count > 0;

        /// <summary>
        /// A <see cref="List{T}"/> that contains <see cref="ISymbol"/>s in order they appear in the visited <see cref="CSharpSyntaxNode"/>.
        /// </summary>
        /// <remarks>Best practice is to use the <see cref="TypeParameterIdentifierCollector"/> and pass its <see cref="TypeParameterIdentifierCollector.OutputSymbols"/> here.</remarks>
        public List<ISymbol?> InputSymbols { get; set; }

        /// <summary>
        /// <see cref="ITypeSymbol"/> that replaces the <see cref="ParameterToReplace"/>.
        /// </summary>
        public ITypeSymbol? NewType { get; set; }

        /// <summary>
        /// <see cref="ITypeParameterSymbol"/> that should be replaced.
        /// </summary>
        public ITypeParameterSymbol? ParameterToReplace { get; set; }

        /// <summary>
        /// <see cref="IdentifierNameSyntax"/> to replace the <see cref="ParameterToReplace"/> with.
        /// </summary>
        public IdentifierNameSyntax? Replacement { get; set; }

        /// <summary>
        /// Determines whether to visit the declaration body of a <see cref="MethodDeclarationSyntax"/>. Defaults to <see langword="true"/>.
        /// </summary>
        public bool VisitDeclarationBody { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeParameterReplacer"/> class.
        /// </summary>
        /// <param name="inputSymbols">A <see cref="List{T}"/> that contains <see cref="ISymbol"/>s in order they appear in the visited <see cref="CSharpSyntaxNode"/>.</param>
        public TypeParameterReplacer(List<ISymbol?> inputSymbols)
        {
            InputSymbols = inputSymbols;
            ChangedConstraintIndices = new();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeParameterReplacer"/> class.
        /// </summary>
        /// <param name="inputSymbols">A collection of <see cref="ISymbol"/>s in order they appear in the visited <see cref="CSharpSyntaxNode"/>.</param>
        public TypeParameterReplacer(IEnumerable<ISymbol> inputSymbols)
        {
            InputSymbols = inputSymbols?.ToList() ?? new();
            ChangedConstraintIndices = new();
        }

        /// <summary>
        /// Resets the replacer.
        /// </summary>
        public void Reset()
        {
            ParameterToReplace = null;
            Replacement = null;
            InputSymbols.Clear();
            VisitDeclarationBody = true;
            NewType = null;
            ResetCounter();
        }

        /// <summary>
        /// Resets the counter of encountered <see cref="IdentifierNameSyntax"/>es.
        /// </summary>
        public void ResetCounter()
        {
            _identifierCounter = 0;
            _constraintCounter = 0;
            ChangedConstraintIndices.Clear();
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (_skip || Count == 0)
            {
                return base.VisitIdentifierName(node);
            }

            if (_identifierCounter >= InputSymbols.Count)
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

        /// <inheritdoc/>
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
    }
}