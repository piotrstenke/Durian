using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers;

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
		public static SymbolContainerBuilder<SymbolContainer<IMethodSymbol, IAccessorData>> Accessors()
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
		public static SymbolContainer<IMethodSymbol, IAccessorData> Accessors(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
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
		public static SymbolContainerBuilder<SymbolContainer<INamedTypeSymbol, IClassData>> Classes()
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
		public static SymbolContainer<INamedTypeSymbol, IClassData> Classes(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(parentCompilation, nameResolver);
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for constructors.
		/// </summary>
		public static SymbolContainerBuilder<SymbolContainer<IMethodSymbol, IConstructorData>> Constructors()
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
		public static SymbolContainer<IMethodSymbol, IConstructorData> Constructors(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(parentCompilation, nameResolver);
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for conversion operators.
		/// </summary>
		public static SymbolContainerBuilder<SymbolContainer<IMethodSymbol, IConversionOperatorData>> ConversionOperators()
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
		public static SymbolContainer<IMethodSymbol, IConversionOperatorData> ConversionOperators(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(parentCompilation, nameResolver);
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for delegates.
		/// </summary>
		public static SymbolContainerBuilder<SymbolContainer<INamedTypeSymbol, IDelegateData>> Delegates()
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
		public static SymbolContainer<INamedTypeSymbol, IDelegateData> Delegates(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(parentCompilation, nameResolver);
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for destructors.
		/// </summary>
		public static SymbolContainerBuilder<SymbolContainer<IMethodSymbol, IDestructorData>> Destructors()
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
		public static SymbolContainer<IMethodSymbol, IDestructorData> Destructors(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(parentCompilation, nameResolver);
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for enums.
		/// </summary>
		public static SymbolContainerBuilder<SymbolContainer<INamedTypeSymbol, IEnumData>> Enums()
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
		public static SymbolContainer<INamedTypeSymbol, IEnumData> Enums(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(parentCompilation, nameResolver);
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for events.
		/// </summary>
		public static SymbolContainerBuilder<SymbolContainer<IEventSymbol, IEventData>> Events()
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
		public static SymbolContainer<IEventSymbol, IEventData> Events(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(parentCompilation, nameResolver);
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for fields.
		/// </summary>
		public static SymbolContainerBuilder<SymbolContainer<IFieldSymbol, IFieldData>> Fields()
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
		public static SymbolContainer<IFieldSymbol, IFieldData> Fields(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(parentCompilation, nameResolver);
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for indexers.
		/// </summary>
		public static SymbolContainerBuilder<SymbolContainer<IPropertySymbol, IIndexerData>> Indexers()
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
		public static SymbolContainer<IPropertySymbol, IIndexerData> Indexers(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(parentCompilation, nameResolver);
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for interfaces.
		/// </summary>
		public static SymbolContainerBuilder<SymbolContainer<INamedTypeSymbol, IInterfaceData>> Interfaces()
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
		public static SymbolContainer<INamedTypeSymbol, IInterfaceData> Interfaces(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(parentCompilation, nameResolver);
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for lambdas.
		/// </summary>
		public static SymbolContainerBuilder<SymbolContainer<IMethodSymbol, ILambdaData>> Lambdas()
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
		public static SymbolContainer<IMethodSymbol, ILambdaData> Lambdas(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(parentCompilation, nameResolver);
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for local functions.
		/// </summary>
		public static SymbolContainerBuilder<SymbolContainer<IMethodSymbol, ILocalFunctionData>> LocalFunctions()
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
		public static SymbolContainer<IMethodSymbol, ILocalFunctionData> LocalFunctions(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(parentCompilation, nameResolver);
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for locals.
		/// </summary>
		public static SymbolContainerBuilder<SymbolContainer<ILocalSymbol, ILocalData>> Locals()
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
		public static SymbolContainer<ILocalSymbol, ILocalData> Locals(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(parentCompilation, nameResolver);
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for methods.
		/// </summary>
		public static SymbolContainerBuilder<SymbolContainer<IMethodSymbol, IMethodData>> Methods()
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
		public static SymbolContainer<IMethodSymbol, IMethodData> Methods(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(parentCompilation, nameResolver);
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for namespaces.
		/// </summary>
		public static SymbolContainerBuilder<SymbolContainer<INamespaceSymbol, INamespaceData>> Namespaces()
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
		public static SymbolContainer<INamespaceSymbol, INamespaceData> Namespaces(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(parentCompilation, nameResolver);
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for namespaces or types.
		/// </summary>
		public static SymbolContainerBuilder<SymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData>> NamespacesOrTypes()
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
		public static SymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData> NamespacesOrTypes(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(parentCompilation, nameResolver);
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for operators.
		/// </summary>
		public static SymbolContainerBuilder<SymbolContainer<IMethodSymbol, IOperatorData>> Operators()
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
		public static SymbolContainer<IMethodSymbol, IOperatorData> Operators(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(parentCompilation, nameResolver);
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for parameters.
		/// </summary>
		public static SymbolContainerBuilder<SymbolContainer<IParameterSymbol, IParameterData>> Parameters()
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
		public static SymbolContainer<IParameterSymbol, IParameterData> Parameters(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(parentCompilation, nameResolver);
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for properties.
		/// </summary>
		public static SymbolContainerBuilder<SymbolContainer<IPropertySymbol, IPropertyData>> Properties()
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
		public static SymbolContainer<IPropertySymbol, IPropertyData> Properties(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(parentCompilation, nameResolver);
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for records.
		/// </summary>
		public static SymbolContainerBuilder<SymbolContainer<INamedTypeSymbol, IRecordData>> Records()
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
		public static SymbolContainer<INamedTypeSymbol, IRecordData> Records(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(parentCompilation, nameResolver);
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for structs.
		/// </summary>
		public static SymbolContainerBuilder<SymbolContainer<INamedTypeSymbol, IStructData>> Structs()
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
		public static SymbolContainer<INamedTypeSymbol, IStructData> Structs(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(parentCompilation, nameResolver);
		}

		/// <summary>
		/// Returns a new <see cref="SymbolContainerBuilder{T}"/> for type parameters.
		/// </summary>
		public static SymbolContainerBuilder<SymbolContainer<ITypeParameterSymbol, ITypeParameterData>> TypeParameters()
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
		public static SymbolContainer<ITypeParameterSymbol, ITypeParameterData> TypeParameters(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default)
		{
			return new(parentCompilation, nameResolver);
		}
	}
}
