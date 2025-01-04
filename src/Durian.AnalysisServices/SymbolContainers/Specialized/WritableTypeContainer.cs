using System;
using System.Collections.Generic;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers.Specialized;

/// <summary>
/// <see cref="WritableSymbolContainer{TSymbol, TData}"/> that provides default configuration for handling <see cref="INamedTypeSymbol"/>s.
/// </summary>
/// <typeparam name="TData">Type of target <see cref="ITypeData"/>.</typeparam>
public sealed class WritableTypeContainer<TData> : WritableSymbolContainer<INamedTypeSymbol, TData> where TData : class, ITypeData
{
	/// <summary>
	/// Initializes a new instance of the <see cref="WritableTypeContainer{TData}"/> class.
	/// </summary>
	public WritableTypeContainer()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="WritableTypeContainer{TData}"/> class.
	/// </summary>
	/// <param name="parentCompilation">Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para></param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public WritableTypeContainer(ICompilationData? parentCompilation, ISymbolNameResolver? nameResolver = default) : base(parentCompilation, nameResolver)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="WritableTypeContainer{TData}"/> class.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para></param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	/// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
	public WritableTypeContainer(IEnumerable<INamedTypeSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) : base(collection, parentCompilation, nameResolver)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="WritableTypeContainer{TData}"/> class.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ISymbolOrMember"/> to add to the container.</param>
	/// <param name="parentCompilation">Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para></param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	/// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
	public WritableTypeContainer(IEnumerable<ISymbolOrMember<INamedTypeSymbol, TData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) : base(collection, parentCompilation, nameResolver)
	{
	}

	/// <inheritdoc/>
	protected override ISymbolNameResolver GetDefaultNameResolver()
	{
		return SymbolNameResolver.GetResolver(SymbolName.Generic);
	}
}
