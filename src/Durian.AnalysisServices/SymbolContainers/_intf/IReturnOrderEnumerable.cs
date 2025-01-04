using System.Collections;
using System.Collections.Generic;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// <see cref="IEnumerable"/> with a <see cref="ReturnOrder"/> specified.
	/// </summary>
	public interface IReturnOrderEnumerable : IEnumerable
	{
		/// <summary>
		/// Order in which elements of this <see cref="IEnumerable{T}"/> are returned.
		/// </summary>
		ReturnOrder Order { get; }

		/// <summary>
		/// Reverses the collection.
		/// </summary>
		IReturnOrderEnumerable Reverse();
	}

	/// <summary>
	/// <see cref="IEnumerable{T}"/> with a <see cref="ReturnOrder"/> specified.
	/// </summary>
	/// <typeparam name="T">Type of objects to enumerate.</typeparam>
	public interface IReturnOrderEnumerable<out T> : IReturnOrderEnumerable, IEnumerable<T>
	{
		/// <inheritdoc cref="IReturnOrderEnumerable.Reverse"/>
		new IReturnOrderEnumerable<T> Reverse();
	}
}
