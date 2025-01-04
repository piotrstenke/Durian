namespace Durian.Analysis
{
	/// <summary>
	/// Represents stage of source generation.
	/// </summary>
	public enum GeneratorState
	{
		/// <summary>
		/// No valid state.
		/// </summary>
		None = 0,

		/// <summary>
		/// The generator is still running.
		/// </summary>
		Running = 1,

		/// <summary>
		/// The generator pass has successfully ended.
		/// </summary>
		Success = 2,

		/// <summary>
		/// The generator pass has failed.
		/// </summary>
		Failed = 3
	}
}
