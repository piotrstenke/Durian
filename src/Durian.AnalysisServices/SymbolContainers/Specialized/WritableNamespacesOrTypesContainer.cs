// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers.Specialized
{
	/// <summary>
	/// <see cref="WritableSymbolContainer{TSymbol, TData}"/> that provides default configuration for handling <see cref="INamespaceOrTypeSymbol"/>s.
	/// </summary>
	/// <typeparam name="TData">Type of target <see cref="INamespaceOrTypeData"/>.</typeparam>
	public sealed class WritableNamespacesOrTypesContainer<TData> : WritableSymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData> where TData : class, INamespaceOrTypeData
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WritableNamespacesOrTypesContainer{TData}"/> class.
		/// </summary>
		public WritableNamespacesOrTypesContainer()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WritableNamespacesOrTypesContainer{TData}"/> class.
		/// </summary>
		/// <param name="parentCompilation">Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para></param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		public WritableNamespacesOrTypesContainer(ICompilationData? parentCompilation, ISymbolNameResolver? nameResolver = default) : base(parentCompilation, nameResolver)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WritableNamespacesOrTypesContainer{TData}"/> class.
		/// </summary>
		/// <param name="collection">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
		/// <param name="parentCompilation">Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para></param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
		public WritableNamespacesOrTypesContainer(IEnumerable<INamespaceOrTypeSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) : base(collection, parentCompilation, nameResolver)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WritableNamespacesOrTypesContainer{TData}"/> class.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ISymbolOrMember"/> to add to the container.</param>
		/// <param name="parentCompilation">Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para></param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
		public WritableNamespacesOrTypesContainer(IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) : base(collection, parentCompilation, nameResolver)
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
				.ToWritableContainer(ParentCompilation, NameResolver);
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
				.ToWritableContainer(ParentCompilation, NameResolver);
		}

		/// <inheritdoc/>
		protected override ISymbolNameResolver GetDefaultNameResolver()
		{
			return SymbolNameResolver.GetResolver(SymbolName.Generic);
		}
	}
}
