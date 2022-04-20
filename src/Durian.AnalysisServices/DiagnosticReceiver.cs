// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	/// <summary>
	/// Basic implementation of the <see cref="IDiagnosticReceiver"/> interface.
	/// </summary>
	public sealed partial class DiagnosticReceiver : IDiagnosticReceiver
	{
		private readonly ReportAction.Basic _basicAction;
		private readonly ReportAction.Direct _directAction;

		/// <summary>
		/// Initializes a new instance of the <see cref="DiagnosticReceiver"/> class.
		/// </summary>
		/// <param name="action">Action that is executed when either <see cref="ReportDiagnostic(Diagnostic)"/> or <see cref="ReportDiagnostic(DiagnosticDescriptor, Location?, object?[])"/> is called.</param>
		/// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
		public DiagnosticReceiver(ReportAction.Direct action)
		{
			if (action is null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			_directAction = action;
			_basicAction = (diagnosticReceiver, location, messageArgs) => _directAction(Diagnostic.Create(diagnosticReceiver, location, messageArgs));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DiagnosticReceiver"/> class.
		/// </summary>
		/// <param name="direct">Action that is executed when <see cref="ReportDiagnostic(Diagnostic)"/> is called.</param>
		/// <param name="basic">Action that is executed when <see cref="ReportDiagnostic(DiagnosticDescriptor, Location?, object?[])"/> is called.</param>
		/// <exception cref="ArgumentNullException"><paramref name="direct"/> is <see langword="null"/>. -or- <paramref name="direct"/> is <see langword="null"/>.</exception>
		public DiagnosticReceiver(ReportAction.Direct direct, ReportAction.Basic basic)
		{
			if (direct is null)
			{
				throw new ArgumentNullException(nameof(direct));
			}

			if (basic is null)
			{
				throw new ArgumentNullException(nameof(basic));
			}

			_directAction = direct;
			_basicAction = basic;
		}

		/// <inheritdoc/>
		public void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs)
		{
			if (descriptor is null)
			{
				throw new ArgumentNullException(nameof(descriptor));
			}

			_basicAction.Invoke(descriptor, location, messageArgs);
		}

		/// <inheritdoc/>
		public void ReportDiagnostic(Diagnostic diagnostic)
		{
			if (diagnostic is null)
			{
				throw new ArgumentNullException(nameof(diagnostic));
			}

			_directAction.Invoke(diagnostic);
		}
	}
}
