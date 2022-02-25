// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis
{
	/// <summary>
	/// Enumerates through a collection of <see cref="IMemberData"/>s of type <typeparamref name="T"/> created by the provided <see cref="INodeValidator{T}"/>.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="IMemberData"/> this enumerator can handle.</typeparam>
	[DebuggerDisplay("Current = {Current}")]
	public struct FilterEnumerator<T> : IEnumerator<T> where T : IMemberData
	{
		internal readonly IEnumerator<CSharpSyntaxNode> _nodes;

		/// <summary>
		/// Parent <see cref="ICompilationData"/> of the provided <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		public readonly ICompilationData Compilation { get; }

		/// <summary>
		/// Current <see cref="IMemberData"/>.
		/// </summary>
		public T? Current { readonly get; private set; }

		/// <summary>
		/// <see cref="INodeValidator{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.
		/// </summary>
		public readonly INodeValidator<T> Validator { get; }

		readonly T IEnumerator<T>.Current => Current!;
		readonly object IEnumerator.Current => Current!;

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterEnumerator{T}"/> struct.
		/// </summary>
		/// <param name="nodes">A collection of <see cref="CSharpSyntaxNode"/>s to use to create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of the provided <paramref name="nodes"/>.</param>
		/// <param name="validator"><see cref="INodeValidator{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
		public FilterEnumerator(
			IEnumerable<CSharpSyntaxNode> nodes,
			ICompilationData compilation,
			INodeValidator<T> validator
		) : this(nodes.GetEnumerator(), compilation, validator)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterEnumerator{T}"/> struct.
		/// </summary>
		/// <param name="provider"><see cref="INodeProvider"/> that creates an array of <see cref="CSharpSyntaxNode"/>s to be used to create the target <see cref="IMemberData"/>s.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of <see cref="CSharpSyntaxNode"/>s provided by the <paramref name="provider"/>.</param>
		/// <param name="validator"><see cref="INodeValidator{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
		public FilterEnumerator(
			INodeProvider provider,
			ICompilationData compilation,
			INodeValidator<T> validator
		) : this(provider.GetNodes().GetEnumerator(), compilation, validator)
		{
		}

		internal FilterEnumerator(IEnumerator<CSharpSyntaxNode> nodes, ICompilationData compilation, INodeValidator<T> validator)
		{
			Validator = validator;
			Compilation = compilation;
			_nodes = nodes;
			Current = default;
		}

		/// <summary>
		/// Creates and validates the next <see cref="IMemberData"/>.
		/// </summary>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> is the <see cref="IMemberData"/> is valid, <see langword="false"/> otherwise.</returns>
		[MemberNotNullWhen(true, nameof(Current))]
		public bool MoveNext(CancellationToken cancellationToken = default)
		{
			while (_nodes.MoveNext())
			{
				CSharpSyntaxNode node = _nodes.Current;

				if (node is null)
				{
					continue;
				}

				if (Validator.ValidateAndCreate(node, Compilation, out T? data, cancellationToken))
				{
					Current = data;
					return true;
				}
			}

			Current = default;
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			return MoveNext();
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
	}
}