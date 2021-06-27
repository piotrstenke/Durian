// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian
{
	/// <summary>
	/// Marks the generic type as a target for specialization.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class AllowSpecializationAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AllowSpecializationAttribute"/> class.
		/// </summary>
		public AllowSpecializationAttribute()
		{
		}
	}
}
