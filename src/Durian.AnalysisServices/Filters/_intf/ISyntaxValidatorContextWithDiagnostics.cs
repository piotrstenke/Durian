// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Filters
{
	/// <summary>
	/// /<see cref="ISyntaxValidatorContext"/> that also provides a <see cref="IDiagnosticReceiver"/> to report appropriate <see cref="Diagnostic"/>s.
	/// </summary>
	public interface ISyntaxValidatorContextWithDiagnostics : ISyntaxValidatorContext
	{
		/// <summary>
		/// <see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.
		/// </summary>
		IDiagnosticReceiver DiagnosticReceiver { get; }
	}
}
