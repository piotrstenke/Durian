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
	public sealed class SubNamespaceContainer : IncludedMemberContainerWithoutInner<INamespaceSymbol, INamespaceData>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SubNamespaceContainer"/> class.
		/// </summary>
		/// <param name="root"><see cref="ISymbol"/> that is a root of all the underlaying containers.</param>
		/// <param name="parentCompilation"><see cref="ICompilationData"/> used to create <see cref="INamespaceData"/>s.</param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		/// <param name="includeRoot">Determines whether the <paramref name="root"/> should be included in the underlaying containers.</param>
		/// <exception cref="ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
		public SubNamespaceContainer(
			ISymbolOrMember<INamespaceSymbol, INamespaceData> root,
			ICompilationData? parentCompilation = default,
			ISymbolNameResolver? nameResolver = default,
			bool includeRoot = false
		) : base(root, parentCompilation, nameResolver, includeRoot)
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

		/// <inheritdoc/>
		protected override IEnumerable<ISymbolOrMember<INamespaceSymbol, INamespaceData>> ResolveRoot(ISymbolOrMember root)
		{
			return GetNamespaces((root as SymbolOrMemberWrapper<INamespaceSymbol, INamespaceData>)!);
		}

		/// <inheritdoc cref="LeveledSymbolContainer{TSymbol, TData}.Reverse"/>
		public new SubNamespaceContainer Reverse()
		{
			return (base.Reverse() as SubNamespaceContainer)!;
		}

		private IEnumerable<ISymbolOrMember<INamespaceSymbol, INamespaceData>> GetNamespaces(ISymbolOrMember<INamespaceSymbol, INamespaceData> member)
		{
			return member.Symbol.GetNamespaceMembers().Select(s => s.ToDataOrSymbol(ParentCompilation));
		}
	}
}
