using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.DefaultParam
{
	public partial class DefaultParamMethodFilter
	{
		public sealed class DeclarationBuilder : IDefaultParamDeclarationBuilder
		{
			private List<int>? _newModifierIndices;
			private GenericNameSyntax? _callMethodSyntax;
			private ArgumentListSyntax? _callArguments;
			private IdentifierNameSyntax? _newType;
			private int _numOriginalConstraints;
			private int _numOriginalParameters;
			private int _numNonDefaultParam;
			private int _indentLevel;
			private bool _applyReturnSyntax;

			public MethodDeclarationSyntax OriginalDeclaration { get; private set; }
			public MethodDeclarationSyntax CurrentDeclaration { get; private set; }
			public SemanticModel SemanticModel { get; private set; }

			CSharpSyntaxNode IDefaultParamDeclarationBuilder.CurrentNode => CurrentDeclaration;
			CSharpSyntaxNode IDefaultParamDeclarationBuilder.OriginalNode => OriginalDeclaration;
			bool IDefaultParamDeclarationBuilder.VisitDeclarationBody => _callMethodSyntax is null;

			public DeclarationBuilder()
			{
				CurrentDeclaration = null!;
				OriginalDeclaration = null!;
				SemanticModel = null!;
			}

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
			public DeclarationBuilder(DefaultParamMethodData data, CancellationToken cancellationToken = default)
			{
				SetData(data, cancellationToken);
			}
#pragma warning restore CS8618

			public SyntaxList<TypeParameterConstraintClauseSyntax> GetOriginalConstraintClauses()
			{
				return OriginalDeclaration.ConstraintClauses;
			}

			public TypeParameterConstraintClauseSyntax GetCurrentConstraintClause(int index)
			{
				return CurrentDeclaration.ConstraintClauses[index];
			}

			public int GetOriginalTypeParameterCount()
			{
				return _numOriginalParameters;
			}

			public void Reset()
			{
				CurrentDeclaration = OriginalDeclaration;
			}

			public void SetData(DefaultParamMethodData data, CancellationToken cancellationToken = default)
			{
				SemanticModel = data.SemanticModel;
				OriginalDeclaration = data.Declaration;

				_indentLevel = DefaultParamUtilities.GetIndent(data.Declaration);
				SetDeclarationWithoutDefaultParamAttribute(data.Declaration, data.ParentCompilation, cancellationToken);

				_newModifierIndices = data.NewModifierIndices;
				_numNonDefaultParam = data.GetTypeParameters().NumNonDefaultParam;

				if (data.CallInsteadOfCopying)
				{
					InitializeCallData(data);
				}
				else
				{
					_applyReturnSyntax = false;
					_callMethodSyntax = null;
					_callArguments = null;
					_newType = null;
				}
			}

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

			public void AcceptTypeParameterReplacer(TypeParameterReplacer replacer)
			{
				_newType = replacer.Replacement;
				CurrentDeclaration = (MethodDeclarationSyntax)replacer.Visit(CurrentDeclaration);
			}

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
				ref readonly TypeParameterContainer typeParameters = ref data.GetTypeParameters();
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
					if (_newType is not null)
					{
						TypeSyntax[] typeArguments = _callMethodSyntax.TypeArgumentList.Arguments.ToArray();
						typeArguments[count] = _newType;
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
				if (_newModifierIndices is not null && _newModifierIndices.Contains(count - _numNonDefaultParam))
				{
					SyntaxTokenList modifiers = CurrentDeclaration.Modifiers;

					if (!modifiers.Any(m => m.IsKind(SyntaxKind.NewKeyword)))
					{
						modifiers = modifiers.Add(SyntaxFactory.Token(SyntaxKind.NewKeyword).WithTrailingTrivia(SyntaxFactory.Space));
						CurrentDeclaration = CurrentDeclaration.WithModifiers(modifiers);
					}
				}
			}

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

				list = SyntaxFactory.SeparatedList(list.Select(p =>
					p.WithAttributeLists(SyntaxFactory.List(p.AttributeLists
						.Where(l => l.Attributes
							.Any(a => !SymbolEqualityComparer.Default.Equals(SemanticModel.GetSymbolInfo(a, cancellationToken).Symbol, compilation.AttributeConstructor))
						)
					))
				));

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
			}

			private IEnumerable<AttributeListSyntax> GetValidAttributes(MethodDeclarationSyntax method, DefaultParamCompilationData compilation, CancellationToken cancellationToken)
			{
				foreach (AttributeListSyntax list in method.AttributeLists)
				{
					SeparatedSyntaxList<AttributeSyntax> l = SyntaxFactory.SeparatedList(list.Attributes.Where(attr =>
					{
						ISymbol? symbol = SemanticModel.GetSymbolInfo(attr, cancellationToken).Symbol;
						return !SymbolEqualityComparer.Default.Equals(symbol, compilation.MethodConfigurationConstructor);
					}));

					if (l.Any())
					{
						yield return SyntaxFactory.AttributeList(l);
					}
				}
			}
		}
	}
}
