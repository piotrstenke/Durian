// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using Durian.Info;

namespace Durian
{
	/// <summary>
	/// Specifies that the target <see cref="DurianModule"/> is enabled for the current project.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
	[Conditional("DEBUG")]
	public sealed class DisableModuleAttribute : Attribute
	{
		/// <summary>
		/// Module that is enabled.
		/// </summary>
		public DurianModule Module { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DisableModuleAttribute"/> class.
		/// </summary>
		/// <param name="module">Module that is enabled.</param>
		public DisableModuleAttribute(DurianModule module)
		{
			Module = module;
		}
	}
}
