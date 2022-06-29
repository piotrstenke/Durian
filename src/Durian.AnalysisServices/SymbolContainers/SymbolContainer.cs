// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// Provides methods for returning symbol representations using either <see cref="ISymbol"/>s or <see cref="IMemberData"/>s.
	/// </summary>
	/// <typeparam name="TSymbol">Type of target <see cref="ISymbol"/>.</typeparam>
	/// <typeparam name="TData">Type of target <see cref="IMemberData"/>.</typeparam>
	public class SymbolContainer<TSymbol, TData> : SymbolContainerBase<TSymbol, TData>, IBuilderReceiver<SymbolContainerBuilder<SymbolContainer<TSymbol, TData>>>
		where TSymbol : class, ISymbol
		where TData : class, IMemberData
	{
		/// <inheritdoc/>
		public override int Count => Content.Count;

		/// <summary>
		/// <see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.
		/// </summary>
		public ISymbolNameResolver SymbolNameResolver { get; }

		/// <summary>
		/// List of all added symbols.
		/// </summary>
		protected List<ISymbolOrMember<TSymbol, TData>> Content { get; }

		private SymbolContainer()
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
		public SymbolContainer(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) : base(parentCompilation)
		{
			Content = new();
			SymbolNameResolver = nameResolver ?? Analysis.SymbolNameResolver.Default;
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
			SymbolNameResolver = nameResolver ?? Analysis.SymbolNameResolver.Default;
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
		public SymbolContainer(IEnumerable<TSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) : base(collection, parentCompilation)
		{
			Content = new();
			SymbolNameResolver = nameResolver ?? Analysis.SymbolNameResolver.Default;
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
		public SymbolContainer(IEnumerable<ISymbolOrMember<TSymbol, TData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) : base(collection, parentCompilation)
		{
			Content = new();
			SymbolNameResolver = nameResolver ?? Analysis.SymbolNameResolver.Default;
			AddRange(collection);
		}

		/// <inheritdoc/>
		public override ISymbolOrMember<TSymbol, TData> First(ReturnOrder order)
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
			return ImmutableArray.CreateRange(Content.Select(s => SymbolNameResolver.ResolveName(s)));
		}

		/// <inheritdoc cref="ISymbolContainer.GetSymbols"/>
		public override ImmutableArray<TSymbol> GetSymbols()
		{
			return ImmutableArray.CreateRange(Content.Select(s => s.Symbol));
		}

		/// <inheritdoc/>
		public override ISymbolOrMember<TSymbol, TData> Last(ReturnOrder order)
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
		protected override void AddCore(ISymbolOrMember<TSymbol, TData> member)
		{
			Content.Add(member);
		}

		/// <inheritdoc/>
		protected override void AddRangeCore(IEnumerable<ISymbolOrMember<TSymbol, TData>> collection)
		{
			Content.AddRange(collection);
		}

		/// <inheritdoc/>
		protected override IEnumerator<ISymbolOrMember<TSymbol, TData>> GetEnumeratorCore()
		{
			return Content.GetEnumerator();
		}

		/// <inheritdoc/>
		protected override void ReverseCore()
		{
			Content.Reverse();
		}

		/// <inheritdoc/>
		protected override bool SealCore()
		{
			Content.TrimExcess();

			return true;
		}

		void IBuilderReceiver<SymbolContainerBuilder<SymbolContainer<TSymbol, TData>>>.Receive(SymbolContainerBuilder<SymbolContainer<TSymbol, TData>> builder)
		{
			throw new NotImplementedException();
		}
	}
}
