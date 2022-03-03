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

namespace Durian.Analysis
{
    /// <summary>
    /// <see cref="ISyntaxFilter"/> that validates the filtrated nodes.
    /// </summary>
    /// <typeparam name="TCompilation">Type of <see cref="ICompilationData"/> this <see cref="SyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/> uses.</typeparam>
    /// <typeparam name="TSyntaxReceiver">Type of <see cref="IDurianSyntaxReceiver"/> this <see cref="SyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/> uses.</typeparam>
    /// <typeparam name="TSyntax">Type of <see cref="CSharpSyntaxNode"/> this <see cref="SyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/> uses.</typeparam>
    /// <typeparam name="TSymbol">Type of <see cref="ISyntaxFilter"/> this <see cref="SyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/> uses.</typeparam>
    /// <typeparam name="TData">Type of <see cref="IMemberData"/> this <see cref="SyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/> returns.</typeparam>
    public abstract class SyntaxFilterValidator<TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData> : GeneratorSyntaxFilter<TCompilation, TSyntaxReceiver, TData>, INodeProvider<TSyntax>, INodeValidator<TData>
        where TCompilation : class, ICompilationData
        where TSyntaxReceiver : IDurianSyntaxReceiver
        where TSyntax : CSharpSyntaxNode
        where TSymbol : ISymbol
        where TData : IMemberData
    {
        /// <summary>
        /// <see cref="SyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/> that reports diagnostics during filtration.
        /// </summary>
        public abstract new class WithDiagnostics : SyntaxFilterValidator<TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData>, INodeValidatorWithDiagnostics<TData>, IGeneratorSyntaxFilterWithDiagnostics
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
            /// Returns a new <see cref="INodeDiagnosticReceiver"/> if it couldn't be created automatically.
            /// </summary>
            /// <param name="includeDiagnostics">Determines whether diagnostics should be reported separately from log files.</param>
            protected virtual INodeDiagnosticReceiver GetNodeDiagnosticReceiverFallback(bool includeDiagnostics)
            {
                return new DiagnosticReceiver.Empty();
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

            private IDiagnosticReceiver GetDiagnosticReceiver()
            {
                if (Generator is DurianGeneratorBase durian)
                {
                    return durian.DiagnosticReceiver!;
                }

                return DiagnosticReceiver.Factory.SourceGenerator();
            }

            private INodeDiagnosticReceiver GetLogReceiver(bool includeDiagnostics)
            {
                if (includeDiagnostics)
                {
                    if (Generator is ILoggableGenerator loggable)
                    {
                        return LoggableDiagnosticReceiver.Factory.SourceGenerator(loggable);
                    }
                }
                else
                {
                    if (Generator is DurianGeneratorBase durian)
                    {
                        return durian.LogReceiver;
                    }

                    if (Generator is ILoggableGenerator loggable)
                    {
                        return LoggableDiagnosticReceiver.Factory.Basic(loggable);
                    }
                }

                return GetNodeDiagnosticReceiverFallback(includeDiagnostics);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/> class.
        /// </summary>
        /// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
        protected SyntaxFilterValidator(IDurianGenerator generator) : base(generator)
        {
        }

        /// <inheritdoc/>
        public override sealed IEnumerable<TData> Filtrate(
            TCompilation compilation,
            TSyntaxReceiver syntaxReceiver,
            CancellationToken cancellationToken = default
        )
        {
            if (GetCandidateNodes(syntaxReceiver) is not IEnumerable<TSyntax> list)
            {
                return Array.Empty<TData>();
            }

            return Filtrate_Internal(compilation, list, cancellationToken);
        }

        /// <inheritdoc/>
        public override sealed IEnumerable<TData> Filtrate(
            TCompilation compilation,
            IEnumerable<CSharpSyntaxNode> collectedNodes,
            CancellationToken cancellationToken = default
        )
        {
            if (collectedNodes is not IEnumerable<TSyntax> list)
            {
                list = collectedNodes.OfType<TSyntax>();
            }

            return Filtrate_Internal(compilation, list, cancellationToken);
        }

        /// <inheritdoc/>
        public override sealed IEnumerable<TData> Filtrate(in GeneratorExecutionContext context)
        {
            if (GetCompilation(in context) is not TCompilation compilation ||
                context.SyntaxReceiver is not TSyntaxReceiver syntaxReceiver ||
                GetCandidateNodes(syntaxReceiver) is not IEnumerable<TSyntax> list
            )
            {
                return Array.Empty<TData>();
            }

            return Filtrate_Internal(compilation, list, context.CancellationToken);
        }

        /// <summary>
        /// Returns a collection of <typeparamref name="TSyntax"/>es collected by the <see cref="GeneratorSyntaxFilter{TCompilation, TSyntaxReceiver, TData}.Generator"/>'s <see cref="IDurianSyntaxReceiver"/> that can be filtrated by this filter.
        /// </summary>
        public IEnumerable<TSyntax> GetCandidateNodes()
        {
            if (Generator.SyntaxReceiver is not TSyntaxReceiver syntaxReceiver || GetCandidateNodes(syntaxReceiver) is not IEnumerable<TSyntax> list)
            {
                return Array.Empty<TSyntax>();
            }

            return list;
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">Target <see cref="GeneratorSyntaxFilter{TCompilation, TSyntaxReceiver, TData}.Generator"/> is not initialized.</exception>
        public override IEnumerator<TData> GetEnumerator()
        {
            EnsureInitialized();

            return new FilterEnumerator<TData>(this, Generator.TargetCompilation, this);
        }

        /// <inheritdoc cref="IValidationDataProvider.GetValidationData(CSharpSyntaxNode, ICompilationData, out SemanticModel?, out ISymbol?, CancellationToken)"/>
        public virtual bool GetValidationData(
            TSyntax node,
            TCompilation compilation,
            [NotNullWhen(true)] out SemanticModel? semanticModel,
            [NotNullWhen(true)] out TSymbol? symbol,
            CancellationToken cancellationToken = default
        )
        {
            SemanticModel semantic = compilation.Compilation.GetSemanticModel(node.SyntaxTree);

            if (semantic.GetDeclaredSymbol(node, cancellationToken) is TSymbol t)
            {
                symbol = t;
                semanticModel = semantic;
                return true;
            }

            symbol = default;
            semanticModel = default;
            return false;
        }

        /// <inheritdoc cref="INodeValidator{T}.ValidateAndCreate(CSharpSyntaxNode, ICompilationData, out T, CancellationToken)"/>
        public virtual bool ValidateAndCreate(
            TSyntax node,
            TCompilation compilation,
            [NotNullWhen(true)] out TData? data,
            CancellationToken cancellationToken = default
        )
        {
            if (!GetValidationData(node, compilation, out SemanticModel? semanticModel, out TSymbol? symbol, cancellationToken))
            {
                data = default;
                return false;
            }

            return ValidateAndCreate(node, compilation, semanticModel, symbol, out data, cancellationToken);
        }

        /// <inheritdoc cref="INodeValidator{T}.ValidateAndCreate(CSharpSyntaxNode, ICompilationData, SemanticModel, ISymbol, out T, CancellationToken)"/>
        public abstract bool ValidateAndCreate(
            TSyntax node,
            TCompilation compilation,
            SemanticModel semanticModel,
            TSymbol symbol,
            [NotNullWhen(true)] out TData? data,
            CancellationToken cancellationToken = default
        );

        IEnumerable<TSyntax> INodeProvider<TSyntax>.GetNodes()
        {
            return GetCandidateNodes();
        }

        IEnumerable<CSharpSyntaxNode> INodeProvider.GetNodes()
        {
            return GetCandidateNodes();
        }

        bool IValidationDataProvider.GetValidationData(
                    CSharpSyntaxNode node,
                    ICompilationData compilation,
                    [NotNullWhen(true)] out SemanticModel? semanticModel,
                    [NotNullWhen(true)] out ISymbol? symbol,
                    CancellationToken cancellationToken
                )
        {
            if (node is not TSyntax syntax || compilation is not TCompilation c)
            {
                semanticModel = default;
                symbol = default;
                return false;
            }

            bool isValid = GetValidationData(
                syntax,
                c,
                out semanticModel,
                out TSymbol? s,
                cancellationToken
            );

            symbol = s;
            return isValid;
        }

        bool INodeValidator<TData>.ValidateAndCreate(
                    CSharpSyntaxNode node,
                    ICompilationData compilation,
                    [NotNullWhen(true)] out TData? data,
                    CancellationToken cancellationToken
                )
        {
            if (node is not TSyntax syntax || compilation is not TCompilation c)
            {
                data = default;
                return false;
            }

            return ValidateAndCreate(syntax, c, out data, cancellationToken);
        }

        bool INodeValidator<TData>.ValidateAndCreate(
                    CSharpSyntaxNode node,
                    ICompilationData compilation,
                    SemanticModel semanticModel,
                    ISymbol symbol,
                    [NotNullWhen(true)] out TData? data,
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
                cancellationToken
            );
        }

        internal TCompilation? GetCompilation(in GeneratorExecutionContext context)
        {
            if (context.Compilation is not null &&
                context.Compilation == Generator.TargetCompilation.Compilation &&
                Generator.TargetCompilation is TCompilation compilation
            )
            {
                return compilation;
            }

            return CreateCompilation(in context);
        }

        /// <summary>
        /// Creates a new <typeparamref name="TCompilation"/> from the specified <paramref name="context"/> if <see cref="GeneratorExecutionContext.Compilation"/>
        /// is not equivalent to <see cref="ICompilationData.Compilation"/> retrieved from the <see cref="GeneratorSyntaxFilter{TCompilation, TSyntaxReceiver, TData}.Generator"/> property.
        /// </summary>
        /// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
        protected virtual TCompilation? CreateCompilation(in GeneratorExecutionContext context)
        {
            return null;
        }

        /// <summary>
        /// If the <see cref="IDurianGenerator.TargetCompilation"/> of <see cref="GeneratorSyntaxFilter{TCompilation, TSyntaxReceiver, TData}.Generator"/> is <see langword="null"/>, throws a <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Target generator is not initialized.</exception>
        protected void EnsureInitialized()
        {
            if (Generator.TargetCompilation is null)
            {
                throw new InvalidOperationException("Target generator is not initialized!");
            }
        }

        /// <summary>
        /// Returns a collection of candidate <typeparamref name="TSyntax"/> collected by the specified <paramref name="syntaxReceiver"/>.
        /// </summary>
        /// <param name="syntaxReceiver"><typeparamref name="TSyntaxReceiver"/> to get the candidate <typeparamref name="TSyntax"/>es from.</param>
        protected virtual IEnumerable<TSyntax>? GetCandidateNodes(TSyntaxReceiver syntaxReceiver)
        {
            return syntaxReceiver.GetNodes().OfType<TSyntax>();
        }

        private IEnumerable<TData> Filtrate_Internal(
                    TCompilation compilation,
            IEnumerable<TSyntax> collectedNodes,
            CancellationToken cancellationToken
        )
        {
            foreach (TSyntax decl in collectedNodes)
            {
                if (decl is null)
                {
                    continue;
                }

                if (ValidateAndCreate(decl, compilation, out TData? data, cancellationToken))
                {
                    yield return data;
                }
            }
        }
    }

    /// <inheritdoc cref="SyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/>
    public abstract class SyntaxFilterValidator<TCompilation, TSyntaxReceiver, TSyntax, TSymbol> : SyntaxFilterValidator<TCompilation, TSyntaxReceiver, TSyntax, TSymbol, IMemberData>
        where TCompilation : class, ICompilationData
        where TSyntaxReceiver : IDurianSyntaxReceiver
        where TSyntax : CSharpSyntaxNode
        where TSymbol : ISymbol
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol}"/> class.
        /// </summary>
        /// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
        protected SyntaxFilterValidator(IDurianGenerator generator) : base(generator)
        {
        }
    }

