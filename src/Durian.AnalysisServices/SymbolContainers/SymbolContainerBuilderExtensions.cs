// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// Contains extension methods for the <see cref="SymbolContainerBuilder{T}"/> struct.
	/// </summary>
	public static class SymbolContainerBuilderExtensions
	{
		/// <summary>
		/// Includes a custom <see cref="ISymbolNameResolver"/>.
		/// </summary>
		/// <param name="builder"><see cref="SymbolContainerBuilder{T}"/> to include the <paramref name="nameResolver"/> for.</param>
		/// <param name="nameResolver">Custom <see cref="ISymbolNameResolver"/> to include.</param>
		public static ref SymbolContainerBuilder<T> WithNameResolver<T>(this ref SymbolContainerBuilder<T> builder, ISymbolNameResolver? nameResolver) where T : ISymbolContainer, IBuilderReceiver<SymbolContainerBuilder<T>>, new()
		{
			builder.SymbolNameResolver = nameResolver;
			return ref builder;
		}

		/// <summary>
		/// Includes a parent <see cref="ICompilationData"/>.
		/// </summary>
		/// <param name="builder"><see cref="SymbolContainerBuilder{T}"/> to include the <paramref name="parentCompilation"/> for.</param>
		/// <param name="parentCompilation"><see cref="ICompilationData"/> to include.</param>
		public static ref SymbolContainerBuilder<T> WithParentCompilation<T>(this ref SymbolContainerBuilder<T> builder, ICompilationData? parentCompilation) where T : ISymbolContainer, IBuilderReceiver<SymbolContainerBuilder<T>>, new()
		{
			builder.ParentCompilation = parentCompilation;
			return ref builder;
		}
	}
}
