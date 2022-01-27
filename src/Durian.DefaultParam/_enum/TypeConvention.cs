// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// Determines how a <c>DefaultParam</c> type is generated.
	/// </summary>
	public enum TypeConvention
	{
		// *********************************************************************************************
		// Values of this enum have to be exactly the same as Durian.Configuration.DPTypeConvention.
		// *********************************************************************************************

		/// <summary>
		/// Uses default convention, which is <see cref="Inherit"/>.
		/// </summary>
		Default = Inherit,

		/// <summary>
		/// Copies contents of the type.
		/// </summary>
#pragma warning disable CA1069 // Enums values should not be duplicated
		Inherit = 0,
#pragma warning restore CA1069 // Enums values should not be duplicated

		/// <summary>
		/// Inherits the type.
		/// </summary>
		Copy = 1
	}
}