﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Info;

namespace Durian.Generator
{
	/// <summary>
	/// Specifies that the target <see cref="DurianModule"/> is enabled for the current project.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
	public sealed class EnableModuleAttribute : Attribute
	{
		/// <summary>
		/// Module that is enabled.
		/// </summary>
		public DurianModule Module { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGeneratedAttribute"/> class.
		/// </summary>
		/// <param name="module">Module that is enabled.</param>
		public EnableModuleAttribute(DurianModule module)
		{
			Module = module;
		}
	}
}
