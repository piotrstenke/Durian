// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers.Specialized
{
	/// <summary>
	/// <see cref="ILeveledSymbolContainer{TSymbol, TData}"/> that handles inner members of a symbol.
	/// </summary>
	internal sealed class InnerMembersContainer<TSymbol, TData, TParentSymbol, TParentData> : IncludedMembersSymbolContainerWithoutInner<TSymbol, TData>
		where TSymbol : class, ISymbol
		where TData : class, IMemberData
		where TParentSymbol : class, ISymbol
		where TParentData : class, IMemberData
	{
		public LeveledSymbolContainer<TParentSymbol, TParentData> ParentContainer { get; }

		public InnerMembersContainer(
			LeveledSymbolContainer<TParentSymbol, TParentData> parentContainer,
			ISymbolOrMember<TSymbol, TData> root
		) : base(root, parentContainer.ParentCompilation, parentContainer.SymbolNameResolver, parentContainer.IncludeRoot)
		{
			ParentContainer = parentContainer;
		}

		public InnerMembersContainer(
			LeveledSymbolContainer<TParentSymbol, TParentData> parentContainer,
			ISymbolOrMember root
		) : base(root, parentContainer.ParentCompilation, parentContainer.SymbolNameResolver)
		{
			ParentContainer = parentContainer;
		}

		/// <inheritdoc/>
		private protected override bool IsHandledExternally(int level)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			LeveledSymbolContainer<TParentSymbol, TParentData>.InnerContainer container = (ParentContainer.ResolveLevel(level) as LeveledSymbolContainer<TParentSymbol, TParentData>.InnerContainer)!;
#pragma warning restore CS0618 // Type or member is obsolete

			LeveledSymbolContainer<TParentSymbol, TParentData>.LevelEntry levelEntry = ParentContainer._levels[level];
			List<ISymbolOrMember<TParentSymbol, TParentData>> baseList = ParentContainer._data;

			if (levelEntry.IsEmpty)
			{
				return true;
			}

			int start = levelEntry.StartIndex;
			int end = container.EndIndex;

			for (int i = start; i < end; i++)
			{
				if (baseList[i] is ISymbolOrMember<TSymbol, TData> @namespace)
				{
					_data.Add(@namespace);
				}
			}

			return true;
		}

		/// <inheritdoc/>
		protected override IEnumerable<ISymbolOrMember<TSymbol, TData>> All(ISymbolOrMember<TSymbol, TData> member)
		{
			return Array.Empty<ISymbolOrMember<TSymbol, TData>>();
		}

		/// <inheritdoc/>
		protected override IEnumerable<ISymbolOrMember<TSymbol, TData>> Direct(ISymbolOrMember<TSymbol, TData> member)
		{
			return Array.Empty<ISymbolOrMember<TSymbol, TData>>();
		}

		/// <inheritdoc/>
		protected override IEnumerable<ISymbolOrMember<TSymbol, TData>> ResolveRoot(ISymbolOrMember root)
		{
			return Array.Empty<ISymbolOrMember<TSymbol, TData>>();
		}
	}
}
