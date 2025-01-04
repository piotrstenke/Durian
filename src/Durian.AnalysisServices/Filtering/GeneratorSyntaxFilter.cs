using System.Collections.Generic;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Filtering;

/// <summary>
/// <see cref="ISyntaxFilter"/> that filters <see cref="SyntaxNode"/>s for the specified <see cref="IDurianGenerator"/>.
/// </summary>
public abstract class GeneratorSyntaxFilter : SyntaxFilter, IGeneratorSyntaxFilterWithDiagnostics
{
	/// <inheritdoc/>
	public virtual bool IncludeGeneratedSymbols { get; } = true;

	/// <summary>
	/// Initializes a new instance of the <see cref="GeneratorSyntaxFilter"/> class.
	/// </summary>
	protected GeneratorSyntaxFilter()
	{
	}

	/// <inheritdoc cref="IGeneratorSyntaxFilter.Filter(IGeneratorPassContext)"/>
	public virtual IEnumerable<IMemberData> Filter(IGeneratorPassContext context)
	{
		IDiagnosticReceiver? diagnosticReceiver = context.GetDiagnosticReceiver();

		if (diagnosticReceiver is null)
		{
			return Filter(context.TargetCompilation, context.SyntaxReceiver, context.CancellationToken);
		}

		return Filter(context.TargetCompilation, context.SyntaxReceiver, diagnosticReceiver, context.CancellationToken);
	}

	/// <inheritdoc cref="IGeneratorSyntaxFilter.GetEnumerator(IGeneratorPassContext)"/>
	public virtual IEnumerator<IMemberData> GetEnumerator(IGeneratorPassContext context)
	{
		return Filter(context).GetEnumerator();
	}
}
