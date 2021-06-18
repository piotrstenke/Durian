// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Extensions
{
	/// <summary>
	/// Contains extension methods for the <see cref="IDiagnosticReceiver"/> interface.
	/// </summary>
	public static class DiagnosticReceiverExtensions
	{
		/// <inheritdoc cref="ReportDiagnostic(IDiagnosticReceiver, DiagnosticDescriptor, ISymbol?, object[])"/>
		public static void ReportDiagnostic(this IDiagnosticReceiver diagnosticReceiver, DiagnosticDescriptor descriptor)
		{
			diagnosticReceiver.ReportDiagnostic(descriptor, Location.None);
		}

		/// <inheritdoc cref="ReportDiagnostic(IDiagnosticReceiver, DiagnosticDescriptor, ISymbol?, object[])"/>
		public static void ReportDiagnostic(this IDiagnosticReceiver diagnosticReceiver, DiagnosticDescriptor descriptor, ISymbol? symbol)
		{
			diagnosticReceiver.ReportDiagnostic(descriptor, symbol?.Locations.FirstOrDefault(), symbol);
		}

		/// <summary>
		/// Reports a <see cref="Diagnostic"/> created from the specified <paramref name="descriptor"/>.
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report the <see cref="Diagnostic"/>.</param>
		/// <param name="descriptor"><see cref="DiagnosticDescriptor"/> that is used to create the <see cref="Diagnostic"/>.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that caused the report.</param>
		/// <param name="messageArgs">Arguments of the diagnostic message.</param>
		public static void ReportDiagnostic(this IDiagnosticReceiver diagnosticReceiver, DiagnosticDescriptor descriptor, ISymbol? symbol, params object?[]? messageArgs)
		{
			Location? location = symbol?.Locations.FirstOrDefault();

			if (messageArgs is null)
			{
				diagnosticReceiver.ReportDiagnostic(descriptor, location, symbol);
			}
			else
			{
				object?[]? args = new object[messageArgs.Length + 1];
				args[0] = symbol;
				Array.Copy(messageArgs, 0, args, 1, messageArgs.Length);

				diagnosticReceiver.ReportDiagnostic(descriptor, location, args);
			}
		}
	}
}
