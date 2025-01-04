using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers;

/// <summary>
/// Provides methods for returning symbol representations using either <see cref="ISymbol"/>s or <see cref="IMemberData"/>s.
/// </summary>
/// <typeparam name="TSymbol">Type of target <see cref="ISymbol"/>.</typeparam>
/// <typeparam name="TData">Type of target <see cref="IMemberData"/>.</typeparam>
public class SymbolContainer<TSymbol, TData> : SymbolContainerBase<TSymbol, TData>, IBuilderReceiver<SymbolContainerBuilder>
	where TSymbol : class, ISymbol
	where TData : class, IMemberData
{
	private enum BuilderState : byte
	{
		NoBuilder,
		Received,
		Missed
	}

	private BuilderState _builderState;

	/// <inheritdoc/>
	public sealed override int Count => Content.Count;

	/// <summary>
	/// <see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.
	/// </summary>
	public ISymbolNameResolver NameResolver { get; protected set; }

	/// <summary>
	/// List of all added symbols.
	/// </summary>
	protected List<ISymbolOrMember<TSymbol, TData>> Content { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="SymbolContainer{TSymbol, TData}"/> class.
	/// </summary>
	public SymbolContainer() : this(null, null)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SymbolContainer{TSymbol, TData}"/> class.
	/// </summary>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public SymbolContainer(ICompilationData? parentCompilation, ISymbolNameResolver? nameResolver = default) : base(parentCompilation)
	{
		Content = new();
		NameResolver = nameResolver ?? GetDefaultNameResolver();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SymbolContainer{TSymbol, TData}"/> class.
	/// </summary>
	/// <param name="capacity">Initial capacity of the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than <c>0</c>.</exception>
	public SymbolContainer(int capacity, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) : base(parentCompilation)
	{
		Content = new(capacity);
		NameResolver = nameResolver ?? GetDefaultNameResolver();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SymbolContainer{TSymbol, TData}"/> class.
	/// </summary>
	/// <param name="collection">Collection of <typeparamref name="TSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	/// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
	public SymbolContainer(IEnumerable<TSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) : base(parentCompilation, GetInitialOrder(collection))
	{
		Content = new();
		NameResolver = nameResolver ?? GetDefaultNameResolver();
		AddRange(collection);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SymbolContainer{TSymbol, TData}"/> class.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ISymbolOrMember"/> to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	/// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
	public SymbolContainer(IEnumerable<ISymbolOrMember<TSymbol, TData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) : base(parentCompilation, GetInitialOrder(collection))
	{
		Content = new();
		NameResolver = nameResolver ?? GetDefaultNameResolver();
		AddRange(collection);
	}

	/// <inheritdoc/>
	public sealed override ISymbolOrMember<TSymbol, TData> First(ReturnOrder order)
	{
		if (Content.Count == 0)
		{
			throw SymbolContainerFactory.Exc_EmptySymbolContainer();
		}

		if (order == Order)
		{
			return Content[0];
		}

		return Content[Content.Count - 1];
	}

	/// <inheritdoc/>
	public override ImmutableArray<TData> GetData()
	{
		return ImmutableArray.CreateRange(Content.Select(s => s.Member));
	}

	/// <inheritdoc/>
	public override ImmutableArray<string> GetNames()
	{
		return ImmutableArray.CreateRange(Content.Select(s => NameResolver.ResolveName(s)));
	}

	/// <inheritdoc cref="ISymbolContainer.GetSymbols"/>
	public override ImmutableArray<TSymbol> GetSymbols()
	{
		return ImmutableArray.CreateRange(Content.Select(s => s.Symbol));
	}

	/// <inheritdoc/>
	public sealed override ISymbolOrMember<TSymbol, TData> Last(ReturnOrder order)
	{
		if (Content.Count == 0)
		{
			throw SymbolContainerFactory.Exc_EmptySymbolContainer();
		}

		if (order == Order)
		{
			return Content[Content.Count - 1];
		}

		return Content[0];
	}

	/// <inheritdoc cref="SymbolContainerBase{TSymbol, TData}.Reverse"/>
	public new SymbolContainer<TSymbol, TData> Reverse()
	{
		return (base.Reverse() as SymbolContainer<TSymbol, TData>)!;
	}

	/// <inheritdoc/>
	protected sealed override void AddCore(ISymbolOrMember<TSymbol, TData> member)
	{
		Content.Add(member);
		ChangeBuilderState();
	}

	/// <inheritdoc/>
	protected sealed override void AddRangeCore(IEnumerable<ISymbolOrMember<TSymbol, TData>> collection)
	{
		Content.AddRange(collection);
		ChangeBuilderState();
	}

	/// <summary>
	/// Returns the <see cref="ISymbolNameResolver"/> that is used when no other is specified (either through constructor or a <see cref="SymbolContainerBuilder{T}"/>).
	/// </summary>
	protected virtual ISymbolNameResolver GetDefaultNameResolver()
	{
		return SymbolNameResolver.Default;
	}

	/// <inheritdoc/>
	protected sealed override IEnumerator<ISymbolOrMember<TSymbol, TData>> GetEnumeratorCore()
	{
		return Content.GetEnumerator();
	}

	/// <inheritdoc cref="IBuilderReceiver{TBuilder}.Receive(TBuilder)"/>
	/// <exception cref="InvalidOperationException">Object cannot receive more than one builder. -or- Builder cannot be passed after manual initialization of the object.</exception>
	protected virtual void Receive(ref SymbolContainerBuilder builder)
	{
		if (_builderState == BuilderState.Received)
		{
			throw new InvalidOperationException("Object cannot receive more than one builder");
		}

		if (_builderState == BuilderState.Missed)
		{
			throw new InvalidOperationException("Builder cannot be passed after manual initialization of the object");
		}

		ParentCompilation = builder.ParentCompilation;
		NameResolver = builder.SymbolNameResolver ?? GetDefaultNameResolver();

		switch (builder.Collection)
		{
			case IEnumerable<TSymbol> symbols:
				AddRange(symbols);
				break;

			case IEnumerable<ISymbolOrMember<TSymbol, TData>> members:
				AddRange(members);
				break;
		}

		_builderState = BuilderState.Received;
	}

	/// <inheritdoc/>
	protected sealed override void ReverseCore()
	{
		Content.Reverse();
	}

	/// <inheritdoc/>
	protected sealed override bool SealCore()
	{
		Content.TrimExcess();

		return true;
	}

	void IBuilderReceiver<SymbolContainerBuilder>.Receive(SymbolContainerBuilder builder)
	{
		Receive(ref builder);
	}

	private void ChangeBuilderState()
	{
		if (_builderState == BuilderState.NoBuilder)
		{
			_builderState = BuilderState.Missed;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ReturnOrder GetInitialOrder(IEnumerable collection)
	{
		return collection switch
		{
			IReturnOrderEnumerable<TSymbol> symbol => symbol.Order,
			IReturnOrderEnumerable<ISymbolOrMember> member => member.Order,
			_ => default
		};
	}
}
