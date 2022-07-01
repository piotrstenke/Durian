// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers.Specialized
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
		/// Current nesting level of <see cref="ISymbolContainer"/>s or <c>-1</c> if no internal container is initialized.
		/// </summary>
		public new IncludedMembers CurrentLevel => MapLevel((IncludedMembers)base.CurrentLevel);

		/// <summary>
		/// Maximal possible nesting level (<see cref="LeveledSymbolContainer{TSymbol, TData}.NumLevels"/> - 1).
		/// </summary>
		public new IncludedMembers MaxLevel => MapLevel((IncludedMembers)base.MaxLevel);

		/// <summary>
		/// Determines whether levels beyond the limit of <see cref="IncludedMembers"/> can be registered.
		/// </summary>
		protected virtual bool AllowCustomLevels => false;

		/// <summary>
		/// Max <see cref="IncludedMembers"/> level possible for the current container.
		/// </summary>
		protected virtual IncludedMembers Limit => IncludedMembers.All;

		/// <summary>
		/// Initializes a new instance of the <see cref="IncludedMembersSymbolContainer{TSymbol, TData}"/> class.
		/// </summary>
		/// <param name="root"><see cref="ISymbol"/> that is a root of all the underlaying containers.</param>
		/// <param name="includeRoot">Determines whether the <paramref name="root"/> should be included in the underlaying containers.</param>
		/// <param name="parentCompilation"><see cref="ICompilationData"/> used to create <typeparamref name="TData"/>s.</param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		/// <exception cref="ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
		protected IncludedMembersSymbolContainer(
			ISymbolOrMember<TSymbol, TData> root,
			bool includeRoot = false,
			ICompilationData? parentCompilation = default,
			ISymbolNameResolver? nameResolver = default
		) : base(root, includeRoot, parentCompilation, nameResolver)
		{
			InitLevels();
		}

		/// <inheritdoc cref="LeveledSymbolContainer{TSymbol, TData}.ResolveLevel(int)"/>
		public ISymbolContainer<TSymbol, TData> ResolveLevel(IncludedMembers level)
		{
			level = MapLevel(level);
			return base.ResolveLevel((int)level);
		}

		/// <inheritdoc cref="LeveledSymbolContainer{TSymbol, TData}.ResolveLevel(int)"/>
		[Obsolete("Use ResolveLevel(IncludedMembers) instead")]
		public new ISymbolContainer<TSymbol, TData> ResolveLevel(int level)
		{
			return base.ResolveLevel(level);
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
		protected abstract IEnumerable<ISymbolOrMember<TSymbol, TData>> All(ISymbolOrMember<TSymbol, TData> member);

		/// <summary>
		/// Returns a <see cref="IReturnOrderEnumerable{T}"/> representing the <see cref="IncludedMembers.Direct"/> of the <see cref="LeveledSymbolContainer{TSymbol, TData}.Root"/>.
		/// </summary>
		/// <param name="member"><see cref="ISymbolOrMember"/> to get the <see cref="IncludedMembers.Direct"/> for.</param>
		protected abstract IEnumerable<ISymbolOrMember<TSymbol, TData>> Direct(ISymbolOrMember<TSymbol, TData> member);

		/// <summary>
		/// Returns a <see cref="IReturnOrderEnumerable{T}"/> representing the <see cref="IncludedMembers.Direct"/> of the <see cref="LeveledSymbolContainer{TSymbol, TData}.Root"/>.
		/// </summary>
		/// <param name="member"><see cref="ISymbolOrMember"/> to get the <see cref="IncludedMembers.Direct"/> for.</param>
		protected abstract IEnumerable<ISymbolOrMember<TSymbol, TData>> Inner(ISymbolOrMember<TSymbol, TData> member);

		/// <summary>
		/// Maps the current <see cref="IncludedMembers"/> to a different value.
		/// </summary>
		/// <param name="level"><see cref="IncludedMembers"/> to map.</param>
		protected virtual IncludedMembers MapLevel(IncludedMembers level)
		{
			return level;
		}

		/// <summary>
		/// Determines whether the specified <see cref="IncludedMembers"/> can be registered as a valid level.
		/// </summary>
		/// <param name="level"><see cref="IncludedMembers"/> to determine whether can be registered as a valid level.</param>
		protected virtual bool AllowLevel(IncludedMembers level)
		{
			return true;
		}

		/// <inheritdoc/>
		[Obsolete("Use OnLevelFilled(IncludedMembers) instead")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		protected sealed override void OnLevelFilled(int level)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
		{
			IncludedMembers members = MapLevel((IncludedMembers)level);
			OnLevelFilled(members);
		}

		/// <inheritdoc/>
		[Obsolete("Use OnLevelFilled(IncludedMembers) instead")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		protected sealed override void OnLevelReady(int level)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
		{
			IncludedMembers members = MapLevel((IncludedMembers)level);
			OnLevelReady(members);
		}

		/// <inheritdoc cref="LeveledSymbolContainer{TSymbol, TData}.OnLevelReady(int)"/>
		protected virtual void OnLevelReady(IncludedMembers level)
		{
			// Do nothing.
		}

		/// <inheritdoc cref="LeveledSymbolContainer{TSymbol, TData}.OnLevelFilled(int)"/>
		protected virtual void OnLevelFilled(IncludedMembers level)
		{
			// Do nothing.
		}

		/// <inheritdoc/>
		[Obsolete("Use SkipMember(ISymbolOrMember<TSymbol, TData>, IncludedMembers) instead")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		protected sealed override bool SkipMember(ISymbolOrMember<TSymbol, TData> member, int level)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
		{
			IncludedMembers members = MapLevel((IncludedMembers)level);
			return SkipMember(member, members);
		}

		/// <summary>
		/// Determines whether the <paramref name="member"/> and its members should be skipped when retrieving a <see cref="IReturnOrderEnumerable{T}"/>.
		/// </summary>
		/// <param name="member"><see cref="ISymbolContainer{TSymbol, TData}"/> to determine whether to skip.</param>
		/// <param name="level">Currently filled level.</param>
		protected virtual bool SkipMember(ISymbolOrMember<TSymbol, TData> member, IncludedMembers level)
		{
			return false;
		}

		private void InitLevels()
		{
			IncludedMembers level = MaxLevel + 1;

			if (CheckLevel(IncludedMembers.Direct))
			{
				RegisterLevel(Direct);
			}

			if (CheckLevel(IncludedMembers.Inner))
			{
				RegisterLevel(Inner);
			}

			if (CheckLevel(IncludedMembers.All))
			{
				RegisterLevel(All);
			}

			if (!AllowCustomLevels)
			{
				Seal();
			}

			bool CheckLevel(IncludedMembers target)
			{
				return level > target && AllowLevel(target);
			}
		}
	}
}
