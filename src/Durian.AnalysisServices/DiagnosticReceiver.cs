// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;

namespace Durian.Generator
{
	/// <summary>
	/// A <see cref="IDirectDiagnosticReceiver"/> that invokes a <see cref="DirectReportAction"/> when the <see cref="ReportDiagnostic(Diagnostic)"/> method is called.
	/// </summary>
	public sealed class DiagnosticReceiver : IDirectDiagnosticReceiver
	{
		private readonly DirectReportAction _action;

		/// <summary>
		/// Initializes a new instance of the <see cref="ReadonlyContextualDiagnosticReceiver{T}"/> class.
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
		public DiagnosticReceiver(DirectReportAction action)
		{
			if (action is null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			_action = action;
		}

		/// <inheritdoc/>
		public void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs)
		{
			_action.Invoke(Diagnostic.Create(descriptor, location, messageArgs));
		}

		/// <inheritdoc/>
		public void ReportDiagnostic(Diagnostic diagnostic)
		{
			_action.Invoke(diagnostic);
		}
	}
}
