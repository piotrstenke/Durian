using System.Collections.Generic;
using System.Threading;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Filtering;

/// <summary>
/// Filters <see cref="SyntaxNode"/>s collected by a <see cref="IDurianSyntaxReceiver"/>.
/// </summary>
public interface ISyntaxFilter
{
	/// <summary>
	/// Decides, which <see cref="SyntaxNode"/>s collected by the <paramref name="syntaxReceiver"/> are valid for the current generator pass and returns a collection of <see cref="IMemberData"/> based on those <see cref="SyntaxNode"/>s.
	/// </summary>
	/// <param name="compilation">Current <see cref="ICompilationData"/>.</param>
	/// <param name="syntaxReceiver">A <see cref="IDurianSyntaxReceiver"/> that collected the <see cref="SyntaxNode"/>s that should be filtered.</param>
	/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
	IEnumerable<IMemberData> Filter(ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken = default);

	/// <summary>
	/// Decides, which <see cref="SyntaxNode"/>s from the <paramref name="collectedNodes"/> are valid for the current generator pass and returns a collection of <see cref="IMemberData"/> based on those <see cref="SyntaxNode"/>s.
	/// </summary>
	/// <param name="compilation">Current <see cref="ICompilationData"/>.</param>
	/// <param name="collectedNodes">A collection of <see cref="SyntaxNode"/>s to filter.</param>
	/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
	IEnumerable<IMemberData> Filter(ICompilationData compilation, IEnumerable<SyntaxNode> collectedNodes, CancellationToken cancellationToken = default);
}
