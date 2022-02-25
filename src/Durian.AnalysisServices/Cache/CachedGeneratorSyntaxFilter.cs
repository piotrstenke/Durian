// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.Cache
{
	/// <summary>
	/// <see cref="ISyntaxFilter"/> that filtrates <see cref="CSharpSyntaxNode"/>s for the specified <see cref="IDurianGenerator"/>.
	/// If the value associated with a <see cref="CSharpSyntaxNode"/> is present in the <see cref="CachedGeneratorExecutionContext{T}"/>, it is re-used instea of creating a new one.
	/// </summary>
	/// <typeparam name="TData">Type of <see cref="IMemberData"/> this <see cref="GeneratorSyntaxFilter{TData, TCompilation, TSyntaxReceiver, TGenerator}"/> returns.</typeparam>
	/// <typeparam name="TCompilation">Type of <see cref="ICompilationData"/> this <see cref="GeneratorSyntaxFilter{TData, TCompilation, TSyntaxReceiver, TGenerator}"/> uses.</typeparam>
	/// <typeparam name="TSyntaxReceiver">Type of <see cref="IDurianSyntaxReceiver"/> this <see cref="GeneratorSyntaxFilter{TData, TCompilation, TSyntaxReceiver, TGenerator}"/> uses.</typeparam>
	/// <typeparam name="TGenerator">Type of <see cref="IDurianGenerator"/> that is the target of this <see cref="GeneratorSyntaxFilter{TData, TCompilation, TSyntaxReceiver, TGenerator}"/>.</typeparam>
	public abstract class CachedGeneratorSyntaxFilter<TData, TCompilation, TSyntaxReceiver, TGenerator> : GeneratorSyntaxFilter<TData, TCompilation, TSyntaxReceiver, TGenerator>, ICachedGeneratorSyntaxFilter<TData>
		where TData : IMemberData
		where TCompilation : ICompilationData
		where TSyntaxReceiver : IDurianSyntaxReceiver
		where TGenerator : IDurianGenerator
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGeneratorSyntaxFilter{TData, TCompilation, TSyntaxReceiver, TGenerator}"/> class.
		/// </summary>
		/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		protected CachedGeneratorSyntaxFilter(TGenerator generator) : base(generator)
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
			using IEnumerator<TData> enumerator = GetEnumerator(in context);

			return Yield();

			IEnumerator<IMemberData> Yield()
			{
				while (enumerator.MoveNext())
				{
					yield return enumerator.Current;
				}
			}
		}

		/// <summary>
		/// <see cref="CachedGeneratorSyntaxFilter{TData, TCompilation, TSyntaxReceiver, TGenerator}"/> that reports diagnostics during filtration.
		/// </summary>
		public new abstract class WithDiagnostics : CachedGeneratorSyntaxFilter<TData, TCompilation, TSyntaxReceiver, TGenerator>, ICachedGeneratorSyntaxFilterWithDiagnostics<TData>
		{
			/// <inheritdoc/>
			public IHintNameProvider HintNameProvider { get; }

			/// <inheritdoc/>
			public virtual FilterMode Mode
			{
				get
				{
					if (Generator is ILoggableGenerator loggable)
					{
						return loggable.LoggingConfiguration.CurrentFilterMode;
					}

					return FilterMode.None;
				}
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="WithDiagnostics"/> class.
			/// </summary>
			/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
			/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
			protected WithDiagnostics(TGenerator generator) : this(generator, new SymbolNameToFile())
			{
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="WithDiagnostics"/> class.
			/// </summary>
			/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
			/// <param name="hintNameProvider"><see cref="IHintNameProvider"/> that is used to create a hint name for the generated source.</param>
			/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="hintNameProvider"/> is <see langword="null"/>.</exception>
			protected WithDiagnostics(TGenerator generator, IHintNameProvider hintNameProvider) : base(generator)
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
	}

	/// <inheritdoc cref="CachedGeneratorSyntaxFilter{TData, TCompilation, TSyntaxReceiver, TGenerator}"/>
	public abstract class CachedGeneratorSyntaxFilter<TData, TCompilation, TSyntaxReceiver> : GeneratorSyntaxFilter<TData, TCompilation, TSyntaxReceiver, IDurianGenerator>
		where TData : IMemberData
		where TCompilation : ICompilationData
		where TSyntaxReceiver : IDurianSyntaxReceiver
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGeneratorSyntaxFilter{TData, TCompilation, TSyntaxReceiver}"/> class.
		/// </summary>
		/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		protected CachedGeneratorSyntaxFilter(IDurianGenerator generator) : base(generator)
		{
		}
	}

	/// <inheritdoc cref="CachedGeneratorSyntaxFilter{TData, TCompilation, TSyntaxReceiver, TGenerator}"/>
	public abstract class CachedGeneratorSyntaxFilter<TData, TCompilation> : GeneratorSyntaxFilter<TData, TCompilation, IDurianSyntaxReceiver, IDurianGenerator>
		where TData : IMemberData
		where TCompilation : ICompilationData
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGeneratorSyntaxFilter{TData, TCompilation}"/> class.
		/// </summary>
		/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		protected CachedGeneratorSyntaxFilter(IDurianGenerator generator) : base(generator)
		{
		}
	}

	/// <inheritdoc cref="CachedGeneratorSyntaxFilter{TData, TCompilation, TSyntaxReceiver, TGenerator}"/>
	public abstract class CachedGeneratorSyntaxFilter<TData> : GeneratorSyntaxFilter<TData, ICompilationData, IDurianSyntaxReceiver, IDurianGenerator>
		where TData : IMemberData
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGeneratorSyntaxFilter{TData}"/> class.
		/// </summary>
		/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		protected CachedGeneratorSyntaxFilter(IDurianGenerator generator) : base(generator)
		{
		}
	}

	/// <inheritdoc cref="CachedGeneratorSyntaxFilter{TData, TCompilation, TSyntaxReceiver, TGenerator}"/>
	public abstract class CachedGeneratorSyntaxFilter : GeneratorSyntaxFilter<IMemberData, ICompilationData, IDurianSyntaxReceiver, IDurianGenerator>
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