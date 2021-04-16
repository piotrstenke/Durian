using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="MemberDeclarationSyntax"/>.
	/// </summary>
	public interface IMemberData
	{
		/// <summary>
		/// <see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the <see cref="Declaration"/>.
		/// </summary>
		SemanticModel SemanticModel { get; }

		/// <summary>
		/// Target <see cref="MemberDeclarationSyntax"/>.
		/// </summary>
		MemberDeclarationSyntax Declaration { get; }

		/// <summary>
		/// <see cref="ISymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		ISymbol Symbol { get; }

		/// <summary>
		/// Parent compilation of this <see cref="IMemberData"/>.
		/// </summary>
		ICompilationData ParentCompilation { get; }

		/// <summary>
		/// Returns all <see cref="ITypeData"/>s that contain the <see cref="Symbol"/>.
		/// </summary>
		IEnumerable<ITypeData> GetContainingTypes();

		/// <summary>
		/// Returns all <see cref="INamespaceSymbol"/>s that contain the <see cref="Symbol"/>.
		/// </summary>
		IEnumerable<INamespaceSymbol> GetContainingNamespaces();

		/// <summary>
		/// Checks, if the <see cref="IMemberData"/> is actually valid for generation.
		/// </summary>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to register the potential errors to.</param>
		bool Validate(in GeneratorExecutionContext context);

		/// <summary>
		/// Returns data of all attributes applied to the <see cref="Symbol"/>.
		/// </summary>
		ImmutableArray<AttributeData> GetAttributes();
	}
}
