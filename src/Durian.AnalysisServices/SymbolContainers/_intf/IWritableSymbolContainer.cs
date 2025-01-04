using System.Text;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers;

/// <summary>
/// <see cref="ISymbolContainer"/> that writes name of the contained <see cref="ISymbol"/>s into a <see cref="StringBuilder"/>.
/// </summary>
public interface IWritableSymbolContainer : ISymbolContainer
{
	/// <summary>
	/// Writes the contents of this container to the specified <paramref name="builder"/>.
	/// </summary>
	/// <param name="builder"><see cref="StringBuilder"/> to write the contents of this container to.</param>
	void WriteTo(StringBuilder builder);
}

/// <inheritdoc cref="IWritableSymbolContainer"/>
/// <typeparam name="TSymbol">Type of target <see cref="ISymbol"/>.</typeparam>
/// <typeparam name="TData">Type of target <see cref="IMemberData"/>.</typeparam>
public interface IWritableSymbolContainer<out TSymbol, out TData> : ISymbolContainer<TSymbol, TData>, IWritableSymbolContainer
	where TSymbol : class, ISymbol
	where TData : class, IMemberData
{
}
