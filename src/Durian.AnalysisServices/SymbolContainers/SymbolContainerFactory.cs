// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// Provides factory methods for implementations of the <see cref="ISymbolContainer"/> interface.
	/// </summary>
	public static class SymbolContainerFactory
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

		private class GenericWrapper<TSymbol, TData> : ISymbolOrMember<TSymbol, TData>
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
			/// Initializes a new instance of the <see cref="GenericWrapper{TSymbol, TData}"/> class.
			/// </summary>
			/// <param name="underlaying">Underlaying <see cref="ISymbolOrMember"/>.</param>
			public GenericWrapper(ISymbolOrMember underlaying)
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
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for property or event accessors.
		/// </summary>
		public static SymbolContainer<IMethodSymbol, AccessorData> ForAccessors()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for any member kind.
		/// </summary>
		public static SymbolContainer<ISymbol, IMemberData> ForAnyMembers()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for any method kind.
		/// </summary>
		public static SymbolContainer<IMethodSymbol, IMethodData> ForAnyMethods()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for any type kind.
		/// </summary>
		public static SymbolContainer<ITypeSymbol, ITypeData> ForAnyTypes()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for classes.
		/// </summary>
		public static SymbolContainer<INamedTypeSymbol, ClassData> ForClasses()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for constructors.
		/// </summary>
		public static SymbolContainer<IMethodSymbol, ConstructorData> ForConstructors()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for conversion operators.
		/// </summary>
		public static SymbolContainer<IMethodSymbol, ConversionOperatorData> ForConversionOperators()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for delegates.
		/// </summary>
		public static SymbolContainer<INamedTypeSymbol, DelegateData> ForDelegates()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for destructors.
		/// </summary>
		public static SymbolContainer<IMethodSymbol, DestructorData> ForDestructors()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for enums.
		/// </summary>
		public static SymbolContainer<INamedTypeSymbol, EnumData> ForEnums()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for events.
		/// </summary>
		public static SymbolContainer<IEventSymbol, EventData> ForEvents()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for fields.
		/// </summary>
		public static SymbolContainer<IFieldSymbol, FieldData> ForFields()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for indexers.
		/// </summary>
		public static SymbolContainer<IPropertySymbol, IndexerData> ForIndexers()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for interfaces.
		/// </summary>
		public static SymbolContainer<INamedTypeSymbol, InterfaceData> ForInterfaces()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for lambdas.
		/// </summary>
		public static SymbolContainer<IMethodSymbol, LambdaData> ForLambdas()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for local functions.
		/// </summary>
		public static SymbolContainer<IMethodSymbol, LocalFunctionData> ForLocalFunctions()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for locals.
		/// </summary>
		public static SymbolContainer<ILocalSymbol, LocalData> ForLocals()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for methods.
		/// </summary>
		public static SymbolContainer<IMethodSymbol, MethodData> ForMethods()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for namespaces.
		/// </summary>
		public static SymbolContainer<INamespaceSymbol, NamespaceData> ForNamespaces()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for namespaces or types.
		/// </summary>
		public static SymbolContainer<INamespaceOrTypeSymbol, NamespaceOrTypeData> ForNamespacesOrTypes()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for operators.
		/// </summary>
		public static SymbolContainer<IMethodSymbol, OperatorData> ForOperators()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for parameters.
		/// </summary>
		public static SymbolContainer<IParameterSymbol, ParameterData> ForParameters()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for properties.
		/// </summary>
		public static SymbolContainer<IPropertySymbol, PropertyData> ForProperties()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for records.
		/// </summary>
		public static SymbolContainer<INamedTypeSymbol, RecordData> ForRecords()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for structs.
		/// </summary>
		public static SymbolContainer<INamedTypeSymbol, StructData> ForStructs()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for type parameters.
		/// </summary>
		public static SymbolContainer<ITypeParameterSymbol, TypeParameterData> ForTypeParameters()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for methods of unknown kind.
		/// </summary>
		public static SymbolContainer<IMethodSymbol, UnknownMethodData> ForUnknownMethods()
		{
			return new();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for types of unknown kind.
		/// </summary>
		public static SymbolContainer<ITypeSymbol, UnknownTypeData> ForUnknownTypes()
		{
			return new();
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ISymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<ISymbol, IMemberData> ToContainer(this IEnumerable<ISymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ISymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<ISymbol, IMemberData> ToContainer(this IEnumerable<ISymbolOrMember> collection, ICompilationData? compilation = default)
		{
			return new(collection.Select(m => EnsureGeneric<ISymbol, IMemberData>(m)), compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ISymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<ISymbol, IMemberData> ToContainer(this IEnumerable<ISymbolOrMember<ISymbol, IMemberData>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ITypeParameterSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<ITypeParameterSymbol, ITypeParameterData> ToContainer(this IEnumerable<ITypeParameterSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ITypeParameterSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<ITypeParameterSymbol, ITypeParameterData> ToContainer(this IEnumerable<ISymbolOrMember<ITypeParameterSymbol, ITypeParameterData>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ITypeParameterSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <typeparam name="TData">Type of target <see cref="ITypeParameterData"/>.</typeparam>
		public static SymbolContainer<ITypeParameterSymbol, TData> ToContainer<TData>(this IEnumerable<ITypeParameterSymbol> collection, ICompilationData? compilation = default) where TData : class, ITypeParameterData
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ITypeParameterSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <typeparam name="TData">Type of target <see cref="ITypeParameterData"/>.</typeparam>
		public static SymbolContainer<ITypeParameterSymbol, TData> ToContainer<TData>(this IEnumerable<ISymbolOrMember<ITypeParameterSymbol, TData>> collection, ICompilationData? compilation = default) where TData : class, ITypeParameterData
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<INamedTypeSymbol, ITypeData> ToContainer(this IEnumerable<INamedTypeSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<INamedTypeSymbol, ITypeData> ToContainer(this IEnumerable<ISymbolOrMember<INamedTypeSymbol, ITypeData>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		///  <typeparam name="TData">Type of target <see cref="ITypeData"/>.</typeparam>
		public static SymbolContainer<INamedTypeSymbol, TData> ToContainer<TData>(this IEnumerable<INamedTypeSymbol> collection, ICompilationData? compilation = default) where TData : class, ITypeData
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <typeparam name="TData">Type of target <see cref="ITypeData"/>.</typeparam>
		public static SymbolContainer<INamedTypeSymbol, TData> ToContainer<TData>(this IEnumerable<ISymbolOrMember<INamedTypeSymbol, TData>> collection, ICompilationData? compilation = default) where TData : class, ITypeData
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ILocalSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<ILocalSymbol, ILocalData> ToContainer(this IEnumerable<ILocalSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ILocalSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<ILocalSymbol, ILocalData> ToContainer(this IEnumerable<ISymbolOrMember<ILocalSymbol, ILocalData>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <typeparam name="TData">Type of target <see cref="ILocalData"/>.</typeparam>
		public static SymbolContainer<ILocalSymbol, TData> ToContainer<TData>(this IEnumerable<ILocalSymbol> collection, ICompilationData? compilation = default) where TData : class, ILocalData
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ILocalSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <typeparam name="TData">Type of target <see cref="ILocalData"/>.</typeparam>
		public static SymbolContainer<ILocalSymbol, TData> ToContainer<TData>(this IEnumerable<ISymbolOrMember<ILocalSymbol, TData>> collection, ICompilationData? compilation = default) where TData : class, ILocalData
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IFieldSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<IFieldSymbol, IFieldData> ToContainer(this IEnumerable<IFieldSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IFieldSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<IFieldSymbol, IFieldData> ToContainer(this IEnumerable<ISymbolOrMember<IFieldSymbol, IFieldData>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IFieldSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <typeparam name="TData">Type of target <see cref="IFieldData"/>.</typeparam>
		public static SymbolContainer<IFieldSymbol, TData> ToContainer<TData>(this IEnumerable<IFieldSymbol> collection, ICompilationData? compilation = default) where TData : class, IFieldData
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IFieldSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <typeparam name="TData">Type of target <see cref="IFieldData"/>.</typeparam>
		public static SymbolContainer<IFieldSymbol, TData> ToContainer<TData>(this IEnumerable<ISymbolOrMember<IFieldSymbol, TData>> collection, ICompilationData? compilation = default) where TData : class, IFieldData
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IParameterSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<IParameterSymbol, IParameterData> ToContainer(this IEnumerable<IParameterSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IParameterSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<IParameterSymbol, IParameterData> ToContainer(this IEnumerable<ISymbolOrMember<IParameterSymbol, IParameterData>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IParameterSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <typeparam name="TData">Type of target <see cref="IParameterData"/>.</typeparam>
		public static SymbolContainer<IParameterSymbol, TData> ToContainer<TData>(this IEnumerable<IParameterSymbol> collection, ICompilationData? compilation = default) where TData : class, IParameterData
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IParameterSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <typeparam name="TData">Type of target <see cref="IParameterData"/>.</typeparam>
		public static SymbolContainer<IParameterSymbol, TData> ToContainer<TData>(this IEnumerable<ISymbolOrMember<IParameterSymbol, TData>> collection, ICompilationData? compilation = default) where TData : class, IParameterData
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IParameterSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<INamespaceSymbol, INamespaceData> ToContainer(this IEnumerable<INamespaceSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<INamespaceSymbol, INamespaceData> ToContainer(this IEnumerable<ISymbolOrMember<INamespaceSymbol, INamespaceData>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <typeparam name="TData">Type of target <see cref="INamespaceData"/>.</typeparam>
		public static SymbolContainer<INamespaceSymbol, TData> ToContainer<TData>(this IEnumerable<INamespaceSymbol> collection, ICompilationData? compilation = default) where TData : class, INamespaceData
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <typeparam name="TData">Type of target <see cref="INamespaceData"/>.</typeparam>
		public static SymbolContainer<INamespaceSymbol, TData> ToContainer<TData>(this IEnumerable<ISymbolOrMember<INamespaceSymbol, TData>> collection, ICompilationData? compilation = default) where TData : class, INamespaceData
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData> ToContainer(this IEnumerable<INamespaceOrTypeSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData> ToContainer(this IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <typeparam name="TData">Type of target <see cref="INamespaceOrTypeData"/>.</typeparam>
		public static SymbolContainer<INamespaceOrTypeSymbol, TData> ToContainer<TData>(this IEnumerable<INamespaceOrTypeSymbol> collection, ICompilationData? compilation = default) where TData : class, INamespaceOrTypeData
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <typeparam name="TData">Type of target <see cref="INamespaceOrTypeData"/>.</typeparam>
		public static SymbolContainer<INamespaceOrTypeSymbol, TData> ToContainer<TData>(this IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, TData>> collection, ICompilationData? compilation = default) where TData : class, INamespaceOrTypeData
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IEventSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<IEventSymbol, IEventData> ToContainer(this IEnumerable<IEventSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IEventData"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<IEventSymbol, IEventData> ToContainer(this IEnumerable<ISymbolOrMember<IEventSymbol, IEventData>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IEventSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <typeparam name="TData">Type of target <see cref="IEventData"/>.</typeparam>
		public static SymbolContainer<IEventSymbol, TData> ToContainer<TData>(this IEnumerable<IEventSymbol> collection, ICompilationData? compilation = default) where TData : class, IEventData
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IEventSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <typeparam name="TData">Type of target <see cref="IEventData"/>.</typeparam>
		public static SymbolContainer<IEventSymbol, TData> ToContainer<TData>(this IEnumerable<ISymbolOrMember<IEventSymbol, TData>> collection, ICompilationData? compilation = default) where TData : class, IEventData
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IMethodSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<IMethodSymbol, IMethodData> ToContainer(this IEnumerable<IMethodSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IMethodSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<IMethodSymbol, IMethodData> ToContainer(this IEnumerable<ISymbolOrMember<IMethodSymbol, IMethodData>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IMethodSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <typeparam name="TData">Type of target <see cref="IMethodData"/>.</typeparam>
		public static SymbolContainer<IMethodSymbol, TData> ToContainer<TData>(this IEnumerable<IMethodSymbol> collection, ICompilationData? compilation = default) where TData : class, IMethodData
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IParameterSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <typeparam name="TData">Type of target <see cref="IMethodData"/>.</typeparam>
		public static SymbolContainer<IMethodSymbol, TData> ToContainer<TData>(this IEnumerable<ISymbolOrMember<IMethodSymbol, TData>> collection, ICompilationData? compilation = default) where TData : class, IMethodData
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="WritableSymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ISymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static WritableSymbolContainer<ISymbol, IMemberData> ToWritableContainer(this IEnumerable<ISymbolOrMember> collection, ICompilationData? compilation = default)
		{
			return new(collection.Select(m => EnsureGeneric<ISymbol, IMemberData>(m)), compilation);
		}

		/// <summary>
		/// Creates a new <see cref="WritableSymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ISymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static WritableSymbolContainer<ISymbol, IMemberData> ToWritableContainer(this IEnumerable<ISymbolOrMember<ISymbol, IMemberData>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="WritableSymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static WritableSymbolContainer<INamespaceSymbol, INamespaceData> ToWritableContainer(this IEnumerable<INamespaceSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="WritableSymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static WritableSymbolContainer<INamespaceSymbol, INamespaceData> ToWritableContainer(this IEnumerable<ISymbolOrMember<INamespaceSymbol, INamespaceData>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="WritableSymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		///  <typeparam name="TData">Type of target <see cref="INamespaceData"/>.</typeparam>
		public static WritableSymbolContainer<INamespaceSymbol, TData> ToWritableContainer<TData>(this IEnumerable<INamespaceSymbol> collection, ICompilationData? compilation = default) where TData : class, INamespaceData
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="WritableSymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		///  <typeparam name="TData">Type of target <see cref="INamespaceData"/>.</typeparam>
		public static WritableSymbolContainer<INamespaceSymbol, TData> ToWritableContainer<TData>(this IEnumerable<ISymbolOrMember<INamespaceSymbol, TData>> collection, ICompilationData? compilation = default) where TData : class, INamespaceData
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="GenericSymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <param name="useArguments">Determines whether to use type arguments instead of type parameters when building a <see cref="string"/>.</param>
		public static GenericSymbolContainer<INamedTypeSymbol, ITypeData> ToWritableContainer(this IEnumerable<INamedTypeSymbol> collection, ICompilationData? compilation = default, bool useArguments = false)
		{
			return new(collection, compilation) { UseArguments = useArguments };
		}

		/// <summary>
		/// Creates a new <see cref="GenericSymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <param name="useArguments">Determines whether to use type arguments instead of type parameters when building a <see cref="string"/>.</param>
		public static GenericSymbolContainer<INamedTypeSymbol, ITypeData> ToWritableContainer(this IEnumerable<ISymbolOrMember<INamedTypeSymbol, ITypeData>> collection, ICompilationData? compilation = default, bool useArguments = false)
		{
			return new(collection, compilation) { UseArguments = useArguments };
		}

		/// <summary>
		/// Creates a new <see cref="GenericSymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <param name="useArguments">Determines whether to use type arguments instead of type parameters when building a <see cref="string"/>.</param>
		/// <typeparam name="TData">Type of target <see cref="ITypeData"/>.</typeparam>
		public static GenericSymbolContainer<INamedTypeSymbol, TData> ToWritableContainer<TData>(this IEnumerable<INamedTypeSymbol> collection, ICompilationData? compilation = default, bool useArguments = false) where TData : class, ITypeData
		{
			return new(collection, compilation) { UseArguments = useArguments };
		}

		/// <summary>
		/// Creates a new <see cref="GenericSymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <param name="useArguments">Determines whether to use type arguments instead of type parameters when building a <see cref="string"/>.</param>
		/// <typeparam name="TData">Type of target <see cref="ITypeData"/>.</typeparam>
		public static GenericSymbolContainer<INamedTypeSymbol, TData> ToWritableContainer<TData>(this IEnumerable<ISymbolOrMember<INamedTypeSymbol, TData>> collection, ICompilationData? compilation = default, bool useArguments = false) where TData : class, ITypeData
		{
			return new(collection, compilation) { UseArguments = useArguments };
		}

		/// <summary>
		/// Creates a new <see cref="NamespaceOrTypeContainer"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <param name="useArguments">Determines whether to use type arguments instead of type parameters when building a <see cref="string"/>.</param>
		public static NamespaceOrTypeContainer ToWritableContainer(this IEnumerable<INamespaceOrTypeSymbol> collection, ICompilationData? compilation = default, bool useArguments = false)
		{
			return new(collection, compilation) { UseArguments = useArguments };
		}

		/// <summary>
		/// Creates a new <see cref="NamespaceOrTypeContainer"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <param name="useArguments">Determines whether to use type arguments instead of type parameters when building a <see cref="string"/>.</param>
		public static NamespaceOrTypeContainer ToWritableContainer(this IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, ITypeData>> collection, ICompilationData? compilation = default, bool useArguments = false)
		{
			return new(collection as Inamed, compilation) { UseArguments = useArguments };
		}

		/// <summary>
		/// Creates a new <see cref="NamespaceOrTypeContainer"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <param name="useArguments">Determines whether to use type arguments instead of type parameters when building a <see cref="string"/>.</param>
		public static NamespaceOrTypeContainer ToWritableContainer(this IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceData>> collection, ICompilationData? compilation = default, bool useArguments = false)
		{
			return new(collection as Inamed, compilation) { UseArguments = useArguments };
		}

		/// <summary>
		/// Creates a new <see cref="NamespaceOrTypeContainer"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <param name="useArguments">Determines whether to use type arguments instead of type parameters when building a <see cref="string"/>.</param>
		public static NamespaceOrTypeContainer ToWritableContainer(this IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>> collection, ICompilationData? compilation = default, bool useArguments = false)
		{
			return new(collection, compilation) { UseArguments = useArguments };
		}

		internal static void DefaultBuild(this ISymbolContainer container, StringBuilder builder)
		{
			ImmutableArray<string> names = container.GetNames();

			if (names.Length == 0)
			{
				return;
			}

			builder.Append(names[0]);

			for (int i = 1; i < names.Length; i++)
			{
				builder.Append('.');
				builder.Append(names[i]);
			}
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

			return new GenericWrapper<TSymbol, TData>(original);
		}
	}
}
