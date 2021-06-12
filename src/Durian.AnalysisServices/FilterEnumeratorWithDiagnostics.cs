// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Durian.Generator.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Generator
{
	/// <summary>
	/// Enumerates through a collection of <see cref="IMemberData"/>s of type <typeparamref name="T"/> created by the provided <see cref="INodeValidatorWithDiagnostics{T}"/> with an option to report diagnostics using a <see cref="IDiagnosticReceiver"/>.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="IMemberData"/> this enumerator can handle.</typeparam>
	[DebuggerDisplay("Current = {Current}")]
	public struct FilterEnumeratorWithDiagnostics<T> : IEnumerator<T> where T : IMemberData
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

		readonly T IEnumerator<T>.Current => Current!;

		readonly object IEnumerator.Current => Current!;

		/// <summary>
		/// <see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.
		/// </summary>
		public readonly IDiagnosticReceiver DiagnosticReceiver { get; }

		/// <summary>
		/// <see cref="INodeValidatorWithDiagnostics{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.
		/// </summary>
		public readonly INodeValidatorWithDiagnostics<T> Validator { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterEnumeratorWithDiagnostics{T}"/> struct.
		/// </summary>
		/// <param name="nodes">An array of <see cref="CSharpSyntaxNode"/>s to use to create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of the provided <paramref name="nodes"/>.</param>
		/// <param name="validator"><see cref="INodeValidatorWithDiagnostics{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		public FilterEnumeratorWithDiagnostics(CSharpSyntaxNode[] nodes, ICompilationData compilation, INodeValidatorWithDiagnostics<T> validator, IDiagnosticReceiver diagnosticReceiver) : this(nodes, compilation, validator, diagnosticReceiver, default)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterEnumeratorWithDiagnostics{T}"/> struct.
		/// </summary>
		/// <param name="provider"><see cref="INodeProvider"/> that creates an array of <see cref="CSharpSyntaxNode"/>s to be used to create the target <see cref="IMemberData"/>s.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of <see cref="CSharpSyntaxNode"/>s provided by the <paramref name="provider"/>.</param>
		/// <param name="validator"><see cref="INodeValidatorWithDiagnostics{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		public FilterEnumeratorWithDiagnostics(INodeProvider provider, ICompilationData compilation, INodeValidatorWithDiagnostics<T> validator, IDiagnosticReceiver diagnosticReceiver) : this(provider.GetNodes().ToArray(), compilation, validator, diagnosticReceiver, default)
		{
		}

		internal FilterEnumeratorWithDiagnostics(CSharpSyntaxNode[] nodes, ICompilationData compilation, INodeValidatorWithDiagnostics<T> validator, IDiagnosticReceiver diagnosticReceiver, int index)
		{
			Validator = validator;
			_nodes = nodes;
			Compilation = compilation;
			_index = index;
			DiagnosticReceiver = diagnosticReceiver;
			Current = default;
		}

		void IDisposable.Dispose()
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

				if (Validator.ValidateAndCreateWithDiagnostics(DiagnosticReceiver, node, Compilation, out T? data))
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
			_index = 0;
			Current = default;
		}

		/// <summary>
		/// Converts this <see cref="FilterEnumeratorWithDiagnostics{T}"/> to a new instance of <see cref="FilterEnumerator{T}"/>.
		/// </summary>
		public readonly FilterEnumerator<T> ToBasicEnumerator()
		{
			return new FilterEnumerator<T>(_nodes, Compilation, Validator, _index);
		}
	}
}
