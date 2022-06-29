// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// <see cref="SymbolContainer{TSymbol, TData}"/> that writes name of the contained <see cref="ISymbol"/>s into a <see cref="StringBuilder"/>.
	/// </summary>
	/// <typeparam name="TSymbol">Type of target <see cref="ISymbol"/>.</typeparam>
	/// <typeparam name="TData">Type of target <see cref="IMemberData"/>.</typeparam>
	public class WritableSymbolContainer<TSymbol, TData> : SymbolContainer<TSymbol, TData>, IWritableSymbolContainer<TSymbol, TData>
		where TSymbol : class, ISymbol
		where TData : class, IMemberData
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WritableSymbolContainer{TSymbol, TData}"/> class.
		/// </summary>
		/// <param name="parentCompilation">Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para></param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		public WritableSymbolContainer(ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) : base(parentCompilation, nameResolver)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WritableSymbolContainer{TSymbol, TData}"/> class.
		/// </summary>
		/// <param name="capacity">Initial capacity of the container.</param>
		/// <param name="parentCompilation">Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para></param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than <c>0</c>.</exception>
		public WritableSymbolContainer(int capacity, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) : base(capacity, parentCompilation, nameResolver)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WritableSymbolContainer{TSymbol, TData}"/> class.
		/// </summary>
		/// <param name="collection">Collection of <typeparamref name="TSymbol"/>s to add to the container.</param>
		/// <param name="parentCompilation">Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para></param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
		public WritableSymbolContainer(IEnumerable<TSymbol> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) : base(collection, parentCompilation, nameResolver)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WritableSymbolContainer{TSymbol, TData}"/> class.
		/// </summary>
		/// <param name="collection">Collection of <see cref="ISymbolOrMember"/> to add to the container.</param>
		/// <param name="parentCompilation">Parent <see cref="ICompilationData"/> of the current container.
		/// <para>Required for converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>s.</para></param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
		public WritableSymbolContainer(IEnumerable<ISymbolOrMember<TSymbol, TData>> collection, ICompilationData? parentCompilation = default, ISymbolNameResolver? nameResolver = default) : base(collection, parentCompilation, nameResolver)
		{
		}

		/// <inheritdoc cref="IWritableSymbolContainer.ToString"/>
		public override string ToString()
		{
			StringBuilder builder = new();
			WriteTo(builder);
			return builder.ToString();
		}

		/// <inheritdoc/>
		public virtual void WriteTo(StringBuilder builder)
		{
			SymbolContainerFactory.DefaultBuild(this, builder);
		}
	}
}
