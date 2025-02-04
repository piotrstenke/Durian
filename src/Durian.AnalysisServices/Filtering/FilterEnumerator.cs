﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Filtering;

/// <summary>
/// Enumerates through a collection of <see cref="IMemberData"/>s of type <typeparamref name="T"/> created by the provided <see cref="ISyntaxValidator{T}"/>.
/// </summary>
/// <typeparam name="T">Type of target <see cref="ISyntaxValidationContext"/>.</typeparam>
[DebuggerDisplay("Current = {Current}")]
public struct FilterEnumerator<T> : IFilterEnumerator<T>, IEnumerator<IMemberData> where T : ISyntaxValidationContext
{
	internal readonly IEnumerator<SyntaxNode> _nodes;

	/// <inheritdoc/>
	public readonly ICompilationData Compilation { get; }

	/// <inheritdoc/>
	public IMemberData? Current { readonly get; private set; }

	/// <inheritdoc/>
	public readonly ISyntaxValidator<T> Validator { get; }

	readonly IMemberData IEnumerator<IMemberData>.Current => Current!;

	readonly object IEnumerator.Current => Current!;

	/// <summary>
	/// Initializes a new instance of the <see cref="FilterEnumerator{T}"/> struct.
	/// </summary>
	/// <param name="compilation">Parent <see cref="ICompilationData"/> of the provided <paramref name="nodes"/>.</param>
	/// <param name="nodes">A collection of <see cref="SyntaxNode"/>s to use to create the <see cref="IMemberData"/>s to enumerate through.</param>
	/// <param name="validator"><see cref="ISyntaxValidator{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
	public FilterEnumerator(
		ICompilationData compilation,
		IEnumerable<SyntaxNode> nodes,
		ISyntaxValidator<T> validator
	) : this(compilation, nodes.GetEnumerator(), validator)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="FilterEnumerator{T}"/> struct.
	/// </summary>
	/// <param name="compilation">Parent <see cref="ICompilationData"/> of <see cref="SyntaxNode"/>s provided by the <paramref name="provider"/>.</param>
	/// <param name="provider"><see cref="INodeProvider"/> that creates an array of <see cref="SyntaxNode"/>s to be used to create the target <see cref="IMemberData"/>s.</param>
	/// <param name="validator"><see cref="ISyntaxValidator{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
	public FilterEnumerator(
		ICompilationData compilation,
		INodeProvider provider,
		ISyntaxValidator<T> validator
	) : this(compilation, provider.GetNodes().GetEnumerator(), validator)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="FilterEnumerator{T}"/> struct.
	/// </summary>
	/// <param name="compilation">Parent <see cref="ICompilationData"/> of the provided <paramref name="nodes"/>.</param>
	/// <param name="nodes">An enumerator that iterates through a collection of <see cref="SyntaxNode"/>s.</param>
	/// <param name="validator"><see cref="ISyntaxValidator{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
	public FilterEnumerator(ICompilationData compilation, IEnumerator<SyntaxNode> nodes, ISyntaxValidator<T> validator)
	{
		Validator = validator;
		Compilation = compilation;
		_nodes = nodes;
		Current = default;
	}

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

			if (Validator.ValidateAndCreate(new PreValidationContext(node, Compilation, cancellationToken), out IMemberData? data))
			{
				Current = data;
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
		Current = default;
		_nodes.Reset();
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
