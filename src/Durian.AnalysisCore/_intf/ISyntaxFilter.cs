using System.Collections.Generic;
using System.Threading;
using Durian.Data;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian
{
	/// <summary>
	/// Filtrates the <see cref="CSharpSyntaxNode"/>s collected by a <see cref="IDurianSyntaxReceiver"/>.
	/// </summary>
	public interface ISyntaxFilter
	{
		/// <summary>
		/// Decides, which <see cref="CSharpSyntaxNode"/>s collected by the <paramref name="syntaxReceiver"/> are valid for the current generator pass and returns a collection of <see cref="IMemberData"/> based on those <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		/// <param name="compilation">Current <see cref="ICompilationData"/>.</param>
		/// <param name="syntaxReceiver">A <see cref="IDurianSyntaxReceiver"/> that collected the <see cref="CSharpSyntaxNode"/>s that should be filtrated.</param>
		/// <param name="cancellationToken">Target <see cref="CancellationToken"/>.</param>
		IEnumerable<IMemberData> Filtrate(ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken = default);

		/// <summary>
		/// Decides, which <see cref="CSharpSyntaxNode"/>s from the <paramref name="collectedNodes"/> are valid for the current generator pass and returns a collection of <see cref="IMemberData"/> based on those <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		/// <param name="compilation">Current <see cref="ICompilationData"/>.</param>
		/// <param name="collectedNodes">A collection of <see cref="CSharpSyntaxNode"/>s to filtrate.</param>
		/// <param name="cancellationToken">Target <see cref="CancellationToken"/>.</param>
		IEnumerable<IMemberData> Filtrate(ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken = default);
	}
}
