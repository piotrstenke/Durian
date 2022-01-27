// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// Builds a new <see cref="MethodDeclarationSyntax"/> based on the value specified in the <c>Durian.DefaultParamAttribute</c>.
	/// </summary>
	public sealed class MethodDeclarationBuilder : IDefaultParamDeclarationBuilder
	{
		private readonly Queue<IdentifierNameSyntax> _callTypeArguments;

		private bool _applyReturnSyntax;

		private ArgumentListSyntax? _callArguments;

		private GenericNameSyntax? _callMethodSyntax;

		private int _indentLevel;

		private HashSet<int>? _newModifierIndexes;

		private int _numNonDefaultParam;

		private int _numOriginalConstraints;

		private int _numOriginalTypeParameters;

		private IMethodSymbol _symbol;

		/// <summary>
		/// <see cref="OriginalDeclaration"/> after modification.
		/// </summary>
		public MethodDeclarationSyntax CurrentDeclaration { get; private set; }

		/// <summary>
		/// Original <see cref="MethodDeclarationSyntax"/>.
		/// </summary>
		public MethodDeclarationSyntax OriginalDeclaration { get; private set; }

		/// <summary>
		/// <see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the <see cref="OriginalDeclaration"/>.
		/// </summary>
		public SemanticModel SemanticModel { get; private set; }

		CSharpSyntaxNode IDefaultParamDeclarationBuilder.CurrentNode => CurrentDeclaration;
		CSharpSyntaxNode IDefaultParamDeclarationBuilder.OriginalNode => OriginalDeclaration;
		bool IDefaultParamDeclarationBuilder.VisitDeclarationBody => _callMethodSyntax is null;

		/// <summary>
		/// Initializes a new instance of the <see cref="MethodDeclarationBuilder"/> class.
		/// </summary>
		public MethodDeclarationBuilder()
		{
			CurrentDeclaration = null!;
			OriginalDeclaration = null!;
			SemanticModel = null!;
			_symbol = null!;
			_callTypeArguments = new(4);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MethodDeclarationBuilder"/> class.
		/// </summary>
		/// <param name="data"><see cref="DefaultParamMethodData"/> to set as the <see cref="OriginalDeclaration"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public MethodDeclarationBuilder(DefaultParamMethodData data, CancellationToken cancellationToken = default)
		{
			_callTypeArguments = new(4);
			SetData(data, cancellationToken);
		}

		/// <inheritdoc/>
		public void AcceptTypeParameterReplacer(TypeParameterReplacer replacer)
		{
			if (_callMethodSyntax is not null)
			{
				_callTypeArguments.Enqueue(replacer.Replacement!);
			}

			CurrentDeclaration = (MethodDeclarationSyntax)replacer.Visit(CurrentDeclaration);
		}

		/// <summary>
		/// Sets the specified <paramref name="declaration"/> as the <see cref="CurrentDeclaration"/> without changing the <see cref="OriginalDeclaration"/>.
		/// </summary>
		/// <param name="declaration"><see cref="MethodDeclarationSyntax"/> to set as <see cref="CurrentDeclaration"/>.</param>
		public void Emplace(MethodDeclarationSyntax declaration)
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
		/// <param name="data"><see cref="DefaultParamMethodData"/> to set as the new <see cref="OriginalDeclaration"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		[MemberNotNull(nameof(OriginalDeclaration), nameof(CurrentDeclaration), nameof(SemanticModel), nameof(_symbol))]
		public void SetData(DefaultParamMethodData data, CancellationToken cancellationToken = default)
		{
			SemanticModel = data.SemanticModel;
			OriginalDeclaration = data.Declaration;

			_symbol = data.Symbol;
			_indentLevel = DefaultParamUtilities.GetIndent(data.Declaration);
			_newModifierIndexes = data.NewModifierIndexes;
			_numNonDefaultParam = data.TypeParameters.NumNonDefaultParam;

			InitializeDeclaration(data, cancellationToken);

			if (data.CallInsteadOfCopying)
			{
				InitializeCallData(in data.TypeParameters);
			}
			else
			{
				_applyReturnSyntax = false;
				_callMethodSyntax = null;
				_callArguments = null;
				_callTypeArguments.Clear();
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
			}
			else if (!_symbol.IsAbstract)
			{
				CurrentDeclaration = CurrentDeclaration.WithParameterList(CurrentDeclaration.ParameterList.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));
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

			CheckDirectCall(count);
			SyntaxTokenList modifiers = CurrentDeclaration.Modifiers;

			if (!modifiers.Any())
			{
				SyntaxTriviaList trivia = CurrentDeclaration.ReturnType.GetLeadingTrivia();

				if (trivia.Any())
				{
					trivia = trivia.RemoveAt(trivia.Count - 1);
					CurrentDeclaration = CurrentDeclaration.WithReturnType(CurrentDeclaration.ReturnType.WithLeadingTrivia(trivia));
				}
			}

			if (DefaultParamUtilities.TryAddNewModifier(_newModifierIndexes, count, _numNonDefaultParam, ref modifiers))
			{
				CurrentDeclaration = CurrentDeclaration.WithModifiers(modifiers);
			}
		}

		void IDefaultParamDeclarationBuilder.Emplace(CSharpSyntaxNode node)
		{
			CurrentDeclaration = (MethodDeclarationSyntax)node;
		}

		private void CheckDirectCall(int count)
		{
			if (_callMethodSyntax is not null)
			{
				TypeSyntax[] typeArguments = _callMethodSyntax.TypeArgumentList.Arguments.ToArray();
				typeArguments[count] = _callTypeArguments.Dequeue();
				_callMethodSyntax = _callMethodSyntax.WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(typeArguments)));

				InvocationExpressionSyntax inv = SyntaxFactory.InvocationExpression(_callMethodSyntax, _callArguments!);
				StatementSyntax statement;

				if (_applyReturnSyntax)
				{
					statement = SyntaxFactory.ReturnStatement(SyntaxFactory.Token(SyntaxKind.ReturnKeyword).WithTrailingTrivia(SyntaxFactory.Space), inv, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
				}
				else
				{
					statement = SyntaxFactory.ExpressionStatement(inv);
				}

				statement = statement.WithLeadingTrivia(DefaultParamUtilities.GetTabs(_indentLevel + 1)).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

				CurrentDeclaration = CurrentDeclaration.WithBody(SyntaxFactory.Block(CurrentDeclaration.Body!.OpenBraceToken, SyntaxFactory.SingletonList(statement), CurrentDeclaration.Body.CloseBraceToken));
			}
		}

		private void InitializeCallData(in TypeParameterContainer typeParameters)
		{
			TypeSyntax[] typeArguments = new TypeSyntax[typeParameters.Length];
			SeparatedSyntaxList<ParameterSyntax> parameters = CurrentDeclaration.ParameterList.Parameters;
			ArgumentSyntax[] arguments = new ArgumentSyntax[parameters.Count];

			for (int i = 0; i < typeParameters.Length; i++)
			{
				ref readonly TypeParameterData param = ref typeParameters[i];

				typeArguments[i] = SyntaxFactory.IdentifierName(param.Symbol.Name);
			}

			for (int i = 0; i < parameters.Count; i++)
			{
				ParameterSyntax param = parameters[i];
				ArgumentSyntax arg = SyntaxFactory.Argument(SyntaxFactory.IdentifierName(param.Identifier));
				SyntaxToken? refKind = param.Modifiers.FirstOrDefault(m => m.IsKind(SyntaxKind.RefKeyword) || m.IsKind(SyntaxKind.InKeyword) || m.IsKind(SyntaxKind.OutKeyword));

				if (refKind is not null)
				{
					arg = arg.WithRefKindKeyword(refKind.Value);
				}

				arguments[i] = arg;
			}

			_callMethodSyntax = SyntaxFactory.GenericName(CurrentDeclaration.Identifier, SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(typeArguments)));
			_callArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments));
			_applyReturnSyntax = CurrentDeclaration.ReturnType is not PredefinedTypeSyntax t || !t.Keyword.IsKind(SyntaxKind.VoidKeyword);

			if (CurrentDeclaration.ExpressionBody is not null)
			{
				CurrentDeclaration = CurrentDeclaration.WithExpressionBody(null).WithBody(GetBlock());
			}
			else if (CurrentDeclaration.Body is not null)
			{
				CurrentDeclaration = CurrentDeclaration.WithBody(GetBlock());
			}

			BlockSyntax GetBlock()
			{
				SyntaxTrivia[] indent = DefaultParamUtilities.GetTabs(_indentLevel);

				return SyntaxFactory.Block(
					SyntaxFactory.Token(SyntaxKind.OpenBraceToken).WithLeadingTrivia(indent).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed),
					SyntaxFactory.List<StatementSyntax>(),
					SyntaxFactory.Token(SyntaxKind.CloseBraceToken).WithLeadingTrivia(indent).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));
			}
		}

		[MemberNotNull(nameof(CurrentDeclaration))]
		private void InitializeDeclaration(DefaultParamMethodData method, CancellationToken cancellationToken)
		{
			MethodDeclarationSyntax decl = method.Declaration;

			if (decl.TypeParameterList is null || !decl.TypeParameterList.Parameters.Any())
			{
				_numOriginalTypeParameters = 0;
				CurrentDeclaration = method.Declaration;
				return;
			}

			decl = (MethodDeclarationSyntax)DefaultParamUtilities.InitializeDeclaration(
				decl,
				method.SemanticModel,
				method.ParentCompilation,
				decl.TypeParameterList,
				cancellationToken,
				out TypeParameterListSyntax updatedParameters
			);

			_numOriginalConstraints = decl.ConstraintClauses.Count;
			_numOriginalTypeParameters = updatedParameters.Parameters.Count;

			CurrentDeclaration = decl.WithTypeParameterList(updatedParameters);
		}
	}
}