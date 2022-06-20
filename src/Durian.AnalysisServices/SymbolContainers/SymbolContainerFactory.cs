// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
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
		private class EmptySymbolContainer<T> : ISymbolContainer<T> where T : class, ISymbol
		{
			private readonly List<ISymbolOrMember<T>> _list = new();

			public static EmptySymbolContainer<T> Instance { get; } = new();

			public int Count => 0;
			public ReturnOrder Order => default;
			public ICompilationData? ParentCompilation => null;

			public ISymbolOrMember<T> First()
			{
				throw Exc_EmptySymbolContainer();
			}

			public ISymbolOrMember<T> First(ReturnOrder order)
			{
				throw Exc_EmptySymbolContainer();
			}

			public ImmutableArray<IMemberData> GetData()
			{
				return ImmutableArray<IMemberData>.Empty;
			}

			public IEnumerator<ISymbolOrMember<T>> GetEnumerator()
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

			public ISymbolOrMember<T> Last()
			{
				throw Exc_EmptySymbolContainer();
			}

			public ISymbolOrMember<T> Last(ReturnOrder order)
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

		/// <summary>
		/// Returns a shared instance of an empty container.
		/// </summary>
		/// <typeparam name="T">Type of symbols stored in the container.</typeparam>
		public static ISymbolContainer<T> Empty<T>() where T : class, ISymbol
		{
			return EmptySymbolContainer<T>.Instance;
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for property or event accessors.
		/// </summary>
		public static SymbolContainer<IMethodSymbol, AccessorData> ForAccessors()
		{
			return new SymbolContainer<IMethodSymbol, AccessorData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for any member kind.
		/// </summary>
		public static SymbolContainer<ISymbol, IMemberData> ForAnyMember()
		{
			return new SymbolContainer<ISymbol, IMemberData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for any method kind.
		/// </summary>
		public static SymbolContainer<IMethodSymbol, IMethodData> ForAnyMethod()
		{
			return new SymbolContainer<IMethodSymbol, IMethodData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for any type kind.
		/// </summary>
		public static SymbolContainer<ITypeSymbol, ITypeData> ForAnyType()
		{
			return new SymbolContainer<ITypeSymbol, ITypeData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for classes.
		/// </summary>
		public static SymbolContainer<INamedTypeSymbol, ClassData> ForClasses()
		{
			return new SymbolContainer<INamedTypeSymbol, ClassData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for constructors.
		/// </summary>
		public static SymbolContainer<IMethodSymbol, ConstructorData> ForConstructors()
		{
			return new SymbolContainer<IMethodSymbol, ConstructorData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for conversion operators.
		/// </summary>
		public static SymbolContainer<IMethodSymbol, ConversionOperatorData> ForConversionOperators()
		{
			return new SymbolContainer<IMethodSymbol, ConversionOperatorData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for delegates.
		/// </summary>
		public static SymbolContainer<INamedTypeSymbol, DelegateData> ForDelegates()
		{
			return new SymbolContainer<INamedTypeSymbol, DelegateData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for destructors.
		/// </summary>
		public static SymbolContainer<IMethodSymbol, DestructorData> ForDestructors()
		{
			return new SymbolContainer<IMethodSymbol, DestructorData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for enums.
		/// </summary>
		public static SymbolContainer<INamedTypeSymbol, EnumData> ForEnums()
		{
			return new SymbolContainer<INamedTypeSymbol, EnumData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for events.
		/// </summary>
		public static SymbolContainer<IEventSymbol, EventData> ForEvents()
		{
			return new SymbolContainer<IEventSymbol, EventData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for fields.
		/// </summary>
		public static SymbolContainer<IFieldSymbol, FieldData> ForFields()
		{
			return new SymbolContainer<IFieldSymbol, FieldData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for indexers.
		/// </summary>
		public static SymbolContainer<IPropertySymbol, IndexerData> ForIndexers()
		{
			return new SymbolContainer<IPropertySymbol, IndexerData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for interfaces.
		/// </summary>
		public static SymbolContainer<INamedTypeSymbol, InterfaceData> ForInterfaces()
		{
			return new SymbolContainer<INamedTypeSymbol, InterfaceData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for lambdas.
		/// </summary>
		public static SymbolContainer<IMethodSymbol, LambdaData> ForLambdas()
		{
			return new SymbolContainer<IMethodSymbol, LambdaData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for local functions.
		/// </summary>
		public static SymbolContainer<IMethodSymbol, LocalFunctionData> ForLocalFunctions()
		{
			return new SymbolContainer<IMethodSymbol, LocalFunctionData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for locals.
		/// </summary>
		public static SymbolContainer<ILocalSymbol, LocalData> ForLocals()
		{
			return new SymbolContainer<ILocalSymbol, LocalData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for methods.
		/// </summary>
		public static SymbolContainer<IMethodSymbol, MethodData> ForMethods()
		{
			return new SymbolContainer<IMethodSymbol, MethodData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for namespaces.
		/// </summary>
		public static SymbolContainer<INamespaceSymbol, NamespaceData> ForNamespaces()
		{
			return new SymbolContainer<INamespaceSymbol, NamespaceData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for namespaces or types.
		/// </summary>
		public static SymbolContainer<INamespaceOrTypeSymbol, NamespaceOrTypeData> ForNamespacesOrTypes()
		{
			return new SymbolContainer<INamespaceOrTypeSymbol, NamespaceOrTypeData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for operators.
		/// </summary>
		public static SymbolContainer<IMethodSymbol, OperatorData> ForOperators()
		{
			return new SymbolContainer<IMethodSymbol, OperatorData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for parameters.
		/// </summary>
		public static SymbolContainer<IParameterSymbol, ParameterData> ForParameters()
		{
			return new SymbolContainer<IParameterSymbol, ParameterData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for properties.
		/// </summary>
		public static SymbolContainer<IPropertySymbol, PropertyData> ForProperties()
		{
			return new SymbolContainer<IPropertySymbol, PropertyData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for records.
		/// </summary>
		public static SymbolContainer<INamedTypeSymbol, RecordData> ForRecords()
		{
			return new SymbolContainer<INamedTypeSymbol, RecordData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for structs.
		/// </summary>
		public static SymbolContainer<INamedTypeSymbol, StructData> ForStructs()
		{
			return new SymbolContainer<INamedTypeSymbol, StructData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for type parameters.
		/// </summary>
		public static SymbolContainer<ITypeParameterSymbol, TypeParameterData> ForTypeParameters()
		{
			return new SymbolContainer<ITypeParameterSymbol, TypeParameterData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for methods of unknown kind.
		/// </summary>
		public static SymbolContainer<IMethodSymbol, UnknownMethodData> ForUnknownMethod()
		{
			return new SymbolContainer<IMethodSymbol, UnknownMethodData>();
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for types of unknown kind.
		/// </summary>
		public static SymbolContainer<ITypeSymbol, UnknownTypeData> ForUnknownType()
		{
			return new SymbolContainer<ITypeSymbol, UnknownTypeData>();
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ITypeParameterSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<ITypeParameterSymbol, TypeParameterData> ToContainer(this IEnumerable<ISymbolOrMember<ITypeParameterSymbol>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ITypeParameterSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<ITypeParameterSymbol, TypeParameterData> ToContainer(this IEnumerable<ISymbolOrMember<ITypeParameterSymbol, TypeParameterData>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ITypeParameterSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<ITypeParameterSymbol, TypeParameterData> ToContainer(this IEnumerable<ITypeParameterSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ITypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<ITypeSymbol, ITypeData> ToContainer(this IEnumerable<ISymbolOrMember<ITypeSymbol>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ITypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<ITypeSymbol, ITypeData> ToContainer(this IEnumerable<ITypeSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ISymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<ISymbol> ToContainer(this IEnumerable<ISymbolOrMember> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ISymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<ISymbol> ToContainer(this IEnumerable<ISymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IFieldSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<IFieldSymbol> ToContainer(this IEnumerable<ISymbolOrMember<IFieldSymbol>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IFieldSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<IFieldSymbol> ToContainer(this IEnumerable<IFieldSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IEventSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<IEventSymbol> ToContainer(this IEnumerable<ISymbolOrMember<IEventSymbol>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IEventSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<IEventSymbol> ToContainer(this IEnumerable<IEventSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IPropertySymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<IPropertySymbol> ToContainer(this IEnumerable<ISymbolOrMember<IPropertySymbol>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IPropertySymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<IPropertySymbol> ToContainer(this IEnumerable<IPropertySymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ILocalSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<ILocalSymbol> ToContainer(this IEnumerable<ISymbolOrMember<ILocalSymbol>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ILocalSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<ILocalSymbol> ToContainer(this IEnumerable<ILocalSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IMethodSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<IMethodSymbol> ToContainer(this IEnumerable<ISymbolOrMember<IMethodSymbol>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IMethodSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<IMethodSymbol> ToContainer(this IEnumerable<IMethodSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<INamespaceSymbol> ToContainer(this IEnumerable<ISymbolOrMember<INamespaceSymbol>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<INamespaceSymbol> ToContainer(this IEnumerable<INamespaceSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<INamedTypeSymbol> ToContainer(this IEnumerable<ISymbolOrMember<INamedTypeSymbol>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>>
		public static SymbolContainer<INamedTypeSymbol> ToContainer(this IEnumerable<INamedTypeSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<INamespaceOrTypeSymbol> ToContainer(this IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol, TData}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<INamespaceOrTypeSymbol> ToContainer(this IEnumerable<INamespaceOrTypeSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="WritableSymbolContainer{TSymbol}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static WritableSymbolContainer<INamespaceSymbol> ToWritableContainer(this IEnumerable<ISymbolOrMember<INamespaceSymbol>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="WritableSymbolContainer{TSymbol}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static WritableSymbolContainer<INamespaceSymbol> ToWritableContainer(this IEnumerable<INamespaceSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="GenericSymbolContainer{TSymbol}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <param name="useArguments">Determines whether to use type arguments instead of type parameters when building a <see cref="string"/>.</param>
		public static GenericSymbolContainer<INamedTypeSymbol> ToWritableContainer(this IEnumerable<ISymbolOrMember<INamedTypeSymbol>> collection, ICompilationData? compilation = default, bool useArguments = false)
		{
			return new(collection, compilation) { UseArguments = useArguments };
		}

		/// <summary>
		/// Creates a new <see cref="GenericSymbolContainer{TSymbol}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <param name="useArguments">Determines whether to use type arguments instead of type parameters when building a <see cref="string"/>.</param>
		public static GenericSymbolContainer<INamedTypeSymbol> ToWritableContainer(this IEnumerable<INamedTypeSymbol> collection, ICompilationData? compilation = default, bool useArguments = false)
		{
			return new(collection, compilation) { UseArguments = useArguments };
		}

		/// <summary>
		/// Creates a new <see cref="GenericSymbolContainer{TSymbol}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IMethodSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <param name="useArguments">Determines whether to use type arguments instead of type parameters when building a <see cref="string"/>.</param>
		public static GenericSymbolContainer<IMethodSymbol> ToWritableContainer(this IEnumerable<ISymbolOrMember<IMethodSymbol>> collection, ICompilationData? compilation = default, bool useArguments = false)
		{
			return new(collection, compilation) { UseArguments = useArguments };
		}

		/// <summary>
		/// Creates a new <see cref="GenericSymbolContainer{TSymbol}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IMethodSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <param name="useArguments">Determines whether to use type arguments instead of type parameters when building a <see cref="string"/>.</param>
		public static GenericSymbolContainer<IMethodSymbol> ToWritableContainer(this IEnumerable<IMethodSymbol> collection, ICompilationData? compilation = default, bool useArguments = false)
		{
			return new(collection, compilation) { UseArguments = useArguments };
		}

		/// <summary>
		/// Creates a new <see cref="NamespaceOrTypeContainer"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <param name="useArguments">Determines whether to use type arguments instead of type parameters when building a <see cref="string"/>.</param>
		public static NamespaceOrTypeContainer ToWritableContainer(this IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol>> collection, ICompilationData? compilation = default, bool useArguments = false)
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
			return new InvalidOperationException("Container does not contain any symbols");
		}
	}
}
