using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Durian.Generator.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// Enumerates through <see cref="IDefaultParamTarget"/>s returned by a <see cref="IDefaultParamFilter"/> and creates log files for each of them.
	/// </summary>
	[DebuggerDisplay("Current = {Current}")]
	public struct LoggableEnumerator : IEnumerator<IDefaultParamTarget>
	{
		private readonly CSharpSyntaxNode[] _nodes;
		private readonly DefaultParamCompilationData _compilation;
		private readonly IDefaultParamFilter _filter;
		private readonly LoggableGeneratorDiagnosticReceiver _logReceiver;
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
		/// Initializes a new instance of the <see cref="LoggableEnumerator"/> struct.
		/// </summary>
		/// <param name="filter"><see cref="IDefaultParamFilter"/> that creates the <see cref="IDefaultParamTarget"/>s to enumerate.</param>
		public LoggableEnumerator(IDefaultParamFilter filter)
		{
			_filter = filter;
			_nodes = filter.GetCandidateNodes();
			_fileNameProvider = filter.FileNameProvider;
			_logReceiver = filter.Generator.LogReceiver;
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

				if (!_filter.GetValidationData(_compilation, node, out SemanticModel? semanticModel, out ISymbol? symbol, out TypeParameterContainer typeParameters, _cancellationToken))
				{
					continue;
				}

				string fileName = _fileNameProvider.GetFileName(symbol);
				_logReceiver.SetTargetNode(node, fileName);
				bool isValid = _filter.ValidateAndCreateWithDiagnostics(_logReceiver, _compilation, node, semanticModel, symbol, in typeParameters, out IDefaultParamTarget? data, _cancellationToken);

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
	}
}
