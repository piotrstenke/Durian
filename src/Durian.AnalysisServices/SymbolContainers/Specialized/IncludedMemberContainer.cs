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
	public abstract class IncludedMemberContainer<TSymbol, TData> : LeveledSymbolContainer<TSymbol, TData>, IMappedSymbolContainer<TSymbol, TData, IncludedMembers>
		where TSymbol : class, ISymbol
		where TData : class, IMemberData
	{
		/// <summary>
		/// Current nesting level of <see cref="ISymbolContainer"/>s.
		/// </summary>
		public new IncludedMembers CurrentLevel => MapLevel(base.CurrentLevel);

		/// <summary>
		/// Maximal possible nesting level.
		/// </summary>
		public new IncludedMembers MaxLevel => MapLevel(base.MaxLevel);

		/// <summary>
		/// Determines whether levels beyond the limit of <see cref="IncludedMembers"/> can be registered.
		/// </summary>
		protected virtual bool AllowCustomLevels => false;

		/// <summary>
		/// Max <see cref="IncludedMembers"/> level possible for the current container.
		/// </summary>
		protected virtual IncludedMembers Limit => IncludedMembers.All;

		/// <summary>
		/// Initializes a new instance of the <see cref="IncludedMemberContainer{TSymbol, TData}"/> class.
		/// </summary>
		/// <param name="root"><see cref="ISymbol"/> that is a root of all the underlaying containers.</param>
		/// <param name="parentCompilation"><see cref="ICompilationData"/> used to create <typeparamref name="TData"/>s.</param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		/// <param name="includeRoot">Determines whether the <paramref name="root"/> should be included in the underlaying containers.</param>
		/// <exception cref="ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
		protected IncludedMemberContainer(
			ISymbolOrMember<TSymbol, TData> root,
			ICompilationData? parentCompilation = default,
			ISymbolNameResolver? nameResolver = default,
			bool includeRoot = false
		) : base(root, parentCompilation, nameResolver, includeRoot)
		{
			InitLevels(false);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IncludedMemberContainer{TSymbol, TData}"/> class.
		/// </summary>
		/// <param name="root"><see cref="ISymbol"/> that is a root of all the underlaying containers.</param>
		/// <param name="parentCompilation"><see cref="ICompilationData"/> used to create <typeparamref name="TData"/>s.</param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		/// <exception cref="ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
		protected IncludedMemberContainer(
			ISymbolOrMember root,
			ICompilationData? parentCompilation = default,
			ISymbolNameResolver? nameResolver = default
		) : base(root, parentCompilation, nameResolver)
		{
			InitLevels(skipRoot: root is not ISymbolOrMember<TSymbol, TData>);
		}

		internal sealed override IEnumerable<ISymbolOrMember<TSymbol, TData>> ResolveRootInternal(ISymbolOrMember root)
		{
			return ResolveRoot(root);
		}

		/// <inheritdoc cref="LeveledSymbolContainer{TSymbol, TData}.ResolveLevel(int)"/>
		public ISymbolContainer<TSymbol, TData> ResolveLevel(IncludedMembers level)
		{
			int mappedLevel = MapLevel(level);
			return base.ResolveLevel(mappedLevel);
		}

		/// <inheritdoc cref="LeveledSymbolContainer{TSymbol, TData}.ClearLevel(int)"/>
		public void ClearLevel(IncludedMembers level)
		{
			int mappedLevel = MapLevel(level);
			base.ClearLevel(mappedLevel);
		}

		/// <inheritdoc cref="LeveledSymbolContainer{TSymbol, TData}.ClearLevel(int)"/>
		[Obsolete("Use ClearLevel(IncludedMembers) instead")]
		public new void ClearLevel(int level)
		{
			base.ClearLevel(level);
		}

		/// <inheritdoc cref="LeveledSymbolContainer{TSymbol, TData}.ResolveLevel(int)"/>
		[Obsolete("Use ResolveLevel(IncludedMembers) instead")]
		public new ISymbolContainer<TSymbol, TData> ResolveLevel(int level)
		{
			return base.ResolveLevel(level);
		}

		/// <inheritdoc cref="LeveledSymbolContainer{TSymbol, TData}.Reverse"/>
		public new IncludedMemberContainer<TSymbol, TData> Reverse()
		{
			return (base.Reverse() as IncludedMemberContainer<TSymbol, TData>)!;
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
		protected virtual IEnumerable<ISymbolOrMember<TSymbol, TData>> Direct(ISymbolOrMember<TSymbol, TData> member)
		{
			return ResolveRoot(member);
		}

		/// <summary>
		/// Resolves the members of root symbol for the lowest level.
		/// </summary>
		/// <param name="root">Root symbol.</param>
		protected abstract IEnumerable<ISymbolOrMember<TSymbol, TData>> ResolveRoot(ISymbolOrMember root);

		/// <summary>
		/// Returns a <see cref="IReturnOrderEnumerable{T}"/> representing the <see cref="IncludedMembers.Direct"/> of the <see cref="LeveledSymbolContainer{TSymbol, TData}.Root"/>.
		/// </summary>
		/// <param name="member"><see cref="ISymbolOrMember"/> to get the <see cref="IncludedMembers.Direct"/> for.</param>
		protected abstract IEnumerable<ISymbolOrMember<TSymbol, TData>> Inner(ISymbolOrMember<TSymbol, TData> member);

		/// <summary>
		/// Maps the current <see cref="int"/> to an <see cref="IncludedMembers"/>.
		/// </summary>
		/// <param name="level">Level to map.</param>
		protected virtual IncludedMembers MapLevel(int level)
		{
			return (IncludedMembers)level + 1;
		}

		/// <summary>
		/// Maps the current <see cref="IncludedMembers"/> to an <see cref="int"/>.
		/// </summary>
		/// <param name="level">Level to map.</param>
		/// <exception cref="ArgumentOutOfRangeException">Level cannot be <see cref="IncludedMembers.None"/> or less.</exception>
		protected virtual int MapLevel(IncludedMembers level)
		{
			int current = (int)level;
			IncludedMembers mappedCurrent = MapLevel(current);

			if (mappedCurrent <= IncludedMembers.None)
			{
				throw new ArgumentOutOfRangeException(nameof(level), $"Level cannot be '{nameof(IncludedMembers.None)}' or less");
			}

			int diff = (int)mappedCurrent - current;
			int mappedLevel = current - diff;

			return mappedLevel;
		}

		/// <summary>
		/// Determines whether the specified <see cref="IncludedMembers"/> can be registered as a valid level.
		/// </summary>
		/// <param name="level"><see cref="IncludedMembers"/> to determine whether can be registered as a valid level.</param>
		protected virtual bool AllowLevel(IncludedMembers level)
		{
			return true;
		}

		/// <inheritdoc cref="LeveledSymbolContainer{TSymbol, TData}.OnLevelCleared(int)"/>
		protected virtual void OnLevelCleared(IncludedMembers level)
		{
			// Do nothing.
		}

		/// <inheritdoc/>
		[Obsolete("Use OnLevelCleared(IncludedMembers) instead")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		protected override void OnLevelCleared(int level)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
		{
			IncludedMembers members = MapLevel(level);
			OnLevelCleared(members);
		}

		/// <inheritdoc/>
		[Obsolete("Use OnLevelFilled(IncludedMembers) instead")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		protected sealed override void OnLevelFilled(int level)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
		{
			IncludedMembers members = MapLevel(level);
			OnLevelFilled(members);
		}

		/// <inheritdoc/>
		[Obsolete("Use OnLevelFilled(IncludedMembers) instead")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		protected sealed override void OnLevelReady(int level)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
		{
			IncludedMembers members = MapLevel(level);
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
			IncludedMembers members = MapLevel(level);
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

		private void InitLevels(bool skipRoot)
		{
			IncludedMembers level = MaxLevel + 1;

			bool isSkipped = false;

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
				if (level > target && AllowLevel(target))
				{
					if (skipRoot && !isSkipped)
					{
						isSkipped = true;
						return false;
					}

					return true;
				}

				return false;
			}
		}

		ISymbolContainer IMappedSymbolContainer<IncludedMembers>.ResolveLevel(IncludedMembers level)
		{
			return ResolveLevel(level);
		}
	}
}
