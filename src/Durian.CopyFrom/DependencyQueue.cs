// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Durian.Analysis.CopyFrom
{
	/// <summary>
	/// Queue of <see cref="ICopyFromMember"/> that are dependent on other members.
	/// </summary>
	public sealed class DependencyQueue
	{
		private readonly ConcurrentQueue<(ICopyFromMember member, string hintName)> _queue;

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
		/// Enqueues the specified <paramref name="member"/>.
		/// </summary>
		/// <param name="member"><see cref="ICopyFromMember"/> to enqueue.</param>
		/// <param name="hintName">Name associated with the <paramref name="member"/>.</param>
		public void Enqueue(ICopyFromMember member, string hintName)
		{
			_queue.Enqueue((member, hintName));
		}

		/// <summary>
		/// Attempt to dequeue a <see cref="ICopyFromMember"/>.
		/// </summary>
		/// <param name="member">Dequeued <see cref="ICopyFromMember"/>.</param>
		/// <param name="hintName">Name associated with the <paramref name="member"/>.</param>
		public bool Dequeue([NotNullWhen(true)]out ICopyFromMember? member, [NotNullWhen(true)] out string? hintName)
		{
			if(_queue.TryDequeue(out (ICopyFromMember member, string hintName) result))
			{
				member = result.member;
				hintName = result.hintName;
				return true;
			}

			member = default;
			hintName = default;
			return false;
		}
	}
}