    /// <inheritdoc cref="SyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/>
    public abstract class SyntaxFilterValidator<TCompilation, TSyntaxReceiver, TSyntax> : SyntaxFilterValidator<TCompilation, TSyntaxReceiver, TSyntax, ISymbol, IMemberData>
        where TCompilation : class, ICompilationData
        where TSyntaxReceiver : IDurianSyntaxReceiver
        where TSyntax : CSharpSyntaxNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax}"/> class.
        /// </summary>
        /// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
        protected SyntaxFilterValidator(IDurianGenerator generator) : base(generator)
        {
        }
    }

    /// <inheritdoc cref="SyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/>
    public abstract class SyntaxFilterValidator<TCompilation, TSyntaxReceiver> : SyntaxFilterValidator<TCompilation, TSyntaxReceiver, CSharpSyntaxNode, ISymbol, IMemberData>
        where TCompilation : class, ICompilationData
        where TSyntaxReceiver : IDurianSyntaxReceiver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxFilterValidator{TCompilation, TSyntaxReceiver}"/> class.
        /// </summary>
        /// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
        protected SyntaxFilterValidator(IDurianGenerator generator) : base(generator)
        {
        }
    }

    /// <inheritdoc cref="SyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/>
    public abstract class SyntaxFilterValidator<TCompilation> : SyntaxFilterValidator<TCompilation, IDurianSyntaxReceiver, CSharpSyntaxNode, ISymbol, IMemberData>
        where TCompilation : class, ICompilationData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxFilterValidator{TCompilation}"/> class.
        /// </summary>
        /// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
        protected SyntaxFilterValidator(IDurianGenerator generator) : base(generator)
        {
        }
    }

    /// <inheritdoc cref="SyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/>
    public abstract class SyntaxFilterValidator : SyntaxFilterValidator<ICompilationData, IDurianSyntaxReceiver, CSharpSyntaxNode, ISymbol, IMemberData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxFilterValidator"/> class.
        /// </summary>
        /// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
        protected SyntaxFilterValidator(IDurianGenerator generator) : base(generator)
        {
        }
    }
}