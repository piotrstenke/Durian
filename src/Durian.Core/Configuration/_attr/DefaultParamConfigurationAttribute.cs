// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Configuration
{
	/// <summary>
	/// Configures how members with the <see cref="DefaultParamAttribute"/> are handled by the generator. Applies only to this member.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
	public sealed class DefaultParamConfigurationAttribute : Attribute
	{
		/// <summary>
		/// Determines whether to apply the <see langword="new"/> modifier to the generated member when possible instead of reporting an error. Defaults to <see langword="true"/>.
		/// </summary>
		public bool ApplyNewModifierWhenPossible { get; set; } = true;

		/// <summary>
		/// Determines, how the <c>DefaultParam</c> generator generates a method. The default value is <see cref="DPMethodConvention.Call"/>.
		/// </summary>
		public DPMethodConvention MethodConvention { get; set; } = DPMethodConvention.Default;

		/// <summary>
		/// Specifies the namespace where the target member should be generated in.
		/// </summary>
		/// <remarks>Set this property to <c>global</c> to use the global namespace or to <see langword="null"/> to use namespace of the original member.</remarks>
		public string? TargetNamespace { get; set; }

		/// <summary>
		/// Determines, how the <c>DefaultParam</c> generator generates a type. The default value is <see cref="DPTypeConvention.Inherit"/>.
		/// </summary>
		public DPTypeConvention TypeConvention { get; set; } = DPTypeConvention.Default;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamConfigurationAttribute"/> class.
		/// </summary>
		public DefaultParamConfigurationAttribute()
		{
		}
	}
}
