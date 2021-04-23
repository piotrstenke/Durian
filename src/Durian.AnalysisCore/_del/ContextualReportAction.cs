using Microsoft.CodeAnalysis;

namespace Durian
{
	/// <summary>
	/// A delegate that defines all arguments that are needed to perform a diagnostic report on the specified <paramref name="context"/>.
	/// </summary>
	/// <typeparam name="T">Type of the <paramref name="context"/>.</typeparam>
	/// <param name="context">Context to report the diagnostics to.</param>
	/// <param name="descriptor"><see cref="DiagnosticDescriptor"/> that is used to report the diagnostics.</param>
	/// <param name="location">Source <see cref="Location"/> of the reported diagnostic.</param>
	/// <param name="messageArgs">Arguments of the diagnostic message.</param>
	public delegate void ContextualReportAction<in T>(T context, DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs) where T : struct;
}
