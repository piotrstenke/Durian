// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.Filters
{
	/// <summary>
	/// <see cref="ISyntaxFilter"/> that validates the filtrated nodes.
	/// </summary>
	public abstract class SyntaxValidator : GeneratorSyntaxFilter, ISyntaxValidatorWithDiagnostics<IMemberData>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxValidator"/> class.
		/// </summary>
		protected SyntaxValidator()
		{
		}

		/// <inheritdoc/>
		public sealed override IEnumerable<IMemberData> Filtrate(
			ICompilationData compilation,
			IDurianSyntaxReceiver syntaxReceiver,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken = default
		)
		{
			if (GetCandidateNodes(syntaxReceiver) is not IEnumerable<CSharpSyntaxNode> list)
			{
				return Array.Empty<IMemberData>();
			}

			return Filtrate(compilation, list, diagnosticReceiver, cancellationToken);
		}

		/// <inheritdoc/>
		public sealed override IEnumerable<IMemberData> Filtrate(
			ICompilationData compilation,
			IEnumerable<CSharpSyntaxNode> collectedNodes,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken = default
		)
		{
			foreach (CSharpSyntaxNode decl in collectedNodes)
			{
				if (decl is null)
				{
					continue;
				}

				if (ValidateAndCreate(decl, compilation, out IMemberData? data, diagnosticReceiver, cancellationToken))
				{
					yield return data;
				}
			}
		}

		/// <inheritdoc/>
		public sealed override IEnumerable<IMemberData> Filtrate(
			ICompilationData compilation,
			IDurianSyntaxReceiver syntaxReceiver,
			CancellationToken cancellationToken = default
		)
		{
			if (GetCandidateNodes(syntaxReceiver) is not IEnumerable<CSharpSyntaxNode> list)
			{
				return Array.Empty<IMemberData>();
			}

			return Filtrate(compilation, list, cancellationToken);
		}

		/// <inheritdoc/>
		public sealed override IEnumerable<IMemberData> Filtrate(
			ICompilationData compilation,
			IEnumerable<CSharpSyntaxNode> collectedNodes,
			CancellationToken cancellationToken = default
		)
		{
			foreach (CSharpSyntaxNode decl in collectedNodes)
			{
				if (decl is null)
				{
					continue;
				}

				if (ValidateAndCreate(decl, compilation, out IMemberData? data, cancellationToken))
				{
					yield return data;
				}
			}
		}

		/// <inheritdoc/>
		public sealed override IEnumerable<IMemberData> Filtrate(IGeneratorPassContext context)
		{
			return base.Filtrate(context);
		}

		/// <inheritdoc/>
		public override IEnumerator<IMemberData> GetEnumerator(IGeneratorPassContext context)
		{
			if (GetCandidateNodes(context.SyntaxReceiver) is not IEnumerable<CSharpSyntaxNode> list)
			{
				return Enumerable.Empty<IMemberData>().GetEnumerator();
			}

			return context.Generator.GetFilterMode() switch
			{
				FilterMode.Diagnostics => new FilterEnumeratorWithDiagnostics<IMemberData>(list, context.TargetCompilation, this, GetDiagnosticReceiver(context)),
				FilterMode.Logs => new LoggableFilterEnumerator<IMemberData>(list, context.TargetCompilation, this, GetLogReceiver(context, false), context.FileNameProvider),
				FilterMode.Both => new LoggableFilterEnumerator<IMemberData>(list, context.TargetCompilation, this, GetLogReceiver(context, true), context.FileNameProvider),
				_ => new FilterEnumerator<IMemberData>(list, context.TargetCompilation, this)
			};
		}

		/// <inheritdoc/>
		public virtual bool GetValidationData(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			[NotNullWhen(true)] out SemanticModel? semanticModel,
			[NotNullWhen(true)] out ISymbol? symbol,
			CancellationToken cancellationToken = default
		)
		{
			SemanticModel semantic = compilation.Compilation.GetSemanticModel(node.SyntaxTree);

			if (semantic.GetDeclaredSymbol(node, cancellationToken) is ISymbol t)
			{
				symbol = t;
				semanticModel = semantic;
				return true;
			}

			symbol = default;
			semanticModel = default;
			return false;
		}

		/// <inheritdoc/>
		public virtual bool ValidateAndCreate(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			[NotNullWhen(true)] out IMemberData? data,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken = default
		)
		{
			if (!GetValidationData(node, compilation, out SemanticModel? semanticModel, out ISymbol? symbol, cancellationToken))
			{
				data = default;
				return false;
			}

			return ValidateAndCreate(node, compilation, semanticModel, symbol, out data, diagnosticReceiver, cancellationToken);
		}

		/// <inheritdoc/>
		public virtual bool ValidateAndCreate(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			[NotNullWhen(true)] out IMemberData? data,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken = default
		)
		{
			return ValidateAndCreate(node, compilation, semanticModel, symbol, out data, cancellationToken);
		}

		/// <inheritdoc/>
		public virtual bool ValidateAndCreate(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			[NotNullWhen(true)] out IMemberData? data,
			CancellationToken cancellationToken = default
		)
		{
			if (!GetValidationData(node, compilation, out SemanticModel? semanticModel, out ISymbol? symbol, cancellationToken))
			{
				data = default;
				return false;
			}

			return ValidateAndCreate(node, compilation, semanticModel, symbol, out data, cancellationToken);
		}

		/// <inheritdoc/>
		public abstract bool ValidateAndCreate(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			[NotNullWhen(true)] out IMemberData? data,
			CancellationToken cancellationToken = default
		);

		/// <summary>
		/// Returns a collection of candidate <see cref="CSharpSyntaxNode"/> collected by the specified <paramref name="syntaxReceiver"/>.
		/// </summary>
		/// <param name="syntaxReceiver"><see cref="IDurianSyntaxReceiver"/> to get the candidate <see cref="CSharpSyntaxNode"/>s from.</param>
		protected virtual IEnumerable<CSharpSyntaxNode>? GetCandidateNodes(IDurianSyntaxReceiver syntaxReceiver)
		{
			return syntaxReceiver.GetNodes();
		}
	}
}
