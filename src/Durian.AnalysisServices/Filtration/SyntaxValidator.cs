// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.Filtration
{
	/// <summary>
	/// <see cref="ISyntaxFilter"/> that validates the filtrated nodes.
	/// </summary>
	/// <typeparam name="TContext">Type of target <see cref="ISyntaxValidationContext"/>.</typeparam>
	public abstract class SyntaxValidator<TContext> : GeneratorSyntaxFilter, ISyntaxValidatorWithDiagnostics<TContext> where TContext : ISyntaxValidationContext
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxValidator{TContext}"/> class.
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
			FilterEnumeratorWithDiagnostics<TContext> e = new(compilation, collectedNodes, this, diagnosticReceiver);

			while (e.MoveNext(cancellationToken))
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
			FilterEnumerator<TContext> e = new(compilation, collectedNodes, this);

			while (e.MoveNext(cancellationToken))
			{
				yield return e.Current;
			}
		}

		/// <inheritdoc/>
		public override IEnumerable<IMemberData> Filtrate(IGeneratorPassContext context)
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
				LoggableFilterEnumerator<TContext> e = new(context.TargetCompilation, list, this, node, context.FileNameProvider);

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

			if (diagnosticReceiver is null)
			{
				return new FilterEnumerator<TContext>(context.TargetCompilation, list, this);
			}

			if (diagnosticReceiver is INodeDiagnosticReceiver node)
			{
				return new LoggableFilterEnumerator<TContext>(context.TargetCompilation, list, this, node, context.FileNameProvider);
			}

			return new FilterEnumeratorWithDiagnostics<TContext>(context.TargetCompilation, list, this, diagnosticReceiver);
		}

		/// <summary>
		/// Checks whether a <see cref="SemanticModel"/> and a <see cref="ISymbol"/> can be created from the a given <see cref="CSharpSyntaxNode"/>.
		/// </summary>
		/// <param name="validationContext"><see cref="PreValidationContext"/> that contains all data necessary to retrieve the required data.</param>
		/// <param name="context">Returned data.</param>
		public virtual bool TryGetContext(in PreValidationContext validationContext, [NotNullWhen(true)] out TContext? context)
		{
			SemanticModel semanticModel = validationContext.TargetCompilation.Compilation.GetSemanticModel(validationContext.Node.SyntaxTree);

			if (semanticModel.GetDeclaredSymbol(validationContext.Node, validationContext.CancellationToken) is not ISymbol s)
			{
				context = default;
				return false;
			}

			return TryCreateContext(validationContext.ToSyntaxContext(semanticModel, s), out context);
		}

		/// <inheritdoc/>
		public abstract bool ValidateAndCreate(in TContext context, out IMemberData? data);

		/// <inheritdoc/>
		public bool ValidateAndCreate(in PreValidationContext context, [NotNullWhen(true)] out IMemberData? data)
		{
			if (TryGetContext(in context, out TContext? validationData))
			{
				return ValidateAndCreate(in validationData, out data);
			}

			data = default;
			return false;
		}

		/// <inheritdoc/>
		public virtual bool ValidateAndCreate(in TContext context, out IMemberData? data, IDiagnosticReceiver diagnosticReceiver)
		{
			return ValidateAndCreate(in context, out data);
		}

		/// <inheritdoc/>
		public bool ValidateAndCreate(in PreValidationContext context, [NotNullWhen(true)] out IMemberData? data, IDiagnosticReceiver diagnosticReceiver)
		{
			if (TryGetContext(in context, out TContext? validationData))
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
		/// Attempts to create a new <typeparamref name="TContext"/> from the specified <paramref name="validationContext"/>.
		/// </summary>
		/// <param name="validationContext"><see cref="SyntaxValidationContext"/> that contains all data necessary to retrieve the required data.</param>
		/// <param name="context">Created <typeparamref name="TContext"/>.</param>
		protected abstract bool TryCreateContext(in SyntaxValidationContext validationContext, [NotNullWhen(true)] out TContext? context);
	}

	/// <inheritdoc cref="SyntaxValidator{T}"/>
	public abstract class SyntaxValidator : SyntaxValidator<SyntaxValidationContext>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxValidator"/> class.
		/// </summary>
		protected SyntaxValidator()
		{
		}

		/// <inheritdoc/>
		public override bool TryGetContext(in PreValidationContext validationContext, [NotNullWhen(true)] out SyntaxValidationContext context)
		{
			SemanticModel semanticModel = validationContext.TargetCompilation.Compilation.GetSemanticModel(validationContext.Node.SyntaxTree);

			if (semanticModel.GetDeclaredSymbol(validationContext.Node, validationContext.CancellationToken) is not ISymbol s)
			{
				context = default;
				return false;
			}

			context = validationContext.ToSyntaxContext(semanticModel, s);
			return true;
		}

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member

		/// <inheritdoc/>
		[Obsolete("This method has no effect.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected sealed override bool TryCreateContext(
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
			in SyntaxValidationContext validationContext,
			[NotNullWhen(true)] out SyntaxValidationContext context
		)
		{
			context = validationContext;
			return true;
		}
	}
}
