namespace Durian.Configuration
{
	/// <summary>
	/// Determines how a <c>DefaultParam</c> type is generated.
	/// </summary>
	public enum DPTypeConvention
	{
		/// <summary>
		/// Uses default convention, which is <see cref="Copy"/>.
		/// </summary>
		Default,

		/// <summary>
		/// Copies contents of the type.
		/// </summary>
		Copy,

		/// <summary>
		/// Inherits the type.
		/// </summary>
		Inherit
	}
}
