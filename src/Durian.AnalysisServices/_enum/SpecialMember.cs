// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Durian.Analysis
{
	/// <summary>
	/// Defines members that can be used in special contexts without the need to implement any interfaces.
	/// </summary>
	public enum SpecialMember
	{
		/// <summary>
		/// The member is not special.
		/// </summary>
		None,

		/// <summary>
		/// The member is equivalent to the <see cref="IEnumerator.Current"/>, <see cref="IEnumerator{T}.Current"/> or <see cref="IAsyncEnumerator{T}.Current"/> property.
		/// </summary>
		Current,

		/// <summary>
		/// The member is equivalent to the <see cref="IEnumerator.MoveNext"/> method.
		/// </summary>
		MoveNext,

		/// <summary>
		/// The member is equivalent to the <see cref="IAsyncEnumerator{T}.MoveNextAsync"/> method.
		/// </summary>
		MoveNextAsync,

		/// <summary>
		/// The member is equivalent to the <see cref="IEnumerable.GetEnumerator"/> or <see cref="IEnumerable{T}.GetEnumerator"/> method.
		/// </summary>
		GetEnumerator,

		/// <summary>
		/// The members is equivalent to the <see cref="IAsyncEnumerable{T}.GetAsyncEnumerator(System.Threading.CancellationToken)"/> method.
		/// </summary>
		GetAsyncEnumerator,

		/// <summary>
		/// The member is the tuple construction method.
		/// </summary>
		Deconstruct,

		/// <summary>
		/// The member is equivalent to the <see cref="Task.GetAwaiter()"/> method.
		/// </summary>
		GetAwaiter,

		/// <summary>
		/// The member is equivalent to the <see cref="IList.Add(object)"/> method.
		/// </summary>
		Add,

		/// <summary>
		/// The member is equivalent to the <see cref="TaskAwaiter.IsCompleted"/> property.
		/// </summary>
		IsCompleted,

		/// <summary>
		/// The member is equivalent to the <see cref="TaskAwaiter.GetResult"/> method.
		/// </summary>
		GetResult
	}
}
