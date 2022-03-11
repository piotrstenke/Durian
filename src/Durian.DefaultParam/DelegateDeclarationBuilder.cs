// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// Builds a new <see cref="DelegateDeclarationSyntax"/> based on the value specified in the <c>Durian.DefaultParamAttribute</c>.
	/// </summary>
	public sealed class DelegateDeclarationBuilder : IDefaultParamDeclarationBuilder
	{
		private HashSet<int>? _newModifierIndexes;

		private int _numNonDefaultParam;

		private int _numOriginalConstraints;

		private int _numOriginalTypeParameters;

		/// <summary>
		/// <see cref="OriginalDeclaration"/> after modification.
		/// </summary>
		public DelegateDeclarationSyntax CurrentDeclaration { get; private set; }

		/// <summary>
		/// Original <see cref="DelegateDeclarationSyntax"/>.
		/// </summary>
		public DelegateDeclarationSyntax OriginalDeclaration { get; private set; }

		/// <summary>
		/// <see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the <see cref="OriginalDeclaration"/>.
		/// </summary>
		public SemanticModel SemanticModel { get; private set; }

		CSharpSyntaxNode IDefaultParamDeclarationBuilder.CurrentNode => CurrentDeclaration;
		CSharpSyntaxNode IDefaultParamDeclarationBuilder.OriginalNode => OriginalDeclaration;
		bool IDefaultParamDeclarationBuilder.VisitDeclarationBody => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateDeclarationBuilder"/> class.
		/// </summary>
		public DelegateDeclarationBuilder()
		{
			CurrentDeclaration = null!;
			OriginalDeclaration = null!;
			SemanticModel = null!;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateDeclarationBuilder"/> class.
		/// </summary>
		/// <param name="data"><see cref="DefaultParamDelegateData"/> to set as the <see cref="OriginalDeclaration"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public DelegateDeclarationBuilder(DefaultParamDelegateData data, CancellationToken cancellationToken = default)
		{
			SetData(data, cancellationToken);
		}

		/// <inheritdoc/>
		public void AcceptTypeParameterReplacer(TypeParameterReplacer replacer)
		{
			CurrentDeclaration = (DelegateDeclarationSyntax)replacer.Visit(CurrentDeclaration);
		}

		/// <summary>
		/// Sets the specified <paramref name="declaration"/> as the <see cref="CurrentDeclaration"/> without changing the <see cref="OriginalDeclaration"/>.
		/// </summary>
		/// <param name="declaration"><see cref="DelegateDeclarationSyntax"/> to set as <see cref="CurrentDeclaration"/>.</param>
		public void Emplace(DelegateDeclarationSyntax declaration)
		{
			CurrentDeclaration = declaration;
		}

		/// <summary>
		/// Returns the <see cref="TypeParameterConstraintClauseSyntax"/> at the specified <paramref name="index"/> in the <see cref="CurrentDeclaration"/>.
		/// </summary>
		/// <param name="index">Index of the <see cref="TypeParameterConstraintClauseSyntax"/> to get.</param>
		public TypeParameterConstraintClauseSyntax GetCurrentConstraintClause(int index)
		{
			return CurrentDeclaration.ConstraintClauses[index];
		}

		/// <summary>
		/// Returns a <see cref="SyntaxList{TNode}"/> of the <see cref="OriginalDeclaration"/>'s <see cref="TypeParameterConstraintClauseSyntax"/>es.
		/// </summary>
		public SyntaxList<TypeParameterConstraintClauseSyntax> GetOriginalConstraintClauses()
		{
			return OriginalDeclaration.ConstraintClauses;
		}

		/// <summary>
		/// Returns number of type parameters in the <see cref="OriginalDeclaration"/>.
		/// </summary>
		public int GetOriginalTypeParameterCount()
		{
			return _numOriginalTypeParameters;
		}

		/// <summary>
		/// Sets the value of <see cref="CurrentDeclaration"/> to the value of <see cref="OriginalDeclaration"/>.
		/// </summary>
		public void Reset()
		{
			CurrentDeclaration = OriginalDeclaration;
		}

		/// <summary>
		/// Sets the specified <paramref name="data"/> as the new <see cref="OriginalDeclaration"/>.
		/// </summary>
		/// <param name="data"><see cref="DefaultParamDelegateData"/> to set as the new <see cref="OriginalDeclaration"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		[MemberNotNull(nameof(OriginalDeclaration), nameof(CurrentDeclaration), nameof(SemanticModel))]
		public void SetData(DefaultParamDelegateData data, CancellationToken cancellationToken = default)
		{
			SemanticModel = data.SemanticModel;
			OriginalDeclaration = data.Declaration;
			_newModifierIndexes = data.NewModifierIndexes;
			_numNonDefaultParam = data.TypeParameters.NumNonDefaultParam;

			DelegateDeclarationSyntax del = data.Declaration;

			if (del.TypeParameterList is null || !del.TypeParameterList.Parameters.Any())
			{
				_numOriginalTypeParameters = 0;
				CurrentDeclaration = data.Declaration;
				return;
			}

			del = (DelegateDeclarationSyntax)DefaultParamUtilities.InitializeDeclaration(
				del,
				data.SemanticModel,
				data.ParentCompilation,
				del.TypeParameterList,
				cancellationToken,
				out TypeParameterListSyntax updatedParameters
			);

			_numOriginalConstraints = del.ConstraintClauses.Count;
			_numOriginalTypeParameters = updatedParameters.Parameters.Count;

			CurrentDeclaration = del.WithTypeParameterList(updatedParameters);
		}

		/// <summary>
		/// Replaces <see cref="TypeParameterConstraintClauseSyntax"/>es of the <see cref="CurrentDeclaration"/> with the specified collection of <see cref="TypeParameterConstraintClauseSyntax"/>es.
		/// </summary>
		/// <param name="constraintClauses">Collection of <see cref="TypeParameterConstraintClauseSyntax"/> to apply to the <see cref="CurrentDeclaration"/>.</param>
		public void WithConstraintClauses(IEnumerable<TypeParameterConstraintClauseSyntax> constraintClauses)
		{
			SyntaxList<TypeParameterConstraintClauseSyntax> clauses = DefaultParamUtilities.ApplyConstraints(constraintClauses, _numOriginalConstraints);

			if (clauses.Any())
			{
				clauses = clauses.Replace(clauses.Last(), clauses.Last().WithTrailingTrivia(null));
			}
			else if (CurrentDeclaration.ConstraintClauses.Any())
			{
				CurrentDeclaration = CurrentDeclaration.WithParameterList(CurrentDeclaration.ParameterList.WithoutTrailingTrivia());
			}

			CurrentDeclaration = CurrentDeclaration.WithConstraintClauses(clauses);
		}

		/// <summary>
		/// Determines how many type parameters of the <see cref="OriginalDeclaration"/> should the <see cref="CurrentDeclaration"/> have.
		/// </summary>
		/// <param name="count">Number of type parameters to take.</param>
		public void WithTypeParameters(int count)
		{
			if (DefaultParamUtilities.TryUpdateTypeParameters(CurrentDeclaration.TypeParameterList, count, out TypeParameterListSyntax? updated))
			{
				CurrentDeclaration = CurrentDeclaration.WithTypeParameterList(updated);
			}

			SyntaxTokenList modifiers = CurrentDeclaration.Modifiers;

			if (!modifiers.Any())
			{
				SyntaxTriviaList trivia = CurrentDeclaration.DelegateKeyword.LeadingTrivia;

				if (trivia.Any())
				{
					trivia = trivia.RemoveAt(trivia.Count - 1);
					CurrentDeclaration = CurrentDeclaration.WithDelegateKeyword(CurrentDeclaration.DelegateKeyword.WithLeadingTrivia(trivia));
				}
			}

			if (DefaultParamUtilities.TryAddNewModifier(_newModifierIndexes, count, _numNonDefaultParam, ref modifiers))
			{
				CurrentDeclaration = CurrentDeclaration.WithModifiers(modifiers);
			}
		}

		void IDefaultParamDeclarationBuilder.Emplace(CSharpSyntaxNode node)
		{
			CurrentDeclaration = (DelegateDeclarationSyntax)node;
		}
	}
}
