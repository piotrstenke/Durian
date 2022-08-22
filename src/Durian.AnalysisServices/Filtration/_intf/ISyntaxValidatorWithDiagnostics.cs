// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Filtration
{
	/// <summary>
	/// Validates <see cref="SyntaxNode"/>s and creates <see cref="IMemberData"/>s and reports appropriate diagnostics through a <see cref="IDiagnosticReceiver"/>.
	/// </summary>
	/// <typeparam name="T">Type of target <see cref="ISyntaxValidationContext"/>.</typeparam>
	public interface ISyntaxValidatorWithDiagnostics<T> : ISyntaxValidator<T> where T : ISyntaxValidationContext
	{
		/// <summary>
		/// Validates a <see cref="SyntaxNode"/> and returns a new instance of <see cref="IMemberData"/> if the validation was a success.
		/// </summary>
		/// <param name="context">Target <see cref="ISyntaxValidationContext"/>.</param>
		/// <param name="data"><see cref="IMemberData"/> that is returned if the validation succeeds.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to report the <see cref="Diagnostic"/>s to.</param>
		bool ValidateAndCreate(in T context, out IMemberData? data, IDiagnosticReceiver diagnosticReceiver);

		/// <summary>
		/// Validates a <see cref="SyntaxNode"/> and returns a new instance of <see cref="IMemberData"/> if the validation was a success.
		/// </summary>
		/// <param name="context">Target <see cref="PreValidationContext"/>.</param>
		/// <param name="data"><see cref="IMemberData"/> that is returned if the validation succeeds.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to report the <see cref="Diagnostic"/>s to.</param>
		bool ValidateAndCreate(in PreValidationContext context, [NotNullWhen(true)] out IMemberData? data, IDiagnosticReceiver diagnosticReceiver);
	}
}
