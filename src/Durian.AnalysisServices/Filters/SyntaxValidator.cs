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
	/// <typeparam name="T">Type of target <see cref="ISyntaxValidatorContext"/>.</typeparam>
	public abstract class SyntaxValidator<T> : GeneratorSyntaxFilter, ISyntaxValidatorWithDiagnostics<T> where T : ISyntaxValidatorContext
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxValidator{T}"/> class.
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
			FilterEnumeratorWithDiagnostics<T> e = new(compilation, collectedNodes, this, diagnosticReceiver);

			while(e.MoveNext(cancellationToken))
			{
				yield return e.Current;
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
			FilterEnumerator<T> e = new(compilation, collectedNodes, this);

			while(e.MoveNext(cancellationToken))
			{
				yield return e.Current;
			}
		}

		/// <inheritdoc/>
		public sealed override IEnumerable<IMemberData> Filtrate(IGeneratorPassContext context)
		{
			if (GetCandidateNodes(context.SyntaxReceiver) is not IEnumerable<CSharpSyntaxNode> list)
			{
				return Array.Empty<IMemberData>();
			}

			IDiagnosticReceiver? diagnosticReceiver = context.GetDiagnosticReceiver();

			if (diagnosticReceiver is null)
			{
				return Filtrate(context.TargetCompilation, list, context.CancellationToken);
			}

			if (diagnosticReceiver is not INodeDiagnosticReceiver node)
			{
				return Filtrate(context.TargetCompilation, list, diagnosticReceiver, context.CancellationToken);
			}

			return YieldLoggable();

			IEnumerable<IMemberData> YieldLoggable()
			{
				LoggableFilterEnumerator<T> e = new(context.TargetCompilation, list, this, node, context.FileNameProvider);

				while (e.MoveNext(context.CancellationToken))
				{
					yield return e.Current;
				}
			}
		}

		/// <inheritdoc/>
		public override IEnumerator<IMemberData> GetEnumerator(IGeneratorPassContext context)
		{
			if (GetCandidateNodes(context.SyntaxReceiver) is not IEnumerable<CSharpSyntaxNode> list)
			{
				return Enumerable.Empty<IMemberData>().GetEnumerator();
			}

			IDiagnosticReceiver? diagnosticReceiver = context.GetDiagnosticReceiver();

			if(diagnosticReceiver is null)
			{
				return new FilterEnumerator<T>(context.TargetCompilation, list, this);
			}

			if(diagnosticReceiver is INodeDiagnosticReceiver node)
			{
				return new LoggableFilterEnumerator<T>(context.TargetCompilation, list, this, node, context.FileNameProvider);
			}

			return new FilterEnumeratorWithDiagnostics<T>(context.TargetCompilation, list, this, diagnosticReceiver);
		}

		/// <inheritdoc/>
		public virtual bool TryGetValidationData(in ValidationDataProviderContext context, [NotNullWhen(true)] out T? data)
		{
			SemanticModel semanticModel = context.TargetCompilation.Compilation.GetSemanticModel(context.Node.SyntaxTree);

			if (semanticModel.GetDeclaredSymbol(context.Node, context.CancellationToken) is ISymbol t)
			{
				data = CreateContext(in context, semanticModel, t);
				return data is not null;
			}

			data = default;
			return false;
		}

		/// <inheritdoc/>
		public abstract bool ValidateAndCreate(in T context, out IMemberData? data);

		/// <inheritdoc/>
		public bool ValidateAndCreate(in ValidationDataProviderContext context, [NotNullWhen(true)] out IMemberData? data)
		{
			if(TryGetValidationData(in context, out T? validationData))
			{
				return ValidateAndCreate(in validationData, out data);
			}

			data = default;
			return false;
		}

		/// <inheritdoc/>
		public bool ValidateAndCreate(in T context, out IMemberData? data, IDiagnosticReceiver diagnosticReceiver)
		{
			return ValidateAndCreate(in context, out data);
		}

		/// <inheritdoc/>
		public bool ValidateAndCreate(in ValidationDataProviderContext context, [NotNullWhen(true)] out IMemberData? data, IDiagnosticReceiver diagnosticReceiver)
		{
			if (TryGetValidationData(in context, out T? validationData))
			{
				return ValidateAndCreate(in validationData, out data, diagnosticReceiver);
			}

			data = default;
			return false;
		}

		/// <summary>
		/// Returns a collection of candidate <see cref="CSharpSyntaxNode"/> collected by the specified <paramref name="syntaxReceiver"/>.
		/// </summary>
		/// <param name="syntaxReceiver"><see cref="IDurianSyntaxReceiver"/> to get the candidate <see cref="CSharpSyntaxNode"/>s from.</param>
		protected virtual IEnumerable<CSharpSyntaxNode>? GetCandidateNodes(IDurianSyntaxReceiver syntaxReceiver)
		{
			return syntaxReceiver.GetNodes();
		}

		/// <summary>
		/// Creates a new <typeparamref name="T"/> from the specified <paramref name="context"/>, <paramref name="semanticModel"/> and <paramref name="symbol"/>.
		/// </summary>
		/// <param name="context"><see cref="ValidationDataProviderContext"/> that contains all data necessary to retrieve the required data.</param>
		/// <param name="semanticModel">Current <see cref="SemanticModel"/>.</param>
		/// <param name="symbol">Current <see cref="ISymbol"/>.</param>
		protected virtual T? CreateContext(in ValidationDataProviderContext context, SemanticModel semanticModel, ISymbol symbol)
		{
			return default;
		}
	}

	/// <inheritdoc cref="SyntaxValidator{T}"/>
	public abstract class SyntaxValidator : SyntaxValidator<SyntaxValidatorContext>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxValidator"/> class.
		/// </summary>
		protected SyntaxValidator()
		{
		}

		/// <inheritdoc/>
		protected override SyntaxValidatorContext CreateContext(in ValidationDataProviderContext context, SemanticModel semanticModel, ISymbol symbol)
		{
			return new SyntaxValidatorContext(context.TargetCompilation, semanticModel, context.Node, symbol, context.CancellationToken);
		}
	}
}
