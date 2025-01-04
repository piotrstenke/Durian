using System;
using System.Collections.Generic;

namespace Durian.Analysis.Filtration;

/// <summary>
/// A collection of <see cref="IFilterGroup{TFilter}"/>s.
/// </summary>
/// <typeparam name="TFilter">Type of <see cref="ISyntaxFilter"/> this collection can store.</typeparam>
public interface IFilterContainer<TFilter> : IReadOnlyFilterContainer<TFilter>, IEnumerable<IFilterGroup<TFilter>>, ISealable where TFilter : ISyntaxFilter
{
	/// <summary>
	/// Adds a filter to a filter group at the specified <paramref name="index"/>
	/// </summary>
	/// <param name="index">Index of group to add the <paramref name="filter"/> to.</param>
	/// <param name="filter">Filter to add.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than <c>0</c>. -or- <paramref name="index"/> is equal to or greater than <see cref="IReadOnlyFilterContainer{TFilter}.NumGroups"/>.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="filter"/> is <see langword="null"/>.</exception>
	/// <exception cref="InvalidOperationException">Filter container is sealed. -or- Filter group is sealed.</exception>
	void AddFilter(int index, TFilter filter);

	/// <summary>
	/// Adds a filter to a filter group with the specified <paramref name="name"/>.
	/// </summary>
	/// <param name="name">Name of filter group to add the <paramref name="filter"/> to.</param>
	/// <param name="filter">Filter to add.</param>
	/// <exception cref="ArgumentNullException"><paramref name="filter"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">
	/// <paramref name="name"/> cannot be null or whitespace only. -or-
	/// filter group with the specified <paramref name="name"/> not found.
	/// </exception>
	/// <exception cref="InvalidOperationException">Filter container is sealed. -or- Filter group is sealed.</exception>
	void AddFilter(string name, TFilter filter);

	/// <summary>
	/// Adds a range of filters to a filter group at the specified <paramref name="index"/>
	/// </summary>
	/// <param name="index">Index of group to add the <paramref name="filters"/> to.</param>
	/// <param name="filters">A collection of <see cref="ISyntaxFilter"/> to add.</param>
	/// <exception cref="ArgumentNullException"><paramref name="filters"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="index"/> is less than <c>0</c>. -or-
	/// <paramref name="index"/> is equal to or greater than <see cref="IReadOnlyFilterContainer{TFilter}.NumGroups"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">Filter container is sealed. -or- Filter group is sealed.</exception>
	void AddFilters(int index, IEnumerable<TFilter> filters);

	/// <summary>
	/// Adds a range of filters to a filter group with the specified <paramref name="name"/>.
	/// </summary>
	/// <param name="name">Name of filter group to add the <paramref name="filters"/> to.</param>
	/// <param name="filters">A collection of filters to add.</param>
	/// <exception cref="ArgumentNullException"><paramref name="filters"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">
	/// <paramref name="name"/> cannot be null or whitespace only. -or-
	/// filter group with the specified <paramref name="name"/> not found.
	/// </exception>
	/// <exception cref="InvalidOperationException">Filter container is sealed. -or- Filter group is sealed.</exception>
	void AddFilters(string name, IEnumerable<TFilter> filters);

	/// <summary>
	/// Removes all filter groups from the Filter container.
	/// </summary>
	/// <exception cref="InvalidOperationException">Filter container is sealed.</exception>
	void Clear();

	/// <summary>
	/// Determines whether this filter container contains the specified <paramref name="group"/>.
	/// </summary>
	/// <param name="group">Filter group to check.</param>
	/// <exception cref="ArgumentNullException"><paramref name="group"/> is <see langword="null"/>.</exception>
	bool ContainsGroup(IFilterGroup<TFilter> group);

	/// <summary>
	/// Returns a filter group at the specified <paramref name="index"/>.
	/// </summary>
	/// <param name="index">Index of the filter group to get.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than <c>0</c>. -or- <paramref name="index"/> is equal to or greater than <see cref="IReadOnlyFilterContainer{T}.NumGroups"/>.</exception>
	new IFilterGroup<TFilter> GetGroup(int index);

	/// <summary>
	/// Returns the filter group with the specified <paramref name="name"/>.
	/// </summary>
	/// <param name="name">Name of the group to get.</param>
	/// <exception cref="ArgumentException">
	/// <paramref name="name"/> cannot be null or whitespace only. -or-
	/// filter group with the specified <paramref name="name"/> not found.
	/// </exception>
	new IFilterGroup<TFilter> GetGroup(string name);

