// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Durian.Analysis.Data;
using Durian.Analysis.Filters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.Cache
{
	/// <summary>
	/// Enumerates through a collection of <see cref="IMemberData"/>s of type <typeparamref name="TContext"/> created by the provided <see cref="ISyntaxValidator{T}"/> or retrieved from a <see cref="CachedData{T}"/> with an option to report diagnostics using a <see cref="IDiagnosticReceiver"/>.
	/// </summary>
	/// <typeparam name="TData">Type of cached data.</typeparam>
	/// <typeparam name="TContext">Type of target <see cref="ISyntaxValidatorContext"/>.</typeparam>
	[DebuggerDisplay("Current = {Current}")]
	public struct CachedFilterEnumeratorWithDiagnostics<TData, TContext> : IFilterEnumerator<TContext>, IEnumerator<TData>
		where TData : class, IMemberData
		where TContext : ISyntaxValidatorContext
	{
		internal readonly CachedData<TData> _cache;

		private readonly IEnumerator<CSharpSyntaxNode> _nodes;

		/// <inheritdoc/>
		public readonly ICompilationData Compilation { get; }

		/// <summary>
		/// <typeparamref name="TData"/> at the current position in the enumerator.
		/// </summary>
		public TData? Current { readonly get; private set; }

		/// <summary>
		/// <see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.
		/// </summary>
		public readonly IDiagnosticReceiver DiagnosticReceiver { get; }

		/// <inheritdoc cref="IFilterEnumerator{T}.Validator"/>
		public readonly ISyntaxValidatorWithDiagnostics<TContext> Validator { get; }

		readonly TData IEnumerator<TData>.Current => Current!;
		readonly IMemberData IFilterEnumerator<TContext>.Current => Current!;
		readonly object IEnumerator.Current => Current!;
		readonly ISyntaxValidator<TContext> IFilterEnumerator<TContext>.Validator => Validator;

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedFilterEnumeratorWithDiagnostics{TData, TContext}"/> struct.
		/// </summary>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of the provided <paramref name="nodes"/>.</param>
		/// <param name="nodes">A collection of <see cref="CSharpSyntaxNode"/>s to use to create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="validator"><see cref="ISyntaxValidatorWithDiagnostics{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="cache">Container of cached <see cref="IMemberData"/>s.</param>
		public CachedFilterEnumeratorWithDiagnostics(
			ICompilationData compilation,
			IEnumerable<CSharpSyntaxNode> nodes,
			ISyntaxValidatorWithDiagnostics<TContext> validator,
			IDiagnosticReceiver diagnosticReceiver,
			in CachedData<TData> cache
		) : this(compilation, nodes.GetEnumerator(), validator, diagnosticReceiver, in cache)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedFilterEnumeratorWithDiagnostics{TData, TContext}"/> struct.
		/// </summary>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of <see cref="CSharpSyntaxNode"/>s provided by the <paramref name="provider"/>.</param>
		/// <param name="provider"><see cref="INodeProvider"/> that creates an array of <see cref="CSharpSyntaxNode"/>s to be used to create the target <see cref="IMemberData"/>s.</param>
		/// <param name="validator"><see cref="ISyntaxValidatorWithDiagnostics{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="cache">Container of cached <see cref="IMemberData"/>s.</param>
		public CachedFilterEnumeratorWithDiagnostics(
			ICompilationData compilation,
			INodeProvider provider,
			ISyntaxValidatorWithDiagnostics<TContext> validator,
			IDiagnosticReceiver diagnosticReceiver,
			in CachedData<TData> cache
		) : this(compilation, provider.GetNodes().GetEnumerator(), validator, diagnosticReceiver, in cache)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedFilterEnumeratorWithDiagnostics{TData, TContext}"/> struct.
		/// </summary>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of the provided <paramref name="nodes"/>.</param>
		/// <param name="nodes">An enumerator that iterates through a collection of <see cref="CSharpSyntaxNode"/>s.</param>
		/// <param name="validator"><see cref="ISyntaxValidatorWithDiagnostics{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="cache">Container of cached <see cref="IMemberData"/>s.</param>
		public CachedFilterEnumeratorWithDiagnostics(
			ICompilationData compilation,
			IEnumerator<CSharpSyntaxNode> nodes,
			ISyntaxValidatorWithDiagnostics<TContext> validator,
			IDiagnosticReceiver diagnosticReceiver,
			in CachedData<TData> cache
		)
		{
			Validator = validator;
			_nodes = nodes;
			Compilation = compilation;
			DiagnosticReceiver = diagnosticReceiver;
			Current = default;
			_cache = cache;
		}

#pragma warning disable RCS1242 // Do not pass non-read-only struct by read-only reference.

		/// <inheritdoc/>
		public static explicit operator CachedFilterEnumeratorWithDiagnostics<TData, TContext>(in FilterEnumeratorWithDiagnostics<TContext> a)
		{
			return new CachedFilterEnumeratorWithDiagnostics<TData, TContext>(a.Compilation, a._nodes, a.Validator, a.DiagnosticReceiver, CachedData<TData>.Empty);
		}

		/// <inheritdoc/>
		public static explicit operator FilterEnumeratorWithDiagnostics<TContext>(in CachedFilterEnumeratorWithDiagnostics<TData, TContext> a)
		{
			return new FilterEnumeratorWithDiagnostics<TContext>(a.Compilation, a._nodes, a.Validator, a.DiagnosticReceiver);
		}

#pragma warning restore RCS1242 // Do not pass non-read-only struct by read-only reference.

		/// <inheritdoc/>
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

				if(_cache.TryGetCachedValue(node, out TData? data))
				{
					Current = data;
					return true;
				}

				if (Validator.ValidateAndCreate(new ValidationDataProviderContext(node, Compilation, cancellationToken), out IMemberData? member, DiagnosticReceiver) && member is TData d)
				{
					Current = d;
					return true;
				}
			}

			Current = default;
			return false;
		}

		/// <inheritdoc cref="FilterEnumerator{T}.Reset"/>
		public void Reset()
		{
			_nodes.Reset();
			Current = default;
		}

		/// <summary>
		/// Converts this <see cref="CachedFilterEnumeratorWithDiagnostics{TData, TContext}"/> to a new instance of <see cref="CachedFilterEnumerator{TData, TContext}"/>.
		/// </summary>
		public readonly CachedFilterEnumerator<TData, TContext> ToBasicCachedEnumerator()
		{
			return new CachedFilterEnumerator<TData, TContext>(Compilation, _nodes, Validator, in _cache);
		}

		/// <summary>
		/// Converts this <see cref="CachedFilterEnumeratorWithDiagnostics{TData, TContext}"/> to a new instance of <see cref="FilterEnumerator{T}"/>.
		/// </summary>
		public readonly FilterEnumerator<TContext> ToBasicEnumerator()
		{
			return new FilterEnumerator<TContext>(Compilation, _nodes, Validator);
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
}
