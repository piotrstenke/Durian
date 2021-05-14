using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Generator
{
	/// <summary>
	/// Determines what information should a <see cref="IGeneratorSyntaxFilterWithDiagnostics"/> emit.
	/// </summary>
	public enum FilterMode
	{
		/// <summary>
		/// Filter emits no additional information.
		/// </summary>
		None,

		/// <summary>
		/// Filter reports <see cref="Diagnostic"/>s for the invalid <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		Diagnostics,

		/// <summary>
		/// Filter creates log files for the invalid <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		Logs,

		/// <summary>
		/// Filter both creates log files and reports <see cref="Diagnostic"/>s for the invalid <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		Both
	}
}
