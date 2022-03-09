// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Durian.Analysis
{
	/// <summary>
	/// A collection of <see cref="ISyntaxFilter"/>s.
	/// </summary>
	/// <typeparam name="TFilter">Type of <see cref="ISyntaxFilter"/> this <see cref="FilterGroup{TFilter}"/> can store.</typeparam>
	[DebuggerDisplay("Name = {_name ?? string.Empty}, {_filters}")]
	public class FilterGroup<TFilter> : ICollection<TFilter>, IFilterGroup where TFilter : ISyntaxFilter
	{
		internal List<FilterContainer<TFilter>> _containers;

		private readonly List<TFilter> _filters;

		private string? _name;

		/// <summary>
		/// Number of <typeparamref name="TFilter"/>s in this <see cref="FilterGroup{TFilter}"/>.
		/// </summary>
		public int Count => _filters.Count;

		/// <summary>
		/// Determines whether this <see cref="FilterGroup{TFilter}"/> has a name.
		/// </summary>
		[MemberNotNullWhen(true, nameof(Name))]
		public bool HasName => Name is not null;

		/// <summary>
		/// Determines whether this <see cref="FilterGroup{TFilter}"/> is sealed by calling the <see cref="Seal"/> method.
		/// </summary>
		public bool IsSealed { get; private set; }

		/// <summary>
		/// Name of this <see cref="FilterGroup{TFilter}"/>.
		/// </summary>
		/// <exception cref="ArgumentException"><see cref="Name"/> cannot be empty or white space only.</exception>
		/// <exception cref="InvalidOperationException"><see cref="FilterGroup{TFilter}"/> with the specified name already defined in parent container.</exception>
		public string? Name
		{
			get => _name;
			set
			{
				if (value is not null)
				{
					if (string.IsNullOrWhiteSpace(value))
					{
						throw new ArgumentException($"{nameof(Name)} cannot be empty or white space only!");
					}

					if (_containers is not null && _containers.Any(p => p.ContainsFilterGroup(value)))
					{
						throw new InvalidOperationException("Filter group with the specified name already defined in the parent container!");
					}
				}

				_name = value;
			}
		}

		bool ICollection<TFilter>.IsReadOnly => IsSealed;

		/// <inheritdoc cref="GetFilter(int)"/>
		public TFilter this[int index] => GetFilter(index);

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterGroup{TFilter}"/> class.
		/// </summary>
		public FilterGroup()
		{
			_filters = new();
			_containers = new();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterGroup{TFilter}"/> class.
		/// </summary>
		/// <param name="name">Name of this <see cref="FilterGroup{TFilter}"/>.</param>
		/// <exception cref="ArgumentException">A filter group name cannot be empty or white space only.</exception>
		public FilterGroup(string? name)
		{
			_filters = new();
			_containers = new();
			Name = name;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterGroup{TFilter}"/> class.
		/// </summary>
		/// <param name="filters">A collection of <typeparamref name="TFilter"/>s to add to the group.</param>
		/// <exception cref="ArgumentNullException" ><paramref name="filters"/> is <see langword="null"/>.</exception>
		public FilterGroup(IEnumerable<TFilter> filters)
		{
			if (filters is null)
			{
				throw new ArgumentNullException(nameof(filters));
			}

			_filters = new(filters);
			_containers = new();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterGroup{TFilter}"/> class.
		/// </summary>
		/// <param name="name">Name of this <see cref="FilterGroup{TFilter}"/>.</param>
		/// <param name="filters">A collection of <typeparamref name="TFilter"/>s to add to the group.</param>
		/// <exception cref="ArgumentNullException"><paramref name="filters"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">A filter group name cannot be empty or white space only.</exception>
		public FilterGroup(string? name, IEnumerable<TFilter> filters) : this(filters)
		{
			Name = name;
		}

		/// <summary>
		/// Adds the specified <paramref name="filter"/> to this <see cref="FilterGroup{TFilter}"/>.
		/// </summary>
		/// <param name="filter"><typeparamref name="TFilter"/> to add to the <see cref="FilterGroup{TFilter}"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="filter"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="FilterGroup{TFilter}"/> is sealed. -or- Parent <see cref="FilterContainer{TFilter}"/> is sealed.</exception>
		public void AddFilter(TFilter filter)
		{
			ThrowIfSealed();

			if (filter is null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			_filters.Add(filter);
		}

		/// <summary>
		/// Adds the collection of <paramref name="filters"/> to this <see cref="FilterGroup{TFilter}"/>.
		/// </summary>
		/// <param name="filters">A collection of <typeparamref name="TFilter"/>s to add to the <see cref="FilterGroup{TFilter}"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="filters"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="FilterGroup{TFilter}"/> is sealed. -or- Parent <see cref="FilterContainer{TFilter}"/> is sealed.</exception>
		public void AddFilters(IEnumerable<TFilter> filters)
		{
			ThrowIfSealed();

			if (filters is null)
			{
				throw new ArgumentNullException(nameof(filters));
			}

			_filters.AddRange(_filters);
		}

		/// <summary>
		/// Removes all <typeparamref name="TFilter"/>s from the <see cref="FilterGroup{TFilter}"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException"><see cref="FilterGroup{TFilter}"/> is sealed. -or- Parent <see cref="FilterContainer{TFilter}"/> is sealed.</exception>
		public void Clear()
		{
			ThrowIfSealed();
			_filters.Clear();
		}

		/// <summary>
		/// Checks if the <see cref="FilterGroup{TFilter}"/> contains the specified <paramref name="filter"/>.
		/// </summary>
		/// <param name="filter"><typeparamref name="TFilter"/> to add to the <see cref="FilterGroup{TFilter}"/>.</param>
		public bool ContainsFilter(TFilter filter)
		{
			return _filters.Contains(filter);
		}

		/// <inheritdoc/>
		public IEnumerator<TFilter> GetEnumerator()
		{
			return _filters.GetEnumerator();
		}

		/// <summary>
		/// Returns a <typeparamref name="TFilter"/> at the specified <paramref name="index"/>.
		/// </summary>
		/// <param name="index">Index to get the <typeparamref name="TFilter"/> at.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than <c>0</c>. -or- <paramref name="index"/> is equal to or greater than <see cref="Count"/>.</exception>
		public TFilter GetFilter(int index)
		{
			return _filters[index];
		}

		/// <summary>
		/// Returns an array representing a range of <typeparamref name="TFilter"/>s in the <see cref="FilterGroup{TFilter}"/>.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than <c>0</c>. -or- <paramref name="index"/> is equal to or greater than <see cref="Count"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="index"/> and <paramref name="count"/> do not denote a valid range of elements in the <see cref="FilterGroup{TFilter}"/>.</exception>
		public TFilter[] GetFilters(int index, int count)
		{
			return _filters.GetRange(index, count).ToArray();
		}

		/// <summary>
		/// Removes a <typeparamref name="TFilter"/> at the specified <paramref name="index"/>.
		/// </summary>
		/// <param name="index"></param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than <c>0</c>. -or- <paramref name="index"/> is equal to or greater than <see cref="Count"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="FilterGroup{TFilter}"/> is sealed. -or- Parent <see cref="FilterContainer{TFilter}"/> is sealed.</exception>
		public void RemoveFilter(int index)
		{
			ThrowIfSealed();
			_filters.RemoveAt(index);
		}

		/// <summary>
		/// Removes the first occurrence of the specified <paramref name="filter"/> from the <see cref="FilterGroup{TFilter}"/>.
		/// </summary>
		/// <param name="filter"></param>
		/// <returns><see langword="true"/> if <paramref name="filter"/> was present in the <see cref="FilterGroup{TFilter}"/>, <see langword="false"/> otherwise.</returns>
		/// <exception cref="InvalidOperationException"><see cref="FilterGroup{TFilter}"/> is sealed. -or- Parent <see cref="FilterContainer{TFilter}"/> is sealed.</exception>
		public bool RemoveFilter(TFilter filter)
		{
			ThrowIfSealed();
			return _filters.Remove(filter);
		}

		/// <summary>
		/// Removes a range of <typeparamref name="TFilter"/>s from the <see cref="FilterGroup{TFilter}"/>.
		/// </summary>
		/// <param name="index">Index at which the removal should be started.</param>
		/// <param name="count">Number of <typeparamref name="TFilter"/>s to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than <c>0</c>. -or- <paramref name="count"/> is less than <c>0</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="index"/> and <paramref name="count"/> do not denote a valid range of elements in the <see cref="FilterGroup{TFilter}"/>.</exception>
		/// <exception cref="InvalidOperationException"><see cref="FilterGroup{TFilter}"/> is sealed. -or- Parent <see cref="FilterContainer{TFilter}"/> is sealed.</exception>
		public void RemoveFilters(int index, int count)
		{
			ThrowIfSealed();
			_filters.RemoveRange(index, count);
		}

		/// <summary>
		/// Prohibits this <see cref="FilterGroup{TFilter}"/> from further modifications until <see cref="Unseal"/> is called.
		/// </summary>
		public void Seal()
		{
			IsSealed = true;
		}

		/// <summary>
		/// Converts this <see cref="FilterGroup{TFilter}"/> into an array of <typeparamref name="TFilter"/>s.
		/// </summary>
		public TFilter[] ToArray()
		{
			return _filters.ToArray();
		}

		/// <summary>
		/// Allows this <see cref="FilterGroup{TFilter}"/> to be modified after the <see cref="Seal"/> method was called.
		/// </summary>
		public void Unseal()
		{
			IsSealed = false;
		}

		void ICollection<TFilter>.Add(TFilter item)
		{
			AddFilter(item);
		}

		void IFilterGroup.AddFilter(ISyntaxFilter filter)
		{
			AddFilter((TFilter)filter);
		}

		void IFilterGroup.AddFilters(IEnumerable<ISyntaxFilter> filters)
		{
			if (filters is IEnumerable<TFilter> f)
			{
				AddFilters(f);
			}
			else
			{
				AddFilters(filters.Cast<TFilter>());
			}
		}

		bool ICollection<TFilter>.Contains(TFilter item)
		{
			return ContainsFilter(item);
		}

		bool IFilterGroup.ContainsFilter(ISyntaxFilter filter)
		{
			return ContainsFilter((TFilter)filter);
		}

		void ICollection<TFilter>.CopyTo(TFilter[] array, int arrayIndex)
		{
			_filters.CopyTo(array, arrayIndex);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		bool ICollection<TFilter>.Remove(TFilter item)
		{
			return RemoveFilter(item);
		}

		ISyntaxFilter[] IFilterGroup.ToArray()
		{
			TFilter[] filters = ToArray();

			if (filters is ISyntaxFilter[] f)
			{
				return f;
			}

			ISyntaxFilter[] array = new ISyntaxFilter[filters.Length];
			filters.CopyTo(array, 0);
			return array;
		}

		private void ThrowIfSealed()
		{
			if (IsSealed)
			{
				throw new InvalidOperationException("Filter group is sealed!");
			}

			if (_containers.Any(p => p.IsSealed))
			{
				throw new InvalidOperationException("Parent filter container is sealed!");
			}
		}
	}
}