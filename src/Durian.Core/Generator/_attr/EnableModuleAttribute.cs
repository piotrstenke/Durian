// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Info;
using System;

namespace Durian.Generator
{
	/// <summary>
	/// Declares that the specified Durian module is present in the compilation.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
	public sealed class EnableModuleAttribute : Attribute
	{
		/// <summary>
		/// Durian module to register.
		/// </summary>
		public DurianModule Module { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="EnableModuleAttribute"/> class.
		/// </summary>
		/// <param name="module">Durian module to register.</param>
		public EnableModuleAttribute(DurianModule module)
		{
			Module = module;
		}
	}
}
