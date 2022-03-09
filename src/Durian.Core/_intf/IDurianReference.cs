// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Info
{
	/// <summary>
	/// Represents a cached reference to a specific object.
	/// </summary>
	public interface IDurianReference : ICloneable
	{
		/// <summary>
		/// Determines whether the internal object is allocated.
		/// </summary>
		bool IsAllocated { get; }

		/// <summary>
		/// Allocates the internal object.
		/// </summary>
		void Allocate();

		/// <inheritdoc cref="ICloneable.Clone"/>
		new IDurianReference Clone();

		/// <summary>
		/// Removes the internal object from the memory.
		/// </summary>
		void Deallocate();

		/// <summary>
		/// Returns the allocated value.
		/// </summary>
		/// <returns>The allocated value. -or- <see langword="null"/> if the value is not allocated.</returns>
		object? GetAllocatedValue();

		/// <summary>
		/// Allocates the internal object or overrides it if it already exists.
		/// </summary>
		void Reallocate();
	}
}