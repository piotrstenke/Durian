// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// <see cref="ISymbolContainer"/> that handles <see cref="INamespaceOrTypeSymbol"/>s.
	/// </summary>
	public class NamespaceOrTypeContainer : WritableSymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NamespaceOrTypeContainer"/> class.
		/// </summary>
		/// <param name="parentCompilation">Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para></param>
		public NamespaceOrTypeContainer(ICompilationData? parentCompilation = default) : base(parentCompilation, GetNameResolver())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NamespaceOrTypeContainer"/> class.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
		/// <param name="parentCompilation">Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para></param>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
		public NamespaceOrTypeContainer(IEnumerable<INamespaceOrTypeSymbol> collection, ICompilationData? parentCompilation = default) : base(collection, parentCompilation, GetNameResolver())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NamespaceOrTypeContainer"/> class.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ISymbolOrMember"/> to add to the container.</param>
		/// <param name="parentCompilation">Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para></param>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
		public NamespaceOrTypeContainer(IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>> collection, ICompilationData? parentCompilation = default
		) : base(collection, parentCompilation, GetNameResolver())
		{
		}

		/// <summary>
		/// Returns all namespaces contained within this container,
		/// </summary>
		public WritableSymbolContainer<INamespaceSymbol, INamespaceData> GetNamespaces()
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
		public WritableSymbolContainer<INamedTypeSymbol, ITypeData> GetTypes()
		{
			return Content
				.Select(s => s.Symbol)
				.Where(s => s.IsType)
				.OfType<INamedTypeSymbol>()
				.ToWritableContainer();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ISymbolNameResolver GetNameResolver()
		{
			return Analysis.SymbolNameResolver.GetResolver(SymbolName.Generic);
		}
	}
}
