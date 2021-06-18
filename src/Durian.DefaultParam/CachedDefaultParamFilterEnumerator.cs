// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Durian.Analysis.Cache;
using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// Enumerates through <see cref="IDefaultParamTarget"/>s returned by a <see cref="IDefaultParamFilter"/> or retrieved from a <see cref="CachedData{T}"/>.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="IDefaultParamTarget"/> this enumerator supports.</typeparam>
	[DebuggerDisplay("Current = {Current}")]
	public struct CachedDefaultParamFilterEnumerator<T> : IEnumerator<T> where T : class, IDefaultParamTarget
	{
		private readonly CachedData<T> _cache;
		private readonly CSharpSyntaxNode[] _nodes;
		private int _index;

		/// <summary>
		/// Parent <see cref="DefaultParamCompilationData"/> of the provided <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		public readonly DefaultParamCompilationData Compilation => Filter.Generator.TargetCompilation!;

		/// <summary>
		/// Number of <see cref="CSharpSyntaxNode"/>s in the collection that this enumerator enumerates on.
		/// </summary>
		public readonly int Count => _nodes.Length;

		/// <summary>
		/// Current <see cref="IDefaultParamTarget"/>.
		/// </summary>
		public T? Current { readonly get; private set; }

		/// <summary>
		/// <see cref="INodeDiagnosticReceiver"/> that writes the reported <see cref="Diagnostic"/>s into a log file or buffer.
		/// </summary>
		public readonly INodeDiagnosticReceiver DiagnosticReceiver { get; }

		/// <summary>
		/// <see cref="IDefaultParamFilter{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.
		/// </summary>
		public readonly IDefaultParamFilter<T> Filter { get; }

		readonly T IEnumerator<T>.Current => Current!;

		readonly object IEnumerator.Current => Current!;

		/// <summary>
		/// <see cref="IHintNameProvider"/> that creates hint names for the <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		public readonly IHintNameProvider HintNameProvider => Filter.HintNameProvider;

		/// <inheritdoc cref="CachedDefaultParamFilterEnumerator(IDefaultParamFilter{T}, INodeDiagnosticReceiver, in CachedData{T})"/>
		public CachedDefaultParamFilterEnumerator(IDefaultParamFilter<T> filter, in CachedData<T> cache) : this(filter, filter.Generator.LogReceiver, in cache, default)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedDefaultParamFilterEnumerator{T}"/> struct.
		/// </summary>
		/// <param name="filter"><see cref="IDefaultParamFilter{T}"/> that creates the <see cref="IDefaultParamTarget"/>s to enumerate through.</param>
		/// <param name="diagnosticReceiver"><see cref="INodeDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="cache">Container of cached <see cref="IDefaultParamTarget"/>s.</param>
		public CachedDefaultParamFilterEnumerator(IDefaultParamFilter<T> filter, INodeDiagnosticReceiver diagnosticReceiver, in CachedData<T> cache) : this(filter, diagnosticReceiver, in cache, default)
		{
		}

		internal CachedDefaultParamFilterEnumerator(IDefaultParamFilter<T> filter, INodeDiagnosticReceiver diagnosticReceiver, in CachedData<T> cache, int index)
		{
			Filter = filter;
			DiagnosticReceiver = diagnosticReceiver;
			_nodes = filter.GetNodes().ToArray();
			_index = index;
			_cache = cache;
			Current = default;
		}

		/// <inheritdoc/>
		public static explicit operator CachedDefaultParamFilterEnumerator<T>(DefaultParamFilterEnumerator<T> a)
		{
			return new CachedDefaultParamFilterEnumerator<T>(a.Filter, a.DiagnosticReceiver, CachedData<T>.Empty, a._index);
		}

		/// <inheritdoc/>
		public static explicit operator DefaultParamFilterEnumerator<T>(CachedDefaultParamFilterEnumerator<T> a)
		{
			return new DefaultParamFilterEnumerator<T>(a.Filter, a.DiagnosticReceiver, a._index);
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

				if (!Filter.GetValidationData(node, Compilation, out SemanticModel? semanticModel, out ISymbol? symbol, out TypeParameterContainer typeParameters))
				{
					continue;
				}

				string fileName = HintNameProvider.GetFileName(symbol);
				DiagnosticReceiver.SetTargetNode(node, fileName);
				bool isValid = Filter.ValidateAndCreateWithDiagnostics(DiagnosticReceiver, node, Compilation, semanticModel, symbol, in typeParameters, out data);

				if (DiagnosticReceiver.Count > 0)
				{
					DiagnosticReceiver.Push();
					HintNameProvider.Success();
				}

				if (isValid)
				{
					Current = data!;
					return true;
				}
			}

			Current = null;
			return false;
		}

		/// <inheritdoc cref="FilterEnumerator{T}.Reset"/>
		public void Reset()
		{
			_index = 0;
			Current = null;
		}

		readonly void IDisposable.Dispose()
		{
			// Do nothing.
		}
	}
}
