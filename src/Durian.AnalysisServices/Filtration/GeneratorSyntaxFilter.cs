using System.Collections.Generic;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Filtration
{
	/// <summary>
	/// <see cref="ISyntaxFilter"/> that filtrates <see cref="SyntaxNode"/>s for the specified <see cref="IDurianGenerator"/>.
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

		/// <inheritdoc cref="IGeneratorSyntaxFilter.Filtrate(IGeneratorPassContext)"/>
		public virtual IEnumerable<IMemberData> Filtrate(IGeneratorPassContext context)
		{
			IDiagnosticReceiver? diagnosticReceiver = context.GetDiagnosticReceiver();

			if (diagnosticReceiver is null)
			{
				return Filtrate(context.TargetCompilation, context.SyntaxReceiver, context.CancellationToken);
			}

			return Filtrate(context.TargetCompilation, context.SyntaxReceiver, diagnosticReceiver, context.CancellationToken);
		}

		/// <inheritdoc cref="IGeneratorSyntaxFilter.GetEnumerator(IGeneratorPassContext)"/>
		public virtual IEnumerator<IMemberData> GetEnumerator(IGeneratorPassContext context)
		{
			return Filtrate(context).GetEnumerator();
		}
	}
}
