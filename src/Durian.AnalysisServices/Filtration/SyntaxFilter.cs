// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Threading;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Filtration
{
	/// <summary>
	/// Filtrates <see cref="SyntaxNode"/>s collected by a <see cref="IDurianSyntaxReceiver"/>.
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
		public virtual IEnumerable<IMemberData> Filtrate(
			ICompilationData compilation,
			IDurianSyntaxReceiver syntaxReceiver,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken = default
		)
		{
			return Filtrate(compilation, syntaxReceiver.GetNodes(), diagnosticReceiver, cancellationToken);
		}

		/// <inheritdoc/>
		public virtual IEnumerable<IMemberData> Filtrate(
			ICompilationData compilation,
			IEnumerable<SyntaxNode> collectedNodes,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken = default
		)
		{
			return Filtrate(compilation, collectedNodes, cancellationToken);
		}

		/// <inheritdoc/>
		public virtual IEnumerable<IMemberData> Filtrate(
			ICompilationData compilation,
			IDurianSyntaxReceiver syntaxReceiver,
			CancellationToken cancellationToken = default
		)
		{
			return Filtrate(compilation, syntaxReceiver.GetNodes(), cancellationToken);
		}

		/// <inheritdoc/>
		public abstract IEnumerable<IMemberData> Filtrate(
			ICompilationData compilation,
			IEnumerable<SyntaxNode> collectedNodes,
			CancellationToken cancellationToken = default
		);
	}
}
