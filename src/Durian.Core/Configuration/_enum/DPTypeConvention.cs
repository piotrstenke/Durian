﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Configuration
{
	/// <summary>
	/// Determines how a <c>DefaultParam</c> type is generated.
	/// </summary>
	public enum DPTypeConvention
	{
		/// <summary>
		/// Uses default convention, which is <see cref="Inherit"/>.
		/// </summary>
		Default = Inherit,

		/// <summary>
		/// Copies contents of the type.
		/// </summary>
		Inherit = 1,

		/// <summary>
		/// Inherits the type.
		/// </summary>
		Copy = 2
	}
}
