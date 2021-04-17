using Microsoft.CodeAnalysis;

namespace Durian
{
	/// <summary>
	/// A <see cref="IDiagnosticReceiver"/> that does not actually report any diagnostics.
	/// </summary>
	public sealed class EmptyDiagnosticReceiver : IDiagnosticReceiver
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
	}
}
