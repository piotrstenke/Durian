// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Cache;
using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Durian.Analysis.CopyFrom
{
	/// <summary>
	/// <c>CopyFrom</c>-specific <see cref="SyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/>.
	/// </summary>
	/// <typeparam name="TSyntax">Type of <see cref="CSharpSyntaxNode"/> this <see cref="CopyFromFilter{TSyntax, TSymbol, TData}"/> uses.</typeparam>
	/// <typeparam name="TSymbol">Type of <see cref="ISyntaxFilter"/> this <see cref="CopyFromFilter{TSyntax, TSymbol, TData}"/> uses.</typeparam>
	/// <typeparam name="TData">Type of <see cref="IMemberData"/> this <see cref="CopyFromFilter{TSyntax, TSymbol, TData}"/> uses.</typeparam>
	public abstract class CopyFromFilter<TSyntax, TSymbol, TData> : CachedSyntaxValidator<CopyFromCompilationData, CopyFromSyntaxReceiver, TSyntax, TSymbol, TData>.WithDiagnostics, ICopyFromFilter
		where TSyntax : CSharpSyntaxNode
		where TSymbol : ISymbol
		where TData : ICopyFromMember
	{
		/// <summary>
		/// <see cref="CopyFromGenerator"/> that created this filter.
		/// </summary>
		public new CopyFromGenerator Generator => (base.Generator as CopyFromGenerator)!;

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromFilter{TSyntax, TSymbol, TData}"/> class.
		/// </summary>
		/// <param name="generator"><see cref="CopyFromGenerator"/> that is the target of this filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		protected CopyFromFilter(CopyFromGenerator generator) : base(generator)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromFilter{TSyntax, TSymbol, TData}"/> class.
		/// </summary>
		/// <param name="generator"><see cref="CopyFromGenerator"/> that is the target of this filter.</param>
		/// <param name="hintNameProvider"><see cref="IHintNameProvider"/> that is used to create a hint name for the generated source.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="hintNameProvider"/> is <see langword="null"/>.</exception>
		protected CopyFromFilter(CopyFromGenerator generator, IHintNameProvider hintNameProvider) : base(generator, hintNameProvider)
		{
		}

		/// <inheritdoc/>
		protected abstract override IEnumerable<TSyntax>? GetCandidateNodes(CopyFromSyntaxReceiver syntaxReceiver);

		/// <inheritdoc/>
		protected sealed override CopyFromCompilationData? CreateCompilation(in GeneratorExecutionContext context)
		{
			if (context.Compilation is not CSharpCompilation compilation)
			{
				return null;
			}

			return new CopyFromCompilationData(compilation);
		}

		IEnumerable<IMemberData> ICachedGeneratorSyntaxFilter<ICopyFromMember>.Filtrate(in CachedGeneratorExecutionContext<ICopyFromMember> context)
		{
			return ((ICachedGeneratorSyntaxFilter<TData>)this).Filtrate(context.CastContext<TData>());
		}

		IEnumerator<IMemberData> ICachedGeneratorSyntaxFilter<ICopyFromMember>.GetEnumerator(in CachedGeneratorExecutionContext<ICopyFromMember> context)
		{
			return ((ICachedGeneratorSyntaxFilter<TData>)this).GetEnumerator(context.CastContext<TData>());
		}

		bool INodeValidatorWithDiagnostics<ICopyFromMember>.ValidateAndCreate(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			[NotNullWhen(true)] out ICopyFromMember? data,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken
		)
		{
			bool value = ((INodeValidatorWithDiagnostics<TData>)this).ValidateAndCreate(node, compilation, out TData? d, diagnosticReceiver, cancellationToken);
			data = d;
			return value;
		}

		bool INodeValidatorWithDiagnostics<ICopyFromMember>.ValidateAndCreate(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			[NotNullWhen(true)] out ICopyFromMember? data,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken
		)
		{
			bool value = ((INodeValidatorWithDiagnostics<TData>)this).ValidateAndCreate(node, compilation, semanticModel, symbol, out TData? d, diagnosticReceiver, cancellationToken);
			data = d;
			return value;
		}

		bool INodeValidator<ICopyFromMember>.ValidateAndCreate(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			[NotNullWhen(true)]out ICopyFromMember? data,
			CancellationToken cancellationToken
		)
		{
			bool value = ((INodeValidator<TData>)this).ValidateAndCreate(node, compilation, out TData? d, cancellationToken);
			data = d;
			return value;
		}

		bool INodeValidator<ICopyFromMember>.ValidateAndCreate(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			[NotNullWhen(true)] out ICopyFromMember? data,
			CancellationToken cancellationToken
		)
		{
			bool value = ((INodeValidator<TData>)this).ValidateAndCreate(node, compilation, semanticModel, symbol, out TData? d, cancellationToken);
			data = d;
			return value;
		}
	}
}
