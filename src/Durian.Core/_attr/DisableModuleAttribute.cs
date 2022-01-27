// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Durian.Info;

namespace Durian
{
	/// <summary>
	/// Disables the specified <see cref="DurianModule"/>.
	/// </summary>
	/// <remarks>Works only if the 'Durian' package is present.</remarks>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
	[Conditional("DEBUG")]
	public sealed class DisableModuleAttribute : Attribute
	{
		/// <summary>
		/// <see cref="DurianModule"/> to disable.
		/// </summary>
		public DurianModule Module { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DisableModuleAttribute"/> class.
		/// </summary>
		/// <param name="module"><see cref="DurianModule"/> to disable.</param>
		public DisableModuleAttribute(DurianModule module)
		{
			Module = module;
		}
	}
}