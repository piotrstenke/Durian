// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Durian.Analysis.Data;
using Durian.Analysis.SymbolContainers.Specialized;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// Provides factory methods for implementations of the <see cref="ISymbolContainer"/> interface.
	/// </summary>
	public static partial class SymbolContainerFactory
	{
		private class EmptySymbolContainer<TSymbol, TData> : ISymbolContainer<TSymbol, TData>
			where TSymbol : class, ISymbol
			where TData : class, IMemberData
		{
			private readonly List<ISymbolOrMember<TSymbol, TData>> _list = new();

			public static EmptySymbolContainer<TSymbol, TData> Instance { get; } = new();

			public int Count => 0;

			public ReturnOrder Order => default;

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
				// Do nothing.
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		private class GenericMemberWrapper<TSymbol, TData> : ISymbolOrMember<TSymbol, TData>
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

		/// <summary>
		/// Returns a shared instance of an empty container.
		/// </summary>
		public static ISymbolContainer<ISymbol, IMemberData> Empty()
		{
			return EmptySymbolContainer<ISymbol, IMemberData>.Instance;
		}

		/// <summary>
		/// Returns a shared instance of an empty container.
		/// </summary>
		/// <typeparam name="TSymbol">Type of <see cref="ISymbol"/>s stored in the container.</typeparam>
		public static ISymbolContainer<TSymbol, IMemberData> Empty<TSymbol>() where TSymbol : class, ISymbol
		{
			return EmptySymbolContainer<TSymbol, IMemberData>.Instance;
		}

		/// <summary>
		/// Returns a shared instance of an empty container.
		/// </summary>
		/// <typeparam name="TSymbol">Type of <see cref="ISymbol"/>s stored in the container.</typeparam>
		/// <typeparam name="TData">Type of <see cref="IMemberData"/>s stored in the container.</typeparam>
		public static ISymbolContainer<TSymbol, TData> Empty<TSymbol, TData>()
			where TSymbol : class, ISymbol
			where TData : class, IMemberData
		{
			return EmptySymbolContainer<TSymbol, TData>.Instance;
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
		public static SymbolContainerBuilder<WritableTypesContainer<ITypeData>> ToWritableContainer(this IEnumerable<INamedTypeSymbol> collection)
		{
			return new(collection);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
		public static SymbolContainerBuilder<WritableTypesContainer<ITypeData>> ToWritableContainer(this IEnumerable<ISymbolOrMember<INamedTypeSymbol, ITypeData>> collection)
		{
			return new(collection);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
		/// <typeparam name="TData">Type of target <see cref="ITypeData"/>.</typeparam>
		public static SymbolContainerBuilder<WritableTypesContainer<TData>> ToWritableContainer<TData>(this IEnumerable<INamedTypeSymbol> collection) where TData : class, ITypeData
		{
			return new(collection);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
		/// <typeparam name="TData">Type of target <see cref="ITypeData"/>.</typeparam>
		public static SymbolContainerBuilder<WritableTypesContainer<TData>> ToWritableContainer<TData>(this IEnumerable<ISymbolOrMember<INamedTypeSymbol, TData>> collection) where TData : class, ITypeData
		{
			return new(collection);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
		public static SymbolContainerBuilder<WritableNamespacesOrTypesContainer<INamespaceOrTypeData>> ToWritableContainer(this IEnumerable<INamespaceOrTypeSymbol> collection)
		{
			return new(collection);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
		public static SymbolContainerBuilder<WritableNamespacesOrTypesContainer<INamespaceOrTypeData>> ToWritableContainer(this IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>> collection)
		{
			return new(collection);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
		/// <typeparam name="TData">Type of target <see cref="INamespaceOrTypeData"/>.</typeparam>
		public static SymbolContainerBuilder<WritableNamespacesOrTypesContainer<TData>> ToWritableContainer<TData>(this IEnumerable<INamespaceOrTypeSymbol> collection) where TData : class, INamespaceOrTypeData
		{
			return new(collection);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainerBuilder{T}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
		/// <typeparam name="TData">Type of target <see cref="INamespaceOrTypeData"/>.</typeparam>
		public static SymbolContainerBuilder<WritableNamespacesOrTypesContainer<TData>> ToWritableContainer<TData>(this IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, TData>> collection) where TData : class, INamespaceOrTypeData
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
		/// Creates a new <see cref="WritableTypesContainer{TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
		/// <param name="parentCompilation">
		/// Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
		/// </param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		public static WritableTypesContainer<ITypeData> ToWritableContainer(this IEnumerable<INamedTypeSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(collection, parentCompilation, nameResolver);
		}

		/// <summary>
		/// Creates a new <see cref="WritableTypesContainer{TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
		/// <param name="parentCompilation">
		/// Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
		/// </param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		public static WritableTypesContainer<ITypeData> ToWritableContainer(this IEnumerable<ISymbolOrMember<INamedTypeSymbol, ITypeData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(collection, parentCompilation, nameResolver);
		}

		/// <summary>
		/// Creates a new <see cref="WritableTypesContainer{TData}"/>.
		/// </summary>
		/// <typeparam name="TData">Type of target <see cref="ITypeData"/>.</typeparam>
		/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
		/// <param name="parentCompilation">
		/// Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
		/// </param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		public static WritableTypesContainer<TData> ToWritableContainer<TData>(this IEnumerable<INamedTypeSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, ITypeData
		{
			return new(collection, parentCompilation, nameResolver);
		}

		/// <summary>
		/// Creates a new <see cref="WritableTypesContainer{TData}"/>.
		/// </summary>
		/// <typeparam name="TData">Type of target <see cref="ITypeData"/>.</typeparam>
		/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
		/// <param name="parentCompilation">
		/// Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
		/// </param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		public static WritableTypesContainer<TData> ToWritableContainer<TData>(this IEnumerable<ISymbolOrMember<INamedTypeSymbol, TData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, ITypeData
		{
			return new(collection, parentCompilation, nameResolver);
		}

		/// <summary>
		/// Creates a new <see cref="WritableNamespacesOrTypesContainer{TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
		/// <param name="parentCompilation">
		/// Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
		/// </param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		public static WritableNamespacesOrTypesContainer<INamespaceOrTypeData> ToWritableContainer(this IEnumerable<INamespaceOrTypeSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(collection, parentCompilation, nameResolver);
		}

		/// <summary>
		/// Creates a new <see cref="WritableNamespacesOrTypesContainer{TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
		/// <param name="parentCompilation">
		/// Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
		/// </param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		public static WritableNamespacesOrTypesContainer<INamespaceOrTypeData> ToWritableContainer(this IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(collection, parentCompilation, nameResolver);
		}

		/// <summary>
		/// Creates a new <see cref="WritableNamespacesOrTypesContainer{TData}"/>.
		/// </summary>
		/// <typeparam name="TData">Type of target <see cref="INamespaceOrTypeData"/>.</typeparam>
		/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
		/// <param name="parentCompilation">
		/// Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
		/// </param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		public static WritableNamespacesOrTypesContainer<TData> ToWritableContainer<TData>(this IEnumerable<INamespaceOrTypeSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, INamespaceOrTypeData
		{
			return new(collection, parentCompilation, nameResolver);
		}

		/// <summary>
		/// Creates a new <see cref="WritableNamespacesOrTypesContainer{TData}"/>.
		/// </summary>
		/// <typeparam name="TData">Type of target <see cref="INamespaceOrTypeData"/>.</typeparam>
		/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
		/// <param name="parentCompilation">
		/// Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
		/// </param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		public static WritableNamespacesOrTypesContainer<TData> ToWritableContainer<TData>(this IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, TData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) where TData : class, INamespaceOrTypeData
		{
			return new(collection, parentCompilation, nameResolver);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static Exception Exc_EmptySymbolContainer()
		{
			return new EmptyContainerException("Container does not contain any symbols");
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
}
