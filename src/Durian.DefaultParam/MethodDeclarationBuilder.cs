using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.DefaultParam
{
	/// <summary>
	/// Builds a new <see cref="MethodDeclarationSyntax"/> based on the value specified in the <see cref="DefaultParamAttribute"/>.
	/// </summary>
	public sealed class MethodDeclarationBuilder : IDefaultParamDeclarationBuilder
	{
		private HashSet<int>? _newModifierIndexes;
		private GenericNameSyntax? _callMethodSyntax;
		private ArgumentListSyntax? _callArguments;
		private readonly Queue<IdentifierNameSyntax> _callTypeArguments;
		private int _numOriginalConstraints;
		private int _numOriginalParameters;
		private int _numNonDefaultParam;
		private int _indentLevel;
		private bool _applyReturnSyntax;

		/// <summary>
		/// Original <see cref="MethodDeclarationSyntax"/>.
		/// </summary>
		public MethodDeclarationSyntax OriginalDeclaration { get; private set; }

		/// <summary>
		/// <see cref="OriginalDeclaration"/> after modification.
		/// </summary>
		public MethodDeclarationSyntax CurrentDeclaration { get; private set; }

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
			_callTypeArguments = new();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MethodDeclarationBuilder"/> class.
		/// </summary>
		/// <param name="data"><see cref="DefaultParamMethodData"/> to set as the <see cref="OriginalDeclaration"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public MethodDeclarationBuilder(DefaultParamMethodData data, CancellationToken cancellationToken = default)
		{
			_callTypeArguments = new();
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
			return _numOriginalParameters;
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
		[MemberNotNull(nameof(OriginalDeclaration), nameof(CurrentDeclaration), nameof(SemanticModel))]
		public void SetData(DefaultParamMethodData data, CancellationToken cancellationToken = default)
		{
			SemanticModel = data.SemanticModel;
			OriginalDeclaration = data.Declaration;

			_indentLevel = DefaultParamUtilities.GetIndent(data.Declaration);
			_newModifierIndexes = data.NewModifierIndexes;
			_numNonDefaultParam = data.TypeParameters.NumNonDefaultParam;
			_callTypeArguments.Clear();

			SetDeclarationWithoutDefaultParamAttribute(data.Declaration, data.ParentCompilation, cancellationToken);

			if (data.CallInsteadOfCopying)
			{
				InitializeCallData(data);
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
			SyntaxList<TypeParameterConstraintClauseSyntax> clauses = SyntaxFactory.List(constraintClauses);

			if (_numOriginalConstraints > 0)
			{
				int count = clauses.Count;

				if (count == 0)
				{
					CurrentDeclaration = CurrentDeclaration.WithParameterList(CurrentDeclaration.ParameterList.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));
				}
				else if (count < _numOriginalConstraints)
				{
					TypeParameterConstraintClauseSyntax last = clauses.Last();
					TypeParameterConstraintClauseSyntax newLast = last.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

					clauses = clauses.Replace(last, newLast);
				}
			}

			CurrentDeclaration = CurrentDeclaration.WithConstraintClauses(clauses);
		}

		/// <summary>
		/// Determines how many type parameters of the <see cref="OriginalDeclaration"/> should the <see cref="CurrentDeclaration"/> have.
		/// </summary>
		/// <param name="count">Number of type parameters to take.</param>
		public void WithTypeParameters(int count)
		{
			if (CurrentDeclaration.TypeParameterList is null || count > CurrentDeclaration.TypeParameterList.Parameters.Count)
			{
				return;
			}

			if (count == 0)
			{
				CurrentDeclaration = CurrentDeclaration.WithTypeParameterList(null);
			}
			else
			{
				CurrentDeclaration = CurrentDeclaration.WithTypeParameterList(SyntaxFactory.TypeParameterList(SyntaxFactory.SeparatedList(CurrentDeclaration.TypeParameterList.Parameters.Take(count))));
			}

			CheckDirectCall(count);
			CheckNewModifier(count);
		}

		/// <inheritdoc/>
		public void AcceptTypeParameterReplacer(TypeParameterReplacer replacer)
		{
			_callTypeArguments.Enqueue(replacer.Replacement!);
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

		void IDefaultParamDeclarationBuilder.Emplace(CSharpSyntaxNode node)
		{
			CurrentDeclaration = (MethodDeclarationSyntax)node;
		}

		private void InitializeCallData(DefaultParamMethodData data)
		{
			ref readonly TypeParameterContainer typeParameters = ref data.TypeParameters;
			TypeSyntax[] typeArguments = new TypeSyntax[typeParameters.Length];
			SeparatedSyntaxList<ParameterSyntax> parameters = data.Declaration.ParameterList.Parameters;
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

			_callMethodSyntax = SyntaxFactory.GenericName(data.Declaration.Identifier, SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(typeArguments)));
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
				SyntaxTrivia[] indent = GetTabs(_indentLevel);

				return SyntaxFactory.Block(
					SyntaxFactory.Token(SyntaxKind.OpenBraceToken).WithLeadingTrivia(indent).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed),
					SyntaxFactory.List<StatementSyntax>(),
					SyntaxFactory.Token(SyntaxKind.CloseBraceToken).WithLeadingTrivia(indent).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));
			}
		}

		private static SyntaxTrivia[] GetTabs(int indent)
		{
			SyntaxTrivia[] trivia = new SyntaxTrivia[indent];

			for (int i = 0; i < indent; i++)
			{
				trivia[i] = SyntaxFactory.Tab;
			}

			return trivia;
		}

		private void CheckDirectCall(int count)
		{
			if (_callMethodSyntax is not null)
			{
				if (_callTypeArguments is not null)
				{
					TypeSyntax[] typeArguments = _callMethodSyntax.TypeArgumentList.Arguments.ToArray();
					typeArguments[count] = _callTypeArguments.Dequeue();
					_callMethodSyntax = _callMethodSyntax.WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(typeArguments)));
				}

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

				statement = statement.WithLeadingTrivia(GetTabs(_indentLevel + 1)).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

				CurrentDeclaration = CurrentDeclaration.WithBody(SyntaxFactory.Block(CurrentDeclaration.Body!.OpenBraceToken, SyntaxFactory.SingletonList(statement), CurrentDeclaration.Body.CloseBraceToken));
			}
		}

		private void CheckNewModifier(int count)
		{
			if (_newModifierIndexes is not null && _newModifierIndexes.Contains(count - _numNonDefaultParam))
			{
				SyntaxTokenList modifiers = CurrentDeclaration.Modifiers;

				if (!modifiers.Any(m => m.IsKind(SyntaxKind.NewKeyword)))
				{
					modifiers = modifiers.Add(SyntaxFactory.Token(SyntaxKind.NewKeyword).WithTrailingTrivia(SyntaxFactory.Space));
					CurrentDeclaration = CurrentDeclaration.WithModifiers(modifiers);
				}
			}
		}

		[MemberNotNull(nameof(CurrentDeclaration))]
		private void SetDeclarationWithoutDefaultParamAttribute(MethodDeclarationSyntax method, DefaultParamCompilationData compilation, CancellationToken cancellationToken)
		{
			TypeParameterListSyntax? parameters = method.TypeParameterList;

			if (parameters is null || !parameters.Parameters.Any())
			{
				_numOriginalParameters = 0;
				CurrentDeclaration = method;
				return;
			}

			SeparatedSyntaxList<TypeParameterSyntax> list = parameters.Parameters;
			list = SyntaxFactory.SeparatedList(list.Select(p => p.WithAttributeLists(SyntaxFactory.List(p.AttributeLists.Where(attrList => attrList.Attributes.Any(attr =>
			{
				SymbolInfo info = SemanticModel.GetSymbolInfo(attr, cancellationToken);
				return !SymbolEqualityComparer.Default.Equals(info.Symbol?.ContainingType, compilation.MainAttribute);
			}
			))))));

			int length = list.Count;

			if (length > 1)
			{
				TypeParameterSyntax[] p = new TypeParameterSyntax[length];
				p[0] = list[0];

				for (int i = 1; i < length; i++)
				{
					p[i] = list[i].WithLeadingTrivia(SyntaxFactory.Space);
				}

				list = SyntaxFactory.SeparatedList(p);
			}

			_numOriginalConstraints = method.ConstraintClauses.Count;
			_numOriginalParameters = list.Count;

			CurrentDeclaration = method
				.WithAttributeLists(SyntaxFactory.List(GetValidAttributes(method, compilation, cancellationToken)))
				.WithTypeParameterList(SyntaxFactory.TypeParameterList(list)).WithoutTrivia();

			//if (_newModifierIndexes is null || _newModifierIndexes.Count == 0)
			//{
			//	CurrentDeclaration = CurrentDeclaration.WithModifiers(SyntaxFactory.TokenList(CurrentDeclaration.Modifiers.Where(m => !m.IsKind(SyntaxKind.NewKeyword))));
			//}
		}

		private IEnumerable<AttributeListSyntax> GetValidAttributes(MethodDeclarationSyntax method, DefaultParamCompilationData compilation, CancellationToken cancellationToken)
		{
			foreach (AttributeListSyntax list in method.AttributeLists)
			{
				SeparatedSyntaxList<AttributeSyntax> l = SyntaxFactory.SeparatedList(list.Attributes.Where(attr =>
				{
					ISymbol? symbol = SemanticModel.GetSymbolInfo(attr, cancellationToken).Symbol;
					return !SymbolEqualityComparer.Default.Equals(symbol?.ContainingType, compilation.ConfigurationAttribute);
				}));

				if (l.Any())
				{
					yield return SyntaxFactory.AttributeList(l);
				}
			}
		}
	}
}
