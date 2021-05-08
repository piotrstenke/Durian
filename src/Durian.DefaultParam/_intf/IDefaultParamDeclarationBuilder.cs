using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.DefaultParam
{
	/// <summary>
	/// Builds a new member declaration based on the value specified in the <see cref="DefaultParamAttribute"/>.
	/// </summary>
	public interface IDefaultParamDeclarationBuilder
	{
		/// <summary>
		/// Original <see cref="CSharpSyntaxNode"/>.
		/// </summary>
		CSharpSyntaxNode OriginalNode { get; }

		/// <summary>
		/// <see cref="OriginalNode"/> after modification.
		/// </summary>
		CSharpSyntaxNode CurrentNode { get; }

		/// <summary>
		/// <see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the <see cref="OriginalNode"/>.
		/// </summary>
		SemanticModel SemanticModel { get; }

		/// <summary>
		/// Determines whether to visit the body of this member's declaration when collecting or replacing type parameters.
		/// </summary>
		bool VisitDeclarationBody { get; }

		/// <summary>
		/// Determines how many type parameters of the <see cref="OriginalNode"/> should the <see cref="CurrentNode"/> have.
		/// </summary>
		/// <param name="count">Number of type parameters to take.</param>
		void WithTypeParameters(int count);

		/// <summary>
		/// Replaces <see cref="TypeParameterConstraintClauseSyntax"/>es of the <see cref="CurrentNode"/> with the specified collection of <see cref="TypeParameterConstraintClauseSyntax"/>es.
		/// </summary>
		/// <param name="constraintClauses">Collection of <see cref="TypeParameterConstraintClauseSyntax"/> to apply to the <see cref="CurrentNode"/>.</param>
		void WithConstraintClauses(IEnumerable<TypeParameterConstraintClauseSyntax> constraintClauses);

		/// <summary>
		/// Returns a <see cref="SyntaxList{TNode}"/> of the <see cref="OriginalNode"/>'s <see cref="TypeParameterConstraintClauseSyntax"/>es.
		/// </summary>
		SyntaxList<TypeParameterConstraintClauseSyntax> GetOriginalConstraintClauses();

		/// <summary>
		/// Returns the <see cref="TypeParameterConstraintClauseSyntax"/> at the specified <paramref name="index"/> in the <see cref="CurrentNode"/>.
		/// </summary>
		/// <param name="index">Index of the <see cref="TypeParameterConstraintClauseSyntax"/> to get.</param>
		TypeParameterConstraintClauseSyntax GetCurrentConstraintClause(int index);

		/// <summary>
		/// Returns number of type parameters in the <see cref="OriginalNode"/>.
		/// </summary>
		int GetOriginalTypeParameterCount();

		/// <summary>
		/// Begins the process of type parameter replacing using the specified <paramref name="replacer"/>.
		/// </summary>
		/// <param name="replacer"><see cref="TypeParameterReplacer"/> to use.</param>
		void AcceptTypeParameterReplacer(TypeParameterReplacer replacer);

		/// <summary>
		/// Sets the specified <paramref name="node"/> as the <see cref="CurrentNode"/> without changing the <see cref="OriginalNode"/>.
		/// </summary>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to set as <see cref="CurrentNode"/>.</param>
		void Emplace(CSharpSyntaxNode node);

		/// <summary>
		/// Sets the value of <see cref="CurrentNode"/> to the value of <see cref="OriginalNode"/>.
		/// </summary>
		void Reset();
	}
}
