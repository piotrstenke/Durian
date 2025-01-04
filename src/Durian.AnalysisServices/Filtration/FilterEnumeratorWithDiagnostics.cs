// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Filtration
{
	/// <summary>
	/// Enumerates through a collection of <see cref="IMemberData"/>s of type <typeparamref name="T"/> created by the provided <see cref="ISyntaxValidator{T}"/> with an option to report diagnostics using a <see cref="IDiagnosticReceiver"/>.
	/// </summary>
	/// <typeparam name="T">Type of target <see cref="ISyntaxValidationContext"/>.</typeparam>
	[DebuggerDisplay("Current = {Current}")]
	public struct FilterEnumeratorWithDiagnostics<T> : IFilterEnumerator<T>, IEnumerator<IMemberData> where T : ISyntaxValidationContext
	{
		internal readonly IEnumerator<SyntaxNode> _nodes;

		/// <inheritdoc/>
		public readonly ICompilationData Compilation { get; }

		/// <inheritdoc/>
		public IMemberData? Current { readonly get; private set; }

		/// <summary>
		/// <see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.
		/// </summary>
		public readonly IDiagnosticReceiver DiagnosticReceiver { get; }

		/// <inheritdoc cref="IFilterEnumerator{T}.Validator"/>
		public readonly ISyntaxValidatorWithDiagnostics<T> Validator { get; }

		readonly IMemberData IEnumerator<IMemberData>.Current => Current!;

		readonly object IEnumerator.Current => Current!;
		readonly ISyntaxValidator<T> IFilterEnumerator<T>.Validator => Validator;

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterEnumeratorWithDiagnostics{T}"/> struct.
		/// </summary>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of the provided <paramref name="nodes"/>.</param>
		/// <param name="nodes">A collection of <see cref="SyntaxNode"/>s to use to create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="validator"><see cref="ISyntaxValidatorWithDiagnostics{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		public FilterEnumeratorWithDiagnostics(
			ICompilationData compilation,
			IEnumerable<SyntaxNode> nodes,
			ISyntaxValidatorWithDiagnostics<T> validator,
			IDiagnosticReceiver diagnosticReceiver
		) : this(compilation, nodes.GetEnumerator(), validator, diagnosticReceiver)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterEnumeratorWithDiagnostics{T}"/> struct.
		/// </summary>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of <see cref="SyntaxNode"/>s provided by the <paramref name="nodeProvider"/>.</param>
		/// <param name="nodeProvider"><see cref="INodeProvider"/> that creates an array of <see cref="SyntaxNode"/>s to be used to create the target <see cref="IMemberData"/>s.</param>
		/// <param name="validator"><see cref="ISyntaxValidatorWithDiagnostics{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		public FilterEnumeratorWithDiagnostics(
			ICompilationData compilation,
			INodeProvider nodeProvider,
			ISyntaxValidatorWithDiagnostics<T> validator,
			IDiagnosticReceiver diagnosticReceiver
		) : this(compilation, nodeProvider.GetNodes().GetEnumerator(), validator, diagnosticReceiver)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterEnumeratorWithDiagnostics{T}"/> struct.
		/// </summary>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of the provided <paramref name="nodes"/>.</param>
		/// <param name="nodes">An enumerator that iterates through a collection of <see cref="SyntaxNode"/>s.</param>
		/// <param name="validator"><see cref="ISyntaxValidatorWithDiagnostics{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		public FilterEnumeratorWithDiagnostics(
			ICompilationData compilation,
			IEnumerator<SyntaxNode> nodes,
			ISyntaxValidatorWithDiagnostics<T> validator,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			Validator = validator;
			_nodes = nodes;
			Compilation = compilation;
			DiagnosticReceiver = diagnosticReceiver;
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

				if (Validator.ValidateAndCreate(new PreValidationContext(node, Compilation, cancellationToken), out IMemberData? data, DiagnosticReceiver))
				{
					Current = data;
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
		/// Converts this <see cref="FilterEnumeratorWithDiagnostics{T}"/> to a new instance of <see cref="FilterEnumerator{T}"/>.
		/// </summary>
		public readonly FilterEnumerator<T> ToBasicEnumerator()
		{
			return new FilterEnumerator<T>(Compilation, _nodes, Validator);
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
