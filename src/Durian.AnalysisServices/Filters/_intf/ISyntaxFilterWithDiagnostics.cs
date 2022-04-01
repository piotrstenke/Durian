// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Threading;

namespace Durian.Analysis.Filters
{
	/// <summary>
	/// A <see cref="ISyntaxFilter"/> that reports diagnostics about the received <see cref="CSharpSyntaxNode"/>s.
	/// </summary>
	public interface ISyntaxFilterWithDiagnostics : ISyntaxFilter
	{
		/// <summary>
		/// Decides, which <see cref="CSharpSyntaxNode"/>s collected by the <paramref name="syntaxReceiver"/> are valid for the current generator pass and returns a collection of <see cref="IMemberData"/> based on those <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		/// <param name="compilation">Current <see cref="ICompilationData"/>.</param>
		/// <param name="syntaxReceiver">A <see cref="IDurianSyntaxReceiver"/> that collected the <see cref="CSharpSyntaxNode"/>s that should be filtrated.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to report the diagnostics to.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		IEnumerable<IMemberData> Filtrate(ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, IDiagnosticReceiver diagnosticReceiver, CancellationToken cancellationToken = default);

		/// <summary>
		/// Decides, which <see cref="CSharpSyntaxNode"/>s from the <paramref name="collectedNodes"/> are valid for the current generator pass and returns a collection of <see cref="IMemberData"/> based on those <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		/// <param name="compilation">Current <see cref="ICompilationData"/>.</param>
		/// <param name="collectedNodes">A collection of <see cref="CSharpSyntaxNode"/>s to filtrate.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to report the diagnostics to.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		IEnumerable<IMemberData> Filtrate(ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, IDiagnosticReceiver diagnosticReceiver, CancellationToken cancellationToken = default);
	}
}
