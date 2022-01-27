// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.Cache
{
	/// <summary>
	/// Enumerates through a collection of <see cref="IMemberData"/>s of type <typeparamref name="T"/> created by the provided <see cref="INodeValidatorWithDiagnostics{T}"/> or retrieved from a <see cref="CachedData{T}"/> with an option to report diagnostics using a <see cref="IDiagnosticReceiver"/>.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="IMemberData"/> this enumerator can handle.</typeparam>
	[DebuggerDisplay("Current = {Current}")]
	public struct CachedFilterEnumeratorWithDiagnostics<T> : IEnumerator<T> where T : IMemberData
	{
		internal readonly CachedData<T> _cache;

		private readonly CSharpSyntaxNode[] _nodes;

		private int _index;

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
		/// <see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.
		/// </summary>
		public readonly IDiagnosticReceiver DiagnosticReceiver { get; }

		/// <summary>
		/// <see cref="INodeValidatorWithDiagnostics{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.
		/// </summary>
		public readonly INodeValidatorWithDiagnostics<T> Validator { get; }

		readonly T IEnumerator<T>.Current => Current!;
		readonly object IEnumerator.Current => Current!;

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedFilterEnumeratorWithDiagnostics{T}"/> struct.
		/// </summary>
		/// <param name="nodes">An array of <see cref="CSharpSyntaxNode"/>s to use to create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of the provided <paramref name="nodes"/>.</param>
		/// <param name="validator"><see cref="INodeValidatorWithDiagnostics{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="cache">Container of cached <see cref="IMemberData"/>s.</param>
		public CachedFilterEnumeratorWithDiagnostics(CSharpSyntaxNode[] nodes, ICompilationData compilation, INodeValidatorWithDiagnostics<T> validator, IDiagnosticReceiver diagnosticReceiver, in CachedData<T> cache) : this(nodes, compilation, validator, diagnosticReceiver, in cache, default)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedFilterEnumeratorWithDiagnostics{T}"/> struct.
		/// </summary>
		/// <param name="provider"><see cref="INodeProvider"/> that creates an array of <see cref="CSharpSyntaxNode"/>s to be used to create the target <see cref="IMemberData"/>s.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of <see cref="CSharpSyntaxNode"/>s provided by the <paramref name="provider"/>.</param>
		/// <param name="validator"><see cref="INodeValidatorWithDiagnostics{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="cache">Container of cached <see cref="IMemberData"/>s.</param>
		public CachedFilterEnumeratorWithDiagnostics(INodeProvider provider, ICompilationData compilation, INodeValidatorWithDiagnostics<T> validator, IDiagnosticReceiver diagnosticReceiver, in CachedData<T> cache) : this(provider.GetNodes().ToArray(), compilation, validator, diagnosticReceiver, in cache, default)
		{
		}

		internal CachedFilterEnumeratorWithDiagnostics(CSharpSyntaxNode[] nodes, ICompilationData compilation, INodeValidatorWithDiagnostics<T> validator, IDiagnosticReceiver diagnosticReceiver, in CachedData<T> cache, int index)
		{
			Validator = validator;
			_nodes = nodes;
			Compilation = compilation;
			_index = index;
			DiagnosticReceiver = diagnosticReceiver;
			Current = default;
			_cache = cache;
		}

#pragma warning disable RCS1242 // Do not pass non-read-only struct by read-only reference.
		/// <inheritdoc/>
		public static explicit operator CachedFilterEnumeratorWithDiagnostics<T>(in FilterEnumeratorWithDiagnostics<T> a)
		{
			return new CachedFilterEnumeratorWithDiagnostics<T>(a._nodes, a.Compilation, a.Validator, a.DiagnosticReceiver, CachedData<T>.Empty, a._index);
		}

		/// <inheritdoc/>
		public static explicit operator FilterEnumeratorWithDiagnostics<T>(in CachedFilterEnumeratorWithDiagnostics<T> a)
		{
			return new FilterEnumeratorWithDiagnostics<T>(a._nodes, a.Compilation, a.Validator, a.DiagnosticReceiver, a._index);
		}
#pragma warning restore RCS1242 // Do not pass non-read-only struct by read-only reference.

		/// <inheritdoc cref="FilterEnumerator{T}.MoveNext"/>
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

				if (_cache.TryGetCachedValue(node.GetLocation().GetLineSpan(), out T? data) ||
					Validator.ValidateAndCreateWithDiagnostics(DiagnosticReceiver, node, Compilation, out data))
				{
					Current = data!;
					return true;
				}
			}

			Current = default;
			return false;
		}

		/// <inheritdoc cref="FilterEnumerator{T}.Reset"/>
		public void Reset()
		{
			_index = 0;
			Current = default;
		}

		/// <summary>
		/// Converts this <see cref="CachedFilterEnumeratorWithDiagnostics{T}"/> to a new instance of <see cref="CachedFilterEnumerator{T}"/>.
		/// </summary>
		public readonly CachedFilterEnumerator<T> ToBasicCachedEnumerator()
		{
			return new CachedFilterEnumerator<T>(_nodes, Compilation, Validator, in _cache, _index);
		}

		/// <summary>
		/// Converts this <see cref="CachedFilterEnumeratorWithDiagnostics{T}"/> to a new instance of <see cref="FilterEnumerator{T}"/>.
		/// </summary>
		public readonly FilterEnumerator<T> ToBasicEnumerator()
		{
			return new FilterEnumerator<T>(_nodes, Compilation, Validator, _index);
		}

		readonly void IDisposable.Dispose()
		{
			// Do nothing.
		}
	}
}