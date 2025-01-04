namespace Durian.Analysis.CodeGeneration
{
	/// <summary>
	/// Determines when to generate XML documentation.
	/// </summary>
	public enum GenerateDocumentation
	{
		/// <summary>
		/// The XML documentation is never generated.
		/// </summary>
		Never = 0,

		/// <summary>
		/// The XML documentation is always generated, even if the source symbol has no documentation.
		/// </summary>
		Always = 1,

		/// <summary>
		/// The XML documentation is generated only if the source symbol has documentation.
		/// </summary>
		WhenPossible = 2
	};
}
