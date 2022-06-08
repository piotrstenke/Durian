// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections;
using System.Collections.Generic;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// Simple implementation of the <see cref="IReturnOrderEnumerable{T}"/> interface.
	/// </summary>
	/// <typeparam name="T">Type of objects to enumerate.</typeparam>
	internal sealed class ReturnOrderEnumerable<T> : IReturnOrderEnumerable<T>
	{
		/// <inheritdoc/>
		public ReturnOrder Order { get; }

		/// <summary>
		/// Underlaying <see cref="IEnumerable{T}"/>.
		/// </summary>
		public IEnumerable<T> Collection { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ReturnOrderEnumerable{T}"/> class.
		/// </summary>
		/// <param name="collection">Underlaying <see cref="IEnumerable{T}"/>.</param>
		/// <param name="order">Order in which elements of this <see cref="IEnumerable{T}"/> are returned.</param>
		public ReturnOrderEnumerable(IEnumerable<T> collection, ReturnOrder order)
		{
			Collection = collection;
			Order = order;
		}

		/// <inheritdoc/>
		public IEnumerator<T> GetEnumerator()
		{
			return Collection.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
