// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Durian.Analysis.Filtration
{
	/// <summary>
	/// A read-only collection of <see cref="ISyntaxFilter"/>s.
	/// </summary>
	/// <typeparam name="TFilter">Type of <see cref="ISyntaxFilter"/> this collection can store.</typeparam>
	public interface IReadOnlyFilterGroup<out TFilter> : IEnumerable<TFilter> where TFilter : ISyntaxFilter
	{
		/// <summary>
		/// Number of filters in this group.
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Determines whether this group has a name.
		/// </summary>
		bool HasName { get; }

		/// <summary>
		/// Name of the group.
		/// </summary>
		[MemberNotNullWhen(true, nameof(Name))]
		string? Name { get; set; }

		/// <summary>
		/// Returns a filter at the specified <paramref name="index"/>.
		/// </summary>
		/// <param name="index">Index to get the filter at.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than <c>0</c>. -or- <paramref name="index"/> is equal to or greater than <see cref="Count"/>.</exception>
		TFilter GetFilter(int index);

		/// <summary>
		/// Returns an array representing a range of filters in this group.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than <c>0</c>. -or- <paramref name="index"/> is equal to or greater than <see cref="Count"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="index"/> and <paramref name="count"/> do not denote a valid range of elements in the group.</exception>
		TFilter[] GetFilters(int index, int count);

		/// <summary>
		/// Returns all filters contained within this group.
		/// </summary>
		TFilter[] ToArray();
	}
}
