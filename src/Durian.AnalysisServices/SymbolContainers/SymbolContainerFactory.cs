using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Durian.Analysis.Data;
using Durian.Analysis.SymbolContainers.Specialized;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers;

/// <summary>
/// Provides factory methods for implementations of the <see cref="ISymbolContainer"/> interface.
/// </summary>
public static partial class SymbolContainerFactory
{
	private class EmptyContainer<TSymbol, TData> : IWritableSymbolContainer<TSymbol, TData>, IReturnOrderEnumerable<ISymbolOrMember<TSymbol, TData>>
		where TSymbol : class, ISymbol
		where TData : class, IMemberData
	{
		private readonly List<ISymbolOrMember<TSymbol, TData>> _list = new(1);

		public static EmptyContainer<TSymbol, TData> Instance { get; } = new();

		public int Count => 0;

		public ReturnOrder Order { get; private set; }

		public ICompilationData? ParentCompilation => null;

		public ISymbolOrMember<TSymbol, TData> First()
		{
			throw Exc_EmptySymbolContainer();
		}

		public ISymbolOrMember<TSymbol, TData> First(ReturnOrder order)
		{
			throw Exc_EmptySymbolContainer();
		}

		public ImmutableArray<IMemberData> GetData()
		{
			return ImmutableArray<IMemberData>.Empty;
		}

		public IEnumerator<ISymbolOrMember<TSymbol, TData>> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		public ImmutableArray<string> GetNames()
		{
			return ImmutableArray<string>.Empty;
		}

		public ImmutableArray<ISymbol> GetSymbols()
		{
			return ImmutableArray<ISymbol>.Empty;
		}

		public ISymbolOrMember<TSymbol, TData> Last()
		{
			throw Exc_EmptySymbolContainer();
		}

		public ISymbolOrMember<TSymbol, TData> Last(ReturnOrder order)
		{
			throw Exc_EmptySymbolContainer();
		}

		public void Reverse()
		{
			Order = Order.Reverse();
		}

		public void WriteTo(StringBuilder builder)
		{
			// Do nothing.
		}

		IEnumerable<ISymbolOrMember<TSymbol, TData>> ISymbolContainer<TSymbol, TData>.AsEnumerable()
		{
			return this;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
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
	}

	private sealed class EmptyLeveledContainer<TSymbol, TData> : EmptyContainer<TSymbol, TData>, IMappedSymbolContainer<TSymbol, TData, IncludedMembers>
				where TSymbol : class, ISymbol
		where TData : class, IMemberData
	{
		public IncludedMembers CurrentLevel => (IncludedMembers)CurrentLevelInt + 1;

		public int CurrentLevelInt { get; private set; }

		public int NumLevels { get; private set; }

		bool ISealable.CanBeSealed => false;

		bool ISealable.CanBeUnsealed => false;

		int ILeveledSymbolContainer.CurrentLevel => CurrentLevelInt;

		bool ISealable.IsSealed => false;

		public void ClearLevel(IncludedMembers level)
		{
			ClearLevel((int)level - 1);
		}

		public void ClearLevel(int level)
		{
			ValidateLevel(level, NumLevels);

			int levelsToDelete = NumLevels - level;

			NumLevels -= levelsToDelete;

			if (CurrentLevelInt >= NumLevels)
			{
				CurrentLevelInt = NumLevels - 1;
			}
		}

		public void RegisterLevel(Func<TSymbol, IEnumerable<TSymbol>> function)
		{
			NumLevels++;

			// Do nothing.
		}

		public void RegisterLevel(Func<TSymbol, IEnumerable<ISymbolOrMember<TSymbol, TData>>> function)
		{
			NumLevels++;

			// Do nothing.
		}

		public void RegisterLevel(Func<ISymbolOrMember<TSymbol, TData>, IEnumerable<ISymbolOrMember<TSymbol, TData>>> function)
		{
			NumLevels++;

			// Do nothing.
		}

		public ISymbolContainer<TSymbol, TData> ResolveLevel(int level)
		{
			ValidateLevel(level, NumLevels);

			if (level > CurrentLevelInt)
			{
				CurrentLevelInt = level;
			}

			return this;
		}

		public IMappedSymbolContainer<TSymbol, TData, IncludedMembers> ResolveLevel(IncludedMembers level)
		{
			return (ResolveLevel((int)level - 1) as IMappedSymbolContainer<TSymbol, TData, IncludedMembers>)!;
		}

		ISymbolContainer ILeveledSymbolContainer.ResolveLevel(int level)
		{
			return ResolveLevel(level);
		}

		ISymbolContainer IMappedSymbolContainer<IncludedMembers>.ResolveLevel(IncludedMembers level)
		{
			return ResolveLevel(level);
		}

		ISymbolContainer<TSymbol, TData> IMappedSymbolContainer<TSymbol, TData, IncludedMembers>.ResolveLevel(IncludedMembers level)
		{
			return ResolveLevel(level);
		}

		IReturnOrderEnumerable<ISymbolOrMember<TSymbol, TData>> IReturnOrderEnumerable<ISymbolOrMember<TSymbol, TData>>.Reverse()
		{
			return this;
		}

		IReturnOrderEnumerable IReturnOrderEnumerable.Reverse()
		{
			return this;
		}

		bool ISealable.Seal()
		{
			return false;
		}

		bool ISealable.Unseal()
		{
			return false;
		}
	}

	private sealed class GenericMemberWrapper<TSymbol, TData> : ISymbolOrMember<TSymbol, TData>
		where TSymbol : class, ISymbol
		where TData : class, IMemberData
	{
		private readonly ISymbolOrMember _underlaying;

		/// <inheritdoc/>
		public bool HasMember => _underlaying.HasMember;

		/// <inheritdoc/>
		public TData Member => (_underlaying.Member as TData)!;

		/// <inheritdoc/>
		public TSymbol Symbol => (_underlaying.Symbol as TSymbol)!;

		IMemberData ISymbolOrMember.Member => _underlaying.Member;

		ISymbol ISymbolOrMember.Symbol => _underlaying.Symbol;

		/// <summary>
		/// Initializes a new instance of the <see cref="GenericMemberWrapper{TSymbol, TData}"/> class.
		/// </summary>
		/// <param name="underlaying">Underlaying <see cref="ISymbolOrMember"/>.</param>
		public GenericMemberWrapper(ISymbolOrMember underlaying)
		{
			_underlaying = underlaying;
		}
	}

	private class SingleElementContainer<TSymbol, TData> : ISymbolContainer<TSymbol, TData>, IReturnOrderEnumerable<ISymbolOrMember<TSymbol, TData>>
		where TSymbol : class, ISymbol
		where TData : class, IMemberData
	{
		private readonly List<ISymbolOrMember<TSymbol, TData>> _list;

		public int Count => 1;

		public ISymbolOrMember<TSymbol, TData> Element => _list[0];

		public ReturnOrder Order { get; private set; }

		public ICompilationData? ParentCompilation { get; }

		public SingleElementContainer(ISymbolOrMember<TSymbol, TData> element, ICompilationData? parentCompilation = default)
		{
			ParentCompilation = parentCompilation;
			_list = new(1) { element };
		}

		public ISymbolOrMember<TSymbol, TData> First()
		{
			return Element;
		}

		public ISymbolOrMember<TSymbol, TData> First(ReturnOrder order)
		{
			return Element;
		}

		public ImmutableArray<TData> GetData()
		{
			return ImmutableArray.Create(Element.Member);
		}

		public IEnumerator<ISymbolOrMember<TSymbol, TData>> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		public ImmutableArray<string> GetNames()
		{
			return ImmutableArray.Create(Element.Symbol.Name);
		}

		public ImmutableArray<TSymbol> GetSymbols()
		{
			return ImmutableArray.Create(Element.Symbol);
		}

		public ISymbolOrMember<TSymbol, TData> Last()
		{
			return Element;
		}

		public ISymbolOrMember<TSymbol, TData> Last(ReturnOrder order)
		{
			return Element;
		}

		public void Reverse()
		{
			Order = Order.Reverse();
		}

		ImmutableArray<IMemberData> ISymbolContainer.GetData()
		{
			return GetData().CastArray<IMemberData>();
		}

		ImmutableArray<ISymbol> ISymbolContainer.GetSymbols()
		{
			return GetSymbols().CastArray<ISymbol>();
		}

		IEnumerable<ISymbolOrMember<TSymbol, TData>> ISymbolContainer<TSymbol, TData>.AsEnumerable()
		{
			return this;
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

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	/// <summary>
	/// Returns a shared instance of an empty <see cref="ISymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	public static ISymbolContainer<ISymbol, IMemberData> Empty()
	{
		return EmptyContainer<ISymbol, IMemberData>.Instance;
	}

	/// <summary>
	/// Returns a shared instance of an empty <see cref="ISymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TSymbol">Type of <see cref="ISymbol"/>s stored in the container.</typeparam>
	public static ISymbolContainer<TSymbol, IMemberData> Empty<TSymbol>() where TSymbol : class, ISymbol
	{
		return EmptyContainer<TSymbol, IMemberData>.Instance;
	}

	/// <summary>
	/// Returns a shared instance of an empty <see cref="ISymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TSymbol">Type of <see cref="ISymbol"/>s stored in the container.</typeparam>
	/// <typeparam name="TData">Type of <see cref="IMemberData"/>s stored in the container.</typeparam>
	public static ISymbolContainer<TSymbol, TData> Empty<TSymbol, TData>()
		where TSymbol : class, ISymbol
		where TData : class, IMemberData
	{
		return EmptyContainer<TSymbol, TData>.Instance;
	}

	/// <summary>
	/// Returns a new instance of an empty <see cref="ILeveledSymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	public static ILeveledSymbolContainer<ISymbol, IMemberData> EmptyLeveled()
	{
		return new EmptyLeveledContainer<ISymbol, IMemberData>();
	}

	/// <summary>
	/// Returns a new instance of an empty <see cref="ILeveledSymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TSymbol">Type of <see cref="ISymbol"/>s stored in the container.</typeparam>
	public static ILeveledSymbolContainer<TSymbol, IMemberData> EmptyLeveled<TSymbol>()
		where TSymbol : class, ISymbol
	{
		return new EmptyLeveledContainer<TSymbol, IMemberData>();
	}

	/// <summary>
	/// Returns a new instance of an empty <see cref="ILeveledSymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TSymbol">Type of <see cref="ISymbol"/>s stored in the container.</typeparam>
	/// <typeparam name="TData">Type of <see cref="IMemberData"/>s stored in the container.</typeparam>
	public static ILeveledSymbolContainer<TSymbol, TData> EmptyLeveled<TSymbol, TData>()
		where TSymbol : class, ISymbol
		where TData : class, IMemberData
	{
		return new EmptyLeveledContainer<TSymbol, TData>();
	}

	/// <summary>
	/// Returns a shared instance of an empty <see cref="IWritableSymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	public static IWritableSymbolContainer<ISymbol, IMemberData> EmptyWritable()
	{
		return EmptyContainer<ISymbol, IMemberData>.Instance;
	}

	/// <summary>
	/// Returns a shared instance of an empty <see cref="IWritableSymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TSymbol">Type of <see cref="ISymbol"/>s stored in the container.</typeparam>
	public static IWritableSymbolContainer<TSymbol, IMemberData> EmptyWritable<TSymbol>() where TSymbol : class, ISymbol
	{
		return EmptyContainer<TSymbol, IMemberData>.Instance;
	}

	/// <summary>
	/// Returns a shared instance of an empty <see cref="IWritableSymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TSymbol">Type of <see cref="ISymbol"/>s stored in the container.</typeparam>
	/// <typeparam name="TData">Type of <see cref="IMemberData"/>s stored in the container.</typeparam>
	public static IWritableSymbolContainer<TSymbol, TData> EmptyWritable<TSymbol, TData>()
		where TSymbol : class, ISymbol
		where TData : class, IMemberData
	{
		return EmptyContainer<TSymbol, TData>.Instance;
	}

	/// <summary>
	/// Returns a new <see cref="ISymbolContainer{TSymbol, TData}"/> that can only hold a single element.
	/// </summary>
	/// <param name="element"><see cref="ISymbolOrMember{TSymbol, TData}"/> to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	public static ISymbolContainer<ISymbol, IMemberData> Single(ISymbolOrMember<ISymbol, IMemberData> element, ICompilationData? parentCompilation = default)
	{
		return new SingleElementContainer<ISymbol, IMemberData>(element, parentCompilation);
	}

	/// <summary>
	/// Returns a new <see cref="ISymbolContainer{TSymbol, TData}"/> that can only hold a single element.
	/// </summary>
	/// <param name="element"><see cref="ISymbolOrMember{TSymbol, TData}"/> to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <typeparam name="TSymbol">Type of <see cref="ISymbol"/>s stored in the container.</typeparam>
	public static ISymbolContainer<TSymbol, IMemberData> Single<TSymbol>(ISymbolOrMember<TSymbol, IMemberData> element, ICompilationData? parentCompilation = default) where TSymbol : class, ISymbol
	{
		return new SingleElementContainer<TSymbol, IMemberData>(element, parentCompilation);
	}

	/// <summary>
	/// Returns a new <see cref="ISymbolContainer{TSymbol, TData}"/> that can only hold a single element.
	/// </summary>
	/// <param name="element"><see cref="ISymbolOrMember{TSymbol, TData}"/> to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <typeparam name="TSymbol">Type of <see cref="ISymbol"/>s stored in the container.</typeparam>
	/// <typeparam name="TData">Type of <see cref="IMemberData"/>s stored in the container.</typeparam>
	public static ISymbolContainer<TSymbol, TData> Single<TSymbol, TData>(ISymbolOrMember<TSymbol, TData> element, ICompilationData? parentCompilation = default)
		where TSymbol : class, ISymbol
		where TData : class, IMemberData
	{
		return new SingleElementContainer<TSymbol, TData>(element, parentCompilation);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ISymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<SymbolContainer<ISymbol, IMemberData>> ToContainer(this IEnumerable<ISymbol> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ISymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<SymbolContainer<ISymbol, IMemberData>> ToContainer(this IEnumerable<ISymbolOrMember> collection)
	{
		return new(collection.Select(m => EnsureGeneric<ISymbol, IMemberData>(m)));
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ISymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<SymbolContainer<ISymbol, IMemberData>> ToContainer(this IEnumerable<ISymbolOrMember<ISymbol, IMemberData>> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ITypeParameterSymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<SymbolContainer<ITypeParameterSymbol, ITypeParameterData>> ToContainer(this IEnumerable<ITypeParameterSymbol> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ITypeParameterSymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<SymbolContainer<ITypeParameterSymbol, ITypeParameterData>> ToContainer(this IEnumerable<ISymbolOrMember<ITypeParameterSymbol, ITypeParameterData>> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ITypeParameterSymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="ITypeParameterData"/>.</typeparam>
	public static SymbolContainerBuilder<SymbolContainer<ITypeParameterSymbol, TData>> ToContainer<TData>(this IEnumerable<ITypeParameterSymbol> collection) where TData : class, ITypeParameterData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ITypeParameterSymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="ITypeParameterData"/>.</typeparam>
	public static SymbolContainerBuilder<SymbolContainer<ITypeParameterSymbol, TData>> ToContainer<TData>(this IEnumerable<ISymbolOrMember<ITypeParameterSymbol, TData>> collection) where TData : class, ITypeParameterData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ITypeSymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<SymbolContainer<ITypeSymbol, ITypeData>> ToContainer(this IEnumerable<ITypeSymbol> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ITypeSymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<SymbolContainer<ITypeSymbol, ITypeData>> ToContainer(this IEnumerable<ISymbolOrMember<ITypeSymbol, ITypeData>> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ITypeSymbol"/>s to add to the container.</param>
	///  <typeparam name="TData">Type of target <see cref="ITypeData"/>.</typeparam>
	public static SymbolContainerBuilder<SymbolContainer<ITypeSymbol, TData>> ToContainer<TData>(this IEnumerable<ITypeSymbol> collection) where TData : class, ITypeData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ITypeSymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="ITypeData"/>.</typeparam>
	public static SymbolContainerBuilder<SymbolContainer<ITypeSymbol, TData>> ToContainer<TData>(this IEnumerable<ISymbolOrMember<ITypeSymbol, TData>> collection) where TData : class, ITypeData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<SymbolContainer<INamedTypeSymbol, ITypeData>> ToContainer(this IEnumerable<INamedTypeSymbol> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<SymbolContainer<INamedTypeSymbol, ITypeData>> ToContainer(this IEnumerable<ISymbolOrMember<INamedTypeSymbol, ITypeData>> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
	///  <typeparam name="TData">Type of target <see cref="ITypeData"/>.</typeparam>
	public static SymbolContainerBuilder<SymbolContainer<INamedTypeSymbol, TData>> ToContainer<TData>(this IEnumerable<INamedTypeSymbol> collection) where TData : class, ITypeData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="ITypeData"/>.</typeparam>
	public static SymbolContainerBuilder<SymbolContainer<INamedTypeSymbol, TData>> ToContainer<TData>(this IEnumerable<ISymbolOrMember<INamedTypeSymbol, TData>> collection) where TData : class, ITypeData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ILocalSymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<SymbolContainer<ILocalSymbol, ILocalData>> ToContainer(this IEnumerable<ILocalSymbol> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ILocalSymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<SymbolContainer<ILocalSymbol, ILocalData>> ToContainer(this IEnumerable<ISymbolOrMember<ILocalSymbol, ILocalData>> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="ILocalData"/>.</typeparam>
	public static SymbolContainerBuilder<SymbolContainer<ILocalSymbol, TData>> ToContainer<TData>(this IEnumerable<ILocalSymbol> collection) where TData : class, ILocalData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ILocalSymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="ILocalData"/>.</typeparam>
	public static SymbolContainerBuilder<SymbolContainer<ILocalSymbol, TData>> ToContainer<TData>(this IEnumerable<ISymbolOrMember<ILocalSymbol, TData>> collection) where TData : class, ILocalData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IFieldSymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<SymbolContainer<IFieldSymbol, IFieldData>> ToContainer(this IEnumerable<IFieldSymbol> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IFieldSymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<SymbolContainer<IFieldSymbol, IFieldData>> ToContainer(this IEnumerable<ISymbolOrMember<IFieldSymbol, IFieldData>> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IFieldSymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="IFieldData"/>.</typeparam>
	public static SymbolContainerBuilder<SymbolContainer<IFieldSymbol, TData>> ToContainer<TData>(this IEnumerable<IFieldSymbol> collection) where TData : class, IFieldData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IFieldSymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="IFieldData"/>.</typeparam>
	public static SymbolContainerBuilder<SymbolContainer<IFieldSymbol, TData>> ToContainer<TData>(this IEnumerable<ISymbolOrMember<IFieldSymbol, TData>> collection) where TData : class, IFieldData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IParameterSymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<SymbolContainer<IParameterSymbol, IParameterData>> ToContainer(this IEnumerable<IParameterSymbol> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IParameterSymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<SymbolContainer<IParameterSymbol, IParameterData>> ToContainer(this IEnumerable<ISymbolOrMember<IParameterSymbol, IParameterData>> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IParameterSymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="IParameterData"/>.</typeparam>
	public static SymbolContainerBuilder<SymbolContainer<IParameterSymbol, TData>> ToContainer<TData>(this IEnumerable<IParameterSymbol> collection) where TData : class, IParameterData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IParameterSymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="IParameterData"/>.</typeparam>
	public static SymbolContainerBuilder<SymbolContainer<IParameterSymbol, TData>> ToContainer<TData>(this IEnumerable<ISymbolOrMember<IParameterSymbol, TData>> collection) where TData : class, IParameterData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IParameterSymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<SymbolContainer<INamespaceSymbol, INamespaceData>> ToContainer(this IEnumerable<INamespaceSymbol> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<SymbolContainer<INamespaceSymbol, INamespaceData>> ToContainer(this IEnumerable<ISymbolOrMember<INamespaceSymbol, INamespaceData>> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="INamespaceData"/>.</typeparam>
	public static SymbolContainerBuilder<SymbolContainer<INamespaceSymbol, TData>> ToContainer<TData>(this IEnumerable<INamespaceSymbol> collection) where TData : class, INamespaceData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="INamespaceData"/>.</typeparam>
	public static SymbolContainerBuilder<SymbolContainer<INamespaceSymbol, TData>> ToContainer<TData>(this IEnumerable<ISymbolOrMember<INamespaceSymbol, TData>> collection) where TData : class, INamespaceData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<SymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData>> ToContainer(this IEnumerable<INamespaceOrTypeSymbol> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<SymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData>> ToContainer(this IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="INamespaceOrTypeData"/>.</typeparam>
	public static SymbolContainerBuilder<SymbolContainer<INamespaceOrTypeSymbol, TData>> ToContainer<TData>(this IEnumerable<INamespaceOrTypeSymbol> collection) where TData : class, INamespaceOrTypeData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="INamespaceOrTypeData"/>.</typeparam>
	public static SymbolContainerBuilder<SymbolContainer<INamespaceOrTypeSymbol, TData>> ToContainer<TData>(this IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, TData>> collection) where TData : class, INamespaceOrTypeData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IEventSymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<SymbolContainer<IEventSymbol, IEventData>> ToContainer(this IEnumerable<IEventSymbol> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IEventData"/>s to add to the container.</param>
	public static SymbolContainerBuilder<SymbolContainer<IEventSymbol, IEventData>> ToContainer(this IEnumerable<ISymbolOrMember<IEventSymbol, IEventData>> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IEventSymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="IEventData"/>.</typeparam>
	public static SymbolContainerBuilder<SymbolContainer<IEventSymbol, TData>> ToContainer<TData>(this IEnumerable<IEventSymbol> collection) where TData : class, IEventData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IEventSymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="IEventData"/>.</typeparam>
	public static SymbolContainerBuilder<SymbolContainer<IEventSymbol, TData>> ToContainer<TData>(this IEnumerable<ISymbolOrMember<IEventSymbol, TData>> collection) where TData : class, IEventData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IPropertySymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<SymbolContainer<IPropertySymbol, IPropertyData>> ToContainer(this IEnumerable<IPropertySymbol> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IPropertyData"/>s to add to the container.</param>
	public static SymbolContainerBuilder<SymbolContainer<IPropertySymbol, IPropertyData>> ToContainer(this IEnumerable<ISymbolOrMember<IPropertySymbol, IPropertyData>> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IPropertySymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="IPropertyData"/>.</typeparam>
	public static SymbolContainerBuilder<SymbolContainer<IPropertySymbol, TData>> ToContainer<TData>(this IEnumerable<IPropertySymbol> collection) where TData : class, IPropertyData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IPropertySymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="IPropertyData"/>.</typeparam>
	public static SymbolContainerBuilder<SymbolContainer<IPropertySymbol, TData>> ToContainer<TData>(this IEnumerable<ISymbolOrMember<IPropertySymbol, TData>> collection) where TData : class, IPropertyData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IMethodSymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<SymbolContainer<IMethodSymbol, IMethodData>> ToContainer(this IEnumerable<IMethodSymbol> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IMethodSymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<SymbolContainer<IMethodSymbol, IMethodData>> ToContainer(this IEnumerable<ISymbolOrMember<IMethodSymbol, IMethodData>> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IMethodSymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="IMethodData"/>.</typeparam>
	public static SymbolContainerBuilder<SymbolContainer<IMethodSymbol, TData>> ToContainer<TData>(this IEnumerable<IMethodSymbol> collection) where TData : class, IMethodData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IParameterSymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="IMethodData"/>.</typeparam>
	public static SymbolContainerBuilder<SymbolContainer<IMethodSymbol, TData>> ToContainer<TData>(this IEnumerable<ISymbolOrMember<IMethodSymbol, TData>> collection) where TData : class, IMethodData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ISymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<ISymbol, IMemberData> ToContainer(this IEnumerable<ISymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ISymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<ISymbol, IMemberData> ToContainer(this IEnumerable<ISymbolOrMember> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection.Select(m => EnsureGeneric<ISymbol, IMemberData>(m)), parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ISymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<ISymbol, IMemberData> ToContainer(this IEnumerable<ISymbolOrMember<ISymbol, IMemberData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ITypeParameterSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<ITypeParameterSymbol, ITypeParameterData> ToContainer(this IEnumerable<ITypeParameterSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ITypeParameterSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<ITypeParameterSymbol, ITypeParameterData> ToContainer(this IEnumerable<ISymbolOrMember<ITypeParameterSymbol, ITypeParameterData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ITypeParameterSymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="ITypeParameterData"/>.</typeparam>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<ITypeParameterSymbol, TData> ToContainer<TData>(this IEnumerable<ITypeParameterSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, ITypeParameterData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ITypeParameterSymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="ITypeParameterData"/>.</typeparam>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<ITypeParameterSymbol, TData> ToContainer<TData>(this IEnumerable<ISymbolOrMember<ITypeParameterSymbol, TData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, ITypeParameterData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ITypeSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<ITypeSymbol, ITypeData> ToContainer(this IEnumerable<ITypeSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ITypeSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<ITypeSymbol, ITypeData> ToContainer(this IEnumerable<ISymbolOrMember<ITypeSymbol, ITypeData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="ITypeData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="ITypeSymbol"/>s to add to the container.</param>
	///  <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<ITypeSymbol, TData> ToContainer<TData>(this IEnumerable<ITypeSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, ITypeData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="ITypeData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="ITypeSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<ITypeSymbol, TData> ToContainer<TData>(this IEnumerable<ISymbolOrMember<ITypeSymbol, TData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, ITypeData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<INamedTypeSymbol, ITypeData> ToContainer(this IEnumerable<INamedTypeSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<INamedTypeSymbol, ITypeData> ToContainer(this IEnumerable<ISymbolOrMember<INamedTypeSymbol, ITypeData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="ITypeData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
	///  <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<INamedTypeSymbol, TData> ToContainer<TData>(this IEnumerable<INamedTypeSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, ITypeData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="ITypeData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<INamedTypeSymbol, TData> ToContainer<TData>(this IEnumerable<ISymbolOrMember<INamedTypeSymbol, TData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, ITypeData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ILocalSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<ILocalSymbol, ILocalData> ToContainer(this IEnumerable<ILocalSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ILocalSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<ILocalSymbol, ILocalData> ToContainer(this IEnumerable<ISymbolOrMember<ILocalSymbol, ILocalData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="ILocalData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<ILocalSymbol, TData> ToContainer<TData>(this IEnumerable<ILocalSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, ILocalData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="ILocalData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="ILocalSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<ILocalSymbol, TData> ToContainer<TData>(this IEnumerable<ISymbolOrMember<ILocalSymbol, TData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, ILocalData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IFieldSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<IFieldSymbol, IFieldData> ToContainer(this IEnumerable<IFieldSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IFieldSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<IFieldSymbol, IFieldData> ToContainer(this IEnumerable<ISymbolOrMember<IFieldSymbol, IFieldData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="IFieldData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="IFieldSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<IFieldSymbol, TData> ToContainer<TData>(this IEnumerable<IFieldSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, IFieldData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="IFieldData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="IFieldSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<IFieldSymbol, TData> ToContainer<TData>(this IEnumerable<ISymbolOrMember<IFieldSymbol, TData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, IFieldData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IParameterSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<IParameterSymbol, IParameterData> ToContainer(this IEnumerable<IParameterSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IParameterSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<IParameterSymbol, IParameterData> ToContainer(this IEnumerable<ISymbolOrMember<IParameterSymbol, IParameterData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="IParameterData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="IParameterSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<IParameterSymbol, TData> ToContainer<TData>(this IEnumerable<IParameterSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, IParameterData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="IParameterData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="IParameterSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<IParameterSymbol, TData> ToContainer<TData>(this IEnumerable<ISymbolOrMember<IParameterSymbol, TData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, IParameterData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IParameterSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<INamespaceSymbol, INamespaceData> ToContainer(this IEnumerable<INamespaceSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<INamespaceSymbol, INamespaceData> ToContainer(this IEnumerable<ISymbolOrMember<INamespaceSymbol, INamespaceData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="INamespaceData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<INamespaceSymbol, TData> ToContainer<TData>(this IEnumerable<INamespaceSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, INamespaceData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="INamespaceData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<INamespaceSymbol, TData> ToContainer<TData>(this IEnumerable<ISymbolOrMember<INamespaceSymbol, TData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, INamespaceData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData> ToContainer(this IEnumerable<INamespaceOrTypeSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData> ToContainer(this IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="INamespaceOrTypeData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<INamespaceOrTypeSymbol, TData> ToContainer<TData>(this IEnumerable<INamespaceOrTypeSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, INamespaceOrTypeData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="INamespaceOrTypeData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<INamespaceOrTypeSymbol, TData> ToContainer<TData>(this IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, TData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, INamespaceOrTypeData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IEventSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<IEventSymbol, IEventData> ToContainer(this IEnumerable<IEventSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IEventData"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<IEventSymbol, IEventData> ToContainer(this IEnumerable<ISymbolOrMember<IEventSymbol, IEventData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="IEventData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="IEventSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<IEventSymbol, TData> ToContainer<TData>(this IEnumerable<IEventSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, IEventData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="IEventData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="IEventSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<IEventSymbol, TData> ToContainer<TData>(this IEnumerable<ISymbolOrMember<IEventSymbol, TData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, IEventData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IPropertySymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<IPropertySymbol, IPropertyData> ToContainer(this IEnumerable<IPropertySymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IPropertyData"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<IPropertySymbol, IPropertyData> ToContainer(this IEnumerable<ISymbolOrMember<IPropertySymbol, IPropertyData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="IPropertyData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="IPropertySymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<IPropertySymbol, TData> ToContainer<TData>(this IEnumerable<IPropertySymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, IPropertyData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="IPropertyData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="IPropertySymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<IPropertySymbol, TData> ToContainer<TData>(this IEnumerable<ISymbolOrMember<IPropertySymbol, TData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, IPropertyData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IMethodSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<IMethodSymbol, IMethodData> ToContainer(this IEnumerable<IMethodSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="IMethodSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<IMethodSymbol, IMethodData> ToContainer(this IEnumerable<ISymbolOrMember<IMethodSymbol, IMethodData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="IMethodData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="IMethodSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<IMethodSymbol, TData> ToContainer<TData>(this IEnumerable<IMethodSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, IMethodData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="IMethodData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="IParameterSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static SymbolContainer<IMethodSymbol, TData> ToContainer<TData>(this IEnumerable<ISymbolOrMember<IMethodSymbol, TData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, IMethodData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ISymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<WritableSymbolContainer<ISymbol, IMemberData>> ToWritableContainer(this IEnumerable<ISymbolOrMember> collection)
	{
		return new(collection.Select(m => EnsureGeneric<ISymbol, IMemberData>(m)));
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ISymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<WritableSymbolContainer<ISymbol, IMemberData>> ToWritableContainer(this IEnumerable<ISymbolOrMember<ISymbol, IMemberData>> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<WritableSymbolContainer<INamespaceSymbol, INamespaceData>> ToWritableContainer(this IEnumerable<INamespaceSymbol> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<WritableSymbolContainer<INamespaceSymbol, INamespaceData>> ToWritableContainer(this IEnumerable<ISymbolOrMember<INamespaceSymbol, INamespaceData>> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
	///  <typeparam name="TData">Type of target <see cref="INamespaceData"/>.</typeparam>
	public static SymbolContainerBuilder<WritableSymbolContainer<INamespaceSymbol, TData>> ToWritableContainer<TData>(this IEnumerable<INamespaceSymbol> collection) where TData : class, INamespaceData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
	///  <typeparam name="TData">Type of target <see cref="INamespaceData"/>.</typeparam>
	public static SymbolContainerBuilder<WritableSymbolContainer<INamespaceSymbol, TData>> ToWritableContainer<TData>(this IEnumerable<ISymbolOrMember<INamespaceSymbol, TData>> collection) where TData : class, INamespaceData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<WritableTypeContainer<ITypeData>> ToWritableContainer(this IEnumerable<INamedTypeSymbol> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<WritableTypeContainer<ITypeData>> ToWritableContainer(this IEnumerable<ISymbolOrMember<INamedTypeSymbol, ITypeData>> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="ITypeData"/>.</typeparam>
	public static SymbolContainerBuilder<WritableTypeContainer<TData>> ToWritableContainer<TData>(this IEnumerable<INamedTypeSymbol> collection) where TData : class, ITypeData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="ITypeData"/>.</typeparam>
	public static SymbolContainerBuilder<WritableTypeContainer<TData>> ToWritableContainer<TData>(this IEnumerable<ISymbolOrMember<INamedTypeSymbol, TData>> collection) where TData : class, ITypeData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<WritableNamespaceOrTypeContainer<INamespaceOrTypeData>> ToWritableContainer(this IEnumerable<INamespaceOrTypeSymbol> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
	public static SymbolContainerBuilder<WritableNamespaceOrTypeContainer<INamespaceOrTypeData>> ToWritableContainer(this IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>> collection)
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="INamespaceOrTypeData"/>.</typeparam>
	public static SymbolContainerBuilder<WritableNamespaceOrTypeContainer<TData>> ToWritableContainer<TData>(this IEnumerable<INamespaceOrTypeSymbol> collection) where TData : class, INamespaceOrTypeData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
	/// <typeparam name="TData">Type of target <see cref="INamespaceOrTypeData"/>.</typeparam>
	public static SymbolContainerBuilder<WritableNamespaceOrTypeContainer<TData>> ToWritableContainer<TData>(this IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, TData>> collection) where TData : class, INamespaceOrTypeData
	{
		return new(collection);
	}

	/// <summary>
	/// Creates a new <see cref="WritableSymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ISymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static WritableSymbolContainer<ISymbol, IMemberData> ToWritableContainer(this IEnumerable<ISymbolOrMember> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection.Select(m => EnsureGeneric<ISymbol, IMemberData>(m)), parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="WritableSymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="ISymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static WritableSymbolContainer<ISymbol, IMemberData> ToWritableContainer(this IEnumerable<ISymbolOrMember<ISymbol, IMemberData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="WritableSymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static WritableSymbolContainer<INamespaceSymbol, INamespaceData> ToWritableContainer(this IEnumerable<INamespaceSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="WritableSymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static WritableSymbolContainer<INamespaceSymbol, INamespaceData> ToWritableContainer(this IEnumerable<ISymbolOrMember<INamespaceSymbol, INamespaceData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="WritableSymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="INamespaceData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static WritableSymbolContainer<INamespaceSymbol, TData> ToWritableContainer<TData>(this IEnumerable<INamespaceSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, INamespaceData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="WritableSymbolContainer{TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="INamespaceData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static WritableSymbolContainer<INamespaceSymbol, TData> ToWritableContainer<TData>(this IEnumerable<ISymbolOrMember<INamespaceSymbol, TData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, INamespaceData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="WritableTypeContainer{TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static WritableTypeContainer<ITypeData> ToWritableContainer(this IEnumerable<INamedTypeSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="WritableTypeContainer{TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static WritableTypeContainer<ITypeData> ToWritableContainer(this IEnumerable<ISymbolOrMember<INamedTypeSymbol, ITypeData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="WritableTypeContainer{TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="ITypeData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static WritableTypeContainer<TData> ToWritableContainer<TData>(this IEnumerable<INamedTypeSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, ITypeData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="WritableTypeContainer{TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="ITypeData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static WritableTypeContainer<TData> ToWritableContainer<TData>(this IEnumerable<ISymbolOrMember<INamedTypeSymbol, TData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, ITypeData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="WritableNamespaceOrTypeContainer{TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static WritableNamespaceOrTypeContainer<INamespaceOrTypeData> ToWritableContainer(this IEnumerable<INamespaceOrTypeSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="WritableNamespaceOrTypeContainer{TData}"/>.
	/// </summary>
	/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static WritableNamespaceOrTypeContainer<INamespaceOrTypeData> ToWritableContainer(this IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="WritableNamespaceOrTypeContainer{TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="INamespaceOrTypeData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static WritableNamespaceOrTypeContainer<TData> ToWritableContainer<TData>(this IEnumerable<INamespaceOrTypeSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, INamespaceOrTypeData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	/// <summary>
	/// Creates a new <see cref="WritableNamespaceOrTypeContainer{TData}"/>.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="INamespaceOrTypeData"/>.</typeparam>
	/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
	/// <param name="parentCompilation">
	/// Parent <see cref="ICompilationData"/> of the current container.
	/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
	/// </param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	public static WritableNamespaceOrTypeContainer<TData> ToWritableContainer<TData>(this IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, TData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, INamespaceOrTypeData
	{
		return new(collection, parentCompilation, nameResolver);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static Exception Exc_EmptySymbolContainer()
	{
		return new EmptyContainerException("Container does not contain any symbols");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void ValidateLevel(int level, int numLevels)
	{
		if (level < 0 || level >= numLevels)
		{
			throw new ArgumentOutOfRangeException(nameof(level), "Level must be greater than 0 and less than NumLevels");
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ISymbolOrMember<TSymbol, TData> EnsureGeneric<TSymbol, TData>(ISymbolOrMember original)
		where TSymbol : class, ISymbol
		where TData : class, IMemberData
	{
		if (original is ISymbolOrMember<TSymbol, TData> member)
		{
			return member;
		}

		return new GenericMemberWrapper<TSymbol, TData>(original);
	}
}
