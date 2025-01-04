using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Durian.Analysis.Extensions;

namespace Durian.Analysis.SymbolContainers;

/// <summary>
/// Simple implementation of the <see cref="IReturnOrderEnumerable{T}"/> interface.
/// </summary>
/// <typeparam name="T">Type of objects to enumerate.</typeparam>
public sealed class ReturnOrderEnumerable<T> : IReturnOrderEnumerable<T>
{
	/// <summary>
	/// Underlaying <see cref="IEnumerable{T}"/>.
	/// </summary>
	public IEnumerable<T> Collection { get; private set; }

	/// <inheritdoc/>
	public ReturnOrder Order { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ReturnOrderEnumerable{T}"/> class.
	/// </summary>
	/// <param name="collection">Underlaying <see cref="IEnumerable{T}"/>.</param>
	/// <param name="order">Order in which elements of this <see cref="IEnumerable{T}"/> are returned.</param>
	/// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
	public ReturnOrderEnumerable(IEnumerable<T> collection, ReturnOrder order)
	{
		if (collection is null)
		{
			throw new ArgumentNullException(nameof(collection));
		}

		Collection = collection;
		Order = order;
	}

	/// <inheritdoc/>
	public IEnumerator<T> GetEnumerator()
	{
		return Collection.GetEnumerator();
	}

	/// <inheritdoc cref="IReturnOrderEnumerable{T}.Reverse"/>
	public ReturnOrderEnumerable<T> Reverse()
	{
		Order = Order.Reverse();
		Collection = Collection.Reverse();

		return this;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	IReturnOrderEnumerable<T> IReturnOrderEnumerable<T>.Reverse()
	{
		return Reverse();
	}

	IReturnOrderEnumerable IReturnOrderEnumerable.Reverse()
	{
		return Reverse();
	}
}
