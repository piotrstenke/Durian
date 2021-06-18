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

namespace Durian.Analysis.Logging
{
	/// <summary>
	/// Enumerates through a collection of <see cref="IMemberData"/>s of type <typeparamref name="T"/> created by the provided <see cref="INodeValidatorWithDiagnostics{T}"/> with an option to log diagnostics using a <see cref="INodeDiagnosticReceiver"/>.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="IMemberData"/> this enumerator can handle.</typeparam>
	[DebuggerDisplay("Current = {Current}")]
	public struct LoggableFilterEnumerator<T> : IEnumerator<T> where T : IMemberData
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

		readonly T IEnumerator<T>.Current => Current!;

		readonly object IEnumerator.Current => Current!;

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggableFilterEnumerator{T}"/> struct.
		/// </summary>
		/// <param name="nodes">An array of <see cref="CSharpSyntaxNode"/>s to use to create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of the provided <paramref name="nodes"/>.</param>
		/// <param name="validator"><see cref="INodeValidatorWithDiagnostics{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="logReceiver"><see cref="INodeDiagnosticReceiver"/> that writes the reported <see cref="Diagnostic"/>s into a log file or buffer.</param>
		/// <param name="hintNameProvider"><see cref="IHintNameProvider"/> that creates hint names for the <paramref name="nodes"/>.</param>
		public LoggableFilterEnumerator(CSharpSyntaxNode[] nodes, ICompilationData compilation, INodeValidatorWithDiagnostics<T> validator, INodeDiagnosticReceiver logReceiver, IHintNameProvider hintNameProvider) : this(nodes, compilation, validator, logReceiver, hintNameProvider, default)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggableFilterEnumerator{T}"/> struct.
		/// </summary>
		/// <param name="provider"><see cref="INodeProvider"/> that creates an array of <see cref="CSharpSyntaxNode"/>s to be used to create the target <see cref="IMemberData"/>s.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of <see cref="CSharpSyntaxNode"/>s provided by the <paramref name="provider"/>.</param>
		/// <param name="validator"><see cref="INodeValidatorWithDiagnostics{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="logReceiver"><see cref="INodeDiagnosticReceiver"/> that writes the reported <see cref="Diagnostic"/>s into a log file or buffer.</param>
		/// <param name="fileNameProvider"><see cref="IHintNameProvider"/> that creates hint names for the <see cref="CSharpSyntaxNode"/>s provided by the <paramref name="provider"/>.</param>
		public LoggableFilterEnumerator(INodeProvider provider, ICompilationData compilation, INodeValidatorWithDiagnostics<T> validator, INodeDiagnosticReceiver logReceiver, IHintNameProvider fileNameProvider) : this(provider.GetNodes().ToArray(), compilation, validator, logReceiver, fileNameProvider, default)
		{
		}

		internal LoggableFilterEnumerator(CSharpSyntaxNode[] nodes, ICompilationData compilation, INodeValidatorWithDiagnostics<T> validator, INodeDiagnosticReceiver logReceiver, IHintNameProvider hintNameProvider, int index)
		{
			Validator = validator;
			Compilation = compilation;
			LogReceiver = logReceiver;
			HintNameProvider = hintNameProvider;
			_nodes = nodes;
			_index = index;
			Current = default;
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

				if (!Validator.GetValidationData(node, Compilation, out SemanticModel? semanticModel, out ISymbol? symbol))
				{
					continue;
				}

				string fileName = HintNameProvider.GetFileName(symbol);
				LogReceiver.SetTargetNode(node, fileName);
				bool isValid = Validator.ValidateAndCreateWithDiagnostics(LogReceiver, node, Compilation, semanticModel, symbol, out T? data);

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
		/// Converts this <see cref="LoggableFilterEnumerator{T}"/> to a new instance of <see cref="FilterEnumerator{T}"/>.
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
