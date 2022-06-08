// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// <see cref="WritableSymbolContainer{TSymbol}"/> that handles generic names when calling <see cref="SymbolContainer{TSymbol}.GetNames"/>.
	/// </summary>
	/// <typeparam name="TSymbol">Type of target <see cref="ISymbol"/>.</typeparam>
	public class GenericSymbolContainer<TSymbol> : WritableSymbolContainer<TSymbol> where TSymbol : class, ISymbol
	{
		/// <summary>
		/// Determines whether to use type arguments instead of type parameters when building a <see cref="string"/>.
		/// </summary>
		public bool UseArguments { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="GenericSymbolContainer{TSymbol}"/> class.
		/// </summary>
		/// <param name="parentCompilation">Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para></param>
		public GenericSymbolContainer(ICompilationData? parentCompilation = default) : base(parentCompilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GenericSymbolContainer{TSymbol}"/> class.
		/// </summary>
		/// <param name="collection">Collection of <typeparamref name="TSymbol"/>s to add to the container.</param>
		/// <param name="parentCompilation">Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para></param>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
		public GenericSymbolContainer(IEnumerable<TSymbol> collection, ICompilationData? parentCompilation = default) : base(collection, parentCompilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GenericSymbolContainer{TSymbol}"/> class.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ISymbolOrMember"/> to add to the container.</param>
		/// <param name="parentCompilation">Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para></param>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
		public GenericSymbolContainer(IEnumerable<ISymbolOrMember<TSymbol>> collection, ICompilationData? parentCompilation = default) : base(collection, parentCompilation)
		{
		}

		/// <inheritdoc/>
		public override ImmutableArray<string> GetNames()
		{
			return ImmutableArray.CreateRange(Content.Select(s => s.HasMember ? s.Member.Name : s.Symbol.GetGenericName(UseArguments)));
		}
	}
}
