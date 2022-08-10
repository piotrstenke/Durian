// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers
{
	public partial class LeveledSymbolContainer<TSymbol, TData> where TSymbol : class, ISymbol
		where TData : class, IMemberData
	{
		internal sealed class InnerContainer : ISymbolContainer<TSymbol, TData>, IReturnOrderEnumerable<ISymbolOrMember<TSymbol, TData>>
		{
			private readonly LeveledSymbolContainer<TSymbol, TData> _parentContainer;

			public int Count => EndIndex;

			public int EndIndex { get; }

			public bool IsEmpty => EndIndex == 0;

			public ReturnOrder Order { get; private set; }

			public ICompilationData? ParentCompilation => _parentContainer.ParentCompilation;

			public ISymbolNameResolver SymbolNameResolver => _parentContainer.SymbolNameResolver;

			public InnerContainer(LeveledSymbolContainer<TSymbol, TData> parentContainer, int endIndex)
			{
				_parentContainer = parentContainer;
				EndIndex = endIndex;
				Order = _parentContainer.Order;
			}

			public ISymbolOrMember<TSymbol, TData> First()
			{
				return First(Order);
			}

			public ISymbolOrMember<TSymbol, TData> First(ReturnOrder order)
			{
				if (IsEmpty)
				{
					throw SymbolContainerFactory.Exc_EmptySymbolContainer();
				}

				if (order == ReturnOrder.ChildToParent)
				{
					return _parentContainer._data[EndIndex - 1];
				}

				return _parentContainer._data[0];
			}

			public ImmutableArray<TData> GetData()
			{
				return _parentContainer.GetData(EndIndex);
			}

			public ImmutableArray<string> GetNames()
			{
				return _parentContainer.GetNames(EndIndex);
			}

			public ImmutableArray<TSymbol> GetSymbols()
			{
				return _parentContainer.GetSymbols(EndIndex);
			}

			public ISymbolOrMember<TSymbol, TData> Last()
			{
				return Last(Order);
			}

			public ISymbolOrMember<TSymbol, TData> Last(ReturnOrder order)
			{
				if (IsEmpty)
				{
					throw SymbolContainerFactory.Exc_EmptySymbolContainer();
				}

				if (order == ReturnOrder.ChildToParent)
				{
					return _parentContainer._data[0];
				}

				return _parentContainer._data[EndIndex - 1];
			}

			public void Reverse()
			{
				Order = Order.Reverse();
			}

			ImmutableArray<IMemberData> ISymbolContainer.GetData()
			{
				return GetData().CastArray<IMemberData>();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumeratorAsInterface();
			}

			IEnumerator<ISymbolOrMember<TSymbol, TData>> IEnumerable<ISymbolOrMember<TSymbol, TData>>.GetEnumerator()
			{
				return GetEnumeratorAsInterface();
			}

			ImmutableArray<ISymbol> ISymbolContainer.GetSymbols()
			{
				return GetSymbols().CastArray<ISymbol>();
			}

			IReturnOrderEnumerable<ISymbolOrMember<TSymbol, TData>> IReturnOrderEnumerable<ISymbolOrMember<TSymbol, TData>>.Reverse()
			{
				Reverse();
				return this;
			}

			IReturnOrderEnumerable IReturnOrderEnumerable.Reverse()
			{
				Reverse();
				return this;
			}

			IEnumerable<ISymbolOrMember<TSymbol, TData>> ISymbolContainer<TSymbol, TData>.AsEnumerable()
			{
				return this;
			}

			IEnumerator<ISymbolOrMember<TSymbol, TData>> ISymbolContainer<TSymbol, TData>.GetEnumerator()
			{
				return GetEnumeratorAsInterface();
			}

			private IEnumerator<ISymbolOrMember<TSymbol, TData>> GetEnumeratorAsInterface()
			{
				if (Order == ReturnOrder.ChildToParent)
				{
					return _parentContainer.GetReversedEnumerator(EndIndex);
				}

				return _parentContainer.GetEnumerator(EndIndex);
			}
		}
	}
}
