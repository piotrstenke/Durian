using Microsoft.CodeAnalysis;

namespace Durian
{
	/// <summary>
	/// Provides a method for reporting a diagnostic messages.
	/// </summary>
	public interface IDiagnosticReceiver
	{
		/// <summary>
		/// Reports a diagnostic.
		/// </summary>
		/// <param name="descriptor"><see cref="DiagnosticDescriptor"/> that is used to report the diagnostics.</param>
		/// <param name="location">Source <see cref="Location"/> of the reported diagnostic.</param>
		/// <param name="messageArgs">Arguments of the diagnostic message.</param>
		void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs);
	}
}
