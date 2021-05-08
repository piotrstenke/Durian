using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.DefaultParam
{
	/// <summary>
	/// Builds a new <see cref="TypeDeclarationSyntax"/> based on the value specified in the <see cref="DefaultParamAttribute"/>.
	/// </summary>
	public sealed class TypeDeclarationBuilder : IDefaultParamDeclarationBuilder
	{
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
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeDeclarationBuilder"/> class.
		/// </summary>
		/// <param name="data"><see cref="DefaultParamTypeData"/> to set as the <see cref="OriginalDeclaration"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public TypeDeclarationBuilder(DefaultParamTypeData data, CancellationToken cancellationToken = default)
		{
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
			if (OriginalDeclaration.TypeParameterList is null)
			{
				return 0;
			}

			return OriginalDeclaration.TypeParameterList.Parameters.Count;
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
				else
				{
					CurrentDeclaration = data.Declaration;
				}
			}
		}

		/// <summary>
		/// Replaces <see cref="TypeParameterConstraintClauseSyntax"/>es of the <see cref="CurrentDeclaration"/> with the specified collection of <see cref="TypeParameterConstraintClauseSyntax"/>es.
		/// </summary>
		/// <param name="constraintClauses">Collection of <see cref="TypeParameterConstraintClauseSyntax"/> to apply to the <see cref="CurrentDeclaration"/>.</param>
		public void WithConstraintClauses(IEnumerable<TypeParameterConstraintClauseSyntax> constraintClauses)
		{
			CurrentDeclaration = CurrentDeclaration.WithConstraintClauses(SyntaxFactory.List(constraintClauses));
		}

		/// <summary>
		/// Determines how many type parameters of the <see cref="OriginalDeclaration"/> should the <see cref="CurrentDeclaration"/> have.
		/// </summary>
		/// <param name="count">Number of type parameters to take.</param>
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

		/// <inheritdoc/>
		public void AcceptTypeParameterReplacer(TypeParameterReplacer replacer)
		{
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
	}
}
