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
	/// <see cref="ILeveledSymbolContainer{TSymbol, TData}"/> that handles sub-namespaces.
	/// </summary>
	public sealed class SubNamespacesContainer : IncludedMembersSymbolContainerWithoutInner<INamespaceSymbol, INamespaceData>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SubNamespacesContainer"/> class.
		/// </summary>
		/// <param name="root"><see cref="ISymbol"/> that is a root of all the underlaying containers.</param>
		/// <param name="includeRoot">Determines whether the <paramref name="root"/> should be included in the underlaying containers.</param>
		/// <param name="parentCompilation"><see cref="ICompilationData"/> used to create <see cref="INamespaceData"/>s.</param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		/// <exception cref="ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
		public SubNamespacesContainer(
			ISymbolOrMember<INamespaceSymbol, INamespaceData> root,
			bool includeRoot = false,
			ICompilationData? parentCompilation = default,
			ISymbolNameResolver? nameResolver = default
		) : base(root, includeRoot, parentCompilation, nameResolver)
		{
		}

		/// <inheritdoc/>
		protected override IEnumerable<ISymbolOrMember<INamespaceSymbol, INamespaceData>> All(ISymbolOrMember<INamespaceSymbol, INamespaceData> member)
		{
			return GetNamespaces(member);
		}

		/// <inheritdoc/>
		protected override IEnumerable<ISymbolOrMember<INamespaceSymbol, INamespaceData>> Direct(ISymbolOrMember<INamespaceSymbol, INamespaceData> member)
		{
			return GetNamespaces(member);
		}

		/// <inheritdoc cref="LeveledSymbolContainer{TSymbol, TData}.Reverse"/>
		public new SubNamespacesContainer Reverse()
		{
			return (base.Reverse() as SubNamespacesContainer)!;
		}

		private IEnumerable<ISymbolOrMember<INamespaceSymbol, INamespaceData>> GetNamespaces(ISymbolOrMember<INamespaceSymbol, INamespaceData> member)
		{
			return member.Symbol.GetNamespaceMembers().Select(s => s.ToDataOrSymbol(ParentCompilation));
		}
	}
}
