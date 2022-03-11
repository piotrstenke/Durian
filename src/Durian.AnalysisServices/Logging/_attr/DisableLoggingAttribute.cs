// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Analysis.Logging
{
	/// <summary>
	/// Disables generator logging for the target generator or the entire assembly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
	public sealed class DisableLoggingAttribute : Attribute
	{
		/// <summary>
		/// Determines whether this <see cref="Attribute"/> should be inherited.
		/// </summary>
		public bool Inherit { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DisableLoggingAttribute"/> class.
		/// </summary>
		public DisableLoggingAttribute()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DisableLoggingAttribute"/> class.
		/// </summary>
		/// <param name="inherit">Determines whether this <see cref="Attribute"/> should be inherited.</param>
		public DisableLoggingAttribute(bool inherit)
		{
			Inherit = inherit;
		}
	}
}
