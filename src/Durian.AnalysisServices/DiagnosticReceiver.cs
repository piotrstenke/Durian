// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using System;

namespace Durian.Analysis
{
	/// <summary>
	/// Static class that contains various <see cref="IDiagnosticReceiver"/> implementations.
	/// </summary>
	public sealed partial class DiagnosticReceiver
	{
		private readonly ReportAction.Direct _action;

		/// <summary>
		/// Initializes a new instance of the <see cref="DiagnosticReceiver"/> class.
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
		public DiagnosticReceiver(ReportAction.Direct action)
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
			if (descriptor is null)
			{
				throw new ArgumentNullException(nameof(descriptor));
			}

			_action.Invoke(Diagnostic.Create(descriptor, location, messageArgs));
		}

		/// <inheritdoc/>
		public void ReportDiagnostic(Diagnostic diagnostic)
		{
			if (diagnostic is null)
			{
				throw new ArgumentNullException(nameof(diagnostic));
			}

			_action.Invoke(diagnostic);
		}
	}
}
