// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers
{
	public static partial class SymbolContainerFactory
	{
		/// <summary>
		/// Contains factory methods for creating empty <see cref="SymbolContainer{TSymbol, TData}"/>s for specific member kinds.
		/// </summary>
		public static class For
		{
			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for property or event accessors.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<IMethodSymbol, AccessorData>> Accessors()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for property or event accessors.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<IMethodSymbol, AccessorData> Accessors(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for any member kind.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<ISymbol, IMemberData>> AnyMembers()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for any member kind.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<ISymbol, IMemberData> AnyMembers(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for any method kind.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<IMethodSymbol, IMethodData>> AnyMethods()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for any method kind.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<IMethodSymbol, IMethodData> AnyMethods(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for any type kind.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<ITypeSymbol, ITypeData>> AnyTypes()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for any type kind.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<ITypeSymbol, ITypeData> AnyTypes(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for classes.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<INamedTypeSymbol, ClassData>> Classes()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for classes.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<INamedTypeSymbol, ClassData> Classes(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for constructors.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<IMethodSymbol, ConstructorData>> Constructors()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for constructors.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<IMethodSymbol, ConstructorData> Constructors(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for conversion operators.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<IMethodSymbol, ConversionOperatorData>> ConversionOperators()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for conversion operators.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<IMethodSymbol, ConversionOperatorData> ConversionOperators(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for delegates.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<INamedTypeSymbol, DelegateData>> Delegates()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for delegates.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<INamedTypeSymbol, DelegateData> Delegates(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for destructors.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<IMethodSymbol, DestructorData>> Destructors()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for destructors.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<IMethodSymbol, DestructorData> Destructors(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for enums.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<INamedTypeSymbol, EnumData>> Enums()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for enums.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<INamedTypeSymbol, EnumData> Enums(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for events.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<IEventSymbol, EventData>> Events()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for events.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<IEventSymbol, EventData> Events(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for fields.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<IFieldSymbol, FieldData>> Fields()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for fields.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<IFieldSymbol, FieldData> Fields(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for indexers.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<IPropertySymbol, IndexerData>> Indexers()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for indexers.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<IPropertySymbol, IndexerData> Indexers(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for interfaces.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<INamedTypeSymbol, InterfaceData>> Interfaces()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for interfaces.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<INamedTypeSymbol, InterfaceData> Interfaces(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for lambdas.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<IMethodSymbol, LambdaData>> Lambdas()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for lambdas.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<IMethodSymbol, LambdaData> Lambdas(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for local functions.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<IMethodSymbol, LocalFunctionData>> LocalFunctions()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for local functions.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<IMethodSymbol, LocalFunctionData> LocalFunctions(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for locals.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<ILocalSymbol, LocalData>> Locals()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for locals.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<ILocalSymbol, LocalData> Locals(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for methods.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<IMethodSymbol, MethodData>> Methods()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for methods.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<IMethodSymbol, MethodData> Methods(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for namespaces.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<INamespaceSymbol, NamespaceData>> Namespaces()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for namespaces.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<INamespaceSymbol, NamespaceData> Namespaces(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for namespaces or types.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<INamespaceOrTypeSymbol, NamespaceOrTypeData>> NamespacesOrTypes()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for namespaces or types.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<INamespaceOrTypeSymbol, NamespaceOrTypeData> NamespacesOrTypes(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for operators.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<IMethodSymbol, OperatorData>> Operators()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for operators.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<IMethodSymbol, OperatorData> Operators(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for parameters.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<IParameterSymbol, ParameterData>> Parameters()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for parameters.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<IParameterSymbol, ParameterData> Parameters(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for properties.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<IPropertySymbol, PropertyData>> Properties()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for properties.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<IPropertySymbol, PropertyData> Properties(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for records.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<INamedTypeSymbol, RecordData>> Records()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for records.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<INamedTypeSymbol, RecordData> Records(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for structs.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<INamedTypeSymbol, StructData>> Structs()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for structs.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<INamedTypeSymbol, StructData> Structs(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for type parameters.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<ITypeParameterSymbol, TypeParameterData>> TypeParameters()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for type parameters.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<ITypeParameterSymbol, TypeParameterData> TypeParameters(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for methods of unknown kind.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<IMethodSymbol, UnknownMethodData>> UnknownMethods()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for methods of unknown kind.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<IMethodSymbol, UnknownMethodData> UnknownMethods(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for types of unknown kind.
			/// </summary>
			public static SymbolContainerBuilder<SymbolContainer<ITypeSymbol, UnknownTypeData>> UnknownTypes()
			{
				return new();
			}

			/// <summary>
			/// Returns a new <see cref="SymbolContainer{TSymbol, TData}"/> for types of unknown kind.
			/// </summary>
			/// <param name="parentCompilation">
			/// Parent <see cref="ICompilationData"/> of the current container.
			/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para>
			/// </param>
			/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
			public static SymbolContainer<ITypeSymbol, UnknownTypeData> UnknownTypes(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
			{
				return new(parentCompilation, nameResolver);
			}
		}
	}
}
