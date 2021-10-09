// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// Determines how a <c>DefaultParam</c> method is generated.
	/// </summary>
	public enum MethodConvention
	{
		// *********************************************************************************************
		// Values of this enum have to be exactly the same as Durian.Configuration.DPMethodConvention.
		// *********************************************************************************************

		/// <summary>
		/// Uses default convention, which is <see cref="Call"/>.
		/// </summary>
		Default = Call,

		/// <summary>
		/// Copies contents of the method.
		/// </summary>
		Call = 0,

		/// <summary>
		/// Call the method.
		/// </summary>
		Copy = 1
	}
}
