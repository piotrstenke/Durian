// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// Enumerates through <see cref="IDefaultParamTarget"/> s returned by a <see cref="IDefaultParamFilter"/>.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="IDefaultParamTarget"/> this enumerator supports.</typeparam>
	[DebuggerDisplay("Current = {Current}")]
	public struct DefaultParamFilterEnumerator<T> : IEnumerator<T> where T : IDefaultParamTarget
	{
		internal readonly IEnumerator<CSharpSyntaxNode> _nodes;

		/// <summary>
		/// Parent <see cref="DefaultParamCompilationData"/> of the provided <see
		/// cref="CSharpSyntaxNode"/> s.
		/// </summary>
		public readonly DefaultParamCompilationData Compilation => Filter.Generator.TargetCompilation!;

		/// <summary>
		/// Current <see cref="IDefaultParamTarget"/>.
		/// </summary>
		public T? Current { readonly get; private set; }

		/// <summary>
		/// <see cref="INodeDiagnosticReceiver"/> that writes the reported <see cref="Diagnostic"/>
		/// s into a log file or buffer.
		/// </summary>
		public readonly INodeDiagnosticReceiver DiagnosticReceiver { get; }

		/// <summary>
		/// <see cref="IDefaultParamFilter"/> that is used to validate and create the <see
		/// cref="IMemberData"/> s to enumerate through.
		/// </summary>
		public readonly IDefaultParamFilter Filter { get; }

		/// <summary>
		/// <see cref="IHintNameProvider"/> that creates hint names for the <see
		/// cref="CSharpSyntaxNode"/> s.
		/// </summary>
		public readonly IHintNameProvider HintNameProvider => Filter.HintNameProvider;

		readonly T IEnumerator<T>.Current => Current!;
		readonly object IEnumerator.Current => Current!;

		/// <inheritdoc cref="DefaultParamFilterEnumerator(IDefaultParamFilter, INodeDiagnosticReceiver)"/>
		public DefaultParamFilterEnumerator(IDefaultParamFilter filter) : this(filter, filter.Generator.LogReceiver)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamFilterEnumerator{T}"/> struct.
		/// </summary>
		/// <param name="filter">
		/// <see cref="IDefaultParamFilter"/> that creates the <see cref="IDefaultParamTarget"/>
		/// s to enumerate through.
		/// </param>
		/// <param name="diagnosticReceiver">
		/// <see cref="INodeDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/> s.
		/// </param>
		public DefaultParamFilterEnumerator(IDefaultParamFilter filter, INodeDiagnosticReceiver diagnosticReceiver)
		{
			Filter = filter;
			DiagnosticReceiver = diagnosticReceiver;
			_nodes = filter.GetNodes().GetEnumerator();
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

				if (!Filter.GetValidationData(node, Compilation, out SemanticModel? semanticModel, out ISymbol? symbol, out TypeParameterContainer typeParameters, cancellationToken))
				{
					continue;
				}

				string fileName = HintNameProvider.GetHintName(symbol);
				DiagnosticReceiver.SetTargetNode(node, fileName);
				bool isValid = Filter.ValidateAndCreate(node, Compilation, semanticModel, symbol, in typeParameters, out IDefaultParamTarget? data, DiagnosticReceiver, cancellationToken);

				if (DiagnosticReceiver.Count > 0)
				{
					DiagnosticReceiver.Push();
					HintNameProvider.Success();
				}

				if (isValid && data is T t)
				{
					Current = t;
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
