using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data;

/// <summary>
/// Encapsulates data associated with a single <see cref="INamedTypeSymbol"/> representing a struct.
/// </summary>
public interface IStructData : ITypeData, ISymbolOrMember<INamedTypeSymbol, IStructData>
{
	/// <summary>
	/// Target <see cref="StructDeclarationSyntax"/>.
	/// </summary>
	new StructDeclarationSyntax Declaration { get; }

	/// <summary>
	/// Creates a shallow copy of the current data.
	/// </summary>
	new IStructData Clone();
}