// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Durian.Info
{
	/// <summary>
	/// A collection of Durian-related objects in form of an <see cref="Enum"/> value, <see cref="IDurianReference"/> or direct <see cref="IDurianIdentity"/>.
	/// </summary>
	public interface IDurianContainer : IStructuralEquatable, ICloneable
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
		IEnumerable<int> AsEnums();

		/// <summary>
		/// Returns all elements in the container as direct <see cref="IDurianIdentity"/>s.
		/// </summary>
		IEnumerable<IDurianIdentity> AsIdentities();

		/// <summary>
		/// Returns all elements in the container as <see cref="IDurianReference"/>s.
		/// </summary>
		IEnumerable<IDurianReference> AsReferences();

		/// <summary>
		/// Creates a new <see cref="IDurianContainer"/> that is a copy of the current instance.
		/// </summary>
		/// <param name="sharedReference">Determines whether to share internal list of objects between both instances.</param>
		IDurianContainer Clone(bool sharedReference = true);
	}
}