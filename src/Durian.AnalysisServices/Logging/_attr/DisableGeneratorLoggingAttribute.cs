// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Analysis.Logging
{
	/// <summary>
	/// Disables generator logging for the target generator or the entire assembly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
	public sealed class DisableGeneratorLoggingAttribute : Attribute
	{
		/// <summary>
		/// Determines whether this <see cref="Attribute"/> should be inherited.
		/// </summary>
		public bool Inherit { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DisableGeneratorLoggingAttribute"/> class.
		/// </summary>
		public DisableGeneratorLoggingAttribute()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DisableGeneratorLoggingAttribute"/> class.
		/// </summary>
		/// <param name="inherit">Determines whether this <see cref="Attribute"/> should be inherited.</param>
		public DisableGeneratorLoggingAttribute(bool inherit)
		{
			Inherit = inherit;
		}
	}
}
