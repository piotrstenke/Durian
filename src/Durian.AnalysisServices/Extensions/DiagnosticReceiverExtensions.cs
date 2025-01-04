using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Extensions;

/// <summary>
/// Contains extension methods for the <see cref="IDiagnosticReceiver"/> interface.
/// </summary>
public static class DiagnosticReceiverExtensions
{
	/// <inheritdoc cref="ReportDiagnostic(IDiagnosticReceiver, DiagnosticDescriptor, ISymbol?, object[])"/>
	public static void ReportDiagnostic(this IDiagnosticReceiver diagnosticReceiver, DiagnosticDescriptor descriptor)
	{
		if (diagnosticReceiver is null)
		{
			throw new ArgumentNullException(nameof(diagnosticReceiver));
		}

		if (descriptor is null)
		{
			return;
		}

		diagnosticReceiver.ReportDiagnostic(descriptor, Location.None);
	}

	/// <inheritdoc cref="ReportDiagnostic(IDiagnosticReceiver, DiagnosticDescriptor, ISymbol?, object[])"/>
	public static void ReportDiagnostic(this IDiagnosticReceiver diagnosticReceiver, DiagnosticDescriptor? descriptor, ISymbol? symbol)
	{
		if (diagnosticReceiver is null)
		{
			throw new ArgumentNullException(nameof(diagnosticReceiver));
		}

		if (descriptor is null)
		{
			return;
		}

		diagnosticReceiver.ReportDiagnostic(descriptor, symbol?.Locations.FirstOrDefault(), symbol);
	}

	/// <summary>
	/// Reports a <see cref="Diagnostic"/> created from the specified <paramref name="descriptor"/>.
	/// </summary>
	/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report the <see cref="Diagnostic"/>.</param>
	/// <param name="descriptor"><see cref="DiagnosticDescriptor"/> that is used to create the <see cref="Diagnostic"/>.</param>
	/// <param name="symbol"><see cref="ISymbol"/> that caused the report.</param>
	/// <param name="messageArgs">Arguments of the diagnostic message.</param>
	/// <exception cref="ArgumentNullException"><paramref name="diagnosticReceiver"/> is <see langword="null"/>.</exception>
	public static void ReportDiagnostic(this IDiagnosticReceiver diagnosticReceiver, DiagnosticDescriptor? descriptor, ISymbol? symbol, params object?[]? messageArgs)
	{
		if (diagnosticReceiver is null)
		{
			throw new ArgumentNullException(nameof(diagnosticReceiver));
		}

		if (descriptor is null)
		{
			return;
		}

		Location? location = symbol?.Locations.FirstOrDefault();
		object?[] args = GetArgsWithSymbol(symbol, messageArgs);
		diagnosticReceiver.ReportDiagnostic(descriptor, location, args);
	}

	private static object?[] GetArgsWithSymbol(ISymbol? symbol, object?[]? initialArgs)
	{
		if (initialArgs is null || initialArgs.Length == 0)
		{
			return new object?[] { symbol };
		}

		object?[] args = new object[initialArgs.Length + 1];
		args[0] = symbol;
		Array.Copy(initialArgs, 0, args, 1, initialArgs.Length);

		return args;
	}
}
