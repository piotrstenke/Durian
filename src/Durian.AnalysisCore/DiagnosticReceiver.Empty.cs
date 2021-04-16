using Microsoft.CodeAnalysis;

namespace Durian
{
	public static partial class DiagnosticReceiver
	{
		/// <summary>
		/// A <see cref="IDiagnosticReceiver"/> that does not actually report any diagnostics.
		/// </summary>
		public static IDiagnosticReceiver Empty { get; } = new EmptyReceiver();

		/// <inheritdoc cref="Empty"/>
		private sealed class EmptyReceiver : IDiagnosticReceiver
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="EmptyReceiver"/> class.
			/// </summary>
			public EmptyReceiver()
			{
			}

			/// <inheritdoc/>
			public void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs)
			{
				// Do nothing
			}
		}
	}
}
