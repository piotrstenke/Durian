// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
		internal readonly CSharpSyntaxNode[] _nodes;

		internal int _index;

		/// <summary>
		/// Parent <see cref="ICompilationData"/> of the provided <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		public readonly ICompilationData Compilation { get; }

		/// <summary>
		/// Number of <see cref="CSharpSyntaxNode"/>s in the collection that this enumerator enumerates on.
		/// </summary>
		public readonly int Count => _nodes.Length;

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
		/// <param name="nodes">An array of <see cref="CSharpSyntaxNode"/>s to use to create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of the provided <paramref name="nodes"/>.</param>
		/// <param name="validator"><see cref="INodeValidator{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
		public FilterEnumerator(CSharpSyntaxNode[] nodes, ICompilationData compilation, INodeValidator<T> validator) : this(nodes, compilation, validator, default)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterEnumerator{T}"/> struct.
		/// </summary>
		/// <param name="provider"><see cref="INodeProvider"/> that creates an array of <see cref="CSharpSyntaxNode"/>s to be used to create the target <see cref="IMemberData"/>s.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of <see cref="CSharpSyntaxNode"/>s provided by the <paramref name="provider"/>.</param>
		/// <param name="validator"><see cref="INodeValidator{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
		public FilterEnumerator(INodeProvider provider, ICompilationData compilation, INodeValidator<T> validator) : this(provider.GetNodes().ToArray(), compilation, validator, default)
		{
		}

		internal FilterEnumerator(CSharpSyntaxNode[] nodes, ICompilationData compilation, INodeValidator<T> validator, int index)
		{
			Validator = validator;
			Compilation = compilation;
			_nodes = nodes;
			_index = index;
			Current = default;
		}

		/// <summary>
		/// Creates and validates the next <see cref="IMemberData"/>.
		/// </summary>
		/// <returns><see langword="true"/> is the <see cref="IMemberData"/> is valid, <see langword="false"/> otherwise.</returns>
		[MemberNotNullWhen(true, nameof(Current))]
		public bool MoveNext()
		{
			int length = _nodes.Length;

			while (_index < length)
			{
				CSharpSyntaxNode node = _nodes[_index];
				_index++;

				if (node is null)
				{
					continue;
				}

				if (Validator.ValidateAndCreate(node, Compilation, out T? data))
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
			_index = 0;
			Current = default;
		}

		readonly void IDisposable.Dispose()
		{
			// Do nothing.
		}
	}
}