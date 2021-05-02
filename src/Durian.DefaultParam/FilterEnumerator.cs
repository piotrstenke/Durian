using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.DefaultParam
{
	[DebuggerDisplay("Current = {Current}")]
	public struct FilterEnumerator : IEnumerator<IDefaultParamTarget>
	{
		private readonly CSharpSyntaxNode[] _nodes;
		private readonly DefaultParamCompilationData _compilation;
		private readonly CancellationToken _cancellationToken;
		private readonly IDefaultParamFilter _filter;
		private int _index;

		public IDefaultParamTarget? Current { get; private set; }

		IDefaultParamTarget IEnumerator<IDefaultParamTarget>.Current => Current!;
		object IEnumerator.Current => Current!;

		public FilterEnumerator(IDefaultParamFilter filter)
		{
			_filter = filter;
			_nodes = filter.GetCandidateNodes();
			_compilation = filter.Generator.TargetCompilation!;
			_cancellationToken = filter.Generator.CancellationToken;
			_index = 0;
			Current = null;
		}

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

				if (_filter.ValidateAndCreate(_compilation, node, out IDefaultParamTarget? data, _cancellationToken))
				{
					Current = data;
					return true;
				}
			}

			Current = null;
			return false;
		}

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
