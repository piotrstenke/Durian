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
		public sealed class Wrapper : IDefaultParamTargetWrapper
		{
			private int _numOriginalConstraints;
			private int _numOriginalParameters;
			private List<int>? _newModifierIndices;

			public MethodDeclarationSyntax OriginalDeclaration { get; private set; }
			public MethodDeclarationSyntax CurrentDeclaration { get; private set; }
			public SemanticModel SemanticModel { get; private set; }

			CSharpSyntaxNode IDefaultParamTargetWrapper.CurrentNode => CurrentDeclaration;
			CSharpSyntaxNode IDefaultParamTargetWrapper.OriginalNode => OriginalDeclaration;

			internal Wrapper()
			{
				CurrentDeclaration = null!;
				OriginalDeclaration = null!;
				SemanticModel = null!;
			}

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
			public Wrapper(DefaultParamMethodData data, CancellationToken cancellationToken = default)
			{
				SetDataAndRemoveDefaultParamAttribute(data, cancellationToken);
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

					if (_newModifierIndices is not null && _newModifierIndices.Contains(count - 1))
					{
						SyntaxTokenList modifiers = CurrentDeclaration.Modifiers;

						if (!modifiers.Any(m => m.IsKind(SyntaxKind.NewKeyword)))
						{
							modifiers = modifiers.Add(SyntaxFactory.Token(SyntaxKind.NewKeyword));
							CurrentDeclaration = CurrentDeclaration.WithModifiers(modifiers);
						}
					}
				}
			}

			public void AcceptTypeParameterReplacer(TypeParameterReplacer replacer)
			{
				CurrentDeclaration = (MethodDeclarationSyntax)replacer.Visit(CurrentDeclaration);
			}

			public void Emplace(MethodDeclarationSyntax declaration)
			{
				CurrentDeclaration = declaration;
			}

			void IDefaultParamTargetWrapper.Emplace(CSharpSyntaxNode node)
			{
				CurrentDeclaration = (MethodDeclarationSyntax)node;
			}

			internal void SetDataAndRemoveDefaultParamAttribute(DefaultParamMethodData data, CancellationToken cancellationToken = default)
			{
				SemanticModel = data.SemanticModel;
				OriginalDeclaration = data.Declaration;

				CurrentDeclaration = GetDeclarationWithoutDefaultParamAttribute(data.Declaration, data.ParentCompilation, cancellationToken);
				CurrentDeclaration = CurrentDeclaration.WithModifiers(SyntaxFactory.TokenList(CurrentDeclaration.Modifiers.Where(m => !m.IsKind(SyntaxKind.PartialKeyword))));
				_newModifierIndices = data.NewModifierIndices;
			}

			private MethodDeclarationSyntax GetDeclarationWithoutDefaultParamAttribute(MethodDeclarationSyntax method, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
			{
				TypeParameterListSyntax? parameters = method.TypeParameterList;

				if (parameters is null || !parameters.Parameters.Any())
				{
					_numOriginalParameters = 0;
					return method;
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

				return method.WithTypeParameterList(SyntaxFactory.TypeParameterList(list)).WithoutTrivia();
			}
		}
	}
}
