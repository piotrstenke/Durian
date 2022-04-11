// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Durian.Analysis.Filters;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Durian.Analysis.Cache
{
	/// <summary>
	/// <see cref="ISyntaxFilter"/> that validates the filtrated nodes.
	/// If the value associated with a <see cref="CSharpSyntaxNode"/> is present in the <see cref="CachedGeneratorExecutionContext{T}"/>, it is re-used.
	/// </summary>
	/// <typeparam name="TData">Type of cached values.</typeparam>
	/// <typeparam name="TContext">Type of target <see cref="ISyntaxValidationContext"/>.</typeparam>
	public abstract class CachedSyntaxValidator<TData, TContext> : SyntaxValidator<TContext>, ICachedGeneratorSyntaxFilterWithDiagnostics<TData>
		where TData : class, IMemberData
		where TContext : ISyntaxValidationContext
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CachedSyntaxValidator{TData, TContext}"/> class.
		/// </summary>
		protected CachedSyntaxValidator()
		{
		}

		/// <inheritdoc/>
		public IEnumerable<IMemberData> Filtrate(ICachedGeneratorPassContext<TData> context)
		{
			if (GetCandidateNodes(context.SyntaxReceiver) is not IEnumerable<CSharpSyntaxNode> list)
			{
				return Array.Empty<TData>();
			}

			ref readonly CachedData<TData> cache = ref context.OriginalContext.GetCachedData();

			IDiagnosticReceiver? diagnosticReceiver = context.GetDiagnosticReceiver();

			if (diagnosticReceiver is null)
			{
				return Yield(new CachedFilterEnumerator<TData, TContext>(context.TargetCompilation, list, this, in cache));
			}

			if(diagnosticReceiver is INodeDiagnosticReceiver node)
			{
				return Yield(new CachedLoggableFilterEnumerator<TData, TContext>(context.TargetCompilation, list, this, node, context.FileNameProvider, in cache));
			}

			return Yield(new CachedFilterEnumeratorWithDiagnostics<TData, TContext>(context.TargetCompilation, list, this, diagnosticReceiver, in cache));

			IEnumerable<IMemberData> Yield<TEnumerator>(TEnumerator enumerator) where TEnumerator : IFilterEnumerator<TContext>
			{
				while(enumerator.MoveNext(context.CancellationToken))
				{
					yield return enumerator.Current;
				}
			}
		}

		/// <inheritdoc/>
		public virtual IEnumerator<IMemberData> GetEnumerator(ICachedGeneratorPassContext<TData> context)
		{
			if (GetCandidateNodes(context.SyntaxReceiver) is not IEnumerable<CSharpSyntaxNode> list)
			{
				return Enumerable.Empty<TData>().GetEnumerator();
			}

			ref readonly CachedData<TData> cache = ref context.OriginalContext.GetCachedData();

			IDiagnosticReceiver? diagnosticReceiver = context.GetDiagnosticReceiver();

			if(diagnosticReceiver is null)
			{
				return new CachedFilterEnumerator<TData, TContext>(context.TargetCompilation, list, this, in cache);
			}

			if(diagnosticReceiver is INodeDiagnosticReceiver node)
			{
				return new CachedLoggableFilterEnumerator<TData, TContext>(context.TargetCompilation, list, this, node, context.FileNameProvider, in cache);
			}

			return new CachedFilterEnumeratorWithDiagnostics<TData, TContext>(context.TargetCompilation, list, this, diagnosticReceiver, in cache);
		}
	}

	/// <inheritdoc cref="CachedSyntaxValidator{TData}"/>
	public abstract class CachedSyntaxValidator<TData> : CachedSyntaxValidator<TData, SyntaxValidationContext> where TData : class, IMemberData
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CachedSyntaxValidator{TData}"/> class.
		/// </summary>
		protected CachedSyntaxValidator()
		{
		}

		/// <inheritdoc/>
		public override bool TryGetContext(in ValidationDataContext validationContext, [NotNullWhen(true)] out SyntaxValidationContext context)
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

		/// <inheritdoc/>
		[Obsolete("This method has no effect.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
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
