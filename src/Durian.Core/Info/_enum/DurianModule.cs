﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Info
{
	/// <summary>
	/// Defines all modules a package can be part of.
	/// </summary>
	public enum DurianModule
	{
		/// <summary>
		/// This package does not belong to any Durian module.
		/// </summary>
		None = 0,

		/// <summary>
		/// Represents the <c>Durian.Core</c> module.
		/// </summary>
		Core = 1,

		/// <summary>
		/// Represents the <c>Durian.DefaultParam</c> module.
		/// </summary>
		DefaultParam = 2,

		/// <summary>
		/// Represents the <c>Durian.GenericSpecialization</c> module.
		/// </summary>
		GenericSpecialization = 3,

		/// <summary>
		/// Represents the <c>Durian.FriendClass</c> module.
		/// </summary>
		FriendClass = 4,

		/// <summary>
		/// Represents the <c>Durian.InterfaceTargets</c> module.
		/// </summary>
		InterfaceTargets = 5,

		/// <summary>
		/// Represents the <c>Durian.Manager</c> module.
		/// </summary>
		Manager = 6
	}
}
