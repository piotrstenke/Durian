// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.EnumServices
{
	[AttributeUsage(AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
	public sealed class EnumServicesAttribute : Attribute
	{
		public string? ClassName { get; set; }

		public string? Namespace { get; set; }

		public string? Prefix { get; set; }

		public EnumServices Services { get; }

		public EnumServicesAttribute()
		{
			Services = EnumServices.All;
		}

		public EnumServicesAttribute(EnumServices services)
		{
			Services = services;
		}
	}
}
