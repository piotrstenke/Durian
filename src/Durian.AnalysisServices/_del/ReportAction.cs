using Microsoft.CodeAnalysis;

namespace Durian.Generator
{
	/// <summary>
	/// A delegate that defines all arguments that are needed to perform a diagnostic report.
	/// </summary>
	/// <param name="descriptor"><see cref="DiagnosticDescriptor"/> that is used to report the diagnostics.</param>
	/// <param name="location">Source <see cref="Location"/> of the reported diagnostic.</param>
	/// <param name="messageArgs">Arguments of the diagnostic message.</param>
	public delegate void ReportAction(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs);
}
