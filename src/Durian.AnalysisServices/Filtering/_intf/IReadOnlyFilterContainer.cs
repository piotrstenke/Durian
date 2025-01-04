using System;
using System.Collections.Generic;

namespace Durian.Analysis.Filtering;

/// <summary>
/// A readonly collection of <see cref="IFilterGroup{TFilter}"/>s.
/// </summary>
/// <typeparam name="TFilter">Type of <see cref="ISyntaxFilter"/> this collection can store.</typeparam>
public interface IReadOnlyFilterContainer<out TFilter> : IEnumerable<IReadOnlyFilterGroup<TFilter>> where TFilter : ISyntaxFilter
{
	/// <summary>
	/// Number of filters in this container.
	/// </summary>
	int Count { get; }

	/// <summary>
	/// Number of filter groups in this container.
	/// </summary>
	int NumGroups { get; }

	/// <summary>
	/// Determines whether any child filter group has the specified <paramref name="name"/>.
	/// </summary>
	/// <param name="name">Name to check for.</param>
	/// <exception cref="ArgumentException"><paramref name="name"/> cannot be null or white space only.</exception>
	bool ContainsGroup(string name);

	/// <summary>
	/// Returns a filter at the given <paramref name="index"/> in the specified <paramref name="group"/>.
	/// </summary>
	/// <param name="group">Group to get the filter from.</param>
	/// <param name="index">Index of the filter in the specified <paramref name="group"/>.</param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="group"/> is less than <c>0</c>. -or-
	/// <paramref name="group"/> is equal to or greater than <see cref="NumGroups"/>. -or-
	/// <paramref name="index"/> is less than <c>0</c>. -or-
	/// <paramref name="index"/> is equal to or greater than <see cref="IReadOnlyFilterGroup{TFilter}.Count"/>.
	/// </exception>
	TFilter GetFilter(int group, int index);

	/// <summary>
	/// Returns a filter at the given <paramref name="index"/> in a filter group with the specified <paramref name="name"/>.
	/// </summary>
	/// <param name="name">Name of filter group the <typeparamref name="TFilter"/> is located in.</param>
	/// <param name="index">Index of the <typeparamref name="TFilter"/> in the specified filter group.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than <c>0</c>. -or- <paramref name="index"/> is equal to or greater than <see cref="FilterGroup{TFilter}.Count"/>.</exception>
	/// <exception cref="ArgumentException">
	/// Name of a filter group cannot be null or whitespace only. -or-
	/// filter group with the specified <paramref name="name"/> not found.
	/// </exception>
	TFilter GetFilter(string name, int index);

	/// <summary>
	/// Returns a filter group at the specified <paramref name="index"/>.
	/// </summary>
	/// <param name="index">Index of the filter group to get.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than <c>0</c>. -or- <paramref name="index"/> is equal to or greater than <see cref="NumGroups"/>.</exception>
	IReadOnlyFilterGroup<TFilter> GetGroup(int index);

	/// <summary>
	/// Returns the filter group with the specified <paramref name="name"/>.
	/// </summary>
	/// <param name="name">Name of the group to get.</param>
	/// <exception cref="ArgumentException">
	/// <paramref name="name"/> cannot be null or whitespace only. -or-
	/// filter group with the specified <paramref name="name"/> not found.
	/// </exception>
	IReadOnlyFilterGroup<TFilter> GetGroup(string name);

	/// <summary>
	/// Returns index of a filter group with the specified <paramref name="name"/>.
	/// </summary>
	/// <param name="name">Name of filter group to return the index of.</param>
	/// <exception cref="ArgumentException"><paramref name="name"/> cannot be <see langword="null"/> or white space only.</exception>
	int IndexOf(string name);

	/// <summary>
	/// Returns name of the filter group at the specified <paramref name="index"/>.
	/// </summary>
	/// <param name="index"></param>
	/// <returns>Name of the filter group at the specified <paramref name="index"/>. -or- <see langword="null"/> if group at the specified <paramref name="index"/> doesn't have a name.</returns>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than <c>0</c>. -or- <paramref name="index"/> is equal to or greater than <see cref="NumGroups"/>.</exception>
	string? NameOf(int index);

	/// <summary>
	/// Returns all groups contained within this collection.
	/// </summary>
	IReadOnlyFilterGroup<TFilter>[] ToArray();
}