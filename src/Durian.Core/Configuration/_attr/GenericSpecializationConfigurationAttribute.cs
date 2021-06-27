// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Configuration
{
	/// <summary>
	/// Configures how generic specializations are generated.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class GenericSpecializationConfigurationAttribute : Attribute
	{
		/// <summary>
		/// Determines whether to force the specialization class to inherit the main implementation class. Defaults to <see langword="false"/>.
		/// </summary>
		public bool ForceInherit { get; set; }

		/// <inheritdoc cref="GenSpecImportOptions"/>
		public GenSpecImportOptions ImportOptions { get; }

		/// <summary>
		/// Name of the generated specialization interface. Defaults to '<c>ISpecialize</c>'.
		/// </summary>
		public string? InterfaceName { get; set; }

		/// <summary>
		/// Name of the class that is the main implementation of the type. Defaults to '<c>Spec</c>'.
		/// </summary>
		public string? TemplateName { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="GenericSpecializationConfigurationAttribute"/> class.
		/// </summary>
		public GenericSpecializationConfigurationAttribute()
		{
		}
	}
}
