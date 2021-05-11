using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Durian.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.DefaultParam
{
	/// <summary>
	/// Enumerates through <see cref="IDefaultParamTarget"/>s returned by a <see cref="IDefaultParamFilter"/>, creates log files for each of them and reports <see cref="Diagnostic"/>s for the invalid ones.
	/// </summary>
	[DebuggerDisplay("Current = {Current}")]
	public struct LoggableDiagnosticEnumerator : IEnumerator<IDefaultParamTarget>
	{
		private readonly CSharpSyntaxNode[] _nodes;
		private readonly DefaultParamCompilationData _compilation;
		private readonly IDefaultParamFilter _filter;
		private readonly LoggableGeneratorDiagnosticReceiver _logReceiver;
		private readonly IDirectDiagnosticReceiver _originalDiagnosticReceiver;
		private readonly IDirectDiagnosticReceiver _combinedDiagnosticReceiver;
		private readonly IFileNameProvider _fileNameProvider;
		private readonly CancellationToken _cancellationToken;
		private int _index;

		/// <summary>
		/// Current <see cref="IDefaultParamTarget"/>.
		/// </summary>
		public IDefaultParamTarget? Current { get; private set; }

		IDefaultParamTarget IEnumerator<IDefaultParamTarget>.Current => Current!;
		object IEnumerator.Current => Current!;

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggableDiagnosticEnumerator"/> struct.
		/// </summary>
		/// <param name="filter"><see cref="IDefaultParamFilter"/> that creates the <see cref="IDefaultParamTarget"/>s to enumerate.</param>
		public LoggableDiagnosticEnumerator(IDefaultParamFilter filter)
		{
			_filter = filter;
			_originalDiagnosticReceiver = filter.Generator.DiagnosticReceiver!;
			_nodes = filter.GetCandidateNodes();
			_fileNameProvider = filter.FileNameProvider;
			_logReceiver = filter.Generator.LogReceiver;
			_compilation = filter.Generator.TargetCompilation!;
			_cancellationToken = filter.Generator.CancellationToken;
			_index = 0;
			Current = null;

			// must be temporarily set in order to access to ReportForBothReceivers.
			_combinedDiagnosticReceiver = null!;
			_combinedDiagnosticReceiver = DiagnosticReceiverFactory.Direct(ReportForBothReceivers);
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

				if (!_filter.GetValidationData(_compilation, node, out SemanticModel? semanticModel, out ISymbol? symbol, out TypeParameterContainer typeParameters, _cancellationToken))
				{
					continue;
				}

				string fileName = _fileNameProvider.GetFileName(symbol);
				_logReceiver.SetTargetNode(node, fileName);
				bool isValid = _filter.ValidateAndCreateWithDiagnostics(_combinedDiagnosticReceiver, _compilation, node, semanticModel, symbol, in typeParameters, out IDefaultParamTarget? data, _cancellationToken);

				if (_logReceiver.Count > 0)
				{
					_logReceiver.Push();
					_fileNameProvider.Success();
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

		private void ReportForBothReceivers(Diagnostic diagnostic)
		{
			_logReceiver.ReportDiagnostic(diagnostic);
			_originalDiagnosticReceiver.ReportDiagnostic(diagnostic);
		}
	}
}
