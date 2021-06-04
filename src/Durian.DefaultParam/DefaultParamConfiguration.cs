using System;
using Durian.Configuration;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// Configures optional features of the <see cref="DefaultParamGenerator"/>.
	/// </summary>
	/// <remarks><para>NOTE: This class implements the <see cref="IEquatable{T}"/> - two values are compared by their values, not references.</para></remarks>
	public sealed class DefaultParamConfiguration : IEquatable<DefaultParamConfiguration>
	{
		private string? _targetNamespace;

		/// <summary>
		/// Determines whether to apply the <see langword="new"/> modifier to the generated member when possible instead of reporting an error. Defaults to <see langword="true"/>.
		/// </summary>
		public bool ApplyNewModifierWhenPossible { get; set; } = true;

		/// <inheritdoc cref="DefaultParamConfigurationAttribute.TypeConvention"/>
		public DPTypeConvention TypeConvention { get; set; } = DPTypeConvention.Default;

		/// <inheritdoc cref="DefaultParamConfigurationAttribute.MethodConvention"/>
		public DPMethodConvention MethodConvention { get; set; } = DPMethodConvention.Default;

		/// <summary>
		/// Specifies the namespace where the target member should be generated in.
		/// </summary>
		/// <remarks>The value will be set to <c>global</c> if the global namespace should be used or to <see langword="null"/> if the namespace of the original member should be used.
		/// <para>Empty or white space only value is treated as <see langword="null"/>.</para></remarks>
		public string? TargetNamespace
		{
			get => _targetNamespace;
			set => _targetNamespace = string.IsNullOrWhiteSpace(value) ? null : value;
		}

		/// <summary>
		/// Returns a new instance of <see cref="DefaultParamConfiguration"/> with all values set to <see langword="default"/>.
		/// </summary>
		public static DefaultParamConfiguration Default => new();

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamConfiguration"/> class.
		/// </summary>
		public DefaultParamConfiguration()
		{
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int hashCode = -726504116;
			hashCode = (hashCode * -1521134295) + ApplyNewModifierWhenPossible.GetHashCode();
			hashCode = (hashCode * -1521134295) + MethodConvention.GetHashCode();
			hashCode = (hashCode * -1521134295) + TypeConvention.GetHashCode();
			return hashCode;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (obj is not DefaultParamConfiguration other)
			{
				return false;
			}

			return other == this;
		}

		/// <inheritdoc/>
		public bool Equals(DefaultParamConfiguration other)
		{
			return other == this;
		}

		/// <inheritdoc/>
		public static bool operator ==(DefaultParamConfiguration a, DefaultParamConfiguration b)
		{
			return
				a.ApplyNewModifierWhenPossible == b.ApplyNewModifierWhenPossible &&
				a.MethodConvention == b.MethodConvention &&
				a.TypeConvention == b.TypeConvention &&
				a.TargetNamespace == b.TargetNamespace;
		}

		/// <inheritdoc/>
		public static bool operator !=(DefaultParamConfiguration a, DefaultParamConfiguration b)
		{
			return !(a == b);
		}
	}
}
