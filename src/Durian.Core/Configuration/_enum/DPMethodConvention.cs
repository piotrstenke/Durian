// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Configuration
{
	/// <summary>
	/// Determines how a <c>DefaultParam</c> method is generated.
	/// </summary>
	public enum DPMethodConvention
	{
		// *********************************************************************************************
		// Values of this enum have to be exactly the same as Durian.Analysis.DefaultParam.MethodConvention.
		// *********************************************************************************************

		/// <summary>
		/// Uses default convention, which is <see cref="Call"/>.
		/// </summary>
		Default = Call,

		/// <summary>
		/// Copies contents of the method.
		/// </summary>
#pragma warning disable CA1069 // Enums values should not be duplicated
		Call = 0,
#pragma warning restore CA1069 // Enums values should not be duplicated

		/// <summary>
		/// Call the method.
		/// </summary>
		Copy = 1
	}
}