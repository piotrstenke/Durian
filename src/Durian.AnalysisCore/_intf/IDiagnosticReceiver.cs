﻿using Microsoft.CodeAnalysis;

namespace Durian
{
	/// <summary>
	/// Provides a method for reporting a <see cref="Diagnostic"/> using a <see cref="DiagnosticDescriptor"/>.
	/// </summary>
	public interface IDiagnosticReceiver
	{
		/// <summary>
		/// Reports a <see cref="Diagnostic"/>.
		/// </summary>
		/// <param name="descriptor"><see cref="DiagnosticDescriptor"/> that is used to report the diagnostics.</param>
		/// <param name="location">Source <see cref="Location"/> of the reported diagnostic.</param>
		/// <param name="messageArgs">Arguments of the diagnostic message.</param>
		void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs);
	}
}
