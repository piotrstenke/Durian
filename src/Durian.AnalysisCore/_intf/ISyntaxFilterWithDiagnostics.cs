using System.Collections.Generic;
using System.Threading;
using Durian.Data;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian
{
	/// <summary>
	/// A <see cref="ISyntaxFilter"/> that reports diagnostics about the receiver <see cref="CSharpSyntaxNode"/>s.
	/// </summary>
	public interface ISyntaxFilterWithDiagnostics : ISyntaxFilter
	{
		/// <summary>
		/// Decides, which <see cref="CSharpSyntaxNode"/>s collected by the <paramref name="syntaxReceiver"/> are valid for the current generator pass and returns a collection of <see cref="IMemberData"/> based on those <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to report the diagnostics to.</param>
		/// <param name="compilation">Current <see cref="ICompilationData"/>.</param>
		/// <param name="syntaxReceiver">A <see cref="IDurianSyntaxReceiver"/> that collected the <see cref="CSharpSyntaxNode"/>s that should be filtrated.</param>
		/// <param name="cancellationToken">Target <see cref="CancellationToken"/>.</param>
		IEnumerable<IMemberData> Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken = default);

		/// <summary>
		/// Decides, which <see cref="CSharpSyntaxNode"/>s from the <paramref name="collectedNodes"/> are valid for the current generator pass and returns a collection of <see cref="IMemberData"/> based on those <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to report the diagnostics to.</param>
		/// <param name="compilation">Current <see cref="ICompilationData"/>.</param>
		/// <param name="collectedNodes">A collection of <see cref="CSharpSyntaxNode"/>s to filtrate.</param>
		/// <param name="cancellationToken">Target <see cref="CancellationToken"/>.</param>
		IEnumerable<IMemberData> Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken = default);
	}
}
