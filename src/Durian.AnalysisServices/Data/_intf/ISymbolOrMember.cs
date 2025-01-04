using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Data;

/// <summary>
/// A wrapper that provides access to a <see cref="ISymbol"/> or a lazy-initialized <see cref="IMemberData"/> of the same member.
/// </summary>
public interface ISymbolOrMember
{
	/// <summary>
	/// Determines whether the <see cref="Member"/> was lazy-initialized.
	/// </summary>
	bool HasMember { get; }

	/// <summary>
	/// Lazy-initialized <see cref="IMemberData"/> of the same member.
	/// </summary>
	IMemberData Member { get; }

	/// <summary>
	/// <see cref="ISymbol"/> of the current member.
	/// </summary>
	ISymbol Symbol { get; }
}

/// <inheritdoc cref="ISymbolOrMember"/>
/// <typeparam name="TSymbol">Type of <see cref="ISymbol"/>.</typeparam>
/// <typeparam name="TData">Type of <see cref="IMemberData"/>.</typeparam>
public interface ISymbolOrMember<out TSymbol, out TData> : ISymbolOrMember
	where TSymbol : class, ISymbol
	where TData : class, IMemberData
{
	/// <inheritdoc cref="ISymbolOrMember.Member"/>
	new TData Member { get; }

	/// <inheritdoc cref="ISymbolOrMember.Symbol"/>
	new TSymbol Symbol { get; }
}
