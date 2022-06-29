// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// <see cref="LeveledSymbolContainer{TSymbol, TData}"/> that uses <see cref="IncludedMembers"/> instead of <see cref="int"/>s.
	/// </summary>
	/// <typeparam name="TSymbol">Type of returned <see cref="ISymbol"/>s.</typeparam>
	/// <typeparam name="TData">Type of returned <see cref="IMemberData"/>s.</typeparam>s
	public abstract class IncludedMembersSymbolContainer<TSymbol, TData> : LeveledSymbolContainer<TSymbol, TData>
		where TSymbol : class, ISymbol
		where TData : class, IMemberData
	{
		/// <summary>
		/// Determines whether levels beyond the limit of <see cref="IncludedMembers"/> can be registered.
		/// </summary>
		protected abstract bool AllowCustomLevels { get; }

		/// <summary>
		/// Max <see cref="IncludedMembers"/> level possible for the current container.
		/// </summary>
		protected abstract IncludedMembers Limit { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="IncludedMembersSymbolContainer{TSymbol, TData}"/> class.
		/// </summary>
		/// <param name="root"><see cref="ISymbol"/> that is a root of all the underlaying containers.</param>
		/// <param name="includeRoot">Determines whether the <paramref name="root"/> should be included in the underlaying containers.</param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		/// <param name="parentCompilation"><see cref="ICompilationData"/> used to create <typeparamref name="TData"/>s.</param>
		/// <exception cref="ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
		protected IncludedMembersSymbolContainer(
			ISymbolOrMember<TSymbol, TData> root,
			bool includeRoot = false,
			ISymbolNameResolver? nameResolver = default,
			ICompilationData? parentCompilation = default
		) : base(root, includeRoot, nameResolver, parentCompilation)
		{
			InitLevels();
		}

		/// <inheritdoc cref="LeveledSymbolContainer{TSymbol, TData}.ResolveLevel(int)"/>
		public ISymbolContainer<TSymbol, TData> ResolveLevel(IncludedMembers level)
		{
			return ResolveLevel((int)level);
		}

		/// <inheritdoc cref="LeveledSymbolContainer{TSymbol, TData}.Reverse"/>
		public new IncludedMembersSymbolContainer<TSymbol, TData> Reverse()
		{
			return (base.Reverse() as IncludedMembersSymbolContainer<TSymbol, TData>)!;
		}

		/// <summary>
		/// Returns a <see cref="IReturnOrderEnumerable{T}"/> representing the <see cref="IncludedMembers.Direct"/> of the <see cref="LeveledSymbolContainer{TSymbol, TData}.Root"/>.
		/// </summary>
		/// <param name="member"><see cref="ISymbolOrMember"/> to get the <see cref="IncludedMembers.Direct"/> for.</param>
		protected abstract IReturnOrderEnumerable<ISymbolOrMember<TSymbol, TData>> All(ISymbolOrMember<TSymbol, TData> member);

		/// <summary>
		/// Returns a <see cref="IReturnOrderEnumerable{T}"/> representing the <see cref="IncludedMembers.Direct"/> of the <see cref="LeveledSymbolContainer{TSymbol, TData}.Root"/>.
		/// </summary>
		/// <param name="member"><see cref="ISymbolOrMember"/> to get the <see cref="IncludedMembers.Direct"/> for.</param>
		protected abstract IReturnOrderEnumerable<ISymbolOrMember<TSymbol, TData>> Direct(ISymbolOrMember<TSymbol, TData> member);

		/// <summary>
		/// Returns a <see cref="IReturnOrderEnumerable{T}"/> representing the <see cref="IncludedMembers.Direct"/> of the <see cref="LeveledSymbolContainer{TSymbol, TData}.Root"/>.
		/// </summary>
		/// <param name="member"><see cref="ISymbolOrMember"/> to get the <see cref="IncludedMembers.Direct"/> for.</param>
		protected abstract IReturnOrderEnumerable<ISymbolOrMember<TSymbol, TData>> Inner(ISymbolOrMember<TSymbol, TData> member);

		private void InitLevels()
		{
			IncludedMembers level = Limit;

			if (level > IncludedMembers.Direct)
			{
				RegisterLevel(Direct);
			}

			if (level > IncludedMembers.Inner)
			{
				RegisterLevel(Inner);
			}

			if (level > IncludedMembers.All)
			{
				RegisterLevel(All);
			}

			if (!AllowCustomLevels)
			{
				Seal();
			}
		}
	}
}
