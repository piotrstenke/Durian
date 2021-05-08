using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.DefaultParam
{
	/// <summary>
	/// Enumerates through <see cref="IDefaultParamTarget"/>s returned by a <see cref="IDefaultParamFilter"/>.
	/// </summary>
	[DebuggerDisplay("Current = {Current}")]
	public struct FilterEnumerator : IEnumerator<IDefaultParamTarget>
	{
		private readonly CSharpSyntaxNode[] _nodes;
		private readonly DefaultParamCompilationData _compilation;
		private readonly CancellationToken _cancellationToken;
		private readonly IDefaultParamFilter _filter;
		private int _index;

		/// <summary>
		/// Current <see cref="IDefaultParamTarget"/>.
		/// </summary>
		public IDefaultParamTarget? Current { get; private set; }

		IDefaultParamTarget IEnumerator<IDefaultParamTarget>.Current => Current!;
		object IEnumerator.Current => Current!;

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterEnumerator"/> struct.
		/// </summary>
		/// <param name="filter"><see cref="IDefaultParamFilter"/> that creates the <see cref="IDefaultParamTarget"/>s to enumerate.</param>
		public FilterEnumerator(IDefaultParamFilter filter)
		{
			_filter = filter;
			_nodes = filter.GetCandidateNodes();
			_compilation = filter.Generator.TargetCompilation!;
			_cancellationToken = filter.Generator.CancellationToken;
			_index = 0;
			Current = null;
		}

		/// <summary>
		/// Creates and validates the next <see cref="IDefaultParamTarget"/>.
		/// </summary>
		/// <returns><see langword="true"/> is the <see cref="IDefaultParamTarget"/> is valid to be used by a <see cref="DefaultParamGenerator"/>, otherwise <see langword="false"/>.</returns>
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

		/// <summary>
		/// Resets the enumerator.
		/// </summary>
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
