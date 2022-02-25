// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	/// <summary>
	/// Contains all report ReportAction* delegates.
	/// </summary>
	public static class ReportAction
	{
		/// <summary>
		/// A delegate that defines all arguments that are needed to perform a diagnostic report.
		/// </summary>
		/// <param name="descriptor"><see cref="DiagnosticDescriptor"/> that is used to report the diagnostics.</param>
		/// <param name="location">Source <see cref="Location"/> of the reported diagnostic.</param>
		/// <param name="messageArgs">Arguments of the diagnostic message.</param>
		public delegate void Basic(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs);

		/// <summary>
		/// A delegate that reports a <see cref="Diagnostic"/>.
		/// </summary>
		/// <param name="diagnostic"><see cref="Diagnostic"/> to report.</param>
		public delegate void Direct(Diagnostic diagnostic);

		/// <summary>
		/// A delegate that defines all arguments that are needed to perform a diagnostic report on the specified <paramref name="context"/>.
		/// </summary>
		/// <typeparam name="T">Type of the <paramref name="context"/>.</typeparam>
		/// <param name="context">Context to report the diagnostics to.</param>
		/// <param name="descriptor"><see cref="DiagnosticDescriptor"/> that is used to report the diagnostics.</param>
		/// <param name="location">Source <see cref="Location"/> of the reported diagnostic.</param>
		/// <param name="messageArgs">Arguments of the diagnostic message.</param>
		public delegate void Contextual<in T>(T context, DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs) where T : struct;

		/// <summary>
		/// A delegate that reports a <see cref="Diagnostic"/> on the specified <paramref name="context"/>.
		/// </summary>
		/// <typeparam name="T">Type of the <paramref name="context"/>.</typeparam>
		/// <param name="context">Context to report the diagnostics to.</param>
		/// <param name="diagnostic"><see cref="Diagnostic"/> to report.</param>
		public delegate void DirectContextual<in T>(T context, Diagnostic diagnostic) where T : struct;

		/// <inheritdoc cref="Contextual{T}"/>
		public delegate void ReadonlyContextual<T>(in T context, DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs) where T : struct;

		/// <inheritdoc cref="DirectContextual{T}"/>
		public delegate void ReadonlyDirectContextual<T>(in T context, Diagnostic diagnostic) where T : struct;
	}
}