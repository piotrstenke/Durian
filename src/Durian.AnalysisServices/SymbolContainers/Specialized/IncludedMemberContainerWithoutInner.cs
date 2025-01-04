using System;
using System.Collections.Generic;
using System.ComponentModel;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers.Specialized;

/// <summary>
/// <see cref="IncludedMemberContainer{TSymbol, TData}"/> that is configured to ignore or map <see cref="IncludedMembers.Inner"/>.
/// </summary>
/// <typeparam name="TSymbol">Type of returned <see cref="ISymbol"/>s.</typeparam>
/// <typeparam name="TData">Type of returned <see cref="IMemberData"/>s.</typeparam>s
public abstract class IncludedMemberContainerWithoutInner<TSymbol, TData> : IncludedMemberContainer<TSymbol, TData>
	where TSymbol : class, ISymbol
	where TData : class, IMemberData
{
	/// <summary>
	/// Initializes a new instance of the <see cref="IncludedMemberContainerWithoutInner{TSymbol, TData}"/> class.
	/// </summary>
	/// <param name="root"><see cref="ISymbol"/> that is a root of all the underlaying containers.</param>
	/// <param name="parentCompilation"><see cref="ICompilationData"/> used to create <typeparamref name="TData"/>s.</param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	/// <param name="includeRoot">Determines whether the <paramref name="root"/> should be included in the underlaying containers.</param>
	/// <exception cref="ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
	protected IncludedMemberContainerWithoutInner(
		ISymbolOrMember<TSymbol, TData> root,
		ICompilationData? parentCompilation = default,
		ISymbolNameResolver? nameResolver = default,
		bool includeRoot = false
	) : base(root, parentCompilation, nameResolver, includeRoot)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="IncludedMemberContainerWithoutInner{TSymbol, TData}"/> class.
	/// </summary>
	/// <param name="root"><see cref="ISymbol"/> that is a root of all the underlaying containers.</param>
	/// <param name="parentCompilation"><see cref="ICompilationData"/> used to create <typeparamref name="TData"/>s.</param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	/// <exception cref="ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
	protected IncludedMemberContainerWithoutInner(
		ISymbolOrMember root,
		ICompilationData? parentCompilation = default,
		ISymbolNameResolver? nameResolver = default
	) : base(root, parentCompilation, nameResolver)
	{
	}

	/// <inheritdoc cref="LeveledSymbolContainer{TSymbol, TData}.Reverse"/>
	public new IncludedMemberContainerWithoutInner<TSymbol, TData> Reverse()
	{
		return (base.Reverse() as IncludedMemberContainerWithoutInner<TSymbol, TData>)!;
	}

	/// <inheritdoc/>
	protected sealed override IncludedMembers MapLevel(int level)
	{
		return level switch
		{
			0 => IncludedMembers.Direct,
			1 => IncludedMembers.All,
			-1 => IncludedMembers.None,
			_ => (IncludedMembers)(level + 2)
		};
	}

	/// <inheritdoc/>
	protected sealed override bool AllowLevel(IncludedMembers members)
	{
		return members != IncludedMembers.Inner;
	}

	/// <inheritdoc/>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Container is configured to ignore or map IncludeMembers.Inner calls")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
	protected sealed override IEnumerable<ISymbolOrMember<TSymbol, TData>> Inner(ISymbolOrMember<TSymbol, TData> member)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
	{
		throw new InvalidOperationException("Container is configured to ignore or map IncludeMembers.Inner");
	}
}
