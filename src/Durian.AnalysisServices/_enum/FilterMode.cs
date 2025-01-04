using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	/// <summary>
	/// Determines how diagnostic information should be emitted.
	/// </summary>
	public enum FilterMode
	{
		/// <summary>
		/// Filter emits no additional information.
		/// </summary>
		None = 0,

		/// <summary>
		/// Filter reports <see cref="Diagnostic"/>s for the invalid <see cref="SyntaxNode"/>s.
		/// </summary>
		Diagnostics = 1,

		/// <summary>
		/// Filter creates log files for the invalid <see cref="SyntaxNode"/>s.
		/// </summary>
		Logs = 2,

		/// <summary>
		/// Filter both creates log files and reports <see cref="Diagnostic"/>s for the invalid <see cref="SyntaxNode"/>s.
		/// </summary>
		Both = 3
	}
}
