// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;

namespace Durian
{
	/// <summary>
	/// Marks the generic type as a target for specialization.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	[Conditional("DEBUG")]
	public sealed class AllowSpecializationAttribute : Attribute
	{
		/// <summary>
		/// Name of the generated specialization interface. Defaults to '<c>ISpecialize</c>'.
		/// </summary>
		public string? InterfaceName { get; set; }

		/// <summary>
		/// Name of the class that is the main implementation of the type. Defaults to '<c>Spec</c>'.
		/// </summary>
		public string? TemplateName { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="AllowSpecializationAttribute"/> class.
		/// </summary>
		public AllowSpecializationAttribute()
		{
		}
	}
}
