using System;
using System.Collections.Generic;

namespace Durian.Analysis.Filtration
{
	/// <summary>
	/// A collection of <see cref="ISyntaxFilter"/>s.
	/// </summary>
	/// <typeparam name="TFilter">Type of <see cref="ISyntaxFilter"/> this collection can store.</typeparam>
	public interface IFilterGroup<TFilter> : IReadOnlyFilterGroup<TFilter>, ISealable where TFilter : ISyntaxFilter
	{
		/// <summary>
		/// Adds the specified <paramref name="filter"/> to this group.
		/// </summary>
		/// <param name="filter"><typeparamref name="TFilter"/> to add to this group.</param>
		/// <exception cref="ArgumentNullException"><paramref name="filter"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Filter group is sealed. -or- Parent filter group is sealed.</exception>
		void AddFilter(TFilter filter);

		/// <summary>
		/// Adds the collection of <paramref name="filters"/> to this <see cref="FilterGroup{TFilter}"/>.
		/// </summary>
		/// <param name="filters">A collection of <typeparamref name="TFilter"/>s to add to this group.</param>
		/// <exception cref="ArgumentNullException"><paramref name="filters"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Filter group is sealed. -or- Parent filter group is sealed.</exception>
		void AddFilters(IEnumerable<TFilter> filters);

		/// <summary>
		/// Removes all <typeparamref name="TFilter"/>s from this group.
		/// </summary>
		/// <exception cref="InvalidOperationException">Filter group is sealed. -or- Parent filter group is sealed.</exception>
		void Clear();

		/// <summary>
		/// Determines whether the specified <paramref name="filter"/> is contained within this group.
		/// </summary>
		/// <param name="filter">Filter to check for.</param>
		bool ContainsFilter(TFilter filter);

		/// <summary>
		/// Removes a <typeparamref name="TFilter"/> at the specified <paramref name="index"/>.
		/// </summary>
		/// <param name="index"></param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than <c>0</c>. -or- <paramref name="index"/> is equal to or greater than <see cref="IReadOnlyFilterGroup{TFilter}.Count"/>.</exception>
		/// <exception cref="InvalidOperationException">Filter group is sealed. -or- Parent filter group is sealed.</exception>
		void RemoveFilter(int index);

		/// <summary>
		/// Removes the first occurrence of the specified <paramref name="filter"/> from this group.
		/// </summary>
		/// <param name="filter"></param>
		/// <returns><see langword="true"/> if <paramref name="filter"/> was present in this group, <see langword="false"/> otherwise.</returns>
		/// <exception cref="InvalidOperationException">Filter group is sealed. -or- Parent filter group is sealed.</exception>
		bool RemoveFilter(TFilter filter);

		/// <summary>
		/// Removes a range of <typeparamref name="TFilter"/>s from this group.
		/// </summary>
		/// <param name="index">Index at which the removal should be started.</param>
		/// <param name="count">Number of <typeparamref name="TFilter"/>s to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than <c>0</c>. -or- <paramref name="count"/> is less than <c>0</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="index"/> and <paramref name="count"/> do not denote a valid range of elements in this group.</exception>
		/// <exception cref="InvalidOperationException">Filter group is sealed. -or- Parent filter group is sealed.</exception>
		void RemoveFilters(int index, int count);
	}
}