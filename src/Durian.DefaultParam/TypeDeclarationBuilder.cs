using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// Builds a new <see cref="TypeDeclarationSyntax"/> based on the value specified in the <see cref="DefaultParamAttribute"/>.
	/// </summary>
	public sealed class TypeDeclarationBuilder : IDefaultParamDeclarationBuilder
	{
		private HashSet<int>? _newModifierIndexes;
		private GenericNameSyntax? _inheritedType;
		private readonly Queue<IdentifierNameSyntax> _inheritTypeArguments;
		private int _numOriginalTypeParameters;
		private int _numOriginalConstraints;
		private int _numNonDefaultParam;

		/// <summary>
		/// Original <see cref="TypeDeclarationSyntax"/>.
		/// </summary>
		public TypeDeclarationSyntax OriginalDeclaration { get; private set; }

		/// <summary>
		/// <see cref="OriginalDeclaration"/> after modification.
		/// </summary>
		public TypeDeclarationSyntax CurrentDeclaration { get; private set; }

		/// <summary>
		/// <see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the <see cref="OriginalDeclaration"/>.
		/// </summary>
		public SemanticModel SemanticModel { get; private set; }

		CSharpSyntaxNode IDefaultParamDeclarationBuilder.CurrentNode => CurrentDeclaration;
		CSharpSyntaxNode IDefaultParamDeclarationBuilder.OriginalNode => OriginalDeclaration;
		bool IDefaultParamDeclarationBuilder.VisitDeclarationBody => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeDeclarationBuilder"/> class.
		/// </summary>
		public TypeDeclarationBuilder()
		{
			CurrentDeclaration = null!;
			OriginalDeclaration = null!;
			SemanticModel = null!;
			_inheritTypeArguments = new(4);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeDeclarationBuilder"/> class.
		/// </summary>
		/// <param name="data"><see cref="DefaultParamTypeData"/> to set as the <see cref="OriginalDeclaration"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public TypeDeclarationBuilder(DefaultParamTypeData data, CancellationToken cancellationToken = default)
		{
			_inheritTypeArguments = new(4);
			SetData(data, cancellationToken);
		}

		/// <summary>
		/// Returns a <see cref="SyntaxList{TNode}"/> of the <see cref="OriginalDeclaration"/>'s <see cref="TypeParameterConstraintClauseSyntax"/>es.
		/// </summary>
		public SyntaxList<TypeParameterConstraintClauseSyntax> GetOriginalConstraintClauses()
		{
			return OriginalDeclaration.ConstraintClauses;
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
		/// <param name="data"><see cref="DefaultParamTypeData"/> to set as the new <see cref="OriginalDeclaration"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		[MemberNotNull(nameof(OriginalDeclaration), nameof(CurrentDeclaration), nameof(SemanticModel))]
		public void SetData(DefaultParamTypeData data, CancellationToken cancellationToken = default)
		{
			SemanticModel = data.SemanticModel;
			OriginalDeclaration = data.Declaration;
			_newModifierIndexes = data.NewModifierIndexes;
			_numNonDefaultParam = data.TypeParameters.NumNonDefaultParam;

			InitializeDeclaration(data, cancellationToken);

			if(data.Inherit)
			{
				InitializeInheritData(data);
			}
			else
			{
				_inheritedType = null;
				_inheritTypeArguments.Clear();
			}
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
				clauses = clauses.Replace(clauses.Last(), clauses.Last().WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));

				if (CurrentDeclaration.BaseList is not null)
				{
					CurrentDeclaration = CurrentDeclaration.WithBaseList(CurrentDeclaration.BaseList.WithTrailingTrivia(SyntaxFactory.Space));
				}

			}
			else if (CurrentDeclaration.TypeParameterList is null)
			{
				if (CurrentDeclaration.BaseList is null)
				{
					CurrentDeclaration = CurrentDeclaration.WithIdentifier(CurrentDeclaration.Identifier.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));
				}
				else
				{
					CurrentDeclaration = CurrentDeclaration.WithIdentifier(CurrentDeclaration.Identifier.WithTrailingTrivia(SyntaxFactory.Space));
				}
			}
			else
			{
				CurrentDeclaration = CurrentDeclaration.WithTypeParameterList(CurrentDeclaration.TypeParameterList.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));
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
				if (updated is null)
				{
					CurrentDeclaration = CurrentDeclaration.WithTypeParameterList(updated).WithIdentifier(CurrentDeclaration.Identifier.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));
				}
				else
				{
					CurrentDeclaration = CurrentDeclaration.WithTypeParameterList(updated.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));
				}
			}

			CheckInherit(count);
			SyntaxTokenList modifiers = CurrentDeclaration.Modifiers;

			if (!modifiers.Any())
			{
				SyntaxTriviaList trivia = CurrentDeclaration.Keyword.LeadingTrivia;

				if (trivia.Any())
				{
					trivia = trivia.RemoveAt(trivia.Count - 1);
					CurrentDeclaration = CurrentDeclaration.WithKeyword(CurrentDeclaration.Keyword.WithLeadingTrivia(trivia));
				}
			}

			if (DefaultParamUtilities.TryAddNewModifierForType(_newModifierIndexes, count, _numNonDefaultParam, ref modifiers))
			{
				CurrentDeclaration = CurrentDeclaration.WithModifiers(modifiers);
			}
		}

		/// <inheritdoc/>
		public void AcceptTypeParameterReplacer(TypeParameterReplacer replacer)
		{
			_inheritTypeArguments.Enqueue(replacer.Replacement!);
			CurrentDeclaration = (TypeDeclarationSyntax)replacer.Visit(CurrentDeclaration);
		}

		/// <summary>
		/// Sets the specified <paramref name="declaration"/> as the <see cref="CurrentDeclaration"/> without changing the <see cref="OriginalDeclaration"/>.
		/// </summary>
		/// <param name="declaration"><see cref="TypeDeclarationSyntax"/> to set as <see cref="CurrentDeclaration"/>.</param>
		public void Emplace(TypeDeclarationSyntax declaration)
		{
			CurrentDeclaration = declaration;
		}

		void IDefaultParamDeclarationBuilder.Emplace(CSharpSyntaxNode node)
		{
			CurrentDeclaration = (TypeDeclarationSyntax)node;
		}

		[MemberNotNull(nameof(CurrentDeclaration))]
		private void InitializeDeclaration(DefaultParamTypeData data, CancellationToken cancellationToken)
		{
			TypeDeclarationSyntax type = data.Declaration;

			if (type.TypeParameterList is null || !type.TypeParameterList.Parameters.Any())
			{
				_numOriginalTypeParameters = 0;
				CurrentDeclaration = data.Declaration;
				return;
			}

			type = (TypeDeclarationSyntax)DefaultParamUtilities.InitializeDeclaration(
				type,
				data.SemanticModel,
				data.ParentCompilation,
				type.TypeParameterList,
				cancellationToken,
				out TypeParameterListSyntax updatedParameters
			);

			_numOriginalConstraints = type.ConstraintClauses.Count;
			_numOriginalTypeParameters = updatedParameters.Parameters.Count;

			CurrentDeclaration = type.WithTypeParameterList(updatedParameters);
		}

		private void InitializeInheritData(DefaultParamTypeData data)
		{
			ref readonly TypeParameterContainer typeParameters = ref data.TypeParameters;
			TypeSyntax[] typeArguments = new TypeSyntax[typeParameters.Length];

			for (int i = 0; i < typeParameters.Length; i++)
			{
				ref readonly TypeParameterData param = ref typeParameters[i];

				typeArguments[i] = SyntaxFactory.IdentifierName(param.Symbol.Name);
			}

			_inheritedType = SyntaxFactory.GenericName(data.Declaration.Identifier, SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(typeArguments)));

			CurrentDeclaration = CurrentDeclaration
				.WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>())
				.WithBaseList(null);
		}

		private void CheckInherit(int count)
		{
			if (_inheritedType is not null)
			{
				TypeSyntax[] typeArguments = _inheritedType.TypeArgumentList.Arguments.ToArray();
				typeArguments[count] = _inheritTypeArguments.Dequeue();
				_inheritedType = _inheritedType.WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(typeArguments)));

				if(count == 0)
				{
					CurrentDeclaration = CurrentDeclaration.WithIdentifier(CurrentDeclaration.Identifier.WithTrailingTrivia(SyntaxFactory.Space));
				}
				else
				{
					CurrentDeclaration = CurrentDeclaration.WithTypeParameterList(CurrentDeclaration.TypeParameterList!.WithTrailingTrivia(SyntaxFactory.Space));
				}

				CurrentDeclaration = CurrentDeclaration.WithBaseList(SyntaxFactory.BaseList(
					SyntaxFactory.Token(SyntaxKind.ColonToken).WithTrailingTrivia(SyntaxFactory.Space),
					SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
						SyntaxFactory.SimpleBaseType(_inheritedType).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed))));
			}
		}
	}
}
