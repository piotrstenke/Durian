// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Durian.Analysis
{
    /// <summary>
    /// Filtrates <see cref="CSharpSyntaxNode"/>s collected by a <see cref="IDurianSyntaxReceiver"/>.
    /// </summary>
    /// <typeparam name="TCompilation">Type of <see cref="ICompilationData"/> this <see cref="SyntaxFilter{TCompilation, TSyntaxReceiver, TData}"/> uses.</typeparam>
    /// <typeparam name="TSyntaxReceiver">Type of <see cref="IDurianSyntaxReceiver"/> this <see cref="SyntaxFilter{TCompilation, TSyntaxReceiver, TData}"/> uses.</typeparam>
    /// <typeparam name="TData">Type of <see cref="IMemberData"/> this <see cref="SyntaxFilter{TCompilation, TSyntaxReceiver, TData}"/> returns.</typeparam>
    public abstract class SyntaxFilter<TCompilation, TSyntaxReceiver, TData> : ISyntaxFilter
        where TCompilation : ICompilationData
        where TSyntaxReceiver : IDurianSyntaxReceiver
        where TData : IMemberData
    {
        /// <summary>
        /// <see cref="SyntaxFilter{TCompilation, TSyntaxReceiver, TData}"/> that reports diagnostics during filtration.
        /// </summary>
        public abstract class WithDiagnostics : SyntaxFilter<TCompilation, TSyntaxReceiver, TData>, ISyntaxFilterWithDiagnostics
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WithDiagnostics"/> class.
            /// </summary>
            protected WithDiagnostics()
            {
            }

            /// <inheritdoc cref="ISyntaxFilterWithDiagnostics.Filtrate(ICompilationData, IDurianSyntaxReceiver, IDiagnosticReceiver, CancellationToken)"/>
            public abstract IEnumerable<TData> Filtrate(TCompilation compilation, TSyntaxReceiver syntaxReceiver, IDiagnosticReceiver diagnosticReceiver, CancellationToken cancellationToken = default);

            /// <inheritdoc cref="ISyntaxFilterWithDiagnostics.Filtrate(ICompilationData, IEnumerable{CSharpSyntaxNode}, IDiagnosticReceiver, CancellationToken)"/>
            public abstract IEnumerable<TData> Filtrate(TCompilation compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, IDiagnosticReceiver diagnosticReceiver, CancellationToken cancellationToken = default);

            IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, IDiagnosticReceiver diagnosticReceiver, CancellationToken cancellationToken)
            {
                return Filtrate((TCompilation)compilation, (TSyntaxReceiver)syntaxReceiver, diagnosticReceiver, cancellationToken).Cast<IMemberData>();
            }

            IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, IDiagnosticReceiver diagnosticReceiver, CancellationToken cancellationToken)
            {
                return Filtrate((TCompilation)compilation, collectedNodes, diagnosticReceiver, cancellationToken).Cast<IMemberData>();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxFilter{TCompilation, TSyntaxReceiver, TData}"/> class.
        /// </summary>
        protected SyntaxFilter()
        {
        }

        /// <inheritdoc cref="ISyntaxFilter.Filtrate(ICompilationData, IDurianSyntaxReceiver, CancellationToken)"/>
        public abstract IEnumerable<TData> Filtrate(TCompilation compilation, TSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken = default);

        /// <inheritdoc cref="ISyntaxFilter.Filtrate(ICompilationData, IEnumerable{CSharpSyntaxNode}, CancellationToken)"/>
        public abstract IEnumerable<TData> Filtrate(TCompilation compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken = default);

        IEnumerable<IMemberData> ISyntaxFilter.Filtrate(ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken)
        {
            return Filtrate((TCompilation)compilation, (TSyntaxReceiver)syntaxReceiver, cancellationToken).Cast<IMemberData>();
        }

        IEnumerable<IMemberData> ISyntaxFilter.Filtrate(ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken)
        {
            return Filtrate((TCompilation)compilation, collectedNodes, cancellationToken).Cast<IMemberData>();
        }
    }

    /// <inheritdoc cref="SyntaxFilter{TCompilation, TSyntaxReceiver, TData}"/>
    public abstract class SyntaxFilter<TCompilation, TSyntaxReceiver> : SyntaxFilter<TCompilation, TSyntaxReceiver, IMemberData>
        where TCompilation : ICompilationData
        where TSyntaxReceiver : IDurianSyntaxReceiver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxFilter{TCompilation, TSyntaxReceiver}"/> class.
        /// </summary>
        protected SyntaxFilter()
        {
        }
    }

    /// <inheritdoc cref="SyntaxFilter{TCompilation, TSyntaxReceiver, TData}"/>
    public abstract class SyntaxFilter<TCompilation> : SyntaxFilter<TCompilation, IDurianSyntaxReceiver, IMemberData>
        where TCompilation : ICompilationData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxFilter{TCompilation}"/> class.
        /// </summary>
        protected SyntaxFilter()
        {
        }
    }

    /// <inheritdoc cref="SyntaxFilter{TCompilation, TSyntaxReceiver, TData}"/>
    public abstract class SyntaxFilter : SyntaxFilter<ICompilationData, IDurianSyntaxReceiver, IMemberData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxFilter"/> class.
        /// </summary>
        protected SyntaxFilter()
        {
        }
    }
}