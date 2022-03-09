// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Durian.Analysis.Cache
{
	/// <summary>
	/// <see cref="ISyntaxFilter"/> that filtrates <see cref="CSharpSyntaxNode"/>s for the specified <see cref="IDurianGenerator"/>.
	/// If the value associated with a <see cref="CSharpSyntaxNode"/> is present in the <see cref="CachedGeneratorExecutionContext{T}"/>, it is re-used instea of creating a new one.
	/// </summary>
	/// <typeparam name="TCompilation">Type of <see cref="ICompilationData"/> this <see cref="GeneratorSyntaxFilter{TCompilation, TSyntaxReceiver, TData}"/> uses.</typeparam>
	/// <typeparam name="TSyntaxReceiver">Type of <see cref="IDurianSyntaxReceiver"/> this <see cref="GeneratorSyntaxFilter{TCompilation, TSyntaxReceiver, TData}"/> uses.</typeparam>
	/// <typeparam name="TData">Type of <see cref="IMemberData"/> this <see cref="GeneratorSyntaxFilter{TCompilation, TSyntaxReceiver, TData}"/> returns.</typeparam>
	public abstract class CachedGeneratorSyntaxFilter<TCompilation, TSyntaxReceiver, TData> : GeneratorSyntaxFilter<TCompilation, TSyntaxReceiver, TData>, ICachedGeneratorSyntaxFilter<TData>
		where TCompilation : ICompilationData
		where TSyntaxReceiver : IDurianSyntaxReceiver
		where TData : IMemberData
	{
		/// <summary>
		/// <see cref="CachedGeneratorSyntaxFilter{TCompilation, TSyntaxReceiver, TData}"/> that reports diagnostics during filtration.
		/// </summary>
		public abstract new class WithDiagnostics : CachedGeneratorSyntaxFilter<TCompilation, TSyntaxReceiver, TData>, ICachedGeneratorSyntaxFilterWithDiagnostics<TData>
		{
			/// <inheritdoc/>
			public IHintNameProvider HintNameProvider { get; }

			/// <inheritdoc/>
			public virtual FilterMode Mode => Generator.GetFilterMode();

			/// <summary>
			/// Initializes a new instance of the <see cref="WithDiagnostics"/> class.
			/// </summary>
			/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
			/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
			protected WithDiagnostics(IDurianGenerator generator) : this(generator, new SymbolNameToFile())
			{
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="WithDiagnostics"/> class.
			/// </summary>
			/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
			/// <param name="hintNameProvider"><see cref="IHintNameProvider"/> that is used to create a hint name for the generated source.</param>
			/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="hintNameProvider"/> is <see langword="null"/>.</exception>
			protected WithDiagnostics(IDurianGenerator generator, IHintNameProvider hintNameProvider) : base(generator)
			{
				if (hintNameProvider is null)
				{
					throw new ArgumentNullException(nameof(hintNameProvider));
				}

				HintNameProvider = hintNameProvider;
			}

			/// <inheritdoc cref="ISyntaxFilterWithDiagnostics.Filtrate(ICompilationData, IDurianSyntaxReceiver, IDiagnosticReceiver, CancellationToken)"/>
			public abstract IEnumerable<TData> Filtrate(
				TCompilation compilation,
				TSyntaxReceiver syntaxReceiver,
				IDiagnosticReceiver diagnosticReceiver,
				CancellationToken cancellationToken = default
			);

			/// <inheritdoc cref="ISyntaxFilterWithDiagnostics.Filtrate(ICompilationData, IEnumerable{CSharpSyntaxNode}, IDiagnosticReceiver, CancellationToken)"/>
			public abstract IEnumerable<TData> Filtrate(
				TCompilation compilation,
				IEnumerable<CSharpSyntaxNode> collectedNodes,
				IDiagnosticReceiver diagnosticReceiver,
				CancellationToken cancellationToken = default
			);

			IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(
				ICompilationData compilation,
				IDurianSyntaxReceiver syntaxReceiver,
				IDiagnosticReceiver diagnosticReceiver,
				CancellationToken cancellationToken
			)
			{
				return Filtrate((TCompilation)compilation, (TSyntaxReceiver)syntaxReceiver, diagnosticReceiver, cancellationToken).Cast<IMemberData>();
			}

			IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(
				ICompilationData compilation,
				IEnumerable<CSharpSyntaxNode> collectedNodes,
				IDiagnosticReceiver diagnosticReceiver,
				CancellationToken cancellationToken
			)
			{
				return Filtrate((TCompilation)compilation, collectedNodes, diagnosticReceiver, cancellationToken).Cast<IMemberData>();
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGeneratorSyntaxFilter{TCompilation, TSyntaxReceiver, TData}"/> class.
		/// </summary>
		/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		protected CachedGeneratorSyntaxFilter(IDurianGenerator generator) : base(generator)
		{
		}

		/// <inheritdoc cref="ICachedGeneratorSyntaxFilter{T}.Filtrate(in CachedGeneratorExecutionContext{T})"/>
		public abstract IEnumerable<TData> Filtrate(in CachedGeneratorExecutionContext<TData> context);

		/// <inheritdoc cref="ICachedGeneratorSyntaxFilter{T}.Filtrate(in CachedGeneratorExecutionContext{T})"/>
		public abstract IEnumerator<TData> GetEnumerator(in CachedGeneratorExecutionContext<TData> context);

		IEnumerable<IMemberData> ICachedGeneratorSyntaxFilter<TData>.Filtrate(in CachedGeneratorExecutionContext<TData> context)
		{
			return Filtrate(in context).Cast<IMemberData>();
		}

		IEnumerator<IMemberData> ICachedGeneratorSyntaxFilter<TData>.GetEnumerator(in CachedGeneratorExecutionContext<TData> context)
		{
			IEnumerator<TData> enumerator = GetEnumerator(in context);

			return Yield();

			IEnumerator<IMemberData> Yield()
			{
				while (enumerator.MoveNext())
				{
					yield return enumerator.Current;
				}

				enumerator.Dispose();
			}
		}
	}

	/// <inheritdoc cref="CachedGeneratorSyntaxFilter{TCompilation, TSyntaxReceiver, TData}"/>
	public abstract class CachedGeneratorSyntaxFilter<TCompilation, TSyntaxReceiver> : GeneratorSyntaxFilter<TCompilation, TSyntaxReceiver, IMemberData>
		where TCompilation : ICompilationData
		where TSyntaxReceiver : IDurianSyntaxReceiver
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGeneratorSyntaxFilter{TCompilation, TSyntaxReceiver}"/> class.
		/// </summary>
		/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		protected CachedGeneratorSyntaxFilter(IDurianGenerator generator) : base(generator)
		{
		}
	}

	/// <inheritdoc cref="CachedGeneratorSyntaxFilter{TCompilation, TSyntaxReceiver, TData}"/>
	public abstract class CachedGeneratorSyntaxFilter<TCompilation> : GeneratorSyntaxFilter<TCompilation, IDurianSyntaxReceiver, IMemberData>
		where TCompilation : ICompilationData
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGeneratorSyntaxFilter{TCompilation}"/> class.
		/// </summary>
		/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		protected CachedGeneratorSyntaxFilter(IDurianGenerator generator) : base(generator)
		{
		}
	}

	/// <inheritdoc cref="CachedGeneratorSyntaxFilter{TCompilation, TSyntaxReceiver, TData}"/>
	public abstract class CachedGeneratorSyntaxFilter : GeneratorSyntaxFilter<ICompilationData, IDurianSyntaxReceiver, IMemberData>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGeneratorSyntaxFilter"/> class.
		/// </summary>
		/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		protected CachedGeneratorSyntaxFilter(IDurianGenerator generator) : base(generator)
		{
		}
	}
}