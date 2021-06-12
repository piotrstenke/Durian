// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Durian.Generator.Data;
using Durian.Generator.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Generator.Cache
{
	/// <summary>
	/// Enumerates through a collection of <see cref="IMemberData"/>s of type <typeparamref name="T"/> created by the provided <see cref="INodeValidatorWithDiagnostics{T}"/> or retrieved from a <see cref="CachedData{T}"/> with an option to log diagnostics using a <see cref="INodeDiagnosticReceiver"/>.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="IMemberData"/> this enumerator can handle.</typeparam>
	[DebuggerDisplay("Current = {Current}")]
	public struct CachedLoggableFilterEnumerator<T> : IEnumerator<T> where T : IMemberData
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

		readonly T IEnumerator<T>.Current => Current!;

		readonly object IEnumerator.Current => Current!;

		/// <summary>
		/// <see cref="IHintNameProvider"/> that creates hint names for the <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		public readonly IHintNameProvider HintNameProvider { get; }

		/// <summary>
		/// <see cref="INodeDiagnosticReceiver"/> that writes the reported <see cref="Diagnostic"/>s into a log file or buffer.
		/// </summary>
		public readonly INodeDiagnosticReceiver LogReceiver { get; }

		/// <summary>
		/// <see cref="INodeValidatorWithDiagnostics{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.
		/// </summary>
		public readonly INodeValidatorWithDiagnostics<T> Validator { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedLoggableFilterEnumerator{T}"/> struct.
		/// </summary>
		/// <param name="nodes">An array of <see cref="CSharpSyntaxNode"/>s to use to create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="validator"><see cref="INodeValidatorWithDiagnostics{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of the provided <paramref name="nodes"/>.</param>
		/// <param name="logReceiver"><see cref="INodeDiagnosticReceiver"/> that writes the reported <see cref="Diagnostic"/>s into a log file or buffer.</param>
		/// <param name="hintNameProvider"><see cref="IHintNameProvider"/> that creates hint names for the <paramref name="nodes"/>.</param>
		/// <param name="cache">Container of cached <see cref="IMemberData"/>s.</param>
		public CachedLoggableFilterEnumerator(CSharpSyntaxNode[] nodes, ICompilationData compilation, INodeValidatorWithDiagnostics<T> validator, INodeDiagnosticReceiver logReceiver, IHintNameProvider hintNameProvider, in CachedData<T> cache) : this(nodes, compilation, validator, logReceiver, hintNameProvider, in cache, default)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedLoggableFilterEnumerator{T}"/> struct.
		/// </summary>
		/// <param name="provider"><see cref="INodeProvider"/> that creates an array of <see cref="CSharpSyntaxNode"/>s to be used to create the target <see cref="IMemberData"/>s.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of <see cref="CSharpSyntaxNode"/>s provided by the <paramref name="provider"/>.</param>
		/// <param name="validator"><see cref="INodeValidatorWithDiagnostics{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="logReceiver"><see cref="INodeDiagnosticReceiver"/> that writes the reported <see cref="Diagnostic"/>s into a log file or buffer.</param>
		/// <param name="hintNameProvider"><see cref="IHintNameProvider"/> that creates hint names for the <see cref="CSharpSyntaxNode"/>s provided by the <paramref name="provider"/>.</param>
		/// <param name="cache">Container of cached <see cref="IMemberData"/>s.</param>
		public CachedLoggableFilterEnumerator(INodeProvider provider, ICompilationData compilation, INodeValidatorWithDiagnostics<T> validator, INodeDiagnosticReceiver logReceiver, IHintNameProvider hintNameProvider, in CachedData<T> cache) : this(provider.GetNodes().ToArray(), compilation, validator, logReceiver, hintNameProvider, in cache, default)
		{
		}

		internal CachedLoggableFilterEnumerator(CSharpSyntaxNode[] nodes, ICompilationData compilation, INodeValidatorWithDiagnostics<T> validator, INodeDiagnosticReceiver logReceiver, IHintNameProvider hintNameProvider, in CachedData<T> cache, int index)
		{
			Validator = validator;
			Compilation = compilation;
			LogReceiver = logReceiver;
			HintNameProvider = hintNameProvider;
			_nodes = nodes;
			_index = index;
			_cache = cache;
			Current = default;
		}

		/// <inheritdoc/>
#pragma warning disable RCS1242 // Do not pass non-read-only struct by read-only reference.

		public static explicit operator CachedLoggableFilterEnumerator<T>(in LoggableFilterEnumerator<T> a)
		{
			return new CachedLoggableFilterEnumerator<T>(a._nodes, a.Compilation, a.Validator, a.LogReceiver, a.HintNameProvider, CachedData<T>.Empty, a._index);
		}

		/// <inheritdoc/>
		public static explicit operator LoggableFilterEnumerator<T>(in CachedLoggableFilterEnumerator<T> a)
		{
			return new LoggableFilterEnumerator<T>(a._nodes, a.Compilation, a.Validator, a.LogReceiver, a.HintNameProvider, a._index);
		}

#pragma warning restore RCS1242 // Do not pass non-read-only struct by read-only reference.

		readonly void IDisposable.Dispose()
		{
			// Do nothing.
		}

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

				if (_cache.TryGetCachedValue(node.GetLocation().GetLineSpan(), out T? data))
				{
					Current = data!;
					return true;
				}

				if (!Validator.GetValidationData(node, Compilation, out SemanticModel? semanticModel, out ISymbol? symbol))
				{
					continue;
				}

				string fileName = HintNameProvider.GetFileName(symbol);
				LogReceiver.SetTargetNode(node, fileName);
				bool isValid = Validator.ValidateAndCreateWithDiagnostics(LogReceiver, node, Compilation, semanticModel, symbol, out data);

				if (LogReceiver.Count > 0)
				{
					LogReceiver.Push();
					HintNameProvider.Success();
				}

				if (isValid)
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
		/// Converts this <see cref="CachedLoggableFilterEnumerator{T}"/> to a new instance of <see cref="CachedFilterEnumerator{T}"/>.
		/// </summary>
		public readonly CachedFilterEnumerator<T> ToBasicCachedEnumerator()
		{
			return new CachedFilterEnumerator<T>(_nodes, Compilation, Validator, in _cache, _index);
		}

		/// <summary>
		/// Converts this <see cref="CachedLoggableFilterEnumerator{T}"/> to a new instance of <see cref="FilterEnumerator{T}"/>.
		/// </summary>
		public readonly FilterEnumerator<T> ToBasicEnumerator()
		{
			return new FilterEnumerator<T>(_nodes, Compilation, Validator, _index);
		}

#pragma warning disable RCS1242 // Do not pass non-read-only struct by read-only reference.
#pragma warning restore RCS1242 // Do not pass non-read-only struct by read-only reference.
	}
}