	/// <summary>
	/// Registers a new filter group.
	/// </summary>
	/// <returns>An <see cref="int"/> that represents the index at which the filter group was registered.</returns>
	int RegisterGroup();

	/// <summary>
	/// Registers the specified <paramref name="group"/>.
	/// </summary>
	/// <param name="group">Filter group to register.</param>
	/// <returns>An <see cref="int"/> that represents the index at which the filter group was registered.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="group"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">Filter group with the specified name already defined.</exception>
	/// <exception cref="InvalidOperationException">Filter container is sealed.</exception>
	int RegisterGroup(IFilterGroup<TFilter> group);

	/// <summary>
	/// Registers a new filter group that contains the specified <paramref name="filters"/>.
	/// </summary>
	/// <param name="filters">A collection of filters to add to the newly-register filter group.</param>
	/// <returns>An <see cref="int"/> that represents the index at which the filter group was registered.</returns>
	/// <exception cref="InvalidOperationException">Filter container is sealed.</exception>
	int RegisterGroup(IEnumerable<TFilter>? filters);

	/// <summary>
	/// Register a new filter group with the specified <paramref name="name"/>.
	/// </summary>
	/// <param name="name">Name to be applied to the filter group.</param>
	/// <returns>An <see cref="int"/> that represents the index at which the filter group was registered.</returns>
	/// <exception cref="ArgumentException"><paramref name="name"/> cannot be empty or whitespace only. -or-
	/// filter group with the specified <paramref name="name"/> already defined.
	/// </exception>
	/// <exception cref="InvalidOperationException">Filter container is sealed.</exception>
	int RegisterGroup(string? name);

	/// <summary>
	/// Registers a new filter group that contains the specified <paramref name="filters"/>.
	/// </summary>
	/// <param name="name">Name to be applied to the newly-registered filter group.</param>
	/// <param name="filters">A collection of filters to add to the newly-register filter group.</param>
	/// <returns>An <see cref="int"/> that represents the index at which the filter group was registered.</returns>
	/// <exception cref="ArgumentException">
	/// <paramref name="name"/> cannot be empty or whitespace only. -or-
	/// filter group with the specified <paramref name="name"/> already defined. -or-
	/// </exception>
	/// <exception cref="InvalidOperationException">Filter container is sealed.</exception>
	int RegisterGroup(string? name, IEnumerable<TFilter>? filters);

	/// <summary>
	/// Registers a new filter group that contains the specified <paramref name="name"/> and <paramref name="filter"/>.
	/// </summary>
	/// <param name="name">Name to be applied to the filter group.</param>
	/// <param name="filter">Filter to add to the newly-registered filter group.</param>
	/// <returns>An <see cref="int"/> that represents the index at which the filter group was registered.</returns>
	/// <exception cref="ArgumentException">
	/// <paramref name="name"/> cannot be empty or whitespace only. -or-
	/// filter group with the specified <paramref name="name"/> already defined.
	/// </exception>
	/// <exception cref="InvalidOperationException">Filter container is sealed.</exception>
	int RegisterGroup(string? name, TFilter? filter);

	/// <summary>
	/// Registers a new filter group that contains the specified <paramref name="filter"/>.
	/// </summary>
	/// <param name="filter">Filter to add to the newly-registered filter group.</param>
	/// <returns>An <see cref="int"/> that represents the index at which the filter group was registered.</returns>
	/// <exception cref="InvalidOperationException">Filter container is sealed.</exception>
	int RegisterGroup(TFilter? filter);

	/// <summary>
	/// Registers a collection of filter groups.
	/// </summary>
	/// <param name="groups">A collection of filter group to register.</param>
	/// <exception cref="ArgumentNullException"><paramref name="groups"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">Filter group with the specified name already defined.</exception>
	/// <exception cref="InvalidOperationException">Filter container is sealed.</exception>
	void RegisterGroups(IEnumerable<IFilterGroup<TFilter>> groups);

	/// <summary>
	/// Removes a filter at the given <paramref name="index"/> in the specified <paramref name="group"/>.
	/// </summary>
	/// <param name="group">Group to remove the filter from.</param>
	/// <param name="index">Index of the filter in the specified <paramref name="group"/>.</param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="group"/> is less than <c>0</c>. -or-
	/// <paramref name="group"/> is equal to or greater than <see cref="IReadOnlyFilterContainer{TFilter}.NumGroups"/>. -or-
	/// <paramref name="index"/> is less than <c>0</c>. -or-
	/// <paramref name="index"/> is equal to or greater than <see cref="IReadOnlyFilterGroup{TFilter}.Count"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">Filter container is sealed. -or- Filter group is sealed.</exception>
	void RemoveFilter(int group, int index);

