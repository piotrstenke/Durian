using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.DefaultParam
{
	public interface IDefaultParamDeclarationBuilder
	{
		CSharpSyntaxNode OriginalNode { get; }
		CSharpSyntaxNode CurrentNode { get; }
		SemanticModel SemanticModel { get; }
		bool VisitDeclarationBody { get; }

		void WithTypeParameters(int length);
		void WithConstraintClauses(IEnumerable<TypeParameterConstraintClauseSyntax> constraintClauses);

		SyntaxList<TypeParameterConstraintClauseSyntax> GetOriginalConstraintClauses();
		TypeParameterConstraintClauseSyntax GetCurrentConstraintClause(int index);
		int GetOriginalTypeParameterCount();
		void AcceptTypeParameterReplacer(TypeParameterReplacer replacer);
		void Emplace(CSharpSyntaxNode node);
	}
}
