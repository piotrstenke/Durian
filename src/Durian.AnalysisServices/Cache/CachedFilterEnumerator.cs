using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Durian.Analysis.Data;
using Durian.Analysis.Filtering;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Cache;

/// <summary>
/// Enumerates through a collection of <see cref="IMemberData"/>s of type <typeparamref name="TContext"/> created by the provided <see cref="ISyntaxValidator{T}"/> or retrieved from a <see cref="CachedData{T}"/>.
/// </summary>
/// <typeparam name="TData">Type of cached data.</typeparam>
/// <typeparam name="TContext">Type of target <see cref="ISyntaxValidationContext"/>.</typeparam>
[DebuggerDisplay("Current = {Current}")]
public struct CachedFilterEnumerator<TData, TContext> : IFilterEnumerator<TContext>, IEnumerator<TData>
	where TData : class, IMemberData
	where TContext : ISyntaxValidationContext
{
	internal readonly CachedData<TData> _cache;

	private readonly IEnumerator<SyntaxNode> _nodes;

	/// <inheritdoc/>
	public readonly ICompilationData Compilation { get; }

	/// <summary>
	/// <typeparamref name="TData"/> at the current position in the enumerator.
	/// </summary>
	public TData? Current { readonly get; private set; }

	/// <inheritdoc/>
	public readonly ISyntaxValidator<TContext> Validator { get; }

	readonly TData IEnumerator<TData>.Current => Current!;
	readonly IMemberData IFilterEnumerator<TContext>.Current => Current!;
	readonly object IEnumerator.Current => Current!;

	/// <summary>
	/// Initializes a new instance of the <see cref="CachedFilterEnumerator{TData, TContext}"/> struct.
	/// </summary>
	/// <param name="compilation">Parent <see cref="ICompilationData"/> of the provided <paramref name="nodes"/>.</param>
	/// <param name="nodes">A collection of <see cref="SyntaxNode"/>s to use to create the <see cref="IMemberData"/>s to enumerate through.</param>
	/// <param name="validator"><see cref="ISyntaxValidator{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
	/// <param name="cache">Container of cached <see cref="IMemberData"/>s.</param>
	public CachedFilterEnumerator(
		ICompilationData compilation,
		IEnumerable<SyntaxNode> nodes,
		ISyntaxValidator<TContext> validator,
		in CachedData<TData> cache
	) : this(compilation, nodes.GetEnumerator(), validator, in cache)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CachedFilterEnumerator{TData, TContext}"/> struct.
	/// </summary>
	/// <param name="compilation">Parent <see cref="ICompilationData"/> of <see cref="SyntaxNode"/>s provided by the <paramref name="provider"/>.</param>
	/// <param name="provider"><see cref="INodeProvider"/> that creates an array of <see cref="SyntaxNode"/>s to be used to create the target <see cref="IMemberData"/>s.</param>
	/// <param name="validator"><see cref="ISyntaxValidator{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
	/// <param name="cache">Container of cached <see cref="IMemberData"/>s.</param>
	public CachedFilterEnumerator(
		ICompilationData compilation,
		INodeProvider provider,
		ISyntaxValidator<TContext> validator,
		in CachedData<TData> cache
	) : this(compilation, provider.GetNodes().GetEnumerator(), validator, in cache)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CachedFilterEnumerator{TData, TContext}"/> struct.
	/// </summary>
	/// <param name="compilation">Parent <see cref="ICompilationData"/> of the provided <paramref name="nodes"/>.</param>
	/// <param name="nodes">An enumerator that iterates through a collection of <see cref="SyntaxNode"/>s.</param>
	/// <param name="validator"><see cref="ISyntaxValidator{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
	/// <param name="cache">Container of cached <see cref="IMemberData"/>s.</param>
	public CachedFilterEnumerator(ICompilationData compilation, IEnumerator<SyntaxNode> nodes, ISyntaxValidator<TContext> validator, in CachedData<TData> cache)
	{
		Validator = validator;
		Compilation = compilation;
		_nodes = nodes;
		Current = default;
		_cache = cache;
	}

	/// <inheritdoc/>
#pragma warning disable RCS1242 // Do not pass non-read-only struct by read-only reference.

	public static explicit operator CachedFilterEnumerator<TData, TContext>(in FilterEnumerator<TContext> a)
	{
		return new CachedFilterEnumerator<TData, TContext>(a.Compilation, a._nodes, a.Validator, CachedData<TData>.Empty);
	}

	/// <inheritdoc/>
	public static explicit operator FilterEnumerator<TContext>(in CachedFilterEnumerator<TData, TContext> a)
	{
		return new FilterEnumerator<TContext>(a.Compilation, a._nodes, a.Validator);
	}

#pragma warning restore RCS1242 // Do not pass non-read-only struct by read-only reference.

	/// <inheritdoc/>
	[MemberNotNullWhen(true, nameof(Current))]
	public bool MoveNext(CancellationToken cancellationToken = default)
	{
		while (_nodes.MoveNext())
		{
			SyntaxNode node = _nodes.Current;

			if (node is null)
			{
				continue;
			}

			if (_cache.TryGetCachedValue(node, out TData? data))
			{
				Current = data;
				return true;
			}

			if (Validator.ValidateAndCreate(new PreValidationContext(node, Compilation, cancellationToken), out IMemberData? member) && member is TData d)
			{
				Current = d;
				return true;
			}
		}

		Current = default;
		return false;
	}

	/// <summary>
	/// Resets the enumerator.
	/// </summary>
	public void Reset()
	{
		_nodes.Reset();
		Current = default;
	}

	readonly void IDisposable.Dispose()
	{
		// Do nothing.
	}

	bool IEnumerator.MoveNext()
	{
		return MoveNext();
	}
}
