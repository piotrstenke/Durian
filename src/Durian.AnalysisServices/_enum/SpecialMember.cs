using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Durian.Analysis;

/// <summary>
/// Defines members that can be used in special contexts without the need to implement any interfaces.
/// </summary>
public enum SpecialMember
{
	/// <summary>
	/// The member is not special.
	/// </summary>
	None = 0,

	/// <summary>
	/// The member is equivalent to the <see cref="IEnumerator.Current"/>, <see cref="IEnumerator{T}.Current"/> or <see cref="IAsyncEnumerator{T}.Current"/> property.
	/// </summary>
	Current = 1,

	/// <summary>
	/// The member is equivalent to the <see cref="IEnumerator.MoveNext"/> method.
	/// </summary>
	MoveNext = 2,

	/// <summary>
	/// The member is equivalent to the <see cref="IAsyncEnumerator{T}.MoveNextAsync"/> method.
	/// </summary>
	MoveNextAsync = 3,

	/// <summary>
	/// The member is equivalent to the <see cref="IEnumerable.GetEnumerator"/> or <see cref="IEnumerable{T}.GetEnumerator"/> method.
	/// </summary>
	GetEnumerator = 4,

	/// <summary>
	/// The members is equivalent to the <see cref="IAsyncEnumerable{T}.GetAsyncEnumerator(System.Threading.CancellationToken)"/> method.
	/// </summary>
	GetAsyncEnumerator = 5,

	/// <summary>
	/// The member is the tuple construction method.
	/// </summary>
	Deconstruct = 6,

	/// <summary>
	/// The member is equivalent to the <see cref="Task.GetAwaiter()"/> method.
	/// </summary>
	GetAwaiter = 7,

	/// <summary>
	/// The member is equivalent to the <see cref="IList.Add(object)"/> method.
	/// </summary>
	Add = 8,

	/// <summary>
	/// The member is equivalent to the <see cref="TaskAwaiter.IsCompleted"/> property.
	/// </summary>
	IsCompleted = 9,

	/// <summary>
	/// The member is equivalent to the <see cref="TaskAwaiter.GetResult"/> method.
	/// </summary>
	GetResult = 10,

	/// <summary>
	/// The member is equivalent to the PrintMembers method in record declarations.
	/// </summary>
	PrintMembers = 11,

	/// <summary>
	/// The member is equivalent to the <see cref="List{T}.Count"/> property.
	/// </summary>
	Count = 12,

	/// <summary>
	/// The member is equivalent to the <see cref="Array.Length"/> property.
	/// </summary>
	Length = 13,

	/// <summary>
	/// The member is equivalent to the <see cref="ReadOnlySpan{T}.Slice(int, int)"/> method.
	/// </summary>
	Slice = 14
}
