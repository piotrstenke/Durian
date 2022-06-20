// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// <see cref="ISymbolContainer"/> that handles <see cref="INamespaceOrTypeSymbol"/>s.
	/// </summary>
	public class NamespaceOrTypeContainer : GenericSymbolContainer<INamespaceOrTypeSymbol, NamespaceOrTypeData>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NamespaceOrTypeContainer"/> class.
		/// </summary>
		/// <param name="parentCompilation">Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para></param>
		public NamespaceOrTypeContainer(ICompilationData? parentCompilation = default) : base(parentCompilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NamespaceOrTypeContainer"/> class.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
		/// <param name="parentCompilation">Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para></param>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
		public NamespaceOrTypeContainer(IEnumerable<INamespaceOrTypeSymbol> collection, ICompilationData? parentCompilation = default) : base(collection, parentCompilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NamespaceOrTypeContainer"/> class.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ISymbolOrMember"/> to add to the container.</param>
		/// <param name="parentCompilation">Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para></param>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
		public NamespaceOrTypeContainer(IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol>> collection, ICompilationData? parentCompilation = default) : base(collection, parentCompilation)
		{
		}

		/// <summary>
		/// Returns all namespaces contained within this container,
		/// </summary>
		public WritableSymbolContainer<INamespaceSymbol, NamespaceData> GetNamespaces()
		{
			return Content
				.Select(s => s.Symbol)
				.Where(s => s.IsNamespace)
				.OfType<INamespaceSymbol>()
				.ToWritableContainer();
		}

		/// <summary>
		/// Returns all types contained within this container,
		/// </summary>
		public GenericSymbolContainer<INamedTypeSymbol, ITypeData> GetTypes()
		{
			return Content
				.Select(s => s.Symbol)
				.Where(s => s.IsType)
				.OfType<INamedTypeSymbol>()
				.ToWritableContainer();
		}
	}
}
