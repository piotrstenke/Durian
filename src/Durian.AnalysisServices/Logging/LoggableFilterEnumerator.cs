// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Durian.Analysis.Logging
{
	/// <summary>
	/// Enumerates through a collection of <see cref="IMemberData"/>s of type <typeparamref name="T"/> created by the provided <see cref="INodeValidatorWithDiagnostics{T}"/> with an option to log diagnostics using a <see cref="INodeDiagnosticReceiver"/>.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="IMemberData"/> this enumerator can handle.</typeparam>
	[DebuggerDisplay("Current = {Current}")]
	public struct LoggableFilterEnumerator<T> : IEnumerator<T> where T : IMemberData
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
		/// <param name="nodes">A collection of <see cref="CSharpSyntaxNode"/>s to use to create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of the provided <paramref name="nodes"/>.</param>
		/// <param name="validator"><see cref="INodeValidatorWithDiagnostics{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="logReceiver"><see cref="INodeDiagnosticReceiver"/> that writes the reported <see cref="Diagnostic"/>s into a log file or buffer.</param>
		/// <param name="hintNameProvider"><see cref="IHintNameProvider"/> that creates hint names for the <paramref name="nodes"/>.</param>
		public LoggableFilterEnumerator(
			IEnumerable<CSharpSyntaxNode> nodes,
			ICompilationData compilation,
			INodeValidatorWithDiagnostics<T> validator,
			INodeDiagnosticReceiver logReceiver,
			IHintNameProvider hintNameProvider
		) : this(nodes.GetEnumerator(), compilation, validator, logReceiver, hintNameProvider)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggableFilterEnumerator{T}"/> struct.
		/// </summary>
		/// <param name="nodeProvider"><see cref="INodeProvider"/> that creates an array of <see cref="CSharpSyntaxNode"/>s to be used to create the target <see cref="IMemberData"/>s.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of <see cref="CSharpSyntaxNode"/>s provided by the <paramref name="nodeProvider"/>.</param>
		/// <param name="validator"><see cref="INodeValidatorWithDiagnostics{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="logReceiver"><see cref="INodeDiagnosticReceiver"/> that writes the reported <see cref="Diagnostic"/>s into a log file or buffer.</param>
		/// <param name="hintNameProvider"><see cref="IHintNameProvider"/> that creates hint names for the <see cref="CSharpSyntaxNode"/>s provided by the <paramref name="nodeProvider"/>.</param>
		public LoggableFilterEnumerator(
			INodeProvider nodeProvider,
			ICompilationData compilation,
			INodeValidatorWithDiagnostics<T> validator,
			INodeDiagnosticReceiver logReceiver,
			IHintNameProvider hintNameProvider
		) : this(nodeProvider.GetNodes().GetEnumerator(), compilation, validator, logReceiver, hintNameProvider)
		{
		}

		internal LoggableFilterEnumerator(
			IEnumerator<CSharpSyntaxNode> nodes,
			ICompilationData compilation,
			INodeValidatorWithDiagnostics<T> validator,
			INodeDiagnosticReceiver logReceiver,
			IHintNameProvider hintNameProvider
		)
		{
			Validator = validator;
			Compilation = compilation;
			LogReceiver = logReceiver;
			HintNameProvider = hintNameProvider;
			_nodes = nodes;
			Current = default;
		}

		/// <inheritdoc cref="FilterEnumerator{T}.MoveNext"/>
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

				if (!Validator.GetValidationData(node, Compilation, out SemanticModel? semanticModel, out ISymbol? symbol, cancellationToken))
				{
					continue;
				}

				string fileName = HintNameProvider.GetHintName(symbol);
				LogReceiver.SetTargetNode(node, fileName);
				bool isValid = Validator.ValidateAndCreate(node, Compilation, semanticModel, symbol, out T? data, LogReceiver, cancellationToken);

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
			_nodes.Reset();
			Current = default;
		}

		/// <summary>
		/// Converts this <see cref="LoggableFilterEnumerator{T}"/> to a new instance of <see cref="FilterEnumerator{T}"/>.
		/// </summary>
		public readonly FilterEnumerator<T> ToBasicEnumerator()
		{
			return new FilterEnumerator<T>(_nodes, Compilation, Validator);
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