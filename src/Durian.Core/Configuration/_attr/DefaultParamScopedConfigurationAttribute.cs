using System;

namespace Durian.Configuration
{
	/// <summary>
	/// Configures how members with the <see cref="DefaultParamAttribute"/> are handled by the generator. Applies to all members in the current scope.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
	public sealed class DefaultParamScopedConfigurationAttribute : Attribute
	{
		/// <inheritdoc cref="DefaultParamConfigurationAttribute.MethodConvention"/>
		public DPMethodConvention MethodConvention { get; set; } = DPMethodConvention.Default;

		/// <inheritdoc cref="DefaultParamConfigurationAttribute.TypeConvention"/>
		public DPTypeConvention TypeConvention { get; set; } = DPTypeConvention.Default;

		/// <inheritdoc cref="DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible"/>
		public bool ApplyNewModifierWhenPossible { get; set; } = true;

		/// <inheritdoc cref="DefaultParamConfigurationAttribute.TargetNamespace"/>
		public string? TargetNamespace { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamScopedConfigurationAttribute"/> class.
		/// </summary>
		public DefaultParamScopedConfigurationAttribute()
		{
		}
	}
}
