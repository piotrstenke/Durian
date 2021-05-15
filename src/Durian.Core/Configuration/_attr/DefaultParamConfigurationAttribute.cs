using System;

namespace Durian.Configuration
{
	/// <summary>
	/// Configures how members with the <see cref="DefaultParamAttribute"/> are handled by the generator. Applies only to this member.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
	public sealed class DefaultParamConfigurationAttribute : Attribute
	{
		/// <inheritdoc cref="DPMethodConvention"/>
		public DPMethodConvention MethodConvention { get; set; }

		/// <inheritdoc cref="DPTypeConvention"/>
		public DPTypeConvention TypeConvention { get; set; }

		/// <summary>
		/// Determines whether to apply the <see langword="new"/> modifier to the generated member when possible instead of reporting an error. Defaults to <see langword="false"/>.
		/// </summary>
		public bool ApplyNewModifierWhenPossible { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamConfigurationAttribute"/> class.
		/// </summary>
		public DefaultParamConfigurationAttribute()
		{
		}
	}
}
