// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Durian.Analysis.Extensions;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="ISymbol"/>.
	/// </summary>
	public interface IMemberData : ISymbolOrMember
	{
		/// <summary>
		/// Target <see cref="SyntaxNode"/>.
		/// </summary>
		SyntaxNode Declaration { get; }

		/// <summary>
		/// Determines whether the current member is declared using the <see langword="new"/> keyword.
		/// </summary>
		bool IsNew { get; }

		/// <summary>
		/// Determines whether the current member is declared using the <see langword="partial"/> keyword.
		/// </summary>
		bool IsPartial { get; }

		/// <summary>
		/// Determines whether the current member is declared using the <see langword="unsafe"/> keyword.
		/// </summary>
		bool IsUnsafe { get; }

		/// <summary>
		/// <see cref="Analysis.Virtuality"/> of the member.
		/// </summary>
		Virtuality Virtuality { get; }

		/// <summary>
		/// Member this member hides using the <see langword="new"/> keyword.
		/// </summary>
		ISymbolOrMember? HiddenSymbol { get; }

		/// <summary>
		/// Location of the member.
		/// </summary>
		Location? Location { get; }

		/// <summary>
		/// Name of the underlaying symbol including the verbatim identifier '@' token.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Name of the underlaying symbol including the verbatim identifier '@' token and its type parameters.
		/// </summary>
		string GenericName { get; }

		/// <summary>
		/// Name of the underlaying symbol including the verbatim identifier '@' token and its type arguments.
		/// </summary>
		string SubstitutedName { get; }

		/// <summary>
		/// Parent compilation of this <see cref="IMemberData"/>.
		/// </summary>
		ICompilationData ParentCompilation { get; }

		/// <summary>
		/// <see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the <see cref="Declaration"/>.
		/// </summary>
		SemanticModel SemanticModel { get; }

		/// <summary>
		/// <see cref="ISymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		new ISymbol Symbol { get; }

		/// <summary>
		/// Data of all attributes applied to the <see cref="Symbol"/>.
		/// </summary>
		ImmutableArray<AttributeData> Attributes { get; }

		/// <summary>
		/// All <see cref="INamespaceSymbol"/>s that contain the <see cref="Symbol"/>.
		/// </summary>
		IWritableSymbolContainer<INamespaceSymbol, INamespaceData> ContainingNamespaces { get; }

		/// <summary>
		/// All <see cref="ITypeData"/>s that contain the <see cref="Symbol"/>.
		/// </summary>
		IWritableSymbolContainer<INamedTypeSymbol, ITypeData> ContainingTypes { get; }

		/// <summary>
		/// All modifiers of the current symbol.
		/// </summary>
		ImmutableArray<string> Modifiers { get; }
	}

	public interface IContainerResolver<out TSymbol, out TData>
		where TSymbol : class, ISymbol
		where TData : class, IMemberData
	{

	}

	public sealed class TypesInNamespaceResolver : ContainerResolver<INamedTypeSymbol, ITypeData>
	{
	}

	public abstract class ContainerResolver<TSymbol, TData> : ISymbolContainer<TSymbol, TData>
		where TSymbol : class, ISymbol
		where TData : class, IMemberData
	{
		private interface IIndexHandler
		{
			int Count { get; }
			int Start { get; }
			int End { get; }

			bool IsEmpty { get; }

			int NextIndex();

			void Reset();
		}

		private struct RangeBasedIndexHandler : IIndexHandler
		{
			private readonly List<(int start, int end)> _ranges;
			private int _currentIndex;
			private int _currentRange;

			public int Count { get; }

			public int Start => _ranges[0].start;

			public int End => _ranges[_ranges.Count - 1].end;

			public bool IsEmpty => Count == 0;

			public RangeBasedIndexHandler(List<(int start, int end)> ranges, int count)
			{
				_ranges = ranges;
				Count = count;

				_currentIndex = ResetCurrentIndex(ranges);
				_currentRange = 0;
			}

			public int NextIndex()
			{
				if (_currentIndex == -1)
				{
					return -1;
				}

				if(_currentIndex == _ranges[_currentRange].end)
				{
					if (_currentRange >= _ranges.Count)
					{
						_currentIndex = -1;
						return -1;
					}

					_currentRange++;
					_currentIndex = _ranges[_currentRange].start;
				}

				return _currentIndex++;
			}

			public void Reset()
			{
				_currentIndex = ResetCurrentIndex(_ranges);
				_currentRange = 0;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static int ResetCurrentIndex(List<(int start, int end)> list)
			{
				return list.Count == 0
					? -1
					: list[0].start;
			}
		}

		private struct IncrementalIndexHandler : IIndexHandler
		{
			private int _current;

			public int Count => End - Start;
			public int Start { get; }
			public int End { get; }

			public bool IsEmpty => Start == End;

			public IncrementalIndexHandler(int start, int end)
			{
				Start = start;
				End = end;
				_current = start;
			}

			public int NextIndex()
			{
				if(_current == End)
				{
					return -1;
				}

				return _current++;
			}

			public void Reset()
			{
				_current = default;
			}
		}

		private sealed class ResolvableContainer : ISymbolContainer<TSymbol, TData>
		{
			private readonly List<ISymbolOrMember<TSymbol, TData>> _original;
			private readonly IncrementalIndexHandler _indexHandler;
			private readonly ISymbolNameResolver _nameResolver;

			private ImmutableArray<TSymbol> _symbols;
			private ImmutableArray<TData> _datas;
			private ImmutableArray<string> _names;

			public ICompilationData? ParentCompilation { get; }

			public int Count => _indexHandler.Count;

			public ReturnOrder Order { get; set; }

			bool ISealable.IsSealed => false;
			bool ISealable.CanBeSealed => false;
			bool ISealable.CanBeUnsealed => false;

			public ResolvableContainer(ContainerResolver<TSymbol, TData> parentResolver, IncrementalIndexHandler indexHandler)
			{
				_original = parentResolver._data;
				_nameResolver = parentResolver.NameResolver;
				ParentCompilation = parentResolver.ParentCompilation;
				_indexHandler = indexHandler;
			}

			public ISymbolOrMember<TSymbol, TData> First()
			{
				return First(Order);
			}

			public ISymbolOrMember<TSymbol, TData> First(ReturnOrder order)
			{
				if (Order == ReturnOrder.ParentToChild)
				{
					return _original[_indexHandler.End];
				}

				return _original[_indexHandler.Start];
			}

			public ImmutableArray<TData> GetData()
			{
				if(!_datas.IsDefault)
				{
					return _datas;
				}

				if (_indexHandler.IsEmpty)
				{
					return _datas = ImmutableArray<TData>.Empty;
				}

				ImmutableArray<TData>.Builder builder = ImmutableArray.CreateBuilder<TData>(Count);

				int index;

				while ((index = _indexHandler.NextIndex()) != -1)
				{
					builder.Add(_original[index].Member);
				}

				_indexHandler.Reset();

				return _datas = builder.ToImmutable();
			}

			public ImmutableArray<string> GetNames()
			{
				if(!_names.IsDefault)
				{
					return _names;
				}

				if(!_datas.IsDefault)
				{
					return _names = _datas.Select(d => _nameResolver.ResolveName(d)).ToImmutableArray();
				}

				ImmutableArray<TSymbol> symbols = _symbols.IsDefault
					? GetSymbols()
					: _symbols;

				return _names = symbols.Select(s => _nameResolver.ResolveName(s)).ToImmutableArray();
			}

			public ImmutableArray<TSymbol> GetSymbols()
			{
				if(!_symbols.IsDefault)
				{
					return _symbols;
				}

				if(_indexHandler.IsEmpty)
				{
					return _symbols = ImmutableArray<TSymbol>.Empty;
				}

				ImmutableArray<TSymbol>.Builder builder = ImmutableArray.CreateBuilder<TSymbol>(Count);

				int index;

				while((index = _indexHandler.NextIndex()) != -1)
				{
					builder.Add(_original[index].Symbol);
				}

				_indexHandler.Reset();

				return _symbols = builder.ToImmutable();
			}

			public ISymbolOrMember<TSymbol, TData> Last()
			{
				return Last(Order);
			}

			public ISymbolOrMember<TSymbol, TData> Last(ReturnOrder order)
			{
				if (Order == ReturnOrder.ParentToChild)
				{
					return _original[_indexHandler.Start];
				}

				return _original[_indexHandler.End];
			}

			public void Reverse()
			{
				Order = Order.Reverse();
			}

			ImmutableArray<ISymbol> ISymbolContainer.GetSymbols()
			{
				return GetSymbols().CastArray<ISymbol>();
			}

			ImmutableArray<IMemberData> ISymbolContainer.GetData()
			{
				return GetData().CastArray<IMemberData>();
			}

			void ISealable.Seal()
			{
				// Do nothing.
			}

			void ISealable.Unseal()
			{
				// Do nothing.
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				int index;

				while((index = _indexHandler.NextIndex()) != -1)
				{
					yield return _original[index];
				}

				_indexHandler.Reset();
			}
		}

		private readonly List<ISymbolOrMember<TSymbol,  TData>> _data;
		private readonly List<ISymbolContainer<TSymbol, TData>?> _containers;
		private readonly List<Func<IReturnOrderEnumerable<TSymbol>>> _creators;

		/// <summary>
		/// Number of possible nesting levels of <see cref="ISymbolContainer"/>s.
		/// </summary>
		public int NumLevels => _containers.Count;

		/// <summary>
		/// Current nesting level of <see cref="ISymbolContainer"/>s.
		/// </summary>
		public int CurrentLevel { get; private set; }

		/// <summary>
		/// Maximal possible nesting level (<see cref="NumLevels"/> - 1).
		/// </summary>
		public int MaxLevel => NumLevels - 1;

		/// <summary>
		/// <see cref="ICompilationData"/> used to create <typeparamref name="TData"/>s.
		/// </summary>
		public ICompilationData? ParentCompilation { get; }

		/// <summary>
		/// <see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.
		/// </summary>
		public ISymbolNameResolver NameResolver { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ContainerResolver{TSymbol, TData}"/> class.
		/// </summary>
		/// <param name="parentCompilation"><see cref="ICompilationData"/> used to create <typeparamref name="TData"/>s.</param>
		protected ContainerResolver(ICompilationData? parentCompilation = default) : this(SymbolNameResolver.Verbatim.Instance, parentCompilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ContainerResolver{TSymbol, TData}"/> class.
		/// </summary>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		/// <param name="parentCompilation"><see cref="ICompilationData"/> used to create <typeparamref name="TData"/>s.</param>
		/// <exception cref="ArgumentNullException"><paramref name="nameResolver"/> is <see langword="null"/>.</exception>
		protected ContainerResolver(ISymbolNameResolver nameResolver, ICompilationData? parentCompilation = default)
		{
			if(nameResolver is null)
			{
				throw new ArgumentNullException(nameof(nameResolver));
			}

			NameResolver = nameResolver;
			ParentCompilation = parentCompilation;

			_data = new();
			_creators = new();
			_containers = new();
		}

		/// <summary>
		/// Registers a new nesting level.
		/// </summary>
		/// <param name="function">Function used to retrieve the ?<typeparamref name="TSymbol"/>s.</param>
		public virtual void RegisterLevel(Func<IReturnOrderEnumerable<TSymbol>> function)
		{
			int level = MaxLevel;

			_containers.Insert(level, null);
			_creators.Insert(level, function);
		}

		public ISymbolContainer<TSymbol, TData> ResolveContainer(int level)
		{
			if (level < 0 || level >= NumLevels)
			{
				throw new ArgumentOutOfRangeException(nameof(level), $"Level must be greater than 0 and less than {nameof(NumLevels)}");
			}

			ISymbolContainer<TSymbol, TData>? container = _containers[level];

			if(container is not null)
			{
				return container;
			}

			int start = CurrentLevel;
			int length = level + 1;

			List<TSymbol> list = new(16);

			for (int i = start; i < length; i++)
			{
				IReturnOrderEnumerable<TSymbol> collection = _creators[i].Invoke();

				if(collection.Order != ReturnOrder.ChildToParent)
				{
					collection.Reverse();
				}

				// A, B
				// A, A1, A2, A3, B, B1, B2

				int current = -1;

				int insertIndex = 0;

				foreach (TSymbol symbol in collection)
				{
					current++;

					if (SkipSymbol(symbol))
					{
						insertIndex = current + 1;
						continue;
					}

					ISymbolOrMember<TSymbol, TData> member = (ISymbolOrMember<TSymbol, TData>)symbol.ToDataOrSymbol(ParentCompilation);

					_data.Add(member);
				}

				_containers[level] = new ResolvableContainer(_data, NameResolver, ParentCompilation, new IncrementalIndexHandler(0, _data.Count));
			}

			CurrentLevel = level;

			return _containers[level]!;
		}

		protected virtual int GetLowestLevel(TSymbol symbol)
		{
			return 0;
		}

		/// <summary>
		/// Determines whether the <paramref name="symbol"/> and its members should be skipped when creating a super-set <see cref="ISymbolContainer"/>.
		/// </summary>
		/// <param name="symbol"><typeparamref name="TSymbol"/> to determine whether to skip.</param>
		protected virtual bool SkipSymbol(TSymbol symbol)
		{
			return false;
		}
	}
}
