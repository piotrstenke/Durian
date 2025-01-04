using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers;

/// <summary>
/// Provides methods for returning symbol representations using either <see cref="ISymbol"/>s or <see cref="IMemberData"/>s.
/// </summary>
/// <typeparam name="TSymbol">Type of target <see cref="ISymbol"/>.</typeparam>
/// <typeparam name="TData">Type of target <see cref="IMemberData"/>.</typeparam>
[DebuggerDisplay("Count = {Count}, Order = {Order}")]
public abstract class SymbolContainerBase<TSymbol, TData> : PermanentSealableObject, ISymbolContainer<TSymbol, TData>, IReturnOrderEnumerable<ISymbolOrMember<TSymbol, TData>>
	where TSymbol : class, ISymbol
	where TData : class, IMemberData
{
	/// <inheritdoc/>
	public abstract int Count { get; }

	/// <summary>
	/// Order in which the elements are added.
	/// </summary>
	public ReturnOrder Order { get; private set; }

	/// <inheritdoc/>
	public ICompilationData? ParentCompilation { get; protected set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="SymbolContainerBase{TSymbol, TData}"/> class.
	/// </summary>
	protected SymbolContainerBase()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SymbolContainerBase{TSymbol, TData}"/> class.
	/// </summary>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="order">Current <see cref="ReturnOrder"/>.</param>
	protected SymbolContainerBase(ICompilationData? parentCompilation, ReturnOrder order = default)
	{
		ParentCompilation = parentCompilation;
		Order = order;
	}

	/// <summary>
	/// Adds the specified <paramref name="symbol"/> to the container.
	/// </summary>
	/// <param name="symbol"><typeparamref name="TSymbol"/> to add to the container.</param>
	/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
	/// <exception cref="SealedObjectException">Cannot add new symbols to a sealed container.</exception>s
	public void Add(TSymbol symbol)
	{
		if (symbol is null)
		{
			throw new ArgumentNullException(nameof(symbol));
		}

		EnsureNotSealed();

		AddCore(new SymbolOrMemberWrapper<TSymbol, TData>(symbol, ParentCompilation));
	}

	/// <summary>
	/// Adds the specified <paramref name="member"/> to the container.
	/// </summary>
	/// <param name="member"><see cref="ISymbolOrMember"/> to add to the container.</param>
	/// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>.</exception>
	/// <exception cref="SealedObjectException">Cannot add new symbols to a sealed container.</exception>
	public void Add(ISymbolOrMember<TSymbol, TData> member)
	{
		if (member is null)
		{
			throw new ArgumentNullException(nameof(member));
		}

		EnsureNotSealed();

		AddCore(member);
	}

	/// <summary>
	/// Adds the specified <paramref name="collection"/> of <see cref="ISymbol"/>s to the container.
	/// </summary>
	/// <param name="collection">Collection of <typeparamref name="TSymbol"/>s to add to the container.</param>
	/// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
	/// <exception cref="SealedObjectException">Cannot add new symbols to a sealed container.</exception>
	public void AddRange(IEnumerable<TSymbol> collection)
	{
		if (collection is null)
		{
			throw new ArgumentNullException(nameof(collection));
		}

		EnsureNotSealed();

		AddRangeCore(collection.Select(symbol => new SymbolOrMemberWrapper<TSymbol, TData>(symbol, ParentCompilation)));
	}

	/// <summary>
	/// Adds the specified <paramref name="collection"/> of <see cref="ISymbolOrMember"/>s to the container.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ISymbolOrMember"/>s to add to the container.</param>
	/// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
	/// <exception cref="SealedObjectException">Cannot add new symbols to a sealed container.</exception>
	public void AddRange(IEnumerable<ISymbolOrMember<TSymbol, TData>> collection)
	{
		if (collection is null)
		{
			throw new ArgumentNullException(nameof(collection));
		}

		EnsureNotSealed();

		AddRangeCore(collection);
	}

	/// <summary>
	/// Returns the first member according to the current <see cref="Order"/>.
	/// </summary>
	/// <exception cref="EmptyContainerException">Container does not contain any symbols.</exception>
	public ISymbolOrMember<TSymbol, TData> First()
	{
		return First(Order);
	}

	/// <inheritdoc/>
	public abstract ISymbolOrMember<TSymbol, TData> First(ReturnOrder order);

	/// <inheritdoc/>
	public abstract ImmutableArray<TData> GetData();

	/// <inheritdoc/>
	public abstract ImmutableArray<string> GetNames();

	/// <inheritdoc cref="ISymbolContainer.GetSymbols"/>
	public abstract ImmutableArray<TSymbol> GetSymbols();

	/// <summary>
	/// Returns the last member according to the current <see cref="Order"/>.
	/// </summary>
	/// <exception cref="EmptyContainerException">Container does not contain any symbols.</exception>
	public ISymbolOrMember<TSymbol, TData> Last()
	{
		return Last(Order);
	}

	/// <inheritdoc/>
	public abstract ISymbolOrMember<TSymbol, TData> Last(ReturnOrder order);

	/// <inheritdoc cref="ISymbolContainer.Reverse()"/>
	public SymbolContainerBase<TSymbol, TData> Reverse()
	{
		Order = Order.Reverse();
		ReverseCore();

		return this;
	}

	/// <summary>
	/// Adds the specified <paramref name="member"/> to the container.
	/// </summary>
	/// <param name="member"><see cref="ISymbolOrMember"/> to add to the container.</param>
	protected abstract void AddCore(ISymbolOrMember<TSymbol, TData> member);

	/// <summary>
	/// Adds the specified <paramref name="collection"/> of <see cref="ISymbolOrMember"/>s to the container.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ISymbolOrMember"/>s to add to the container.</param>
	protected abstract void AddRangeCore(IEnumerable<ISymbolOrMember<TSymbol, TData>> collection);

	/// <inheritdoc/>
	protected abstract IEnumerator<ISymbolOrMember<TSymbol, TData>> GetEnumeratorCore();

	/// <summary>
	/// Reverses the contents of the container.
	/// </summary>
	protected virtual void ReverseCore()
	{
		// Do nothing.
	}

	ImmutableArray<IMemberData> ISymbolContainer.GetData()
	{
		return GetData().CastArray<IMemberData>();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumeratorCore();
	}

	IEnumerator<ISymbolOrMember<TSymbol, TData>> ISymbolContainer<TSymbol, TData>.GetEnumerator()
	{
		return GetEnumeratorCore();
	}

	IEnumerator<ISymbolOrMember<TSymbol, TData>> IEnumerable<ISymbolOrMember<TSymbol, TData>>.GetEnumerator()
	{
		return GetEnumeratorCore();
	}

	ImmutableArray<ISymbol> ISymbolContainer.GetSymbols()
	{
		return GetSymbols().CastArray<ISymbol>();
	}

	void ISymbolContainer.Reverse()
	{
		Reverse();
	}

	IReturnOrderEnumerable<ISymbolOrMember<TSymbol, TData>> IReturnOrderEnumerable<ISymbolOrMember<TSymbol, TData>>.Reverse()
	{
		return Reverse();
	}

	IReturnOrderEnumerable IReturnOrderEnumerable.Reverse()
	{
		return Reverse();
	}

	IEnumerable<ISymbolOrMember<TSymbol, TData>> ISymbolContainer<TSymbol, TData>.AsEnumerable()
	{
		return this;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void EnsureNotSealed()
	{
		if (IsSealed)
		{
			throw new SealedObjectException(this, "Cannot add new symbols to a sealed container");
		}
	}
}
