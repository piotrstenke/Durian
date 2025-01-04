using System.Collections.Generic;
using System.Threading;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Filtering;

/// <summary>
/// Filters <see cref="SyntaxNode"/>s collected by a <see cref="IDurianSyntaxReceiver"/>.
/// </summary>
public abstract class SyntaxFilter : ISyntaxFilterWithDiagnostics
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SyntaxFilter"/> class.
	/// </summary>
	protected SyntaxFilter()
	{
	}

	/// <inheritdoc/>
	public virtual IEnumerable<IMemberData> Filter(
		ICompilationData compilation,
		IDurianSyntaxReceiver syntaxReceiver,
		IDiagnosticReceiver diagnosticReceiver,
		CancellationToken cancellationToken = default
	)
	{
		return Filter(compilation, syntaxReceiver.GetNodes(), diagnosticReceiver, cancellationToken);
	}

	/// <inheritdoc/>
	public virtual IEnumerable<IMemberData> Filter(
		ICompilationData compilation,
		IEnumerable<SyntaxNode> collectedNodes,
		IDiagnosticReceiver diagnosticReceiver,
		CancellationToken cancellationToken = default
	)
	{
		return Filter(compilation, collectedNodes, cancellationToken);
	}

	/// <inheritdoc/>
	public virtual IEnumerable<IMemberData> Filter(
		ICompilationData compilation,
		IDurianSyntaxReceiver syntaxReceiver,
		CancellationToken cancellationToken = default
	)
	{
		return Filter(compilation, syntaxReceiver.GetNodes(), cancellationToken);
	}

	/// <inheritdoc/>
	public abstract IEnumerable<IMemberData> Filter(
		ICompilationData compilation,
		IEnumerable<SyntaxNode> collectedNodes,
		CancellationToken cancellationToken = default
	);
}
