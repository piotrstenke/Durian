// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// <see cref="ISymbolContainer"/> that handles <see cref="INamespaceSymbol"/>s.
	/// </summary>
	public sealed class NamespaceContainer : SymbolContainer, IEnumerable<INamespaceSymbol>, IEnumerable<NamespaceData>
	{
		internal NamespaceContainer(IEnumerable<INamespaceSymbol> collection, ICompilationData? compilation, ReturnOrder order) : base(collection, compilation, order)
		{
		}

		internal NamespaceContainer(IEnumerable<NamespaceData> collection, ReturnOrder order) : base(collection, order)
		{
		}

		/// <inheritdoc/>
		public override void Build(StringBuilder builder)
		{
			DefaultBuild(builder, GetNames());
		}

		/// <summary>
		/// Returns the <see cref="NamespaceData"/>s contained within this instance.
		/// </summary>
		/// <exception cref="InvalidOperationException"><see cref="ISymbol"/>s cannot be converted into <see cref="IMemberData"/>, because <see cref="SymbolContainer.TargetCompilation"/> wasn't specified for the current container.</exception>
		public new ImmutableArray<NamespaceData> GetData()
		{
			return base.GetData().CastArray<NamespaceData>();
		}

		/// <summary>
		/// Returns the <see cref="INamespaceSymbol"/>s contained within this instance.
		/// </summary>
		public new ImmutableArray<INamespaceSymbol> GetSymbols()
		{
			return base.GetSymbols().CastArray<INamespaceSymbol>();
		}

		/// <inheritdoc/>
		protected override IMemberData GetData(ISymbol symbol, ICompilationData compilation)
		{
			return new NamespaceData((symbol as INamespaceSymbol)!, compilation);
		}

		IEnumerator<INamespaceSymbol> IEnumerable<INamespaceSymbol>.GetEnumerator()
		{
			IEnumerable<INamespaceSymbol> symbols = GetSymbols();
			return symbols.GetEnumerator();
		}

		IEnumerator<NamespaceData> IEnumerable<NamespaceData>.GetEnumerator()
		{
			IEnumerable<NamespaceData> namespaces = GetData();
			return namespaces.GetEnumerator();
		}
	}

	public partial class SymbolContainerFactory
	{
		/// <summary>
		/// Creates a new <see cref="NamespaceContainer"/>.
		/// </summary>
		/// <param name="namespaces">Collection of <see cref="NamespaceData"/>s to add to the container.</param>
		/// <param name="order">Specifies ordering of the returned members.</param>
		public static NamespaceContainer ToContainer(this IEnumerable<NamespaceData> namespaces, ReturnOrder order = ReturnOrder.Root)
		{
			return new(namespaces, order);
		}

		/// <summary>
		/// Creates a new <see cref="NamespaceContainer"/>.
		/// </summary>
		/// <param name="symbols">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
		/// <param name="order">Specifies ordering of the returned members.</param>
		public static NamespaceContainer ToContainer(this IEnumerable<INamespaceSymbol> symbols, ReturnOrder order = ReturnOrder.Root)
		{
			return new(symbols, null, order);
		}

		/// <summary>
		/// Creates a new <see cref="NamespaceContainer"/>.
		/// </summary>
		/// <param name="symbols">Collection of <see cref="INamespaceSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <param name="order">Specifies ordering of the returned members.</param>
		public static NamespaceContainer ToContainer(this IEnumerable<INamespaceSymbol> symbols, ICompilationData? compilation, ReturnOrder order = ReturnOrder.Root)
		{
			return new(symbols, compilation, order);
		}
	}
}
