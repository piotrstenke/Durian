// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers
{
	public partial class LeveledSymbolContainer<TSymbol, TData> where TSymbol : class, ISymbol
		where TData : class, IMemberData
	{
		private sealed class LevelEntry
		{
			public InnerContainer? Container { get; set; }

			public Func<ISymbolOrMember<TSymbol, TData>, IReturnOrderEnumerable<ISymbolOrMember<TSymbol, TData>>> Creator { get; }

			public int StartIndex { get; set; }

			public LevelEntry(Func<ISymbolOrMember<TSymbol, TData>, IReturnOrderEnumerable<ISymbolOrMember<TSymbol, TData>>> creator)
			{
				Creator = creator;
			}
		}
	}
}
