// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers.Specialized
{
	/// <summary>
	/// <see cref="ILeveledSymbolContainer{TSymbol, TData}"/> that handles sub-namespaces and types.
	/// </summary>
	public sealed class NamespacesOrTypesContainer : IncludedMembersSymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NamespacesOrTypesContainer"/> class.
		/// </summary>
		/// <param name="root"><see cref="ISymbol"/> that is a root of all the underlaying containers.</param>
		/// <param name="includeRoot">Determines whether the <paramref name="root"/> should be included in the underlaying containers.</param>
		/// <param name="parentCompilation"><see cref="ICompilationData"/> used to create <see cref="INamespaceOrTypeData"/>s.</param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		/// <exception cref="ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
		public NamespacesOrTypesContainer(
			ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData> root,
			bool includeRoot = false,
			ICompilationData? parentCompilation = default,
			ISymbolNameResolver? nameResolver = default
		) : base(root, includeRoot, parentCompilation, nameResolver)
		{
		}

		public SubNamespacesContainer GetNamespaces(IncludedMembers level)
		{

		}

		public InnerTypesContainer GetTypes(IncludedMembers level)
		{

		}

		/// <inheritdoc/>
		protected override IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>> All(ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData> member)
		{
			return GetNamespacesOrTypes(member);
		}

		/// <inheritdoc/>
		protected override IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>> Direct(ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData> member)
		{
			return GetNamespacesOrTypes(member);
		}

		/// <inheritdoc/>
		protected override IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>> Inner(ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData> member)
		{
			return GetNamespacesOrTypes(member);
		}

		/// <inheritdoc cref="LeveledSymbolContainer{TSymbol, TData}.Reverse"/>
		public new NamespacesOrTypesContainer Reverse()
		{
			return (base.Reverse() as NamespacesOrTypesContainer)!;
		}

		private IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>> GetNamespacesOrTypes(ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData> member)
		{
			INamespaceOrTypeSymbol symbol = member.Symbol;

			if(symbol.IsNamespace)
			{
				return (symbol as INamespaceSymbol)!.GetMembers().Select(s => s.ToDataOrSymbol(ParentCompilation));
			}

			return symbol.GetTypeMembers().Select(s => (s as INamespaceOrTypeSymbol).ToDataOrSymbol(ParentCompilation));
		}
	}
}
