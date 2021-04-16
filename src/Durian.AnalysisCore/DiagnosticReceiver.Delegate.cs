using System;
using Microsoft.CodeAnalysis;

namespace Durian
{
	public static partial class DiagnosticReceiver
	{
		/// <summary>
		/// Allows to report diagnostics through a delegate.
		/// </summary>
		public sealed class Delegate : IDiagnosticReceiver
		{
			private readonly ReportAction _reportAction;

			/// <summary>
			/// Initializes a new instance of the <see cref="Delegate"/> class.
			/// </summary>
			/// <param name="reportAction">Action that is performed when <see cref="ReportDiagnostic(DiagnosticDescriptor, Location, object[])"/> is called.</param>
			/// <exception cref="ArgumentNullException"><paramref name="reportAction"/> was <c>null</c>.</exception>
			public Delegate(ReportAction reportAction)
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
}
