// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Durian.Analysis.Filtration
{
	/// <summary>
	/// A collection of <see cref="ISyntaxFilter"/>s that binds its elements into separate <see cref="FilterGroup{TFilter}"/>s.
	/// </summary>
	/// <typeparam name="TFilter">Type of <see cref="ISyntaxFilter"/> that can be stored in this list.</typeparam>
	[DebuggerDisplay("NumGroups = {_filterGroups?.Count ?? 0}, IsSealed = {IsSealed}")]
	public class FilterContainer<TFilter> : ICollection<FilterGroup<TFilter>>, IFilterContainer<TFilter> where TFilter : ISyntaxFilter
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

		/// <summary>
		/// Determines whether this <see cref="FilterContainer{TFilter}"/> is sealed by calling the <see cref="Seal"/> method.
		/// </summary>
		public bool IsSealed { get; private set; }

		/// <summary>
		/// Number of filter groups in this list.
		/// </summary>
		public int NumGroups => _filterGroups.Count;

		bool ICollection<FilterGroup<TFilter>>.IsReadOnly => IsSealed;

		bool ISealable.CanBeSealed => !IsSealed;

		bool ISealable.CanBeUnsealed =>IsSealed;

		/// <inheritdoc cref="GetGroup(int)"/>
		public FilterGroup<TFilter> this[int index] => GetGroup(index);

		/// <inheritdoc cref="GetGroup(string)"/>
		public FilterGroup<TFilter> this[string name] => GetGroup(name);

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

			_filterGroups = new() { new FilterGroup<TFilter>(filters) };
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
				_filterGroups.Add(new FilterGroup<TFilter>(group));
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
			RegisterGroups(groups);
		}

		/// <inheritdoc/>
		public void AddFilter(int index, TFilter filter)
		{
			ThrowIfSealed();
			_filterGroups[index].AddFilter(filter);
		}

		/// <inheritdoc/>
		public void AddFilter(string name, TFilter filter)
		{
			ThrowIfSealed();
			GetGroup(name).AddFilter(filter);
		}

		/// <inheritdoc/>
		public void AddFilters(int index, IEnumerable<TFilter> filters)
		{
			ThrowIfSealed();
			_filterGroups[index].AddFilters(filters);
		}

		/// <inheritdoc/>
		public void AddFilters(string name, IEnumerable<TFilter> filters)
		{
			ThrowIfSealed();
			GetGroup(name).AddFilters(filters);
		}

		/// <inheritdoc/>
		public void Clear()
		{
			ThrowIfSealed();
			_filterGroups.Clear();
		}

		/// <inheritdoc/>
		public bool ContainsGroup(string name)
		{
			ThrowIfNullOrWhiteSpace(name);

			return _filterGroups.Any(g => g.Name == name);
		}

		/// <inheritdoc/>
		public bool ContainsGroup(FilterGroup<TFilter> group)
		{
			if (group is null)
			{
				throw new ArgumentNullException(nameof(group));
			}

			return _filterGroups.Contains(group);
		}

		/// <inheritdoc/>
		public IEnumerator<FilterGroup<TFilter>> GetEnumerator()
		{
			return _filterGroups.GetEnumerator();
		}

		/// <inheritdoc/>
		public TFilter GetFilter(int group, int index)
		{
			return _filterGroups[group].GetFilter(index);
		}

		/// <inheritdoc/>
		public TFilter GetFilter(string name, int index)
		{
			return GetGroup(name).GetFilter(index);
		}

		/// <inheritdoc/>
		public FilterGroup<TFilter> GetGroup(int index)
		{
			return _filterGroups[index];
		}

		/// <inheritdoc/>
		public FilterGroup<TFilter> GetGroup(string name)
		{
			ThrowIfNullOrWhiteSpace(name);

			if (_filterGroups.Find(g => g.Name == name) is not FilterGroup<TFilter> group)
			{
				throw new ArgumentNullException($"Filter group with name '{name}' not found!");
			}

			return group;
		}

		/// <inheritdoc/>
		public int IndexOf(string name)
		{
			ThrowIfNullOrWhiteSpace(name);
			return _filterGroups.FindIndex(g => g.Name == name);
		}

		/// <inheritdoc/>
		public string? NameOf(int index)
		{
			return _filterGroups[index].Name;
		}

		/// <inheritdoc/>
		public int RegisterGroup()
		{
			return RegisterGroup(null, null);
		}

		/// <inheritdoc/>
		public int RegisterGroup(FilterGroup<TFilter> group)
		{
			ThrowIfSealed();

			if (group is null)
			{
				throw new ArgumentNullException(nameof(group));
			}

			RegisterGroup_Internal(group);

			return Count - 1;
		}

		/// <inheritdoc/>
		public int RegisterGroup(string? name)
		{
			return RegisterGroup(name, null);
		}

		/// <inheritdoc/>
		public int RegisterGroup(TFilter? filter)
		{
			return RegisterGroup(null, filter);
		}

		/// <inheritdoc/>
		public int RegisterGroup(string? name, TFilter? filter)
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

		/// <inheritdoc/>
		public int RegisterGroup(IEnumerable<TFilter>? filters)
		{
			ThrowIfSealed();

			int index = _filterGroups.Count;
			FilterGroup<TFilter> group = filters is not null ? new(filters) : new();
			_filterGroups.Add(group);
			AddParent(group);
			return index;
		}

		/// <inheritdoc/>
		public int RegisterGroup(string? name, IEnumerable<TFilter>? filters)
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

		/// <inheritdoc/>
		public void RegisterGroups(IEnumerable<FilterGroup<TFilter>> groups)
		{
			ThrowIfSealed();

			if (groups is null)
			{
				throw new ArgumentNullException(nameof(groups));
			}

			foreach (FilterGroup<TFilter> group in groups)
			{
				if (group is null)
				{
					continue;
				}

				RegisterGroup_Internal(group);
			}
		}

		/// <inheritdoc/>
		public void RemoveFilter(int group, int index)
		{
			ThrowIfSealed();
			GetGroup(group).RemoveFilter(index);
		}

		/// <inheritdoc/>
		public void RemoveFilter(string name, int index)
		{
			ThrowIfSealed();
			GetGroup(name).RemoveFilter(index);
		}

		/// <inheritdoc/>
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

		/// <inheritdoc/>
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

		/// <inheritdoc/>
		public bool Seal()
		{
			if(IsSealed)
			{
				return false;
			}

			IsSealed = true;
			return true;
		}

		/// <inheritdoc/>
		public FilterGroup<TFilter>[] ToArray()
		{
			return _filterGroups.ToArray();
		}

		/// <inheritdoc/>
		public void UnregisterGroup(int index)
		{
			ThrowIfSealed();
			RemoveParent(_filterGroups[index]);
		}

		/// <inheritdoc/>
		public int UnregisterGroup(FilterGroup<TFilter> group)
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

		/// <inheritdoc/>
		public int UnregisterGroup(string name)
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

		/// <inheritdoc/>
		public void UnregisterGroups(int index, int count)
		{
			ThrowIfSealed();

			int length = index + count;

			for (int i = index; i < length; i++)
			{
				RemoveParent(_filterGroups[i]);
			}

			_filterGroups.RemoveRange(index, count);
		}

		/// <inheritdoc/>
		public bool Unseal()
		{
			if(!IsSealed)
			{
				return false;
			}

			IsSealed = false;
			return true;
		}

		void ICollection<FilterGroup<TFilter>>.Add(FilterGroup<TFilter> item)
		{
			RegisterGroup(item);
		}

		bool ICollection<FilterGroup<TFilter>>.Contains(FilterGroup<TFilter> item)
		{
			return ContainsGroup(item);
		}

		bool IFilterContainer<TFilter>.ContainsGroup(IFilterGroup<TFilter> group)
		{
			return group is FilterGroup<TFilter> g && ContainsGroup(g);
		}

		void ICollection<FilterGroup<TFilter>>.CopyTo(FilterGroup<TFilter>[] array, int arrayIndex)
		{
			_filterGroups.CopyTo(array, arrayIndex);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator<IReadOnlyFilterGroup<TFilter>> IEnumerable<IReadOnlyFilterGroup<TFilter>>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator<IFilterGroup<TFilter>> IEnumerable<IFilterGroup<TFilter>>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IFilterGroup<TFilter> IFilterContainer<TFilter>.GetGroup(int index)
		{
			return GetGroup(index);
		}

		IFilterGroup<TFilter> IFilterContainer<TFilter>.GetGroup(string name)
		{
			return GetGroup(name);
		}

		IReadOnlyFilterGroup<TFilter> IReadOnlyFilterContainer<TFilter>.GetGroup(int index)
		{
			return GetGroup(index);
		}

		IReadOnlyFilterGroup<TFilter> IReadOnlyFilterContainer<TFilter>.GetGroup(string name)
		{
			return GetGroup(name);
		}

		int IFilterContainer<TFilter>.RegisterGroup(IFilterGroup<TFilter> group)
		{
			if (group is null)
			{
				throw new ArgumentNullException(nameof(group));
			}

			if (group is not FilterGroup<TFilter> g)
			{
				throw new ArgumentException($"Unsupported type: '{group.GetType()}'", nameof(group));
			}

			return RegisterGroup(g);
		}

		void IFilterContainer<TFilter>.RegisterGroups(IEnumerable<IFilterGroup<TFilter>> groups)
		{
			ThrowIfSealed();

			if (groups is null)
			{
				throw new ArgumentNullException(nameof(groups));
			}

			foreach (IFilterGroup<TFilter> group in groups)
			{
				if (group is null)
				{
					continue;
				}

				if (group is not FilterGroup<TFilter> g)
				{
					throw new ArgumentException($"Unsupported element type: '{group.GetType()}'", nameof(groups));
				}

				RegisterGroup_Internal(g);
			}
		}

		bool ICollection<FilterGroup<TFilter>>.Remove(FilterGroup<TFilter> item)
		{
			UnregisterGroup(item);
			return true;
		}

		IFilterGroup<TFilter>[] IFilterContainer<TFilter>.ToArray()
		{
			return ToArray();
		}

		IReadOnlyFilterGroup<TFilter>[] IReadOnlyFilterContainer<TFilter>.ToArray()
		{
			return ToArray();
		}

		int IFilterContainer<TFilter>.UnregisterGroup(IFilterGroup<TFilter> group)
		{
			if (group is null)
			{
				throw new ArgumentNullException(nameof(group));
			}

			if (group is not FilterGroup<TFilter> g)
			{
				throw new ArgumentException($"Unsupported type: '{group.GetType()}'", nameof(group));
			}

			return UnregisterGroup(g);
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

		private void RegisterGroup_Internal(FilterGroup<TFilter> group)
		{
			if (group.Name is not null && _filterGroups.Any(g => g.Name == group.Name))
			{
				throw Exc_GroupWithNameAlreadyDefined(group.Name);
			}

			_filterGroups.Add(group);
			AddParent(group);
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
