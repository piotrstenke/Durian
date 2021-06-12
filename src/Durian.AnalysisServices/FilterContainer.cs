// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Durian.Generator
{
	/// <summary>
	/// A collection of <see cref="ISyntaxFilter"/>s that binds its elements into separate <see cref="FilterGroup{TFilter}"/>s.
	/// </summary>
	/// <typeparam name="TFilter">Type of <see cref="ISyntaxFilter"/> that can be stored in this list.</typeparam>
	[DebuggerDisplay("NumGroups = {NumGroups}, IsSealed = {IsSealed}")]
	public class FilterContainer<TFilter> : ICollection<FilterGroup<TFilter>> where TFilter : ISyntaxFilter
	{
		private readonly List<FilterGroup<TFilter>> _filterGroups;

		/// <summary>
		/// Number of filters in this list.
		/// </summary>
		public int Count
		{
			get
			{
				int count = 0;
				int length = NumGroups;

				for (int i = 0; i < length; i++)
				{
					count += _filterGroups[i].Count;
				}

				return count;
			}
		}

		bool ICollection<FilterGroup<TFilter>>.IsReadOnly => IsSealed;

		/// <summary>
		/// Determines whether this <see cref="FilterContainer{TFilter}"/> is sealed by calling the <see cref="Seal"/> method.
		/// </summary>
		public bool IsSealed { get; private set; }

		/// <summary>
		/// Number of filter groups in this list.
		/// </summary>
		public int NumGroups => _filterGroups.Count;

		/// <inheritdoc cref="GetFilterGroup(int)"/>
		public FilterGroup<TFilter> this[int index] => _filterGroups[index];

		/// <inheritdoc cref="GetFilterGroup(string)"/>
		public FilterGroup<TFilter> this[string name] => GetFilterGroup(name);

		/// <inheritdoc cref="GetFilter(int, int)"/>
		public TFilter this[int group, int index] => GetFilter(group, index);

		/// <inheritdoc cref="GetFilter(string, int)"/>
		public TFilter this[string name, int index] => GetFilter(name, index);

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterContainer{TFilter}"/> class.
		/// </summary>
		public FilterContainer()
		{
			_filterGroups = new();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterContainer{TFilter}"/> class.
		/// </summary>
		/// <param name="filters">A collection of <typeparamref name="TFilter"/>s that represent the initial <see cref="FilterGroup{TFilter}"/> of the <see cref="FilterContainer{TFilter}"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="filters"/> is <see langword="null"/>.</exception>
		public FilterContainer(IEnumerable<TFilter> filters)
		{
			if (filters is null)
			{
				throw new ArgumentNullException(nameof(filters));
			}

			_filterGroups = new() { new(filters) };
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterContainer{TFilter}"/> class.
		/// </summary>
		/// <param name="groups">Initial <see cref="FilterGroup{TFilter}"/>s of this <see cref="FilterContainer{TFilter}"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="groups"/> is <see langword="null"/>.</exception>
		public FilterContainer(IEnumerable<IEnumerable<TFilter>> groups)
		{
			_filterGroups = new();

			if (groups is null)
			{
				throw new ArgumentNullException(nameof(groups));
			}

			foreach (IEnumerable<TFilter> group in groups)
			{
				_filterGroups.Add(new(group));
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterContainer{TFilter}"/> class.
		/// </summary>
		/// <param name="groups">Initial <see cref="FilterGroup{TFilter}"/>s of this <see cref="FilterContainer{TFilter}"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="groups"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><see cref="FilterGroup{TFilter}"/> with the specified name already defined.</exception>
		public FilterContainer(IEnumerable<FilterGroup<TFilter>> groups)
		{
			if (groups is null)
			{
				throw new ArgumentNullException(nameof(groups));
			}

			_filterGroups = new();
			RegisterFilterGroups(groups);
		}

		void ICollection<FilterGroup<TFilter>>.Add(FilterGroup<TFilter> item)
		{
			RegisterFilterGroup(item);
		}

		/// <summary>
		/// Adds a <typeparamref name="TFilter"/> to a <see cref="FilterGroup{TFilter}"/> at the specified <paramref name="index"/>
		/// </summary>
		/// <param name="index">Index of group to add the <paramref name="filter"/> to.</param>
		/// <param name="filter"><typeparamref name="TFilter"/> to add.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than <c>0</c>. -or- <paramref name="index"/> is equal to or greater than <see cref="NumGroups"/>.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="filter"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="FilterContainer{TFilter}"/> is sealed. -or- <see cref="FilterGroup{TFilter}"/> is sealed.</exception>
		public void AddFilter(int index, TFilter filter)
		{
			ThrowIfSealed();
			_filterGroups[index].AddFilter(filter);
		}

		/// <summary>
		/// Adds a <typeparamref name="TFilter"/> to a <see cref="FilterGroup{TFilter}"/> with the specified <paramref name="name"/>.
		/// </summary>
		/// <param name="name">Name of <see cref="FilterGroup{TFilter}"/> to add the <paramref name="filter"/> to.</param>
		/// <param name="filter"><typeparamref name="TFilter"/> to add.</param>
		/// <exception cref="ArgumentNullException"><paramref name="filter"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="name"/> cannot be null or whitespace only. -or-
		/// <see cref="FilterGroup{TFilter}"/> with the specified <paramref name="name"/> not found.
		/// </exception>
		/// <exception cref="InvalidOperationException"><see cref="FilterContainer{TFilter}"/> is sealed. -or- <see cref="FilterGroup{TFilter}"/> is sealed.</exception>
		public void AddFilter(string name, TFilter filter)
		{
			ThrowIfSealed();
			GetFilterGroup(name).AddFilter(filter);
		}

		/// <summary>
		/// Adds a range of <typeparamref name="TFilter"/>s to a <see cref="FilterGroup{TFilter}"/> at the specified <paramref name="index"/>
		/// </summary>
		/// <param name="index">Index of group to add the <paramref name="filters"/> to.</param>
		/// <param name="filters">A collection of <see cref="ISyntaxFilter"/> to add.</param>
		/// <exception cref="ArgumentNullException"><paramref name="filters"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than <c>0</c>. -or-
		/// <paramref name="index"/> is equal to or greater than <see cref="NumGroups"/>.
		/// </exception>
		/// <exception cref="InvalidOperationException"><see cref="FilterContainer{TFilter}"/> is sealed. -or- <see cref="FilterGroup{TFilter}"/> is sealed.</exception>
		public void AddFilters(int index, IEnumerable<TFilter> filters)
		{
			ThrowIfSealed();
			_filterGroups[index].AddFilters(filters);
		}

		/// <summary>
		/// Adds a range of <typeparamref name="TFilter"/>s to a <see cref="FilterGroup{TFilter}"/> with the specified <paramref name="name"/>.
		/// </summary>
		/// <param name="name">Name of <see cref="FilterGroup{TFilter}"/> to add the <paramref name="filters"/> to.</param>
		/// <param name="filters">A collection of <typeparamref name="TFilter"/>s to add.</param>
		/// <exception cref="ArgumentNullException"><paramref name="filters"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="name"/> cannot be null or whitespace only. -or-
		/// <see cref="FilterGroup{TFilter}"/> with the specified <paramref name="name"/> not found.
		/// </exception>
		/// <exception cref="InvalidOperationException"><see cref="FilterContainer{TFilter}"/> is sealed. -or- <see cref="FilterGroup{TFilter}"/> is sealed.</exception>
		public void AddFilters(string name, IEnumerable<TFilter> filters)
		{
			ThrowIfSealed();
			GetFilterGroup(name).AddFilters(filters);
		}

		/// <summary>
		/// Removes all <see cref="FilterGroup{TFilter}"/>s from the <see cref="FilterContainer{TFilter}"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException"><see cref="FilterContainer{TFilter}"/> is sealed.</exception>
		public void Clear()
		{
			ThrowIfSealed();
			_filterGroups.Clear();
		}

		bool ICollection<FilterGroup<TFilter>>.Contains(FilterGroup<TFilter> item)
		{
			return ContainsFilterGroup(item);
		}

		/// <summary>
		/// Determines whether any <see cref="FilterGroup{TFilter}"/> has the specified <paramref name="name"/>.
		/// </summary>
		/// <param name="name">Name to check for.</param>
		/// <exception cref="ArgumentException"><paramref name="name"/> cannot be null or white space only.</exception>
		public bool ContainsFilterGroup(string name)
		{
			ThrowIfNullOrWhiteSpace(name);

			return _filterGroups.Any(g => g.Name == name);
		}

		/// <summary>
		/// Determines whether this <see cref="FilterContainer{TFilter}"/> contains the specified <paramref name="group"/>.
		/// </summary>
		/// <param name="group"><see cref="FilterGroup{TFilter}"/> to check.</param>
		/// <exception cref="ArgumentNullException"><paramref name="group"/> is <see langword="null"/>.</exception>
		public bool ContainsFilterGroup(FilterGroup<TFilter> group)
		{
			if (group is null)
			{
				throw new ArgumentNullException(nameof(group));
			}

			return _filterGroups.Contains(group);
		}

		void ICollection<FilterGroup<TFilter>>.CopyTo(FilterGroup<TFilter>[] array, int arrayIndex)
		{
			_filterGroups.CopyTo(array, arrayIndex);
		}

		/// <inheritdoc/>
		public IEnumerator<FilterGroup<TFilter>> GetEnumerator()
		{
			return _filterGroups.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Returns a <typeparamref name="TFilter"/> at the given <paramref name="index"/> in the specified <paramref name="group"/>.
		/// </summary>
		/// <param name="group">Group to get the <typeparamref name="TFilter"/> from.</param>
		/// <param name="index">Index of the <typeparamref name="TFilter"/> in the specified <paramref name="group"/>.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="group"/> is less than <c>0</c>. -or-
		/// <paramref name="group"/> is equal to or greater than <see cref="NumGroups"/>. -or-
		/// <paramref name="index"/> is less than <c>0</c>. -or-
		/// <paramref name="index"/> is equal to or greater than <see cref="FilterGroup{TFilter}.Count"/>.
		/// </exception>
		public TFilter GetFilter(int group, int index)
		{
			return _filterGroups[group].GetFilter(index);
		}

		/// <summary>
		/// Returns a <typeparamref name="TFilter"/> at the given <paramref name="index"/> in a <see cref="FilterGroup{TFilter}"/> with the specified <paramref name="name"/>.
		/// </summary>
		/// <param name="name">Name of <see cref="FilterGroup{TFilter}"/> the <typeparamref name="TFilter"/> is located in.</param>
		/// <param name="index">Index of the <typeparamref name="TFilter"/> in the specified <see cref="FilterGroup{TFilter}"/>.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than <c>0</c>. -or- <paramref name="index"/> is equal to or greater than <see cref="FilterGroup{TFilter}.Count"/>.</exception>
		/// <exception cref="ArgumentException">
		/// Name of a filter group cannot be null or whitespace only. -or-
		/// <see cref="FilterGroup{TFilter}"/> with the specified <paramref name="name"/> not found.
		/// </exception>
		public TFilter GetFilter(string name, int index)
		{
			return GetFilterGroup(name)[index];
		}

		/// <summary>
		/// Returns a <see cref="FilterGroup{TFilter}"/> at the specified <paramref name="index"/>.
		/// </summary>
		/// <param name="index">Index of the <see cref="FilterGroup{TFilter}"/> to get.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than <c>0</c>. -or- <paramref name="index"/> is equal to or greater than <see cref="NumGroups"/>.</exception>
		public FilterGroup<TFilter> GetFilterGroup(int index)
		{
			return _filterGroups[index];
		}

		/// <summary>
		/// Returns the <see cref="FilterGroup{TFilter}"/> with the specified <paramref name="name"/>.
		/// </summary>
		/// <param name="name">Name of the group to get.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="name"/> cannot be null or whitespace only. -or-
		/// <see cref="FilterGroup{TFilter}"/> with the specified <paramref name="name"/> not found.
		/// </exception>
		public FilterGroup<TFilter> GetFilterGroup(string name)
		{
			ThrowIfNullOrWhiteSpace(name);

			if (_filterGroups.Find(g => g.Name == name) is not FilterGroup<TFilter> group)
			{
				throw new ArgumentNullException($"Filter group with name '{name}' not found!");
			}

			return group;
		}

		/// <summary>
		/// Returns index of a <see cref="FilterGroup{TFilter}"/> with the specified <paramref name="name"/>.
		/// </summary>
		/// <param name="name">Name of <see cref="FilterGroup{TFilter}"/> to return the index of.</param>
		/// <exception cref="ArgumentException"><paramref name="name"/> cannot be <see langword="null"/> or white space only.</exception>
		public int IndexOf(string name)
		{
			ThrowIfNullOrWhiteSpace(name);
			return _filterGroups.FindIndex(g => g.Name == name);
		}

		/// <summary>
		/// Returns name of the <see cref="FilterGroup{TFilter}"/> at the specified <paramref name="index"/>.
		/// </summary>
		/// <param name="index"></param>
		/// <returns>Name of the <see cref="FilterGroup{TFilter}"/> at the specified <paramref name="index"/>. -or- <see langword="null"/> if group at the specified <paramref name="index"/> doesn't have a name.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than <c>0</c>. -or- <paramref name="index"/> is equal to or greater than <see cref="NumGroups"/>.</exception>
		public string? NameOf(int index)
		{
			return _filterGroups[index].Name;
		}

		/// <summary>
		/// Registers a new <see cref="FilterGroup{TFilter}"/>.
		/// </summary>
		/// <returns>An <see cref="int"/> that represents the index at which the <see cref="FilterGroup{TFilter}"/> was registered.</returns>
		public int RegisterFilterGroup()
		{
			return RegisterFilterGroup(null, null);
		}

		/// <summary>
		/// Registers the specified <paramref name="group"/>.
		/// </summary>
		/// <param name="group"><see cref="FilterGroup{TFilter}"/> to register.</param>
		/// <returns>An <see cref="int"/> that represents the index at which the <see cref="FilterGroup{TFilter}"/> was registered.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="group"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><see cref="FilterGroup{TFilter}"/> with the specified name already defined.</exception>
		/// <exception cref="InvalidOperationException"><see cref="FilterContainer{TFilter}"/> is sealed.</exception>
		public int RegisterFilterGroup(FilterGroup<TFilter> group)
		{
			ThrowIfSealed();

			if (group is null)
			{
				throw new ArgumentNullException(nameof(group));
			}

			if (group.Name is not null && _filterGroups.Any(g => g.Name == group.Name))
			{
				throw Exc_GroupWithNameAlreadyDefined(group.Name);
			}

			_filterGroups.Add(group);
			AddParent(group);

			return Count - 1;
		}

		/// <summary>
		/// Register a new <see cref="FilterGroup{TFilter}"/> with the specified <paramref name="name"/>.
		/// </summary>
		/// <param name="name">Name to be applied to the <see cref="FilterGroup{TFilter}"/>.</param>
		/// <returns>An <see cref="int"/> that represents the index at which the <see cref="FilterGroup{TFilter}"/> was registered.</returns>
		/// <exception cref="ArgumentException"><paramref name="name"/> cannot be empty or whitespace only. -or-
		/// <see cref="FilterGroup{TFilter}"/> with the specified <paramref name="name"/> already defined.
		/// </exception>
		/// <exception cref="InvalidOperationException"><see cref="FilterContainer{TFilter}"/> is sealed.</exception>
		public int RegisterFilterGroup(string? name)
		{
			return RegisterFilterGroup(name, null);
		}

		/// <summary>
		/// Registers a new <see cref="FilterGroup{TFilter}"/> that contains the specified <paramref name="filter"/>.
		/// </summary>
		/// <param name="filter"><typeparamref name="TFilter"/> to add to the newly-registered <see cref="FilterGroup{TFilter}"/>.</param>
		/// <returns>An <see cref="int"/> that represents the index at which the <see cref="FilterGroup{TFilter}"/> was registered.</returns>
		/// <exception cref="InvalidOperationException"><see cref="FilterContainer{TFilter}"/> is sealed.</exception>
		public int RegisterFilterGroup(TFilter? filter)
		{
			return RegisterFilterGroup(null, filter);
		}

		/// <summary>
		/// Registers a new <see cref="FilterGroup{TFilter}"/> that contains the specified <paramref name="name"/> and <paramref name="filter"/>.
		/// </summary>
		/// <param name="name">Name to be applied to the <see cref="FilterGroup{TFilter}"/>.</param>
		/// <param name="filter"><typeparamref name="TFilter"/> to add to the newly-registered <see cref="FilterGroup{TFilter}"/>.</param>
		/// <returns>An <see cref="int"/> that represents the index at which the <see cref="FilterGroup{TFilter}"/> was registered.</returns>
		/// <exception cref="ArgumentException">
		/// <paramref name="name"/> cannot be empty or whitespace only. -or-
		/// <see cref="FilterGroup{TFilter}"/> with the specified <paramref name="name"/> already defined.
		/// </exception>
		/// <exception cref="InvalidOperationException"><see cref="FilterContainer{TFilter}"/> is sealed.</exception>
		public int RegisterFilterGroup(string? name, TFilter? filter)
		{
			ThrowIfSealed();

			if (name is not null)
			{
				ThrowIfEmptyOrWhiteSpace(name);
				ThrowIfNameExists(name);
			}

			int index = _filterGroups.Count;
			FilterGroup<TFilter> group = new(name);

			if (filter is not null)
			{
				group.AddFilter(filter);
			}

			_filterGroups.Add(group);
			AddParent(group);
			return index;
		}

		/// <summary>
		/// Registers a new <see cref="FilterGroup{TFilter}"/> that contains the specified <paramref name="filters"/>.
		/// </summary>
		/// <param name="filters">A collection of <typeparamref name="TFilter"/>s to add to the newly-register <see cref="FilterGroup{TFilter}"/>.</param>
		/// <returns>An <see cref="int"/> that represents the index at which the <see cref="FilterGroup{TFilter}"/> was registered.</returns>
		/// <exception cref="InvalidOperationException"><see cref="FilterContainer{TFilter}"/> is sealed.</exception>
		public int RegisterFilterGroup(IEnumerable<TFilter>? filters)
		{
			ThrowIfSealed();

			int index = _filterGroups.Count;
			FilterGroup<TFilter> group = filters is not null ? new(filters) : new();
			_filterGroups.Add(group);
			AddParent(group);
			return index;
		}

		/// <summary>
		/// Registers a new <see cref="FilterGroup{TFilter}"/> that contains the specified <paramref name="filters"/>.
		/// </summary>
		/// <param name="name">Name to be applied to the newly-registered <see cref="FilterGroup{TFilter}"/>.</param>
		/// <param name="filters">A collection of <typeparamref name="TFilter"/>s to add to the newly-register <see cref="FilterGroup{TFilter}"/>.</param>
		/// <returns>An <see cref="int"/> that represents the index at which the <see cref="FilterGroup{TFilter}"/> was registered.</returns>
		/// <exception cref="ArgumentException">
		/// <paramref name="name"/> cannot be empty or whitespace only. -or-
		/// <see cref="FilterGroup{TFilter}"/> with the specified <paramref name="name"/> already defined. -or-
		/// </exception>
		/// <exception cref="InvalidOperationException"><see cref="FilterContainer{TFilter}"/> is sealed.</exception>
		public int RegisterFilterGroup(string? name, IEnumerable<TFilter>? filters)
		{
			ThrowIfSealed();

			if (name is not null)
			{
				ThrowIfEmptyOrWhiteSpace(name);
				ThrowIfNameExists(name);
			}

			int index = _filterGroups.Count;
			FilterGroup<TFilter> group = filters is not null ? new(name, filters) : new(name);
			_filterGroups.Add(group);
			AddParent(group);
			return index;
		}

		/// <summary>
		/// Registers a collection of <see cref="FilterGroup{TFilter}"/>s.
		/// </summary>
		/// <param name="groups">A collection of <see cref="FilterGroup{TFilter}"/> to register.</param>
		/// <exception cref="ArgumentNullException"><paramref name="groups"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><see cref="FilterGroup{TFilter}"/> with the specified name already defined.</exception>
		/// <exception cref="InvalidOperationException"><see cref="FilterContainer{TFilter}"/> is sealed.</exception>
		public void RegisterFilterGroups(IEnumerable<FilterGroup<TFilter>> groups)
		{
			ThrowIfSealed();

			if (groups is null)
			{
				throw new ArgumentNullException(nameof(groups));
			}

			FilterGroup<TFilter>[] g = groups.ToArray();
			int length = g.Length;

			foreach (FilterGroup<TFilter> group in g)
			{
				if (group.Name is not null && _filterGroups.Any(g => g.Name == group.Name))
				{
					throw Exc_GroupWithNameAlreadyDefined(group.Name);
				}
			}

			foreach (FilterGroup<TFilter> group in g)
			{
				AddParent(group);
				_filterGroups.Add(group);
			}
		}

		bool ICollection<FilterGroup<TFilter>>.Remove(FilterGroup<TFilter> item)
		{
			UnregisterFilterGroup(item);
			return true;
		}

		/// <summary>
		/// Removes a <typeparamref name="TFilter"/> at the given <paramref name="index"/> in the specified <paramref name="group"/>.
		/// </summary>
		/// <param name="group">Group to remove the <typeparamref name="TFilter"/> from.</param>
		/// <param name="index">Index of the <typeparamref name="TFilter"/> in the specified <paramref name="group"/>.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="group"/> is less than <c>0</c>. -or-
		/// <paramref name="group"/> is equal to or greater than <see cref="NumGroups"/>. -or-
		/// <paramref name="index"/> is less than <c>0</c>. -or-
		/// <paramref name="index"/> is equal to or greater than <see cref="FilterGroup{TFilter}.Count"/>.
		/// </exception>
		/// <exception cref="InvalidOperationException"><see cref="FilterContainer{TFilter}"/> is sealed. -or- <see cref="FilterGroup{TFilter}"/> is sealed.</exception>
		public void RemoveFilter(int group, int index)
		{
			ThrowIfSealed();
			GetFilterGroup(group).RemoveFilter(index);
		}

		/// <summary>
		/// Removes a <typeparamref name="TFilter"/> at the given <paramref name="index"/> in a <see cref="FilterGroup{TFilter}"/> with the specified <paramref name="name"/>.
		/// </summary>
		/// <param name="name">Name of the <see cref="FilterGroup{TFilter}"/> to remove the <typeparamref name="TFilter"/> from.</param>
		/// <param name="index">Index of the <typeparamref name="TFilter"/> in the specified group.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than <c>0</c>. -or- <paramref name="index"/> is equal to or greater than <see cref="FilterGroup{TFilter}.Count"/>.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="name"/> cannot be null or whitespace only. -or-
		/// <see cref="FilterGroup{TFilter}"/> with the specified <paramref name="name"/> not found.
		/// </exception>
		/// <exception cref="InvalidOperationException"><see cref="FilterContainer{TFilter}"/> is sealed. -or- <see cref="FilterGroup{TFilter}"/> is sealed.</exception>
		public void RemoveFilter(string name, int index)
		{
			ThrowIfSealed();
			GetFilterGroup(name).RemoveFilter(index);
		}

		/// <summary>
		/// Changes the name of the <see cref="FilterGroup{TFilter}"/> at the specified <paramref name="index"/>.
		/// </summary>
		/// <param name="index">Index of group to rename.</param>
		/// <param name="newName">New name for the <see cref="FilterGroup{TFilter}"/>.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than <c>0</c>. -or- <paramref name="index"/> is equal to or greater than <see cref="NumGroups"/>.</exception>
		/// <exception cref="ArgumentException"><see cref="FilterGroup{TFilter}"/> with the specified <paramref name="newName"/> already defined. -or- <paramref name="newName"/> cannot be empty or white space only.</exception>
		public void Rename(int index, string? newName)
		{
			if (newName is not null && string.IsNullOrWhiteSpace(newName))
			{
				throw new ArgumentException($"{nameof(newName)} cannot be empty or white space only!");
			}

			FilterGroup<TFilter> group = _filterGroups[index];

			if (newName != group.Name)
			{
				bool exists = false;

				foreach (string? n in _filterGroups.Select(g => g.Name))
				{
					if (n == newName)
					{
						if (exists)
						{
							throw Exc_GroupWithNameAlreadyDefined(newName!);
						}

						exists = true;
					}
				}
			}

			group.Name = newName;
		}

		/// <summary>
		/// Changes the name of a <see cref="FilterGroup{TFilter}"/>.
		/// </summary>
		/// <param name="name">Name of target <see cref="FilterGroup{TFilter}"/>.</param>
		/// <param name="newName">New name for the <see cref="FilterGroup{TFilter}"/>.</param>
		/// <exception cref="ArgumentException">
		/// <see cref="FilterGroup{TFilter}"/> with the specified <paramref name="newName"/> already defined. -or-
		/// <see cref="FilterGroup{TFilter}"/> with the <paramref name="name"/> not found. -or-
		/// <paramref name="name"/> cannot be null or white space only. -or-
		/// <paramref name="newName"/> cannot be empty or white space only.
		/// </exception>
		public void Rename(string name, string? newName)
		{
			ThrowIfNullOrWhiteSpace(name);

			int index = _filterGroups.FindIndex(g => g.Name == name);

			if (index == -1)
			{
				throw new ArgumentException($"Group with name '{name}' not found!");
			}

			Rename(index, newName);
		}

		/// <summary>
		/// After this method is called, no more <typeparamref name="TFilter"/>s or <see cref="FilterGroup{TFilter}"/>s can be added to or removed from this <see cref="FilterContainer{TFilter}"/>.
		/// </summary>
		public void Seal()
		{
			IsSealed = true;
		}

		/// <summary>
		/// Converts this <see cref="FilterContainer{TFilter}"/> into an array of <see cref="FilterGroup{TFilter}"/>s.
		/// </summary>
		public FilterGroup<TFilter>[] ToArray()
		{
			return _filterGroups.ToArray();
		}

		/// <summary>
		/// Unregisters the <see cref="FilterGroup{TFilter}"/> group at the specified <paramref name="index"/>.
		/// </summary>
		/// <param name="index">Index of <see cref="FilterGroup{TFilter}"/> to unregister.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than <c>0</c>. -or- <paramref name="index"/> is equal to or greater than <see cref="NumGroups"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="FilterContainer{TFilter}"/> is sealed.</exception>
		public void UnregisterFilterGroup(int index)
		{
			ThrowIfSealed();
			RemoveParent(_filterGroups[index]);
		}

		/// <summary>
		/// Unregisters the specified <paramref name="group"/>.
		/// </summary>
		/// <param name="group"></param>
		/// <exception cref="ArgumentNullException"><paramref name="group"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="FilterContainer{TFilter}"/> is sealed. -or- <paramref name="group"/> is not contained within this <see cref="FilterContainer{TFilter}"/>.</exception>
		public int UnregisterFilterGroup(FilterGroup<TFilter> group)
		{
			ThrowIfSealed();

			if (group is null)
			{
				throw new ArgumentNullException(nameof(group));
			}

			int index = _filterGroups.IndexOf(group);

			if (index == -1)
			{
				throw Exc_UnknownGroup();
			}

			_filterGroups.RemoveAt(index);

			if (!_filterGroups.Contains(group))
			{
				group._containers.Remove(this);
			}

			return index;
		}

		/// <summary>
		/// Unregisters the <see cref="FilterGroup{TFilter}"/> group with the specified <paramref name="name"/>.
		/// </summary>
		/// <param name="name">Name of <see cref="FilterGroup{TFilter}"/> to unregister.</param>
		/// <exception cref="ArgumentException"><paramref name="name"/> cannot be null or white space only.</exception>
		/// <returns>An <see cref="int"/> that represents the index at which the <see cref="FilterGroup{TFilter}"/> was registered. -or- <c>-1</c> if there was no <see cref="FilterGroup{TFilter}"/> with that <paramref name="name"/>.</returns>
		/// <exception cref="InvalidOperationException"><see cref="FilterContainer{TFilter}"/> is sealed.</exception>
		public int UnregisterFilterGroup(string name)
		{
			ThrowIfSealed();
			ThrowIfNullOrWhiteSpace(name);

			int index = _filterGroups.FindIndex(g => g.Name == name);

			if (index == -1)
			{
				return index;
			}

			RemoveParent(_filterGroups[index]);
			_filterGroups.RemoveAt(index);
			return index;
		}

		/// <summary>
		/// Unregisters a range of <see cref="FilterGroup{TFilter}"/>.
		/// </summary>
		/// <param name="index">Index at which the removal should be started.</param>
		/// <param name="count">Number of <see cref="FilterGroup{TFilter}"/>s to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than <c>0</c>. -or- <paramref name="index"/> is equal to or greater than <see cref="NumGroups"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="FilterContainer{TFilter}"/> is sealed.</exception>
		public void UnregisterFilterGroups(int index, int count)
		{
			ThrowIfSealed();

			int length = index + count;

			for (int i = index; i < length; i++)
			{
				RemoveParent(_filterGroups[i]);
			}

			_filterGroups.RemoveRange(index, count);
		}

		/// <summary>
		/// Removes effect of the <see cref="Seal"/> method.
		/// </summary>
		public void Unseal()
		{
			IsSealed = false;
		}

		private static ArgumentException Exc_GroupWithNameAlreadyDefined(string name)
		{
			return new ArgumentException($"Group with name '{name}' already defined!");
		}

		private static InvalidOperationException Exc_UnknownGroup()
		{
			return new InvalidOperationException("Specified group is not contained within this filter container!");
		}

		private static void ThrowIfEmptyOrWhiteSpace(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException($"{nameof(name)} cannot be empty or whitespace only!");
			}
		}

		private static void ThrowIfNullOrWhiteSpace(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException($"{nameof(name)} cannot be null or whitespace only!");
			}
		}

		private void AddParent(FilterGroup<TFilter> group)
		{
			if (!group._containers.Contains(this))
			{
				group._containers.Add(this);
			}
		}

		private void RemoveParent(FilterGroup<TFilter> group)
		{
			if (!_filterGroups.Remove(group))
			{
				throw Exc_UnknownGroup();
			}

			if (!_filterGroups.Contains(group))
			{
				group._containers.Remove(this);
			}
		}

		private void ThrowIfNameExists(string name)
		{
			if (_filterGroups.Any(g => g.Name == name))
			{
				throw Exc_GroupWithNameAlreadyDefined(name);
			}
		}

		private void ThrowIfSealed()
		{
			if (IsSealed)
			{
				throw new InvalidOperationException("Filter container is sealed!");
			}
		}
	}
}
