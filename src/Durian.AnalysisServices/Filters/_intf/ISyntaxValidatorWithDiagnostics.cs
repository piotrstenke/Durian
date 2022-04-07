// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.Filters
{
	/// <summary>
	/// Validates <see cref="CSharpSyntaxNode"/>s and creates <see cref="IMemberData"/>s and reports appropriate diagnostics through a <see cref="IDiagnosticReceiver"/>.
	/// </summary>
	/// <typeparam name="T">Type of target <see cref="ISyntaxValidatorContext"/>.</typeparam>
	public interface ISyntaxValidatorWithDiagnostics<T> : ISyntaxValidator<T> where T : ISyntaxValidatorContext
	{
		/// <summary>
		/// Validates a <see cref="CSharpSyntaxNode"/> and returns a new instance of <see cref="IMemberData"/> if the validation was a success.
		/// </summary>
		/// <param name="context">Target <see cref="ISyntaxValidatorContext"/>.</param>
		/// <param name="data"><see cref="IMemberData"/> that is returned if the validation succeeds.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to report the <see cref="Diagnostic"/>s to.</param>
		bool ValidateAndCreate(in T context, out IMemberData? data, IDiagnosticReceiver diagnosticReceiver);

		/// <summary>
		/// Validates a <see cref="CSharpSyntaxNode"/> and returns a new instance of <see cref="IMemberData"/> if the validation was a success.
		/// </summary>
		/// <param name="context">Target <see cref="ValidationDataProviderContext"/>.</param>
		/// <param name="data"><see cref="IMemberData"/> that is returned if the validation succeeds.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to report the <see cref="Diagnostic"/>s to.</param>
		bool ValidateAndCreate(in ValidationDataProviderContext context, [NotNullWhen(true)] out IMemberData? data, IDiagnosticReceiver diagnosticReceiver);
	}
}
