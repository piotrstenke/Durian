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

namespace Durian.Analysis.Cache
{
	/// <summary>
	/// Enumerates through a collection of <see cref="IMemberData"/>s of type <typeparamref name="T"/> created by the provided <see cref="INodeValidator{T}"/> or retrieved from a <see cref="CachedData{T}"/>.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="IMemberData"/> this enumerator can handle.</typeparam>
	[DebuggerDisplay("Current = {Current}")]
	public struct CachedFilterEnumerator<T> : IEnumerator<T> where T : IMemberData
	{
		internal readonly CachedData<T> _cache;
		private readonly CSharpSyntaxNode[] _nodes;
		private int _index;

		/// <inheritdoc cref="FilterEnumerator{T}.Compilation"/>
		public readonly ICompilationData Compilation { get; }

		/// <summary>
		/// Number of <see cref="CSharpSyntaxNode"/>s in the collection that this enumerator enumerates on.
		/// </summary>
		public readonly int Count => _nodes.Length;

		/// <inheritdoc cref="FilterEnumerator{T}.Current"/>
		public T? Current { readonly get; private set; }

		/// <inheritdoc cref="FilterEnumerator{T}.Validator"/>
		public readonly INodeValidator<T> Validator { get; }

		readonly T IEnumerator<T>.Current => Current!;

		readonly object IEnumerator.Current => Current!;

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterEnumerator{T}"/> struct.
		/// </summary>
		/// <param name="nodes">An array of <see cref="CSharpSyntaxNode"/>s to use to create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of the provided <paramref name="nodes"/>.</param>
		/// <param name="validator"><see cref="INodeValidator{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="cache">Container of cached <see cref="IMemberData"/>s.</param>
		public CachedFilterEnumerator(CSharpSyntaxNode[] nodes, ICompilationData compilation, INodeValidator<T> validator, in CachedData<T> cache) : this(nodes, compilation, validator, in cache, default)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterEnumerator{T}"/> struct.
		/// </summary>
		/// <param name="provider"><see cref="INodeProvider"/> that creates an array of <see cref="CSharpSyntaxNode"/>s to be used to create the target <see cref="IMemberData"/>s.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of <see cref="CSharpSyntaxNode"/>s provided by the <paramref name="provider"/>.</param>
		/// <param name="validator"><see cref="INodeValidator{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="cache">Container of cached <see cref="IMemberData"/>s.</param>
		public CachedFilterEnumerator(INodeProvider provider, ICompilationData compilation, INodeValidator<T> validator, in CachedData<T> cache) : this(provider.GetNodes().ToArray(), compilation, validator, in cache, default)
		{
		}

		internal CachedFilterEnumerator(CSharpSyntaxNode[] nodes, ICompilationData compilation, INodeValidator<T> validator, in CachedData<T> cache, int index)
		{
			Validator = validator;
			Compilation = compilation;
			_nodes = nodes;
			_index = index;
			Current = default;
			_cache = cache;
		}

#pragma warning disable RCS1242 // Do not pass non-read-only struct by read-only reference.

		/// <inheritdoc/>
		public static explicit operator CachedFilterEnumerator<T>(in FilterEnumerator<T> a)
		{
			return new CachedFilterEnumerator<T>(a._nodes, a.Compilation, a.Validator, CachedData<T>.Empty, a._index);
		}

		/// <inheritdoc/>
		public static explicit operator FilterEnumerator<T>(in CachedFilterEnumerator<T> a)
		{
			return new FilterEnumerator<T>(a._nodes, a.Compilation, a.Validator, a._index);
		}

#pragma warning restore RCS1242 // Do not pass non-read-only struct by read-only reference.

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

				if (_cache.TryGetCachedValue(node.GetLocation().GetLineSpan(), out T? data) || Validator.ValidateAndCreate(node, Compilation, out data))
				{
					Current = data!;
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
