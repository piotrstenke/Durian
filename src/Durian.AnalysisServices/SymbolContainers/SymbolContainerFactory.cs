// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// Provides factory methods for implementations of the <see cref="ISymbolContainer"/> interface.
	/// </summary>
	public static partial class SymbolContainerFactory
	{
		/// <summary>
		/// Returns a new empty container.
		/// </summary>
		/// <typeparam name="T">Type of container to create.</typeparam>
		public static T Empty<T>() where T : ISymbolContainer, new()
		{
			return new T();
		}
	}
}
