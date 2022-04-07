// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.Filters
{
	/// <summary>
	/// <see cref="ISyntaxFilter"/> that filtrates <see cref="CSharpSyntaxNode"/>s for the specified <see cref="IDurianGenerator"/>.
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

			if(diagnosticReceiver is null)
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
