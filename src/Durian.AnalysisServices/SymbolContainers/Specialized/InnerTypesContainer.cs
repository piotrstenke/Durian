// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers.Specialized
{
	/// <summary>
	/// <see cref="ILeveledSymbolContainer{TSymbol, TData}"/> that handles inner types.
	/// </summary>
	public sealed class InnerTypesContainer : IncludedMembersSymbolContainerWithoutInner<INamedTypeSymbol, ITypeData>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="InnerTypesContainer"/> class.
		/// </summary>
		/// <param name="root"><see cref="ISymbol"/> that is a root of all the underlaying containers.</param>
		/// <param name="includeRoot">Determines whether the <paramref name="root"/> should be included in the underlaying containers.</param>
		/// <param name="parentCompilation"><see cref="ICompilationData"/> used to create <see cref="ITypeData"/>s.</param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		/// <exception cref="ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
		public InnerTypesContainer(
			ISymbolOrMember<INamedTypeSymbol, ITypeData> root,
			bool includeRoot = false,
			ICompilationData? parentCompilation = default,
			ISymbolNameResolver? nameResolver = default
		) : base(root, includeRoot, parentCompilation, nameResolver)
		{
		}

		/// <inheritdoc/>
		protected override IEnumerable<ISymbolOrMember<INamedTypeSymbol, ITypeData>> All(ISymbolOrMember<INamedTypeSymbol, ITypeData> member)
		{
			return GetTypes(member);
		}

		/// <inheritdoc/>
		protected override IEnumerable<ISymbolOrMember<INamedTypeSymbol, ITypeData>> Direct(ISymbolOrMember<INamedTypeSymbol, ITypeData> member)
		{
			return GetTypes(member);
		}

		/// <inheritdoc cref="LeveledSymbolContainer{TSymbol, TData}.Reverse"/>
		public new InnerTypesContainer Reverse()
		{
			return (base.Reverse() as InnerTypesContainer)!;
		}

		private IEnumerable<ISymbolOrMember<INamedTypeSymbol, ITypeData>> GetTypes(ISymbolOrMember<INamedTypeSymbol, ITypeData> member)
		{
			return member.Symbol.GetInnerTypes().Select(s => s.ToDataOrSymbol(ParentCompilation));
		}
	}
}
