// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using System;

namespace Durian.Analysis
{
	/// <summary>
	/// Provides a method for reporting a <see cref="Diagnostic"/> using a <see cref="DiagnosticDescriptor"/>.
	/// </summary>
	public interface IDiagnosticReceiver
	{
		/// <summary>
		/// Reports a <see cref="Diagnostic"/> created from the specified <paramref name="descriptor"/>.
		/// </summary>
		/// <param name="descriptor"><see cref="DiagnosticDescriptor"/> that is used to create the <see cref="Diagnostic"/>.</param>
		/// <param name="location">Source <see cref="Location"/> of the reported diagnostic.</param>
		/// <param name="messageArgs">Arguments of the diagnostic message.</param>
		/// <exception cref="ArgumentNullException"><paramref name="descriptor"/> is <see langword="null"/>.</exception>
		void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs);
	}
}