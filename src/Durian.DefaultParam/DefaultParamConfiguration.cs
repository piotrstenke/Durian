using System.Collections.Generic;
using System.Diagnostics;

namespace Durian.DefaultParam
{
	/// <summary>
	/// Configures optional features of the <see cref="DefaultParamGenerator"/>.
	/// </summary>
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	public sealed record DefaultParamConfiguration
	{
		/// <summary>
		/// Determines whether to allow to override values of <see cref="DefaultParamAttribute"/>s for virtual methods. Defaults to <see langword="false"/>.
		/// </summary>
		public bool AllowOverridingOfDefaultParamValues { get; set; }

		/// <summary>
		/// Determines whether to allow adding new <see cref="DefaultParamAttribute"/>s to type parameters that don't have the <see cref="DefaultParamAttribute"/> defined on the base method. Defaults to <see langword="false"/>.
		/// </summary>
		public bool AllowAddingDefaultParamToNewParameters { get; set; }

		/// <summary>
		/// Determines whether to apply the <see langwrod="new"/> modifier to the generated member when possible instead of reporting an error. Defaults to <see langword="false"/>.
		/// </summary>
		public bool ApplyNewToGeneratedMembersWithEquivalentSignature { get; set; }

		/// <summary>
		/// Determines whether the generated method should call this method instead of copying its contents. Defaults to <see langword="false"/>.
		/// </summary>
		public bool CallInsteadOfCopying { get; set; }

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
			hashCode = (hashCode * -1521134295) + EqualityComparer<bool>.Default.GetHashCode(AllowOverridingOfDefaultParamValues);
			hashCode = (hashCode * -1521134295) + EqualityComparer<bool>.Default.GetHashCode(AllowAddingDefaultParamToNewParameters);
			hashCode = (hashCode * -1521134295) + EqualityComparer<bool>.Default.GetHashCode(ApplyNewToGeneratedMembersWithEquivalentSignature);
			hashCode = (hashCode * -1521134295) + EqualityComparer<bool>.Default.GetHashCode(CallInsteadOfCopying);
			return hashCode;
		}

		private string GetDebuggerDisplay()
		{
			return $"{{{base.ToString()}}}";
		}
	}
}
