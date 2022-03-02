// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Durian.Analysis
{
    /// <summary>
    /// <see cref="ISyntaxFilter"/> that filtrates <see cref="CSharpSyntaxNode"/>s for the specified <see cref="IDurianGenerator"/>.
    /// </summary>
    /// <typeparam name="TCompilation">Type of <see cref="ICompilationData"/> this <see cref="GeneratorSyntaxFilter{TCompilation, TSyntaxReceiver, TData}"/> uses.</typeparam>
    /// <typeparam name="TSyntaxReceiver">Type of <see cref="IDurianSyntaxReceiver"/> this <see cref="GeneratorSyntaxFilter{TCompilation, TSyntaxReceiver, TData}"/> uses.</typeparam>
    /// <typeparam name="TData">Type of <see cref="IMemberData"/> this <see cref="GeneratorSyntaxFilter{TCompilation, TSyntaxReceiver, TData}"/> returns.</typeparam>
    public abstract class GeneratorSyntaxFilter<TCompilation, TSyntaxReceiver, TData> : SyntaxFilter<TCompilation, TSyntaxReceiver, TData>, IGeneratorSyntaxFilter
        where TCompilation : ICompilationData
        where TSyntaxReceiver : IDurianSyntaxReceiver
        where TData : IMemberData
    {
        /// <summary>
        /// <see cref="GeneratorSyntaxFilter{TCompilation, TSyntaxReceiver, TData}"/> that reports diagnostics during filtration.
        /// </summary>
        public abstract new class WithDiagnostics : GeneratorSyntaxFilter<TCompilation, TSyntaxReceiver, TData>, IGeneratorSyntaxFilterWithDiagnostics
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

        /// <inheritdoc cref="IGeneratorSyntaxFilter.Generator"/>
        public IDurianGenerator Generator { get; }

        /// <inheritdoc/>
        public virtual bool IncludeGeneratedSymbols { get; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorSyntaxFilter{TCompilation, TSyntaxReceiver, TData}"/> class.
        /// </summary>
        /// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
        protected GeneratorSyntaxFilter(IDurianGenerator generator)
        {
            if (generator is null)
            {
                throw new ArgumentNullException(nameof(generator));
            }

            Generator = generator;
        }

        /// <inheritdoc cref="IGeneratorSyntaxFilter.Filtrate(in GeneratorExecutionContext)"/>
        public abstract IEnumerable<TData> Filtrate(in GeneratorExecutionContext context);

        /// <inheritdoc cref="IEnumerable{T}.GetEnumerator()"/>
        public abstract IEnumerator<TData> GetEnumerator();

        IEnumerable<IMemberData> IGeneratorSyntaxFilter.Filtrate(in GeneratorExecutionContext context)
        {
            return Filtrate(in context).Cast<IMemberData>();
        }

        IEnumerator<IMemberData> IEnumerable<IMemberData>.GetEnumerator()
        {
            IEnumerator<TData> enumerator = GetEnumerator();

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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <inheritdoc cref="GeneratorSyntaxFilter{TCompilation, TSyntaxReceiver, TData}"/>
    public abstract class GeneratorSyntaxFilter<TCompilation, TSyntaxReceiver> : GeneratorSyntaxFilter<TCompilation, TSyntaxReceiver, IMemberData>
        where TCompilation : ICompilationData
        where TSyntaxReceiver : IDurianSyntaxReceiver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorSyntaxFilter{TCompilation, TSyntaxReceiver}"/> class.
        /// </summary>
        /// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
        protected GeneratorSyntaxFilter(IDurianGenerator generator) : base(generator)
        {
        }
    }

    /// <inheritdoc cref="GeneratorSyntaxFilter{TCompilation, TSyntaxReceiver, TData}"/>
    public abstract class GeneratorSyntaxFilter<TCompilation> : GeneratorSyntaxFilter<TCompilation, IDurianSyntaxReceiver, IMemberData>
        where TCompilation : ICompilationData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorSyntaxFilter{TCompilation}"/> class.
        /// </summary>
        /// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
        protected GeneratorSyntaxFilter(IDurianGenerator generator) : base(generator)
        {
        }
    }

    /// <inheritdoc cref="GeneratorSyntaxFilter{TCompilation, TSyntaxReceiver, TData}"/>
    public abstract class GeneratorSyntaxFilter : GeneratorSyntaxFilter<ICompilationData, IDurianSyntaxReceiver, IMemberData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorSyntaxFilter"/> class.
        /// </summary>
        /// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
        protected GeneratorSyntaxFilter(IDurianGenerator generator) : base(generator)
        {
        }
    }
}