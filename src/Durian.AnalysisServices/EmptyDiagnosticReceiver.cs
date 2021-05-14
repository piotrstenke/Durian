using Microsoft.CodeAnalysis;

namespace Durian.Generator
{
	/// <summary>
	/// A <see cref="IDirectDiagnosticReceiver"/> that does not actually report any diagnostics.
	/// </summary>
	public sealed class EmptyDiagnosticReceiver : IDirectDiagnosticReceiver
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EmptyDiagnosticReceiver"/> class.
		/// </summary>
		public EmptyDiagnosticReceiver()
		{
		}

		/// <inheritdoc/>
		public void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs)
		{
			// Do nothing
		}

		/// <inheritdoc/>
		public void ReportDiagnostic(Diagnostic diagnostic)
		{
			// Do nothing
		}
	}
}
