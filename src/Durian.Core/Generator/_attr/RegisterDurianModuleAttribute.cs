// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Info;

namespace Durian.Generator
{
	/// <summary>
	/// Declares that the specified Durian module is present in the compilation.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
	public sealed class RegisterDurianModuleAttribute : Attribute
	{
		/// <summary>
		/// Durian module to register.
		/// </summary>
		public DurianModule Module { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="RegisterDurianModuleAttribute"/> class.
		/// </summary>
		/// <param name="module">Durian module to register.</param>
		public RegisterDurianModuleAttribute(DurianModule module)
		{
			Module = module;
		}
	}
}