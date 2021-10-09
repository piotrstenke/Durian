// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;

namespace Durian.Info
{
	/// <summary>
	/// A collection of Durian-relate objects in form of an <see cref="Enum"/> value, <see cref="IDurianReference"/> or direct <see cref="IDurianIdentity"/>.
	/// </summary>
	public interface IDurianContainer : IStructuralEquatable
	{
		/// <summary>
		/// Number of elements in the container.
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Determines whether the container is empty.
		/// </summary>
		bool IsEmpty { get; }

		/// <summary>
		/// Returns all elements in the container as <see cref="int"/>s representing <see cref="Enum"/> values.
		/// </summary>
		int[] AsEnums();

		/// <summary>
		/// Returns all elements in the container as direct <see cref="IDurianIdentity"/>s.
		/// </summary>
		IDurianIdentity[] AsIdentities();

		/// <summary>
		/// Returns all elements in the container as <see cref="IDurianReference"/>s.
		/// </summary>
		IDurianReference[] AsReferences();
	}
}
