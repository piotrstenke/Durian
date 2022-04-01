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
			return context.Generator.GetFilterMode() switch
			{
				FilterMode.Diagnostics => Filtrate(context.TargetCompilation, context.SyntaxReceiver, GetDiagnosticReceiver(context), context.CancellationToken),
				FilterMode.Logs => Filtrate(context.TargetCompilation, context.SyntaxReceiver, GetLogReceiver(context, false), context.CancellationToken),
				FilterMode.Both => Filtrate(context.TargetCompilation, context.SyntaxReceiver, GetLogReceiver(context, true), context.CancellationToken),
				_ => Filtrate(context.TargetCompilation, context.SyntaxReceiver, context.CancellationToken)
			};
		}

		/// <inheritdoc cref="IGeneratorSyntaxFilter.GetEnumerator(IGeneratorPassContext)"/>
		public virtual IEnumerator<IMemberData> GetEnumerator(IGeneratorPassContext context)
		{
			return Filtrate(context).GetEnumerator();
		}

		/// <summary>
		/// Returns a <see cref="IDiagnosticReceiver"/> that will be used during enumeration of the filter when <see cref="FilterMode"/> of the <paramref name="context"/> is equal to <see cref="FilterMode.Diagnostics"/>.
		/// </summary>
		/// <param name="context"><see cref="IGeneratorPassContext"/> to retrieve the <see cref="IDiagnosticReceiver"/> from.</param>
		protected virtual IDiagnosticReceiver GetDiagnosticReceiver(IGeneratorPassContext context)
		{
			return context.GetDiagnosticReceiverOrEmpty();
		}

		/// <summary>
		/// Returns a <see cref="INodeDiagnosticReceiver"/> that will be used enumeration of the filter when <see cref="FilterMode"/> of the <paramref name="context"/>  is equal to <see cref="FilterMode.Logs"/> or <see cref="FilterMode.Both"/>.
		/// </summary>
		/// <param name="context"><see cref="IGeneratorPassContext"/> to retrieve the <see cref="INodeDiagnosticReceiver"/> from.</param>
		/// <param name="includeDiagnostics">
		/// Determines whether to include diagnostics other than log files.
		/// <para>If <see cref="FilterMode"/> is equal to <see cref="FilterMode.Both"/>,
		/// <paramref name="includeDiagnostics"/> is <see langword="true"/>, otherwise <see langword="false"/>.</para>
		/// </param>
		protected virtual INodeDiagnosticReceiver GetLogReceiver(IGeneratorPassContext context, bool includeDiagnostics)
		{
			return context.GetLogReceiverOrEmpty(includeDiagnostics);
		}
	}
}
