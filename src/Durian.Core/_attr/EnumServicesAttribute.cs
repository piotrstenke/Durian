// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian
{
	/// <summary>
	/// Specifies extension methods that should be generated for the specified <see langword="enum"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
	public sealed class EnumServicesAttribute : Attribute
	{
		/// <summary>
		/// Determines the accessibility of the generated extension methods.
		/// </summary>
		public GeneratedTypeAccess Accessibility { get; set; }

		/// <summary>
		/// Determines whether to allow custom implementation of a generated extension method.
		/// If <see langword="true"/>, no diagnostic will be reported if a method with the same signature
		/// as the generated one already exists in the type. Defaults to <see langword="false"/>.
		/// </summary>
		public bool AllowCustomization { get; set; }

		/// <summary>
		/// Name of class that should contain the generated extension methods.
		/// If the class already exists, it must be <see langword="partial"/>.
		/// </summary>
		/// <remarks>Setting this property to <see langword="null"/> will use the default value specified for the current scope, or 'EnumExtensions' if no default value found.</remarks>
		public string? ClassName { get; set; }

		/// <summary>
		/// Namespace where the generated extension class will be placed.
		/// </summary>
		/// <remarks>Setting this property to <see langword="null"/> will use the default value specified for the current scope, or the current namespace if no default value found.</remarks>
		public string? Namespace { get; set; }

		/// <summary>
		/// Prefix that should be applied to generated extension methods.
		/// </summary>
		public string? Prefix { get; set; }

		/// <summary>
		/// Specifies extension methods that should be generated for the target <see langword="enum"/>.
		/// </summary>
		public EnumServices Services { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="EnumServicesAttribute"/> class.
		/// </summary>
		/// <remarks>Using this constructor is equivalent to specifying <see cref="EnumServices.All"/>.</remarks>
		public EnumServicesAttribute()
		{
			Services = EnumServices.All;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EnumServicesAttribute"/> class.
		/// </summary>
		/// <param name="services">Specifies extension methods that should be generated for the target <see langword="enum"/>.</param>
		public EnumServicesAttribute(EnumServices services)
		{
			Services = services;
		}
	}
}
