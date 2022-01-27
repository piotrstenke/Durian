// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Configuration
{
	/// <summary>
	/// Configures how members with the <see cref="DefaultParamAttribute"/> are handled by the generator. Applies to all members in the current scope.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
	public sealed class DefaultParamScopedConfigurationAttribute : Attribute
	{
		/// <inheritdoc cref="DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible"/>
		public bool ApplyNewModifierWhenPossible { get; set; } = true;

		/// <inheritdoc cref="DefaultParamConfigurationAttribute.MethodConvention"/>
		public DPMethodConvention MethodConvention { get; set; }

		/// <inheritdoc cref="DefaultParamConfigurationAttribute.TargetNamespace"/>
		public string? TargetNamespace { get; set; }

		/// <inheritdoc cref="DefaultParamConfigurationAttribute.TypeConvention"/>
		public DPTypeConvention TypeConvention { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamScopedConfigurationAttribute"/> class.
		/// </summary>
		public DefaultParamScopedConfigurationAttribute()
		{
		}
	}
}