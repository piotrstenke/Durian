namespace Durian.Configuration
{
	/// <summary>
	/// Determines how a <c>DefaultParam</c> method is generated.
	/// </summary>
	public enum DPMethodConvention
	{
		/// <summary>
		/// Uses default convention, which is <see cref="Copy"/>.
		/// </summary>
		Default,

		/// <summary>
		/// Copies contents of the method.
		/// </summary>
		Copy,

		/// <summary>
		/// Call the method.
		/// </summary>
		Call
	}
}
