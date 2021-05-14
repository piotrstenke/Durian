using System;

namespace Durian.Generator
{
	/// <summary>
	/// Types of logs that a source generator can produce.
	/// </summary>
	[Flags]
	public enum GeneratorLogs
	{
		/// <summary>
		/// No logs.
		/// </summary>
		None = 0,
		/// <summary>
		/// Log containing exceptions.
		/// </summary>
		Exception = 1,
		/// <summary>
		/// Log containing input and output syntax node.
		/// </summary>
		InputOutput = 2,
		/// <summary>
		/// Log containing static syntax node.
		/// </summary>
		Node = 4,
		/// <summary>
		/// Log containing diagnostics that were produced.
		/// </summary>
		Diagnostics = 8,
		/// <summary>
		/// Includes all types of logs.
		/// </summary>
		All = Exception | InputOutput | Node | Diagnostics
	}
}
