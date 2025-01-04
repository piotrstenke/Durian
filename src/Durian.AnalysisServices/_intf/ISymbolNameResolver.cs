using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis;

/// <summary>
/// Provides methods for resolving name of a <see cref="ISymbol"/>, <see cref="IMemberData"/> or <see cref="ISymbolOrMember"/>.
/// </summary>
public interface ISymbolNameResolver
{
	/// <summary>
	/// Resolves the name of the <paramref name="symbol"/>.
	/// </summary>
	/// <param name="symbol"><see cref="ISymbol"/> to resolve the name of.</param>
	string ResolveName(ISymbol symbol);

	/// <summary>
	/// Resolves the name of the <paramref name="member"/>.
	/// </summary>
	/// <param name="member"><see cref="IMemberData"/> to resolve the name of.</param>
	string ResolveName(IMemberData member);

	/// <summary>
	/// Resolves the name of the <paramref name="member"/>.
	/// </summary>
	/// <param name="member"><see cref="ISymbolOrMember"/> to resolve the name of.</param>
	string ResolveName(ISymbolOrMember member);
}
