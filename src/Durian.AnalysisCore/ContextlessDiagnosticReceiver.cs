using System;
using Microsoft.CodeAnalysis;

namespace Durian
{
	/// <summary>
	/// A <see cref="IDiagnosticReceiver"/> to report diagnostics through a delegate without a context object.
	/// </summary>
	public sealed class ContextlessDiagnosticReceiver : IDiagnosticReceiver
	{
		private readonly ReportAction _reportAction;

		/// <summary>
		/// Initializes a new instance of the <see cref="ContextlessDiagnosticReceiver"/> class.
		/// </summary>
		/// <param name="reportAction">Action that is performed when <see cref="ReportDiagnostic(DiagnosticDescriptor, Location, object[])"/> is called.</param>
		/// <exception cref="ArgumentNullException"><paramref name="reportAction"/> is <c>null</c>.</exception>
		public ContextlessDiagnosticReceiver(ReportAction reportAction)
		{
			if (reportAction is null)
			{
				throw new ArgumentNullException(nameof(reportAction));
			}

			_reportAction = reportAction;
		}

		/// <inheritdoc/>
		public void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs)
		{
			_reportAction.Invoke(descriptor, location, messageArgs);
		}
	}
}
