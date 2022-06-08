// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Collections.Immutable;
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
		/// <summary>
		/// Returns a new empty container.
		/// </summary>
		/// <typeparam name="T">Type of container to create.</typeparam>
		public static SymbolContainer<T> Empty<T>() where T : class, ISymbol
		{
			return new SymbolContainer<T>();
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ITypeParameterSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<ITypeParameterSymbol> ToContainer(this IEnumerable<ISymbolOrMember<ITypeParameterSymbol>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ITypeParameterSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<ITypeParameterSymbol> ToContainer(this IEnumerable<ITypeParameterSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ITypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<ITypeSymbol> ToContainer(this IEnumerable<ISymbolOrMember<ITypeSymbol>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ITypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<ITypeSymbol> ToContainer(this IEnumerable<ITypeSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IFieldSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<IFieldSymbol> ToContainer(this IEnumerable<ISymbolOrMember<IFieldSymbol>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IFieldSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<IFieldSymbol> ToContainer(this IEnumerable<IFieldSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IEventSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<IEventSymbol> ToContainer(this IEnumerable<ISymbolOrMember<IEventSymbol>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IEventSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<IEventSymbol> ToContainer(this IEnumerable<IEventSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IPropertySymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<IPropertySymbol> ToContainer(this IEnumerable<ISymbolOrMember<IPropertySymbol>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IPropertySymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<IPropertySymbol> ToContainer(this IEnumerable<IPropertySymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ILocalSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<ILocalSymbol> ToContainer(this IEnumerable<ISymbolOrMember<ILocalSymbol>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ILocalSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<ILocalSymbol> ToContainer(this IEnumerable<ILocalSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IMethodSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<IMethodSymbol> ToContainer(this IEnumerable<ISymbolOrMember<IMethodSymbol>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="IMethodSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<IMethodSymbol> ToContainer(this IEnumerable<IMethodSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<INamespaceSymbol> ToContainer(this IEnumerable<ISymbolOrMember<INamespaceSymbol>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<INamespaceSymbol> ToContainer(this IEnumerable<INamespaceSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<INamedTypeSymbol> ToContainer(this IEnumerable<ISymbolOrMember<INamedTypeSymbol>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>>
		public static SymbolContainer<INamedTypeSymbol> ToContainer(this IEnumerable<INamedTypeSymbol> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol}"/>.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		public static SymbolContainer<INamespaceOrTypeSymbol> ToContainer(this IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol>> collection, ICompilationData? compilation = default)
		{
			return new(collection, compilation);
		}

		/// <summary>
		/// Creates a new <see cref="SymbolContainer{TSymbol}"/>.
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
	}
}
