using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.DefaultParam
{
	/// <summary>
	/// Enumerates through <see cref="IDefaultParamTarget"/>s returned by a <see cref="IDefaultParamFilter"/> and reports <see cref="Diagnostic"/>s for the invalid ones.
	/// </summary>
	[DebuggerDisplay("Current = {Current}")]
	public struct DiagnosticEnumerator : IEnumerator<IDefaultParamTarget>
	{
		private readonly CSharpSyntaxNode[] _nodes;
		private readonly DefaultParamCompilationData _compilation;
		private readonly IDefaultParamFilter _filter;
		private readonly IDirectDiagnosticReceiver _diagnosticReceiver;
		private readonly CancellationToken _cancellationToken;
		private int _index;

		/// <summary>
		/// Current <see cref="IDefaultParamTarget"/>.
		/// </summary>
		public IDefaultParamTarget? Current { get; private set; }

		IDefaultParamTarget IEnumerator<IDefaultParamTarget>.Current => Current!;
		object IEnumerator.Current => Current!;

		/// <summary>
		/// Initializes a new instance of the <see cref="DiagnosticEnumerator"/> struct.
		/// </summary>
		/// <param name="filter"><see cref="IDefaultParamFilter"/> that creates the <see cref="IDefaultParamTarget"/>s to enumerate.</param>
		public DiagnosticEnumerator(IDefaultParamFilter filter)
		{
			_filter = filter;
			_nodes = filter.GetCandidateNodes();
			_diagnosticReceiver = filter.Generator.DiagnosticReceiver!;
			_compilation = filter.Generator.TargetCompilation!;
			_cancellationToken = filter.Generator.CancellationToken;
			_index = 0;
			Current = null;
		}

		/// <inheritdoc cref="FilterEnumerator.MoveNext"/>
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

				if (_filter.ValidateAndCreateWithDiagnostics(_diagnosticReceiver, _compilation, node, out IDefaultParamTarget? data, _cancellationToken))
				{
					Current = data;
					return true;
				}
			}

			Current = null;
			return false;
		}

		/// <inheritdoc cref="FilterEnumerator.Reset"/>
		public void Reset()
		{
			_index = 0;
			Current = null;
		}

		void IDisposable.Dispose()
		{
			// Do nothing.
		}
	}
}
