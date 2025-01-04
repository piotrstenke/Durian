using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Durian.Analysis.Filtration;

/// <summary>
/// A collection of <see cref="ISyntaxFilter"/>s.
/// </summary>
/// <typeparam name="TFilter">Type of <see cref="ISyntaxFilter"/> this <see cref="FilterGroup{TFilter}"/> can store.</typeparam>
[DebuggerDisplay("Name = {_name ?? string.Empty}, {_filters}")]
public class FilterGroup<TFilter> : ICollection<TFilter>, IFilterGroup<TFilter> where TFilter : ISyntaxFilter
{
	internal List<FilterContainer<TFilter>> _containers;

	private readonly List<TFilter> _filters;

	private string? _name;

	/// <inheritdoc/>
	public int Count => _filters.Count;

	/// <inheritdoc/>
	[MemberNotNullWhen(true, nameof(Name))]
	public bool HasName => Name is not null;

	/// <inheritdoc/>
	public bool IsSealed { get; private set; }

	/// <inheritdoc/>
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

				if (_containers is not null && _containers.Any(p => p.ContainsGroup(value)))
				{
					throw new InvalidOperationException("Filter group with the specified name already defined in the parent container!");
				}
			}

			_name = value;
		}
	}

	bool ICollection<TFilter>.IsReadOnly => IsSealed;

	bool ISealable.CanBeSealed => !IsSealed;

	bool ISealable.CanBeUnsealed => IsSealed;

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

	/// <inheritdoc/>
	public void AddFilter(TFilter filter)
	{
		ThrowIfSealed();

		if (filter is null)
		{
			throw new ArgumentNullException(nameof(filter));
		}

		_filters.Add(filter);
	}

	/// <inheritdoc/>
	public void AddFilters(IEnumerable<TFilter> filters)
	{
		ThrowIfSealed();

		if (filters is null)
		{
			throw new ArgumentNullException(nameof(filters));
		}

		_filters.AddRange(_filters);
	}

	/// <inheritdoc/>
	public void Clear()
	{
		ThrowIfSealed();
		_filters.Clear();
	}

	/// <inheritdoc/>
	public bool ContainsFilter(TFilter filter)
	{
		return _filters.Contains(filter);
	}

	/// <inheritdoc/>
	public IEnumerator<TFilter> GetEnumerator()
	{
		return _filters.GetEnumerator();
	}

	/// <inheritdoc/>
	public TFilter GetFilter(int index)
	{
		return _filters[index];
	}

	/// <inheritdoc/>
	public TFilter[] GetFilters(int index, int count)
	{
		return _filters.GetRange(index, count).ToArray();
	}

	/// <inheritdoc/>
	public void RemoveFilter(int index)
	{
		ThrowIfSealed();
		_filters.RemoveAt(index);
	}

	/// <inheritdoc/>
	public bool RemoveFilter(TFilter filter)
	{
		ThrowIfSealed();
		return _filters.Remove(filter);
	}

	/// <inheritdoc/>
	public void RemoveFilters(int index, int count)
	{
		ThrowIfSealed();
		_filters.RemoveRange(index, count);
	}

	/// <inheritdoc/>
	public bool Seal()
	{
		if (IsSealed)
		{
			return false;
		}

		IsSealed = true;
		return true;
	}

	/// <inheritdoc/>
	public TFilter[] ToArray()
	{
		return _filters.ToArray();
	}

	/// <inheritdoc/>
	public bool Unseal()
	{
		if (!IsSealed)
		{
			return false;
		}

		IsSealed = false;
		return true;
	}

	void ICollection<TFilter>.Add(TFilter item)
	{
		AddFilter(item);
	}

	bool ICollection<TFilter>.Contains(TFilter item)
	{
		return ContainsFilter(item);
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
