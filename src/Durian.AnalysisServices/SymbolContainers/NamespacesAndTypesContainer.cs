// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// <see cref="ISymbolContainer"/> that handles <see cref="INamespaceOrTypeSymbol"/>s.
	/// </summary>
	public sealed class NamespacesAndTypesContainer : SymbolContainer, IEnumerable<INamespaceOrTypeSymbol>, IEnumerable<NamespaceOrTypeData>
	{
		/// <summary>
		/// Determines whether to use type arguments instead of type parameters when building a <see cref="string"/>.
		/// </summary>
		public bool UseArguments { get; set; }

		internal NamespacesAndTypesContainer(IEnumerable<INamespaceOrTypeSymbol> collection, ICompilationData? compilation, bool useArguments, ReturnOrder order) : base(collection, compilation, order)
		{
			UseArguments = useArguments;
		}

		internal NamespacesAndTypesContainer(IEnumerable<NamespaceOrTypeData> collection, bool useArguments, ReturnOrder order) : base(collection, order)
		{
			UseArguments = useArguments;
		}

		/// <inheritdoc/>
		public override void Build(StringBuilder builder)
		{
			DefaultBuild(builder, GetNames());
		}

		/// <inheritdoc/>
		public override ImmutableArray<string> GetNames()
		{
			GenericSubstitution substituion = UseArguments ? GenericSubstitution.TypeArguments : default;

			return (this as ISymbolContainer).GetSymbols().Select(s => s.GetGenericName(substituion));
		}

		/// <summary>
		/// Returns all types contained within this container.
		/// </summary>
		public TypeContainer GetTypes()
		{
			return _collection switch
			{
				IEnumerable<ITypeData> data => Types(data, UseArguments, Order),
				IEnumerable<IMemberData> members => Types(members.OfType<ITypeData>(), UseArguments, Order),
				IEnumerable<INamedTypeSymbol> types => Types(types, TargetCompilation, UseArguments, Order),
				IEnumerable<INamespaceSymbol> => Types(Array.Empty<ITypeData>(), UseArguments, Order),
				_ => Types((_collection as IEnumerable<INamespaceOrTypeSymbol>)!.OfType<INamedTypeSymbol>(), TargetCompilation, UseArguments, Order)
			};
		}

		/// <summary>
		/// Returns all namespaces contained within this container.
		/// </summary>
		public NamespaceContainer GetNamespaces()
		{
			return _collection switch
			{
				IEnumerable<INamespaceSymbol> namespaces => Namespaces(namespaces, TargetCompilation, Order),
				IEnumerable<ITypeSymbol> or IEnumerable<ITypeData> => Namespaces(Array.Empty<NamespaceData>(), Order),
				IEnumerable<IMemberData> members => Namespaces(members.OfType<NamespaceData>(), Order),
				_ => Namespaces((_collection as IEnumerable<INamespaceOrTypeSymbol>)!.OfType<INamespaceSymbol>(), TargetCompilation, Order)
			};
		}

		/// <summary>
		/// Returns the <see cref="INamedTypeSymbol"/>s contained within this instance.
		/// </summary>
		public new ImmutableArray<INamespaceOrTypeSymbol> GetSymbols()
		{
			return base.GetSymbols().CastArray<INamespaceOrTypeSymbol>();
		}

		/// <inheritdoc/>
		protected override IMemberData GetData(ISymbol symbol, ICompilationData compilation)
		{
			return new NamespaceOrTypeData((symbol as INamespaceOrTypeSymbol)!, compilation);
		}

		IEnumerator<INamespaceOrTypeSymbol> IEnumerable<INamespaceOrTypeSymbol>.GetEnumerator()
		{
			IEnumerable<INamespaceSymbol> symbols = GetSymbols();
			return symbols.GetEnumerator();
		}

		IEnumerator<NamespaceOrTypeData> IEnumerable<NamespaceOrTypeData>.GetEnumerator()
		{
			IEnumerable<NamespaceOrTypeData> namespaces = GetData();
			return namespaces.GetEnumerator();
		}
	}

	public partial class SymbolContainer
	{
		/// <summary>
		/// Creates a new <see cref="NamespacesAndTypesContainer"/>.
		/// </summary>
		/// <param name="types">Collection of <see cref="ITypeData"/>s to add to the container.</param>
		/// <param name="useArguments">Determines whether to use type arguments instead of type parameters when building a <see cref="string"/>.</param>
		/// <param name="order">Specifies ordering of the returned members.</param>
		public static TypeContainer NamespacesAndTypes(IEnumerable<IMemberData> types, bool useArguments = false, ReturnOrder order = ReturnOrder.Root)
		{
			return new(types, useArguments, order);
		}

		/// <summary>
		/// Creates a new <see cref="TypeContainer"/>.
		/// </summary>
		/// <param name="symbols">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
		/// <param name="useArguments">Determines whether to use type arguments instead of type parameters when building a <see cref="string"/>.</param>
		/// <param name="order">Specifies ordering of the returned members.</param>
		public static TypeContainer Types(IEnumerable<INamedTypeSymbol> symbols, bool useArguments = false, ReturnOrder order = ReturnOrder.Root)
		{
			return new(symbols, null, useArguments, order);
		}

		/// <summary>
		/// Creates a new <see cref="TypeContainer"/>.
		/// </summary>
		/// <param name="symbols">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <param name="useArguments">Determines whether to use type arguments instead of type parameters when building a <see cref="string"/>.</param>
		/// <param name="order">Specifies ordering of the returned members.</param>
		public static TypeContainer Types(IEnumerable<INamedTypeSymbol> symbols, ICompilationData? compilation, bool useArguments = false, ReturnOrder order = ReturnOrder.Root)
		{
			return new(symbols, compilation, useArguments, order);
		}
	}
}
