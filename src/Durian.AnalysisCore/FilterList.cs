using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Durian
{
	/// <summary>
	/// A collection of <see cref="ISyntaxFilter"/>s that binds its elements into separate <see cref="ISyntaxFilter"/> groups.
	/// </summary>
	/// <typeparam name="TFilter">Type of <see cref="ISyntaxFilter"/> that can be stored in this list.</typeparam>
	[DebuggerDisplay("{_filterGroups}")]
	public class FilterList<TFilter> : IEnumerable<TFilter[]> where TFilter : ISyntaxFilter
	{
		private readonly List<(string? name, List<TFilter> list)> _filterGroups;

		/// <summary>
		/// Number of filter groups in this list.
		/// </summary>
		public int NumGroups => _filterGroups.Count;

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
					count += _filterGroups[i].list.Count;
				}

				return count;
			}
		}

		/// <summary>
		/// Returns an array of <see cref="ISyntaxFilter"/>s that represents a <see cref="ISyntaxFilter"/> group.
		/// </summary>
		/// <param name="group">Index of the group to get.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="group"/> was out of range.</exception>
		public TFilter[] this[int group] => _filterGroups[group].list.ToArray();

		/// <summary>
		/// Returns an array of <see cref="ISyntaxFilter"/>s that represents a <see cref="ISyntaxFilter"/> group.
		/// </summary>
		/// <param name="groupName">Name of the group to get.</param>
		/// <exception cref="ArgumentException">
		/// Name of a filter group cannot be null or whitespace only. -or-
		/// <see cref="ISyntaxFilter"/> group with the specified <paramref name="groupName"/> not found.
		/// </exception>
		public TFilter[] this[string groupName] => GetList(groupName).ToArray();

		/// <summary>
		/// Returns a <see cref="ISyntaxFilter"/> at the given <paramref name="index"/> in the specified <paramref name="group"/>.
		/// </summary>
		/// <param name="group">Group to get the <see cref="ISyntaxFilter"/> from.</param>
		/// <param name="index">Index of the <see cref="ISyntaxFilter"/> in the specified <paramref name="group"/>.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="group"/> was out of range. -or- <paramref name="index"/> was out of range.</exception>
		public TFilter this[int group, int index] => _filterGroups[group].list[index];

		/// <summary>
		/// Returns a <see cref="ISyntaxFilter"/> at the given <paramref name="index"/> in an <see cref="ISyntaxFilter"/> group with the specified <paramref name="groupName"/>.
		/// </summary>
		/// <param name="groupName">Name of <see cref="ISyntaxFilter"/> the <see cref="ISyntaxFilter"/> is located in.</param>
		/// <param name="index">Index of the <see cref="ISyntaxFilter"/> in the specified group.</param>
		/// <exception cref="ArgumentException">
		/// Name of a filter group cannot be null or whitespace only. -or-
		/// <see cref="ISyntaxFilter"/> group with the specified <paramref name="groupName"/> not found.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> was out of range.</exception>
		public TFilter this[string groupName, int index] => GetList(groupName)[index];

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterList{TFilter}"/> class.
		/// </summary>
		public FilterList()
		{
			_filterGroups = new();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterList{TFilter}"/> class.
		/// </summary>
		/// <param name="filters">A collection of <see cref="ISyntaxFilter"/>s that represent the initial <see cref="ISyntaxFilter"/> group of the <see cref="FilterList{TFilter}"/>.</param>
		/// <exception cref="ArgumentException">
		/// An <see cref="ISyntaxFilter"/> group cannot be <see langword="null"/> or empty. -or-
		/// An <see cref="ISyntaxFilter"/> inside a group cannot be <see langword="null"/>.
		/// </exception>
		public FilterList(IEnumerable<TFilter> filters)
		{
			_filterGroups = new() { (null, ValidateGroup(filters)) };
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterList{TFilter}"/> class.
		/// </summary>
		/// <param name="groups">Initial <see cref="ISyntaxFilter"/> groups of this <see cref="FilterList{TFilter}"/>.</param>
		/// <exception cref="ArgumentException">
		/// An <see cref="ISyntaxFilter"/> group cannot be <see langword="null"/> or empty. -or-
		/// An <see cref="ISyntaxFilter"/> inside a group cannot be <see langword="null"/>.
		/// </exception>
		public FilterList(IEnumerable<IEnumerable<TFilter>> groups)
		{
			_filterGroups = new();

			if (groups is not null)
			{
				foreach (IEnumerable<TFilter> group in groups)
				{
					_filterGroups.Add((null, ValidateGroup(group)));
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterList{TFilter}"/> class.
		/// </summary>
		/// <param name="groups">Initial <see cref="ISyntaxFilter"/> groups of this <see cref="FilterList{TFilter}"/>.</param>
		/// <exception cref="ArgumentException">
		/// An <see cref="ISyntaxFilter"/> group cannot be <see langword="null"/> or empty. -or-
		/// An <see cref="ISyntaxFilter"/> inside a group cannot be <see langword="null"/>. -or-
		/// Group with the specified name already defined.
		/// </exception>
		public FilterList(IEnumerable<(string name, IEnumerable<TFilter> group)> groups)
		{
			_filterGroups = new();
			HashSet<string> names = new();

			foreach ((string name, IEnumerable<TFilter> collection) group in groups)
			{
				if (!names.Add(group.name))
				{
					throw new ArgumentException($"Group with name '{group.name}' already defined!");
				}

				List<TFilter> list = ValidateGroup(group.collection);

				_filterGroups.Add((group.name, list));
			}
		}

		/// <summary>
		/// Adds a filter to the specified <paramref name="group"/>.
		/// </summary>
		/// <param name="group">Index of group to add the <paramref name="filter"/> to.</param>
		/// <param name="filter"><see cref="ISyntaxFilter"/> to add.</param>
		/// <exception cref="ArgumentNullException"><paramref name="filter"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="group"/> was out of range.</exception>
		public void AddFilter(int group, TFilter filter)
		{
			if (filter is null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			_filterGroups[group].list.Add(filter);
		}

		/// <summary>
		/// Adds a filter to the specified <paramref name="group"/>.
		/// </summary>
		/// <param name="group">Name of group to add the <paramref name="filter"/> to.</param>
		/// <param name="filter"><see cref="ISyntaxFilter"/> to add.</param>
		/// <exception cref="ArgumentNullException"><paramref name="filter"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">
		/// Name of a filter group cannot be null or whitespace only. -or-
		/// <see cref="ISyntaxFilter"/> group with the specified name was not found.
		/// </exception>
		public void AddFilter(string group, TFilter filter)
		{
			if (filter is null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			GetList(group).Add(filter);
		}

		/// <summary>
		/// Adds a range of filters to the specified <paramref name="group"/>.
		/// </summary>
		/// <param name="group">Index of group to add the <paramref name="filters"/> to.</param>
		/// <param name="filters">A collection of <see cref="ISyntaxFilter"/> to add.</param>
		/// <exception cref="ArgumentException">
		/// An <see cref="ISyntaxFilter"/> group cannot be <see langword="null"/> or empty. -or-
		/// An <see cref="ISyntaxFilter"/> inside a group cannot be <see langword="null"/>.
		/// </exception>
		public void AddFilters(int group, IEnumerable<TFilter> filters)
		{
			_filterGroups[group].list.AddRange(ValidateGroup(filters));
		}

		/// <summary>
		/// Adds a range of filters to the specified <paramref name="group"/>.
		/// </summary>
		/// <param name="group">Name of group to add the <paramref name="filters"/> to.</param>
		/// <param name="filters">A collection of <see cref="ISyntaxFilter"/> to add.</param>
		/// <exception cref="ArgumentException">
		/// Name of a filter group cannot be null or whitespace only. -or-
		/// <see cref="ISyntaxFilter"/> group with the specified name was not found.
		/// An <see cref="ISyntaxFilter"/> group cannot be <see langword="null"/> or empty. -or-
		/// An <see cref="ISyntaxFilter"/> inside a group cannot be <see langword="null"/>.
		/// </exception>
		public void AddFilters(string group, IEnumerable<TFilter> filters)
		{
			GetList(group).AddRange(ValidateGroup(filters));
		}

		/// <summary>
		/// Returns a <see cref="ISyntaxFilter"/> at the given <paramref name="index"/> in the specified <paramref name="group"/>.
		/// </summary>
		/// <param name="group">Group to get the <see cref="ISyntaxFilter"/> from.</param>
		/// <param name="index">Index of the <see cref="ISyntaxFilter"/> in the specified <paramref name="group"/>.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="group"/> was out of range. -or- <paramref name="index"/> was out of range.</exception>
		public TFilter GetFilter(int group, int index)
		{
			return _filterGroups[group].list[index];
		}

		/// <summary>
		/// Returns a <see cref="ISyntaxFilter"/> at the given <paramref name="index"/> in an <see cref="ISyntaxFilter"/> group with the specified <paramref name="groupName"/>.
		/// </summary>
		/// <param name="groupName">Name of <see cref="ISyntaxFilter"/> the <see cref="ISyntaxFilter"/> is located in.</param>
		/// <param name="index">Index of the <see cref="ISyntaxFilter"/> in the specified group.</param>
		/// <exception cref="ArgumentException">
		/// Name of a filter group cannot be null or whitespace only. -or-
		/// <see cref="ISyntaxFilter"/> group with the specified <paramref name="groupName"/> not found.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> was out of range.</exception>
		public TFilter GetFilter(string groupName, int index)
		{
			return GetList(groupName)[index];
		}

		/// <summary>
		/// Removes a <see cref="ISyntaxFilter"/> at the given <paramref name="index"/> in the specified <paramref name="group"/>.
		/// </summary>
		/// <param name="group">Group to remove the <see cref="ISyntaxFilter"/> from.</param>
		/// <param name="index">Index of the <see cref="ISyntaxFilter"/> in the specified <paramref name="group"/>.</param>
		/// <exception cref="InvalidOperationException">An <see cref="ISyntaxFilter"/> group must have at least one element.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="group"/> was out of range. -or- <paramref name="index"/> was out of range.</exception>
		public void RemoveFilter(int group, int index)
		{
			List<TFilter> g = _filterGroups[group].list;
			int length = g.Count;

			if (length == 1 && index == length - 1)
			{
				throw new InvalidOperationException($"An {nameof(TFilter)} group must have at least one element!");
			}

			_filterGroups[group].list.RemoveAt(index);
		}

		/// <summary>
		/// Removes a <see cref="ISyntaxFilter"/> at the given <paramref name="index"/> in an <see cref="ISyntaxFilter"/> group with the specified <paramref name="groupName"/>.
		/// </summary>
		/// <param name="groupName">Name of the <see cref="ISyntaxFilter"/> group to remove the <see cref="ISyntaxFilter"/> from.</param>
		/// <param name="index">Index of the <see cref="ISyntaxFilter"/> in the specified group.</param>
		/// <exception cref="ArgumentException">
		/// Name of a filter group cannot be null or whitespace only. -or-
		/// <see cref="ISyntaxFilter"/> group with the specified <paramref name="groupName"/> not found.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> was out of range.</exception>
		public void RemoveFilter(string groupName, int index)
		{
			GetList(groupName).RemoveAt(index);
		}

		/// <summary>
		/// Returns an array of <see cref="ISyntaxFilter"/>s that represents a <see cref="ISyntaxFilter"/> group.
		/// </summary>
		/// <param name="group">Index of the group to get.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="group"/> was out of range.</exception>
		public TFilter[] GetFilterGroup(int group)
		{
			return _filterGroups[group].list.ToArray();
		}

		/// <summary>
		/// Returns an array of <see cref="ISyntaxFilter"/>s that represents a <see cref="ISyntaxFilter"/> group.
		/// </summary>
		/// <param name="groupName">Name of the group to get.</param>
		/// <exception cref="ArgumentException">
		/// Name of a filter group cannot be null or whitespace only. -or-
		/// <see cref="ISyntaxFilter"/> group with the specified <paramref name="groupName"/> not found.
		/// </exception>
		public TFilter[] GetFilterGroup(string groupName)
		{
			return GetList(groupName).ToArray();
		}

		/// <summary>
		/// Registers a new filter group.
		/// </summary>
		/// <param name="filter"><see cref="ISyntaxFilter"/> to add to the newly-registered <see cref="ISyntaxFilter"/> group.</param>
		/// <returns>An <see cref="int"/> that represents the index at which the <see cref="ISyntaxFilter"/> group was registered.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="filter"/> is <see langword="null"/>.</exception>
		public int RegisterFilterGroup(TFilter filter)
		{
			if (filter is null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			int index = _filterGroups.Count;
			_filterGroups.Add((null, new List<TFilter>() { filter }));
			return index;
		}

		/// <summary>
		/// Registers a new filter group.
		/// </summary>
		/// <param name="name">Name to be applied to the <see cref="ISyntaxFilter"/> group.</param>
		/// <param name="filter"><see cref="ISyntaxFilter"/> to add to the newly-registered <see cref="ISyntaxFilter"/> group.</param>
		/// <returns>An <see cref="int"/> that represents the index at which the <see cref="ISyntaxFilter"/> group was registered.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="filter"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">
		/// Name of a filter group cannot be null or whitespace only. -or-
		/// Group with the specified <paramref name="name"/> already defined.
		/// </exception>
		public int RegisterFilterGroup(string name, TFilter filter)
		{
			ValidateName(name);

			if (filter is null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			int index = _filterGroups.Count;
			_filterGroups.Add((name, new List<TFilter>() { filter }));
			return index;
		}

		/// <summary>
		/// Registers a new filter group that contains the specified <paramref name="filters"/>.
		/// </summary>
		/// <param name="filters">A collection of <see cref="ISyntaxFilter"/>s to add to the newly-register <see cref="ISyntaxFilter"/> group.</param>
		/// <returns>An <see cref="int"/> that represents the index at which the <see cref="ISyntaxFilter"/> group was registered.</returns>
		/// <exception cref="ArgumentException">
		/// An <see cref="ISyntaxFilter"/> group cannot be <see langword="null"/> or empty. -or-
		/// An <see cref="ISyntaxFilter"/> inside a group cannot be <see langword="null"/>.
		/// </exception>
		public int RegisterFilterGroup(IEnumerable<TFilter> filters)
		{
			int index = _filterGroups.Count;
			_filterGroups.Add((null, ValidateGroup(filters)));
			return index;
		}

		/// <summary>
		/// Registers a new filter group that contains the specified <paramref name="filters"/>.
		/// </summary>
		/// <param name="name">Name to be applied to the <see cref="ISyntaxFilter"/> group.</param>
		/// <param name="filters">A collection of <see cref="ISyntaxFilter"/>s to add to the newly-register <see cref="ISyntaxFilter"/> group.</param>
		/// <returns>An <see cref="int"/> that represents the index at which the <see cref="ISyntaxFilter"/> group was registered.</returns>
		/// <exception cref="ArgumentException">
		/// Name of a filter group cannot be null or whitespace only. -or-
		/// Group with the specified <paramref name="name"/> already defined.
		/// An <see cref="ISyntaxFilter"/> group cannot be <see langword="null"/> or empty. -or-
		/// An <see cref="ISyntaxFilter"/> inside a group cannot be <see langword="null"/>.
		/// </exception>
		public int RegisterFilterGroup(string name, IEnumerable<TFilter> filters)
		{
			ValidateName(name);
			int index = _filterGroups.Count;
			_filterGroups.Add((name, ValidateGroup(filters)));
			return index;
		}

		/// <summary>
		/// Unregisters a <see cref="ISyntaxFilter"/> group.
		/// </summary>
		/// <param name="group">Index of <see cref="ISyntaxFilter"/> group to unregister.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="group"/> was out of range.</exception>
		public void UnregisterFilterGroup(int group)
		{
			_filterGroups.RemoveAt(group);
		}

		/// <summary>
		/// Unregisters a <see cref="ISyntaxFilter"/> group.
		/// </summary>
		/// <param name="name">Name of <see cref="ISyntaxFilter"/> group to unregister.</param>
		/// <returns>An <see cref="int"/> that represents the index at which the <see cref="ISyntaxFilter"/> group was registered. -or- <c>-1</c> if there was no <see cref="ISyntaxFilter"/> group with that <paramref name="name"/>.</returns>
		public int UnregisterFilterGroup(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return -1;
			}

			int index = _filterGroups.FindIndex(g => g.name == name);

			if (index == -1)
			{
				return index;
			}

			UnregisterFilterGroup(index);
			return index;
		}

		/// <summary>
		/// Changes the name of the <see cref="ISyntaxFilter"/> group at the specified <paramref name="index"/>.
		/// </summary>
		/// <param name="index">Index of group to rename.</param>
		/// <param name="newName">New name for the <see cref="ISyntaxFilter"/> group.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> was out of range.</exception>
		/// <exception cref="ArgumentException">Group with the specified <paramref name="newName"/> already defined.</exception>
		public void Rename(int index, string? newName)
		{
			string? name = newName;

			if (string.IsNullOrWhiteSpace(name))
			{
				name = null;
			}
			else if (name != _filterGroups[index].name)
			{
				bool exists = false;

				foreach (string? n in _filterGroups.Select(g => g.name))
				{
					if (n == name)
					{
						if (exists)
						{
							throw Exc_GroupWithNameAlreadyDefined(name!);
						}

						exists = true;
					}
				}
			}

			_filterGroups[index] = (name, _filterGroups[index].list);
		}

		/// <summary>
		/// Changes the name of the specified <see cref="ISyntaxFilter"/> group.
		/// </summary>
		/// <param name="oldName">Current name of the <see cref="ISyntaxFilter"/> to replace.</param>
		/// <param name="newName">New name for the <see cref="ISyntaxFilter"/> group.</param>
		/// <exception cref="ArgumentException">Group with the specified <paramref name="newName"/> already defined. -or-
		/// <see cref="ISyntaxFilter"/> group with the <paramref name="oldName"/> not found.</exception>
		public void Rename(string oldName, string? newName)
		{
			if (string.IsNullOrWhiteSpace(oldName))
			{
				return;
			}

			int index = _filterGroups.FindIndex(g => g.name == oldName);

			if (index == -1)
			{
				throw new ArgumentException($"Group with name '{oldName}' not found!");
			}

			Rename(index, newName);
		}

		/// <summary>
		/// Returns name of the group at the specified <paramref name="index"/>.
		/// </summary>
		/// <param name="index"></param>
		/// <returns>Name of the group at the specified <paramref name="index"/>. -or- <see langword="null"/> if group at the specified index doesn't have a name.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> was out of range.</exception>
		public string? GetName(int index)
		{
			return _filterGroups[index].name;
		}

		/// <summary>
		/// Returns index of a <see cref="ISyntaxFilter"/> group with the specified <paramref name="name"/>.
		/// </summary>
		/// <param name="name"></param>
		/// <returns>An <see cref="int"/> that represents the index of the <see cref="ISyntaxFilter"/> group with the specified <paramref name="name"/>. -or-
		/// -1 if no such <see cref="ISyntaxFilter"/> group was found..</returns>
		public int GetIndex(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return -1;
			}

			return _filterGroups.FindIndex(g => g.name == name);
		}

		/// <summary>
		/// Returns an array that represents the contents of the <see cref="FilterList{TFilter}"/>
		/// </summary>
		public TFilter[][] ToArray()
		{
			int length = _filterGroups.Count;
			TFilter[][] filters = new TFilter[length][];

			for (int i = 0; i < length; i++)
			{
				filters[i] = _filterGroups[i].list.ToArray();
			}

			return filters;
		}

		/// <summary>
		/// Enumerates through the <see cref="FilterList{TFilter}"/>'s groups.
		/// </summary>
		public IEnumerator<TFilter[]> GetEnumerator()
		{
			foreach (List<TFilter> group in _filterGroups.Select(g => g.list))
			{
				yield return group.ToArray();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private static List<TFilter> ValidateGroup(IEnumerable<TFilter> group)
		{
			List<TFilter>? list = group?.ToList();

			if (list is null || list.Count == 0)
			{
				throw new ArgumentException($"An {nameof(TFilter)} group cannot be null or empty.");
			}

			foreach (TFilter filter in list)
			{
				if (filter is null)
				{
					throw new ArgumentException($"An {nameof(TFilter)} inside a group cannot be null.");
				}
			}

			return list;
		}

		private static ArgumentException Exc_GroupWithNameAlreadyDefined(string name)
		{
			return new ArgumentException($"Group with name '{name}' already defined!");
		}

		private static ArgumentException Exc_GroupWithNameNotFound(string name)
		{
			return new ArgumentNullException($"Filter group with name '{name}' not found!");
		}

		private static ArgumentException Exc_NameOfFilterGroupCannotBeNullOrWhitespaceOnly()
		{
			return new ArgumentException("Name of a filter group cannot be null or whitespace only!");
		}

		private void ValidateName(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw Exc_NameOfFilterGroupCannotBeNullOrWhitespaceOnly();
			}

			if (_filterGroups.Select(g => g.name).Any(n => n == name))
			{
				throw Exc_GroupWithNameAlreadyDefined(name);
			}
		}

		private List<TFilter> GetList(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw Exc_NameOfFilterGroupCannotBeNullOrWhitespaceOnly();
			}

			List<TFilter>? list = _filterGroups.Find(g => g.name == name).list;

			if (list is null)
			{
				throw Exc_GroupWithNameNotFound(name);
			}

			return list;
		}
	}
}