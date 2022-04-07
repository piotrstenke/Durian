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
	/// <typeparam name="TContext">Type of target <see cref="ISyntaxValidatorContext"/>.</typeparam>
	public abstract class CachedSyntaxValidator<TData, TContext> : SyntaxValidator<TContext>, ICachedGeneratorSyntaxFilterWithDiagnostics<TData>
		where TData : class, IMemberData
		where TContext : ISyntaxValidatorContext
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CachedSyntaxValidator{TData, TContext}"/> class.
		/// </summary>
		protected CachedSyntaxValidator()
		{
		}

		/// <inheritdoc/>
		public IEnumerable<TData> Filtrate(ICachedGeneratorPassContext<TData> context)
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

			IEnumerable<TData> Yield<TEnumerator>(TEnumerator enumerator) where TEnumerator : IFilterEnumerator<TContext>
			{
				while(enumerator.MoveNext(context.CancellationToken))
				{
					yield return (enumerator.Current as TData)!;
				}
			}
		}

		/// <inheritdoc/>
		public virtual IEnumerator<TData> GetEnumerator(ICachedGeneratorPassContext<TData> context)
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
	public abstract class CachedSyntaxValidator<TData> : CachedSyntaxValidator<TData, SyntaxValidatorContext> where TData : IMemberData
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CachedSyntaxValidator{TData}"/> class.
		/// </summary>
		protected CachedSyntaxValidator()
		{
		}

		/// <inheritdoc/>
		protected override SyntaxValidatorContext CreateContext(in ValidationDataProviderContext context, SemanticModel semanticModel, ISymbol symbol)
		{
			return new SyntaxValidatorContext(context.TargetCompilation, semanticModel, context.Node, symbol, context.CancellationToken);
		}
	}
}
