using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.DefaultParam
{
	public partial class DefaultParamDelegateFilter
	{
		public sealed class Wrapper : IDefaultParamTargetWrapper
		{
			public DelegateDeclarationSyntax OriginalDeclaration { get; private set; }
			public DelegateDeclarationSyntax CurrentDeclaration { get; private set; }
			public SemanticModel SemanticModel { get; private set; }

			CSharpSyntaxNode IDefaultParamTargetWrapper.CurrentNode => CurrentDeclaration;
			CSharpSyntaxNode IDefaultParamTargetWrapper.OriginalNode => OriginalDeclaration;

			public Wrapper()
			{
				CurrentDeclaration = null!;
				OriginalDeclaration = null!;
				SemanticModel = null!;
			}

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
			public Wrapper(DefaultParamDelegateData data, CancellationToken cancellationToken = default)
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
				if (OriginalDeclaration.TypeParameterList is null)
				{
					return 0;
				}

				return OriginalDeclaration.TypeParameterList.Parameters.Count;
			}

			public void Reset()
			{
				CurrentDeclaration = OriginalDeclaration;
			}

			public void SetData(DefaultParamDelegateData data, CancellationToken cancellationToken = default)
			{
				SemanticModel = data.SemanticModel;
				OriginalDeclaration = data.Declaration;

				if (data.Declaration.TypeParameterList is null)
				{
					CurrentDeclaration = data.Declaration;
				}
				else
				{
					SeparatedSyntaxList<TypeParameterSyntax> typeParameters = data.Declaration.TypeParameterList.Parameters;

					if (typeParameters.Any())
					{
						typeParameters = SyntaxFactory.SeparatedList(typeParameters
							.Select(p =>
								p.WithAttributeLists(SyntaxFactory.List(p.AttributeLists
									.Where(l => l.Attributes
										.Any(a => !SymbolEqualityComparer.Default.Equals(SemanticModel.GetTypeInfo(a, cancellationToken).Type, data.ParentCompilation.Attribute))
									)
								))
							)
						);

						CurrentDeclaration = data.Declaration.WithTypeParameterList(data.Declaration.TypeParameterList.WithParameters(typeParameters));
					}
				}
			}

			public void WithConstraintClauses(IEnumerable<TypeParameterConstraintClauseSyntax> constraintClauses)
			{
				CurrentDeclaration = CurrentDeclaration.WithConstraintClauses(SyntaxFactory.List(constraintClauses));
			}

			public void WithTypeParameters(int count)
			{
				if (CurrentDeclaration.TypeParameterList is null)
				{
					return;
				}

				SeparatedSyntaxList<TypeParameterSyntax> typeParameters = CurrentDeclaration.TypeParameterList.Parameters;

				if (!typeParameters.Any())
				{
					return;
				}

				if (count == 0)
				{
					CurrentDeclaration = CurrentDeclaration.WithTypeParameterList(null);
				}
				else
				{
					CurrentDeclaration = CurrentDeclaration.WithTypeParameterList(SyntaxFactory.TypeParameterList(SyntaxFactory.SeparatedList(typeParameters.Take(count))));
				}
			}

			public void AcceptTypeParameterReplacer(TypeParameterReplacer replacer)
			{
				CurrentDeclaration = (DelegateDeclarationSyntax)replacer.Visit(CurrentDeclaration);
			}

			public void Emplace(DelegateDeclarationSyntax declaration)
			{
				CurrentDeclaration = declaration;
			}

			void IDefaultParamTargetWrapper.Emplace(CSharpSyntaxNode node)
			{
				CurrentDeclaration = (DelegateDeclarationSyntax)node;
			}
		}
	}
}