	/// <summary>
	/// Removes a filter at the given <paramref name="index"/> in a filter group with the specified <paramref name="name"/>.
	/// </summary>
	/// <param name="name">Name of the filter group to remove the filter from.</param>
	/// <param name="index">Index of the filter in the specified group.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than <c>0</c>. -or- <paramref name="index"/> is equal to or greater than <see cref="IReadOnlyFilterGroup{TFilter}.Count"/>.</exception>
	/// <exception cref="ArgumentException">
	/// <paramref name="name"/> cannot be null or whitespace only. -or-
	/// filter group with the specified <paramref name="name"/> not found.
	/// </exception>
	/// <exception cref="InvalidOperationException">Filter container is sealed. -or- Filter group is sealed.</exception>
	void RemoveFilter(string name, int index);

	/// <summary>
	/// Changes the name of the filter group at the specified <paramref name="index"/>.
	/// </summary>
	/// <param name="index">Index of group to rename.</param>
	/// <param name="newName">New name for the filter group.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than <c>0</c>. -or- <paramref name="index"/> is equal to or greater than <see cref="IReadOnlyFilterContainer{TFilter}.NumGroups"/>.</exception>
	/// <exception cref="ArgumentException">Filter group with the specified <paramref name="newName"/> already defined. -or- <paramref name="newName"/> cannot be empty or white space only.</exception>
	void Rename(int index, string? newName);

	/// <summary>
	/// Changes the name of a filter group.
	/// </summary>
	/// <param name="name">Name of target filter group.</param>
	/// <param name="newName">New name for the filter group.</param>
	/// <exception cref="ArgumentException">
	/// filter group with the specified <paramref name="newName"/> already defined. -or-
	/// filter group with the <paramref name="name"/> not found. -or-
	/// <paramref name="name"/> cannot be null or white space only. -or-
	/// <paramref name="newName"/> cannot be empty or white space only.
	/// </exception>
	void Rename(string name, string? newName);

	/// <summary>
	/// Returns all groups contained within this collection.
	/// </summary>
	new IFilterGroup<TFilter>[] ToArray();

	/// <summary>
	/// Unregisters the specified <paramref name="group"/>.
	/// </summary>
	/// <param name="group"></param>
	/// <exception cref="ArgumentNullException"><paramref name="group"/> is <see langword="null"/>.</exception>
	/// <exception cref="InvalidOperationException">Filter container is sealed. -or- <paramref name="group"/> is not contained within this filter container.</exception>
	int UnregisterGroup(IFilterGroup<TFilter> group);

	/// <summary>
	/// Unregisters the filter group at the specified <paramref name="index"/>.
	/// </summary>
	/// <param name="index">Index of filter group to unregister.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than <c>0</c>. -or- <paramref name="index"/> is equal to or greater than <see cref="IReadOnlyFilterContainer{TFilter}.NumGroups"/>.</exception>
	/// <exception cref="InvalidOperationException">Filter container is sealed.</exception>
	void UnregisterGroup(int index);

	/// <summary>
	/// Unregisters the filter group with the specified <paramref name="name"/>.
	/// </summary>
	/// <param name="name">Name of filter group to unregister.</param>
	/// <exception cref="ArgumentException"><paramref name="name"/> cannot be null or white space only.</exception>
	/// <returns>An <see cref="int"/> that represents the index at which the filter group was registered. -or- <c>-1</c> if there was no filter group with that <paramref name="name"/>.</returns>
	/// <exception cref="InvalidOperationException">Filter container is sealed.</exception>
	int UnregisterGroup(string name);

	/// <summary>
	/// Unregisters a range of filter groups.
	/// </summary>
	/// <param name="index">Index at which the removal should be started.</param>
	/// <param name="count">Number of filter groups to remove.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than <c>0</c>. -or- <paramref name="index"/> is equal to or greater than <see cref="IReadOnlyFilterContainer{TFilter}.NumGroups"/>.</exception>
	/// <exception cref="InvalidOperationException">Filter container is sealed.</exception>
	void UnregisterGroups(int index, int count);
}
