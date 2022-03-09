// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
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
	/// If the value associated with a <see cref="CSharpSyntaxNode"/> is present in the <see cref="CachedGeneratorExecutionContext{T}"/>, it is re-used instead of creating a new one.
	/// </summary>
	/// <typeparam name="TCompilation">Type of <see cref="ICompilationData"/> this <see cref="CachedSyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/> uses.</typeparam>
	/// <typeparam name="TSyntaxReceiver">Type of <see cref="IDurianSyntaxReceiver"/> this <see cref="CachedSyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/> uses.</typeparam>
	/// <typeparam name="TSyntax">Type of <see cref="CSharpSyntaxNode"/> this <see cref="CachedSyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/> uses.</typeparam>
	/// <typeparam name="TSymbol">Type of <see cref="ISyntaxFilter"/> this <see cref="CachedSyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/> uses.</typeparam>
	/// <typeparam name="TData">Type of <see cref="IMemberData"/> this <see cref="CachedSyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/> returns.</typeparam>
	public abstract class CachedSyntaxFilterValidator<TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData> : SyntaxFilterValidator<TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData>, ICachedGeneratorSyntaxFilter<TData>
		where TCompilation : class, ICompilationData
		where TSyntaxReceiver : IDurianSyntaxReceiver
		where TSyntax : CSharpSyntaxNode
		where TSymbol : ISymbol
		where TData : IMemberData
	{
		/// <summary>
		/// <see cref="CachedSyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/> that reports diagnostics during filtration.
		/// </summary>
		public abstract new class WithDiagnostics : CachedSyntaxFilterValidator<TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData>, ICachedGeneratorSyntaxFilterWithDiagnostics<TData>, INodeValidatorWithDiagnostics<TData>
		{
			/// <inheritdoc/>
			public IHintNameProvider HintNameProvider { get; }

			/// <inheritdoc/>
			public FilterMode Mode => Generator.GetFilterMode();

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
			public IEnumerable<TData> Filtrate(
				TCompilation compilation,
				TSyntaxReceiver syntaxReceiver,
				IDiagnosticReceiver diagnosticReceiver,
				CancellationToken cancellationToken = default
			)
			{
				if (GetCandidateNodes(syntaxReceiver) is not IEnumerable<TSyntax> list)
				{
					return Array.Empty<TData>();
				}

				return Filtrate_Internal(compilation, list, diagnosticReceiver, cancellationToken);
			}

			/// <inheritdoc cref="ISyntaxFilterWithDiagnostics.Filtrate(ICompilationData, IEnumerable{CSharpSyntaxNode}, IDiagnosticReceiver, CancellationToken)"/>
			public IEnumerable<TData> Filtrate(
				TCompilation compilation,
				IEnumerable<CSharpSyntaxNode> collectedNodes,
				IDiagnosticReceiver diagnosticReceiver,
				CancellationToken cancellationToken = default
			)
			{
				if (collectedNodes is not IEnumerable<TSyntax> list)
				{
					list = collectedNodes.OfType<TSyntax>();
				}

				return Filtrate_Internal(compilation, list, diagnosticReceiver, cancellationToken);
			}

			/// <inheritdoc/>
			/// <exception cref="InvalidOperationException">Target <see cref="GeneratorSyntaxFilter{TCompilation, TSyntaxReceiver, TData}.Generator"/> is not initialized.</exception>
			public override IEnumerator<TData> GetEnumerator()
			{
				EnsureInitialized();

				return Mode switch
				{
					FilterMode.Diagnostics => new FilterEnumeratorWithDiagnostics<TData>(this, Generator.TargetCompilation, this, GetDiagnosticReceiver()),
					FilterMode.Logs => new LoggableFilterEnumerator<TData>(this, Generator.TargetCompilation, this, GetLogReceiver(false), HintNameProvider),
					FilterMode.Both => new LoggableFilterEnumerator<TData>(this, Generator.TargetCompilation, this, GetLogReceiver(true), HintNameProvider),
					_ => new FilterEnumerator<TData>(this, Generator.TargetCompilation, this)
				};
			}

			/// <inheritdoc/>
			public override IEnumerator<TData> GetEnumerator(in CachedGeneratorExecutionContext<TData> context)
			{
				ref readonly CachedData<TData> cache = ref context.GetCachedData();

				return Mode switch
				{
					FilterMode.Diagnostics => new CachedFilterEnumeratorWithDiagnostics<TData>(this, Generator.TargetCompilation, this, GetDiagnosticReceiver(), in cache),
					FilterMode.Logs => new CachedLoggableFilterEnumerator<TData>(this, Generator.TargetCompilation, this, GetLogReceiver(false), HintNameProvider, in cache),
					FilterMode.Both => new CachedLoggableFilterEnumerator<TData>(this, Generator.TargetCompilation, this, GetLogReceiver(true), HintNameProvider, in cache),
					_ => new CachedFilterEnumerator<TData>(this, Generator.TargetCompilation, this, in cache)
				};
			}

			/// <inheritdoc cref="INodeValidatorWithDiagnostics{T}.ValidateAndCreate(CSharpSyntaxNode, ICompilationData, out T, IDiagnosticReceiver, CancellationToken)"/>
			public virtual bool ValidateAndCreate(
				TSyntax node,
				TCompilation compilation,
				[NotNullWhen(true)] out TData? data,
				IDiagnosticReceiver diagnosticReceiver,
				CancellationToken cancellationToken = default
			)
			{
				if (!GetValidationData(node, compilation, out SemanticModel? semanticModel, out TSymbol? symbol, cancellationToken))
				{
					data = default;
					return false;
				}

				return ValidateAndCreate(node, compilation, semanticModel, symbol, out data, diagnosticReceiver, cancellationToken);
			}

			/// <inheritdoc cref="INodeValidatorWithDiagnostics{T}.ValidateAndCreate(CSharpSyntaxNode, ICompilationData, SemanticModel, ISymbol, out T, IDiagnosticReceiver, CancellationToken)"/>
			public abstract bool ValidateAndCreate(
				TSyntax node,
				TCompilation compilation,
				SemanticModel semanticModel,
				TSymbol symbol,
				[NotNullWhen(true)] out TData? data,
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

			bool INodeValidatorWithDiagnostics<TData>.ValidateAndCreate(
				CSharpSyntaxNode node,
				ICompilationData compilation,
				[NotNullWhen(true)] out TData? data,
				IDiagnosticReceiver diagnosticReceiver,
				CancellationToken cancellationToken
			)
			{
				if (node is not TSyntax syntax || compilation is not TCompilation c)
				{
					data = default;
					return false;
				}

				return ValidateAndCreate(
					syntax,
					c,
					out data,
					diagnosticReceiver,
					cancellationToken
				);
			}

			bool INodeValidatorWithDiagnostics<TData>.ValidateAndCreate(
				CSharpSyntaxNode node,
				ICompilationData compilation,
				SemanticModel semanticModel,
				ISymbol symbol,
				[NotNullWhen(true)] out TData? data,
				IDiagnosticReceiver diagnosticReceiver,
				CancellationToken cancellationToken
			)
			{
				if (node is not TSyntax syntax || symbol is not TSymbol s || compilation is not TCompilation c)
				{
					data = default;
					return false;
				}

				return ValidateAndCreate(
					syntax,
					c,
					semanticModel,
					s,
					out data,
					diagnosticReceiver,
					cancellationToken
				);
			}

			/// <summary>
			/// Returns a <see cref="IDiagnosticReceiver"/> that will be used during enumeration of the filter when <see cref="Mode"/> is equal to <see cref="FilterMode.Diagnostics"/>.
			/// </summary>
			protected virtual IDiagnosticReceiver GetDiagnosticReceiver()
			{
				return DurianGeneratorBase.GetDiagnosticReceiver(Generator) ?? DiagnosticReceiver.Factory.Empty();
			}

			/// <summary>
			/// Returns a <see cref="INodeDiagnosticReceiver"/> that will be used enumeration of the filter when <see cref="Mode"/> is equal to <see cref="FilterMode.Logs"/> or <see cref="FilterMode.Both"/>.
			/// </summary>
			/// <param name="includeDiagnostics">
			/// Determines whether to include diagnostics other than log files.
			/// <para>If <see cref="Mode"/> is equal to <see cref="FilterMode.Both"/>,
			/// <paramref name="includeDiagnostics"/> is <see langword="true"/>, otherwise <see langword="false"/>.</para>
			/// </param>
			protected virtual INodeDiagnosticReceiver GetLogReceiver(bool includeDiagnostics)
			{
				return DurianGeneratorBase.GetLogReceiver(Generator, includeDiagnostics) ?? DiagnosticReceiver.Factory.Empty();
			}

			private IEnumerable<TData> Filtrate_Internal(
				TCompilation compilation,
				IEnumerable<TSyntax> collectedNodes,
				IDiagnosticReceiver diagnosticReceiver,
				CancellationToken cancellationToken
			)
			{
				foreach (TSyntax decl in collectedNodes)
				{
					if (decl is null)
					{
						continue;
					}

					if (ValidateAndCreate(decl, compilation, out TData? data, diagnosticReceiver, cancellationToken))
					{
						yield return data;
					}
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedSyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/> class.
		/// </summary>
		/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		protected CachedSyntaxFilterValidator(IDurianGenerator generator) : base(generator)
		{
		}

		/// <inheritdoc cref="ICachedGeneratorSyntaxFilter{T}.Filtrate(in CachedGeneratorExecutionContext{T})"/>
		public IEnumerable<TData> Filtrate(in CachedGeneratorExecutionContext<TData> context)
		{
			ref readonly GeneratorExecutionContext ex = ref context.GetContext();

			if (GetCompilation(in ex) is not TCompilation compilation ||
				ex.SyntaxReceiver is not TSyntaxReceiver syntaxReceiver ||
				GetCandidateNodes(syntaxReceiver) is not IEnumerable<TSyntax> list
			)
			{
				return Array.Empty<TData>();
			}

			return Filtrate_Internal(compilation, list, context.GetCachedData(), ex.CancellationToken);
		}

		/// <inheritdoc cref="ICachedGeneratorSyntaxFilter{T}.Filtrate(in CachedGeneratorExecutionContext{T})"/>
		/// <exception cref="InvalidOperationException">Target <see cref="GeneratorSyntaxFilter{TCompilation, TSyntaxReceiver, TData}.Generator"/> is not initialized.</exception>
		public virtual IEnumerator<TData> GetEnumerator(in CachedGeneratorExecutionContext<TData> context)
		{
			EnsureInitialized();

			return new CachedFilterEnumerator<TData>(this, Generator.TargetCompilation, this, in context.GetCachedData());
		}

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

		private IEnumerable<TData> Filtrate_Internal(TCompilation compilation, IEnumerable<TSyntax> collectedNodes, CachedData<TData> cache, CancellationToken cancellationToken)
		{
			foreach (TSyntax node in collectedNodes)
			{
				if (node is null)
				{
					continue;
				}

				if (cache.TryGetCachedValue(node.GetLocation().GetLineSpan(), out TData? data) ||
					ValidateAndCreate(node, compilation, out data, cancellationToken))
				{
					yield return data;
				}
			}
		}
	}

	/// <inheritdoc cref="CachedSyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/>
	public abstract class CachedSyntaxFilterValidator<TCompilation, TSyntaxReceiver, TSyntax, TSymbol> : CachedSyntaxFilterValidator<TCompilation, TSyntaxReceiver, TSyntax, TSymbol, IMemberData>
		where TCompilation : class, ICompilationData
		where TSyntaxReceiver : IDurianSyntaxReceiver
		where TSyntax : CSharpSyntaxNode
		where TSymbol : ISymbol
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CachedSyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol}"/> class.
		/// </summary>
		/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		protected CachedSyntaxFilterValidator(IDurianGenerator generator) : base(generator)
		{
		}
	}

	/// <inheritdoc cref="CachedSyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/>
	public abstract class CachedSyntaxFilterValidator<TCompilation, TSyntaxReceiver, TSyntax> : CachedSyntaxFilterValidator<TCompilation, TSyntaxReceiver, TSyntax, ISymbol, IMemberData>
		where TCompilation : class, ICompilationData
		where TSyntaxReceiver : IDurianSyntaxReceiver
		where TSyntax : CSharpSyntaxNode
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CachedSyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax}"/> class.
		/// </summary>
		/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		protected CachedSyntaxFilterValidator(IDurianGenerator generator) : base(generator)
		{
		}
	}

	/// <inheritdoc cref="CachedSyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/>
	public abstract class CachedSyntaxFilterValidator<TCompilation, TSyntaxReceiver> : CachedSyntaxFilterValidator<TCompilation, TSyntaxReceiver, CSharpSyntaxNode, ISymbol, IMemberData>
		where TCompilation : class, ICompilationData
		where TSyntaxReceiver : IDurianSyntaxReceiver
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CachedSyntaxFilterValidator{TCompilation, TSyntaxReceiver}"/> class.
		/// </summary>
		/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		protected CachedSyntaxFilterValidator(IDurianGenerator generator) : base(generator)
		{
		}
	}

	/// <inheritdoc cref="CachedSyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/>
	public abstract class CachedSyntaxFilterValidator<TCompilation> : CachedSyntaxFilterValidator<TCompilation, IDurianSyntaxReceiver, CSharpSyntaxNode, ISymbol, IMemberData>
		where TCompilation : class, ICompilationData
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CachedSyntaxFilterValidator{TCompilation}"/> class.
		/// </summary>
		/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		protected CachedSyntaxFilterValidator(IDurianGenerator generator) : base(generator)
		{
		}
	}

	/// <inheritdoc cref="CachedSyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/>
	public abstract class CachedSyntaxFilterValidator : CachedSyntaxFilterValidator<ICompilationData, IDurianSyntaxReceiver, CSharpSyntaxNode, ISymbol, IMemberData>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CachedSyntaxFilterValidator"/> class.
		/// </summary>
		/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		protected CachedSyntaxFilterValidator(IDurianGenerator generator) : base(generator)
		{
		}
	}
}