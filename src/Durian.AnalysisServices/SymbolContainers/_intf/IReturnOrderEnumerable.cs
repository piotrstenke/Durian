// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// <see cref="IEnumerable{T}"/> with a <see cref="ReturnOrder"/> specified.
	/// </summary>
	/// <typeparam name="T">Type of objects to enumerate.</typeparam>
	public interface IReturnOrderEnumerable<out T> : IEnumerable<T>
	{
		/// <summary>
		/// Order in which elements of this <see cref="IEnumerable{T}"/> are returned.
		/// </summary>
		ReturnOrder Order { get; }
	}
}
