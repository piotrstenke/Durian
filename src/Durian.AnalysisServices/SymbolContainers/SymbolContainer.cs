// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

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

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// Provides methods for returning symbol representations using either <see cref="ISymbol"/>s or <see cref="IMemberData"/>s.
	/// </summary>
	/// <typeparam name="TSymbol">Type of target <see cref="ISymbol"/>.</typeparam>
	[DebuggerDisplay("Count = {Count}, Order = {Order}")]
	public class SymbolContainer<TSymbol> : ISymbolContainer, IReturnOrderEnumerable<ISymbolOrMember<TSymbol>> where TSymbol : class, ISymbol
	{
		private ReturnOrder _order;

		/// <summary>
		/// Number of elements in the container.
		/// </summary>
		public int Count => Content.Count;

		/// <summary>
		/// Order in which the elements are added.
		/// </summary>
		public ReturnOrder Order
		{
			get => _order;
			set
			{
				if (_order == value)
				{
					return;
				}

				Reverse();
			}
		}

		/// <summary>
		/// Parent <see cref="ICompilationData"/> of the current container.
		/// </summary>
		public ICompilationData? ParentCompilation { get; }

		/// <summary>
		/// List of all <see cref="ISymbolOrMember"/>s added to the container.
		/// </summary>
		protected List<ISymbolOrMember<TSymbol>> Content { get; } = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="SymbolContainer{TSymbol}"/> class.
		/// </summary>
		/// <param name="parentCompilation">Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para></param>
		public SymbolContainer(ICompilationData? parentCompilation = default)
		{
			ParentCompilation = parentCompilation;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SymbolContainer{TSymbol}"/> class.
		/// </summary>
		/// <param name="capacity">Initial capacity of the container.</param>
		/// <param name="parentCompilation">Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para></param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than <c>0</c>.</exception>
		public SymbolContainer(int capacity, ICompilationData? parentCompilation = default)
		{
			Content = new(capacity);
			ParentCompilation = parentCompilation;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SymbolContainer{TSymbol}"/> class.
		/// </summary>
		/// <param name="collection">Collection of <typeparamref name="TSymbol"/>s to add to the container.</param>
		/// <param name="parentCompilation">Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para></param>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
		public SymbolContainer(IEnumerable<TSymbol> collection, ICompilationData? parentCompilation = default)
		{
			Order = GetInitialOrder(collection);
			ParentCompilation = parentCompilation;
			AddRange(collection);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SymbolContainer{TSymbol}"/> class.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ISymbolOrMember"/> to add to the container.</param>
		/// <param name="parentCompilation">Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para></param>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
		public SymbolContainer(IEnumerable<ISymbolOrMember<TSymbol>> collection, ICompilationData? parentCompilation = default)
		{
			Order = GetInitialOrder(collection);
			ParentCompilation = parentCompilation;
			AddRange(collection);
		}

		/// <summary>
		/// Adds the specified <paramref name="symbol"/> to the container.
		/// </summary>
		/// <param name="symbol"><typeparamref name="TSymbol"/> to add to the container.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public void Add(TSymbol symbol)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			Content.Add(new SymbolOrMemberWrapper<TSymbol, IMemberData>(symbol, ParentCompilation));
		}

		/// <summary>
		/// Adds the specified <paramref name="member"/> to the container.
		/// </summary>
		/// <param name="member"><see cref="ISymbolOrMember"/> to add to the container.</param>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>.</exception>
		public void Add(ISymbolOrMember<TSymbol> member)
		{
			if (member is null)
			{
				throw new ArgumentNullException(nameof(member));
			}

			Content.Add(member);
		}

		/// <summary>
		/// Adds the specified <paramref name="collection"/> of <see cref="ISymbol"/>s to the container.
		/// </summary>
		/// <param name="collection">Collection of <typeparamref name="TSymbol"/>s to add to the container.</param>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
		public void AddRange(IEnumerable<TSymbol> collection)
		{
			if (collection is null)
			{
				throw new ArgumentNullException(nameof(collection));
			}

			Content.AddRange(collection.Select(symbol => new SymbolOrMemberWrapper<TSymbol, IMemberData>(symbol, ParentCompilation)));
		}

		/// <summary>
		/// Adds the specified <paramref name="collection"/> of <see cref="ISymbolOrMember"/>s to the container.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ISymbolOrMember"/>s to add to the container.</param>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
		public void AddRange(IEnumerable<ISymbolOrMember<TSymbol>> collection)
		{
			if (collection is null)
			{
				throw new ArgumentNullException(nameof(collection));
			}

			Content.AddRange(collection);
		}

		/// <inheritdoc/>
		public virtual ImmutableArray<IMemberData> GetData()
		{
			return ImmutableArray.CreateRange(Content.Select(s => s.Member));
		}

		/// <inheritdoc/>
		public IEnumerator<ISymbolOrMember<TSymbol>> GetEnumerator()
		{
			return Content.GetEnumerator();
		}

		/// <inheritdoc/>
		public virtual ImmutableArray<string> GetNames()
		{
			return ImmutableArray.CreateRange(Content.Select(s => s.HasMember ? s.Member.Name : s.Symbol.GetVerbatimName()));
		}

		/// <inheritdoc cref="ISymbolContainer.GetSymbols"/>
		public virtual ImmutableArray<TSymbol> GetSymbols()
		{
			return ImmutableArray.CreateRange(Content.Select(s => s.Symbol));
		}

		/// <summary>
		/// Reverses the container.
		/// </summary>
		public void Reverse()
		{
			_order = Order.Reverse();
			Content.Reverse();
		}

		/// <summary>
		/// Returns the first member according to the current <see cref="Order"/>.
		/// </summary>
		public ISymbolOrMember<TSymbol> First()
		{
			return First(Order);
		}

		/// <summary>
		/// Returns the first member according to the specified <paramref name="order"/>.
		/// </summary>
		/// <param name="order"><see cref="ReturnOrder"/> by which to return the first member.</param>
		public ISymbolOrMember<TSymbol> First(ReturnOrder order)
		{
			if (order == Order)
			{
				return Content[0];
			}

			return Content[Content.Count - 1];
		}

		/// <summary>
		/// Returns the last member according to the current <see cref="Order"/>.
		/// </summary>
		public ISymbolOrMember<TSymbol> Last()
		{
			return Last(Order);
		}

		/// <summary>
		/// Returns the first member according to the specified <paramref name="order"/>.
		/// </summary>
		/// <param name="order"><see cref="ReturnOrder"/> by which to return the last member.</param>
		public ISymbolOrMember<TSymbol> Last(ReturnOrder order)
		{
			if (order == Order)
			{
				return Content[Content.Count - 1];
			}

			return Content[0];
		}

		/// <summary>
		/// Converts the current array into an array of <typeparamref name="TSymbol"/>s.
		/// </summary>
		public virtual TSymbol[] ToArray()
		{
			return Content.Select(s => s.Symbol).ToArray();
		}

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		ImmutableArray<ISymbol> ISymbolContainer.GetSymbols()
		{
			return GetSymbols().CastArray<ISymbol>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ReturnOrder GetInitialOrder(IEnumerable collection)
		{
			return collection switch
			{
				IReturnOrderEnumerable<TSymbol> symbol => symbol.Order,
				IReturnOrderEnumerable<ISymbolOrMember<TSymbol>> member => member.Order,
				_ => default
			};
		}
	}
}
