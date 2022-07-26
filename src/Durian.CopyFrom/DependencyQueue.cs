// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.CopyFrom
{
	/// <summary>
	/// Queue of <see cref="ICopyFromMember"/> that are dependent on other members.
	/// </summary>
	public sealed class DependencyQueue
	{
		private readonly ConcurrentQueue<(SyntaxReference node, string hintName)> _queue;

		/// <summary>
		/// Number of elements in the queue.
		/// </summary>
		public int Count => _queue.Count;

		/// <summary>
		/// Initializes a new instance of the <see cref="DependencyQueue"/> class.
		/// </summary>
		public DependencyQueue()
		{
			_queue = new();
		}

		/// <summary>
		/// Attempts to dequeue a <see cref="SyntaxReference"/> to an enqueued <see cref="SyntaxNode"/>.
		/// </summary>
		/// <param name="reference"><see cref="SyntaxReference"/> to an enqueued <see cref="SyntaxNode"/>.</param>
		/// <param name="hintName">Name associated with the <paramref name="reference"/>.</param>
		public bool Dequeue([NotNullWhen(true)] out SyntaxReference? reference, [NotNullWhen(true)] out string? hintName)
		{
			if (_queue.TryDequeue(out (SyntaxReference node, string hintName) result))
			{
				reference = result.node;
				hintName = result.hintName;
				return true;
			}

			reference = default;
			hintName = default;
			return false;
		}

		/// <summary>
		/// Enqueues the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="SyntaxNode"/> to enqueue.</param>
		/// <param name="hintName">Name associated with the <paramref name="node"/>.</param>
		public void Enqueue(SyntaxNode node, string hintName)
		{
			_queue.Enqueue((node.GetReference(), hintName));
		}

		/// <summary>
		/// Enqueues the specified <paramref name="reference"/>.
		/// </summary>
		/// <param name="reference"><see cref="SyntaxReference"/> to a <see cref="SyntaxNode"/> to enqueue.</param>
		/// <param name="hintName">Name associated with the <paramref name="reference"/>.</param>
		public void Enqueue(SyntaxReference reference, string hintName)
		{
			_queue.Enqueue((reference, hintName));
		}

		/// <summary>
		/// Converts the current <see cref="DependencyQueue"/> to a <see cref="Queue{T}"/>.
		/// </summary>
		public Queue<(SyntaxReference, string)> ToSystemQueue()
		{
			Queue<(SyntaxReference, string)> queue = new(_queue.Count);

			while (!_queue.IsEmpty && _queue.TryDequeue(out (SyntaxReference, string) result))
			{
				queue.Enqueue(result);
			}

			return queue;
		}
	}
}
