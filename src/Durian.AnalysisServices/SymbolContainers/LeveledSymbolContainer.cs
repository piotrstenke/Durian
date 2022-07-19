// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// <see cref="ISymbolContainer{TSymbol, TData}"/> that allows to share a single symbol collection between multiple sub-set containers.
	/// </summary>
	/// <typeparam name="TSymbol">Type of returned <see cref="ISymbol"/>s.</typeparam>
	/// <typeparam name="TData">Type of returned <see cref="IMemberData"/>s.</typeparam>
	public partial class LeveledSymbolContainer<TSymbol, TData> : PermanentSealableObject, ILeveledSymbolContainer<TSymbol, TData>
		where TSymbol : class, ISymbol
		where TData : class, IMemberData
	{
		internal sealed class LevelEntry
		{
			public InnerContainer? Container { get; set; }

			public Func<ISymbolOrMember<TSymbol, TData>, IEnumerable<ISymbolOrMember<TSymbol, TData>>> Creator { get; }

			public int StartIndex { get; set; }

			public bool IsEmpty { get; set; }

			public LevelEntry(Func<ISymbolOrMember<TSymbol, TData>, IEnumerable<ISymbolOrMember<TSymbol, TData>>> creator)
			{
				Creator = creator;
				StartIndex = -1;
			}

			public void Reset()
			{
				Container = null;
				StartIndex = -1;
				IsEmpty = false;
			}
		}

		internal readonly List<ISymbolOrMember<TSymbol, TData>> _data;
		internal readonly List<LevelEntry> _levels;

		private readonly Func<ISymbolOrMember, IEnumerable<ISymbolOrMember<TSymbol, TData>>>? _rootFunction;

		/// <inheritdoc/>
		public int Count => _data.Count;

		/// <summary>
		/// Current nesting level of <see cref="ISymbolContainer"/>s or <c>-1</c> if no internal container is initialized.
		/// </summary>
		public int CurrentLevel { get; private set; }

		/// <summary>
		/// Determines whether the <see cref="Root"/> is included in the underlaying containers.
		/// </summary>
		[MemberNotNullWhen(true, nameof(Root))]
		public bool IncludeRoot { get; }

		/// <summary>
		/// Determines whether the container is empty.
		/// </summary>
		public bool IsEmpty => Count == 0;

		/// <summary>
		/// Determines whether the container is empty excluding the root symbol.
		/// </summary>
		public bool IsEmptyNoRoot
		{
			get
			{
				if (IncludeRoot)
				{
					return Count == 1;
				}

				return Count == 0;
			}
		}

		/// <summary>
		/// Maximal currently possible nesting level (<see cref="NumLevels"/> - 1).
		/// </summary>
		public int MaxLevel => NumLevels - 1;

		/// <summary>
		/// Number of possible nesting levels of <see cref="ISymbolContainer"/>s.
		/// </summary>
		public int NumLevels => _levels.Count;

		/// <inheritdoc/>
		public ReturnOrder Order { get; private set; }

		/// <summary>
		/// <see cref="ICompilationData"/> used to create <typeparamref name="TData"/>s.
		/// </summary>
		public ICompilationData? ParentCompilation { get; }

		/// <summary>
		/// <see cref="ISymbol"/> that is a root of all the underlaying containers.
		/// </summary>
		public ISymbolOrMember<TSymbol, TData>? Root => TargetRoot as ISymbolOrMember<TSymbol, TData>;

		/// <summary>
		/// <see cref="ISymbol"/> that is a root of all the underlaying containers. Used when the root <see cref="ISymbol"/> is of different kind that all its children. (e.g. namespace for inner types).
		/// </summary>
		public ISymbolOrMember TargetRoot { get; }

		/// <summary>
		/// <see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.
		/// </summary>
		public ISymbolNameResolver SymbolNameResolver { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="LeveledSymbolContainer{TSymbol, TData}"/> class.
		/// </summary>
		/// <param name="root"><see cref="ISymbol"/> that is a root of all the underlaying containers.</param>
		/// <param name="parentCompilation"><see cref="ICompilationData"/> used to create <typeparamref name="TData"/>s.</param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		/// <param name="includeRoot">Determines whether the <paramref name="root"/> should be included in the underlaying containers.</param>
		/// <exception cref="ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
		public LeveledSymbolContainer(
			ISymbolOrMember<TSymbol, TData> root,
			ICompilationData? parentCompilation = default,
			ISymbolNameResolver? nameResolver = default,
			bool includeRoot = false
		) : this(root as ISymbolOrMember, parentCompilation, nameResolver, includeRoot)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LeveledSymbolContainer{TSymbol, TData}"/> class.
		/// </summary>
		/// <param name="root"><see cref="ISymbol"/> that is a root of all the underlaying containers.</param>
		/// <param name="rootFunction">Function to use when retrieving data from the <paramref name="root"/> itself.</param>
		/// <param name="parentCompilation"><see cref="ICompilationData"/> used to create <typeparamref name="TData"/>s.</param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		/// <exception cref="ArgumentNullException"><paramref name="root"/> is <see langword="null"/>. -or- <paramref name="rootFunction"/> is <see langword="null"/>.</exception>
		public LeveledSymbolContainer(
			ISymbolOrMember root,
			Func<ISymbolOrMember, IEnumerable<ISymbolOrMember<TSymbol, TData>>> rootFunction,
			ICompilationData? parentCompilation = default,
			ISymbolNameResolver? nameResolver = default
		) : this(root, parentCompilation, nameResolver, false)
		{
			if(rootFunction is null)
			{
				throw new ArgumentNullException(nameof(rootFunction));
			}

			_rootFunction = rootFunction;

			_levels.Add(new LevelEntry(null!));
		}

		private protected LeveledSymbolContainer(
			ISymbolOrMember root,
			ICompilationData? parentCompilation = default,
			ISymbolNameResolver? nameResolver = default
		) : this(root, parentCompilation, nameResolver, false)
		{
			if(root is not ISymbolOrMember<TSymbol, TData>)
			{
				_rootFunction = ResolveRootInternal;
				_levels.Add(new LevelEntry(null!));
			}
		}

		private LeveledSymbolContainer(
			ISymbolOrMember root,
			ICompilationData? parentCompilation,
			ISymbolNameResolver? nameResolver,
			bool includeRoot
		)
		{
			if (root is null)
			{
				throw new ArgumentNullException(nameof(root));
			}

			SymbolNameResolver = nameResolver ?? Analysis.SymbolNameResolver.Default;
			ParentCompilation = parentCompilation;
			TargetRoot = root;
			IncludeRoot = includeRoot;

			_levels = new(8);
			_data = new();

			if (IncludeRoot)
			{
				_data.Add(Root);
			}
		}

		/// <inheritdoc/>
		public ISymbolOrMember<TSymbol, TData> First()
		{
			return First(Order);
		}

		/// <inheritdoc/>
		public ISymbolOrMember<TSymbol, TData> First(ReturnOrder order)
		{
			if (IsEmpty)
			{
				throw SymbolContainerFactory.Exc_EmptySymbolContainer();
			}

			if (order == ReturnOrder.ChildToParent)
			{
				return _data[_data.Count - 1];
			}

			return _data[0];
		}

		/// <inheritdoc cref="ISymbolContainer.GetData"/>
		public ImmutableArray<TData> GetData()
		{
			return GetData(Count);
		}

		/// <inheritdoc/>
		public IEnumerator<ISymbolOrMember<TSymbol, TData>> GetEnumerator()
		{
			return _data.GetEnumerator();
		}

		/// <inheritdoc/>
		public ImmutableArray<string> GetNames()
		{
			return GetNames(Count);
		}

		/// <inheritdoc cref="ISymbolContainer.GetSymbols"/>
		public ImmutableArray<TSymbol> GetSymbols()
		{
			return GetSymbols(Count);
		}

		/// <inheritdoc/>
		public ISymbolOrMember<TSymbol, TData> Last()
		{
			return Last(Order);
		}

		/// <inheritdoc/>
		public ISymbolOrMember<TSymbol, TData> Last(ReturnOrder order)
		{
			if (IsEmpty)
			{
				throw SymbolContainerFactory.Exc_EmptySymbolContainer();
			}

			if (order == ReturnOrder.ChildToParent)
			{
				return _data[0];
			}

			return _data[_data.Count - 1];
		}

		/// <inheritdoc/>
		public void RegisterLevel(Func<TSymbol, IEnumerable<TSymbol>> function)
		{
			if (function is null)
			{
				throw new ArgumentNullException(nameof(function));
			}

			RegisterLevelInternal(member => function(member.Symbol).Select(s => (ISymbolOrMember<TSymbol, TData>)s.ToDataOrSymbol(ParentCompilation)));
		}

		/// <inheritdoc/>
		public void RegisterLevel(Func<TSymbol, IEnumerable<ISymbolOrMember<TSymbol, TData>>> function)
		{
			if (function is null)
			{
				throw new ArgumentNullException(nameof(function));
			}

			RegisterLevelInternal(member => function(member.Symbol));
		}

		/// <inheritdoc/>
		public void RegisterLevel(Func<ISymbolOrMember<TSymbol, TData>, IEnumerable<ISymbolOrMember<TSymbol, TData>>> function)
		{
			if (function is null)
			{
				throw new ArgumentNullException(nameof(function));
			}

			RegisterLevelInternal(function);
		}

		/// <inheritdoc/>
		public void ClearLevel(int level)
		{
			SymbolContainerFactory.ValidateLevel(level, NumLevels);

			for (int i = _levels.Count - 1; i >= level; i--)
			{
				LevelEntry entry = _levels[i];

				int start = entry.StartIndex;

				if(start > -1 && !entry.IsEmpty)
				{
					_data.RemoveRange(entry.StartIndex, _data.Count - entry.StartIndex);
				}

				_levels[i].Reset();
				OnLevelCleared(level);
			}
		}

		/// <inheritdoc/>
		public ISymbolContainer<TSymbol, TData> ResolveLevel(int level)
		{
			SymbolContainerFactory.ValidateLevel(level, NumLevels);

			LevelEntry levelData = _levels[level];

			if (levelData.Container is not null)
			{
				return levelData.Container;
			}

			// Example level arrangement:

			// -1: (root)
			// 0: ((root), A, B)
			// 1: (((root), A, B), A1, B1)
			// 2: ((((root), A, B), A1, B1), A2, A3, B2, B3)

			// The inclusion of root in a container is determined by the IncludeRoot property.

			if (CurrentLevel == -1)
			{
				FillFirstLevel();
			}

			int previousStart = _levels[CurrentLevel].StartIndex;
			int length = level + 1;
			bool nextLevelsAreEmpty = false;

			for (int i = CurrentLevel + 1; i < length; i++)
			{
				OnLevelReady(i);
				LevelEntry levelEntry = _levels[i];
				int firstIndex = _data.Count;

				if (nextLevelsAreEmpty || IsHandledExternally(i))
				{
					goto LEVEL_FILLED;
				}

				for (int j = previousStart; j < firstIndex; j++)
				{
					ISymbolOrMember<TSymbol, TData> member = _data[j];

					if (SkipMember(member, i))
					{
						continue;
					}

					FillLevel(levelEntry.Creator(member));
				}

			LEVEL_FILLED:

				levelEntry.StartIndex = firstIndex;
				levelEntry.Container = new InnerContainer(this, _data.Count);

				// If current level is empty, all next levels are also empty.
				if (firstIndex == _data.Count)
				{
					levelEntry.IsEmpty = true;
					nextLevelsAreEmpty = true;
				}

				previousStart = firstIndex;

				OnLevelFilled(i);
			}

			if (level == MaxLevel)
			{
				_data.TrimExcess();
			}

			CurrentLevel = level;
			return _levels[CurrentLevel].Container!;
		}

		/// <inheritdoc cref="ISymbolContainer.Reverse"/>
		public LeveledSymbolContainer<TSymbol, TData> Reverse()
		{
			Order = Order.Reverse();

			foreach (LevelEntry level in _levels)
			{
				if (level.Container is null)
				{
					break;
				}

				level.Container.Reverse();
			}

			return this;
		}

		/// <inheritdoc/>
		protected sealed override bool SealCore()
		{
			_levels.TrimExcess();

			return true;
		}

		/// <summary>
		/// Determines whether the <paramref name="member"/> and its members should be skipped when retrieving a <see cref="IReturnOrderEnumerable{T}"/>.
		/// </summary>
		/// <param name="member"><see cref="ISymbolContainer{TSymbol, TData}"/> to determine whether to skip.</param>
		/// <param name="level">Currently filled level.</param>
		protected virtual bool SkipMember(ISymbolOrMember<TSymbol, TData> member, int level)
		{
			return false;
		}

		/// <summary>
		/// Called after a level is cleared of cached data.
		/// </summary>
		/// <param name="level">Level that was cleared.</param>
		protected virtual void OnLevelCleared(int level)
		{
			// Do nothing.
		}

		/// <summary>
		/// Called after a <paramref name="level"/> is filled with data.
		/// </summary>
		/// <param name="level">Level that was filled.</param>
		protected virtual void OnLevelFilled(int level)
		{
			// Do nothing.
		}

		/// <summary>
		/// Called before a <paramref name="level"/> is filled with data.
		/// </summary>
		/// <param name="level">Level that is about to be filled.</param>
		protected virtual void OnLevelReady(int level)
		{
			// Do nothing.
		}

		/// <summary>
		/// Determines whether the specified <paramref name="level"/> is handled by external means.
		/// </summary>
		/// <param name="level">Level to determine whether is handled externally.</param>
		private protected virtual bool IsHandledExternally(int level)
		{
			return false;
		}

		ImmutableArray<IMemberData> ISymbolContainer.GetData()
		{
			return GetData().CastArray<IMemberData>();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
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

		internal virtual IEnumerable<ISymbolOrMember<TSymbol, TData>> ResolveRootInternal(ISymbolOrMember root)
		{
			throw new NotImplementedException("ResolveRoot(ISymbolOrMember) is not implemented");
		}

		private void FillFirstLevel()
		{
			const int firstLevelIndex = 0;

			OnLevelReady(firstLevelIndex);

			LevelEntry firstLevel = _levels[firstLevelIndex];

			if (!IsHandledExternally(firstLevelIndex))
			{
				if (_rootFunction is null)
				{
					FillLevel(firstLevel.Creator(Root!));
				}
				else
				{
					IEnumerable<ISymbolOrMember<TSymbol, TData>> collection = _rootFunction(TargetRoot);
					FillLevel(collection);
				}
			}

			OnLevelFilled(firstLevelIndex);
			CurrentLevel++;

			_levels[firstLevelIndex].StartIndex = 1;
		}

		private void FillLevel(IEnumerable<ISymbolOrMember<TSymbol, TData>> collection)
		{
			if (collection is IReturnOrderEnumerable<ISymbolOrMember<TSymbol, TData>> orderEnumerable && orderEnumerable.Order == ReturnOrder.ChildToParent)
			{
				collection = orderEnumerable.Reverse();
			}

			_data.AddRange(collection);
		}

		private ImmutableArray<TData> GetData(int endIndex)
		{
			int count = Count;

			if (count == 0)
			{
				return ImmutableArray<TData>.Empty;
			}

			ImmutableArray<TData>.Builder builder = ImmutableArray.CreateBuilder<TData>(count);

			if (Order == ReturnOrder.ChildToParent)
			{
				ReversedContainerEnumerator iterator = GetReversedEnumerator(endIndex);

				while (iterator.MoveNext())
				{
					builder.Add(iterator.Current.Member);
				}
			}
			else
			{
				ContainerEnumerator iterator = GetEnumerator(endIndex);

				while (iterator.MoveNext())
				{
					builder.Add(iterator.Current.Member);
				}
			}

			return builder.ToImmutable();
		}

		private ContainerEnumerator GetEnumerator(int endIndex)
		{
			return new ContainerEnumerator(_data, endIndex);
		}

		private ImmutableArray<string> GetNames(int endIndex)
		{
			int count = Count;

			if (count == 0)
			{
				return ImmutableArray<string>.Empty;
			}

			ImmutableArray<string>.Builder builder = ImmutableArray.CreateBuilder<string>(count);

			if (Order == ReturnOrder.ChildToParent)
			{
				ReversedContainerEnumerator iterator = GetReversedEnumerator(endIndex);

				while (iterator.MoveNext())
				{
					builder.Add(SymbolNameResolver.ResolveName(iterator.Current));
				}
			}
			else
			{
				ContainerEnumerator iterator = GetEnumerator(endIndex);

				while (iterator.MoveNext())
				{
					builder.Add(SymbolNameResolver.ResolveName(iterator.Current));
				}
			}

			return builder.ToImmutable();
		}

		private ReversedContainerEnumerator GetReversedEnumerator(int endIndex)
		{
			return new ReversedContainerEnumerator(_data, endIndex);
		}

		private ImmutableArray<TSymbol> GetSymbols(int endIndex)
		{
			int count = Count;

			if (count == 0)
			{
				return ImmutableArray<TSymbol>.Empty;
			}

			ImmutableArray<TSymbol>.Builder builder = ImmutableArray.CreateBuilder<TSymbol>(count);

			if (Order == ReturnOrder.ChildToParent)
			{
				ReversedContainerEnumerator iterator = GetReversedEnumerator(endIndex);

				while (iterator.MoveNext())
				{
					builder.Add(iterator.Current.Symbol);
				}
			}
			else
			{
				ContainerEnumerator iterator = GetEnumerator(endIndex);

				while (iterator.MoveNext())
				{
					builder.Add(iterator.Current.Symbol);
				}
			}

			return builder.ToImmutable();
		}

		private void RegisterLevelInternal(Func<ISymbolOrMember<TSymbol, TData>, IEnumerable<ISymbolOrMember<TSymbol, TData>>> function)
		{
			if (IsSealed)
			{
				throw new SealedObjectException("Cannot register new level to a sealed container");
			}

			_levels.Add(new LevelEntry(function));
		}

		ISymbolContainer ILeveledSymbolContainer.ResolveLevel(int level)
		{
			return ResolveLevel(level);
		}

		IEnumerable<ISymbolOrMember<TSymbol, TData>> ISymbolContainer<TSymbol, TData>.AsEnumerable()
		{
			foreach (ISymbolOrMember<TSymbol, TData> member in _data)
			{
				yield return member;
			}
		}
	}
}
