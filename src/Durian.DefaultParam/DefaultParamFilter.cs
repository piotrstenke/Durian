// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Cache;
using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Durian.Analysis.DefaultParam
{
    /// <summary>
    /// <c>DefaultParam</c>-specific <see cref="SyntaxFilterValidator{TCompilation, TSyntaxReceiver, TSyntax, TSymbol, TData}"/>.
    /// </summary>
    /// <typeparam name="TSyntax">Type of <see cref="CSharpSyntaxNode"/> this <see cref="DefaultParamFilter{TSyntax, TSymbol, TData}"/> uses.</typeparam>
    /// <typeparam name="TSymbol">Type of <see cref="ISyntaxFilter"/> this <see cref="DefaultParamFilter{TSyntax, TSymbol, TData}"/> uses.</typeparam>
    /// <typeparam name="TData">Type of <see cref="IMemberData"/> this <see cref="DefaultParamFilter{TSyntax, TSymbol, TData}"/> uses.</typeparam>
    public abstract class DefaultParamFilter<TSyntax, TSymbol, TData> : CachedSyntaxFilterValidator<DefaultParamCompilationData, DefaultParamSyntaxReceiver, TSyntax, TSymbol, TData>.WithDiagnostics, IDefaultParamFilter
        where TSyntax : CSharpSyntaxNode
        where TSymbol : class, ISymbol
        where TData : IDefaultParamTarget
    {
        /// <inheritdoc/>
        public new DefaultParamGenerator Generator => (base.Generator as DefaultParamGenerator)!;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultParamFilter{TSyntax, TSymbol, TData}"/> class.
        /// </summary>
        /// <param name="generator"><see cref="DefaultParamGenerator"/> that is the target of this filter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
        protected DefaultParamFilter(DefaultParamGenerator generator) : base(generator)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultParamFilter{TSyntax, TSymbol, TData}"/> class.
        /// </summary>
        /// <param name="generator"><see cref="DefaultParamGenerator"/> that is the target of this filter.</param>
        /// <param name="hintNameProvider"><see cref="IHintNameProvider"/> that is used to create a hint name for the generated source.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="hintNameProvider"/> is <see langword="null"/>.</exception>
        protected DefaultParamFilter(DefaultParamGenerator generator, IHintNameProvider hintNameProvider) : base(generator, hintNameProvider)
        {
        }

        /// <inheritdoc/>
        public override sealed IEnumerator<TData> GetEnumerator()
        {
            EnsureInitialized();

            return Mode switch
            {
                FilterMode.Diagnostics => new FilterEnumeratorWithDiagnostics<TData>(this, Generator.TargetCompilation!, this, Generator.DiagnosticReceiver!),
                FilterMode.Logs => new DefaultParamFilterEnumerator<TData>(this, Generator.LogReceiver),
                FilterMode.Both => new DefaultParamFilterEnumerator<TData>(this, LoggableDiagnosticReceiver.Factory.SourceGenerator(Generator, Generator.DiagnosticReceiver!)),
                _ => new FilterEnumerator<TData>(this, Generator.TargetCompilation!, this)
            };
        }

        /// <inheritdoc/>
        public override sealed IEnumerator<TData> GetEnumerator(in CachedGeneratorExecutionContext<TData> context)
        {
            EnsureInitialized();

            ref readonly CachedData<TData> cache = ref context.GetCachedData();

            return Mode switch
            {
                FilterMode.Diagnostics => new CachedFilterEnumeratorWithDiagnostics<TData>(this, Generator.TargetCompilation!, this, Generator.DiagnosticReceiver!, in cache),
                FilterMode.Logs => new CachedDefaultParamFilterEnumerator<TData>(this, Generator.LogReceiver, in cache),
                FilterMode.Both => new CachedDefaultParamFilterEnumerator<TData>(this, LoggableDiagnosticReceiver.Factory.SourceGenerator(Generator, Generator.DiagnosticReceiver!), in cache),
                _ => new CachedFilterEnumerator<TData>(this, Generator.TargetCompilation!, this, in cache)
            };
        }

        /// <inheritdoc/>
        [Obsolete("TypeParameterContainer is required for proper analysis. Use GetValidationData with a TypeParameterContainer parameter instead.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
        public override sealed bool GetValidationData(
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
            TSyntax node,
            DefaultParamCompilationData compilation,
            [NotNullWhen(true)] out SemanticModel? semanticModel,
            [NotNullWhen(true)] out TSymbol? symbol,
            CancellationToken cancellationToken = default
        )
        {
            return GetValidationData(node, compilation, out semanticModel, out symbol, out _, cancellationToken);
        }

        /// <inheritdoc/>
        public bool GetValidationData(
            TSyntax node,
            DefaultParamCompilationData compilation,
            [NotNullWhen(true)] out SemanticModel? semanticModel,
            [NotNullWhen(true)] out TSymbol? symbol,
            out TypeParameterContainer typeParameters,
            CancellationToken cancellationToken = default
        )
        {
            SemanticModel s = compilation.Compilation.GetSemanticModel(node.SyntaxTree);
            TypeParameterListSyntax? parameterList = GetTypeParameterList(node);
            typeParameters = GetTypeParameters(parameterList, s, compilation, cancellationToken);

            if (TypeParametersAreValid(in typeParameters, node))
            {
                symbol = s.GetDeclaredSymbol(node, cancellationToken) as TSymbol;

                if(symbol is not null)
                {
                    semanticModel = s;
                    return true;
                }
            }

            symbol = default;
            semanticModel = default;
            return false;
        }

        /// <inheritdoc/>
        public override sealed bool ValidateAndCreate(
            TSyntax node,
            DefaultParamCompilationData compilation,
            [NotNullWhen(true)] out TData? data,
            IDiagnosticReceiver diagnosticReceiver,
            CancellationToken cancellationToken = default
        )
        {
            if (!GetValidationData(node, compilation, out SemanticModel? semanticModel, out TSymbol? symbol, out TypeParameterContainer typeParameters, cancellationToken))
            {
                data = default;
                return false;
            }

            return ValidateAndCreate(node, compilation, semanticModel, symbol, in typeParameters, out data, diagnosticReceiver, cancellationToken);
        }

        /// <summary>
        /// Validates the specified <paramref name="node"/> and returns a new instance of <see cref="IMemberData"/> if the validation was a success.
        /// </summary>
        /// <param name="node"><see cref="CSharpSyntaxNode"/> to validate.</param>
        /// <param name="compilation">Parent <see cref="ICompilationData"/> of the target <paramref name="node"/>.</param>
        /// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="node"/>.</param>
        /// <param name="symbol"><see cref="ISymbol"/> that is represented by the <paramref name="node"/>.</param>
        /// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="node"/>'s type parameters.</param>
        /// <param name="data"><see cref="IMemberData"/> that is returned if the validation succeeds.</param>
        /// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
        /// <returns><see langword="true"/> if the validation was a success, <see langword="false"/> otherwise.</returns>
        public abstract bool ValidateAndCreate(
            TSyntax node,
            DefaultParamCompilationData compilation,
            SemanticModel semanticModel,
            TSymbol symbol,
            in TypeParameterContainer typeParameters,
            [NotNullWhen(true)] out TData? data,
            IDiagnosticReceiver diagnosticReceiver,
            CancellationToken cancellationToken = default
        );

        /// <inheritdoc/>
        [Obsolete("TypeParameterContainer is required for proper analysis. Use ValidateAndCreate with a TypeParameterContainer parameter instead.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
        public override sealed bool ValidateAndCreate(
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
            TSyntax node,
            DefaultParamCompilationData compilation,
            SemanticModel semanticModel,
            TSymbol symbol,
            [NotNullWhen(true)] out TData? data,
            IDiagnosticReceiver diagnosticReceiver,
            CancellationToken cancellationToken = default
        )
        {
            TypeParameterContainer typeParameters = GetTypeParameters(GetTypeParameterList(node), semanticModel, compilation, cancellationToken);

            return ValidateAndCreate(node, compilation, semanticModel, symbol, in typeParameters, out data, diagnosticReceiver, cancellationToken);
        }

        /// <inheritdoc/>
        public override sealed bool ValidateAndCreate(
            TSyntax node,
            DefaultParamCompilationData compilation,
            [NotNullWhen(true)] out TData? data,
            CancellationToken cancellationToken = default
        )
        {
            if (!GetValidationData(node, compilation, out SemanticModel? semanticModel, out TSymbol? symbol, out TypeParameterContainer typeParameters, cancellationToken))
            {
                data = default;
                return false;
            }

            return ValidateAndCreate(node, compilation, semanticModel, symbol, in typeParameters, out data, cancellationToken);
        }

        /// <summary>
        /// Validates the specified <paramref name="node"/> and returns a new instance of <see cref="IMemberData"/> if the validation was a success.
        /// </summary>
        /// <param name="node"><see cref="CSharpSyntaxNode"/> to validate.</param>
        /// <param name="compilation">Parent <see cref="ICompilationData"/> of the target <paramref name="node"/>.</param>
        /// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="node"/>.</param>
        /// <param name="symbol"><see cref="ISymbol"/> that is represented by the <paramref name="node"/>.</param>
        /// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="node"/>'s type parameters.</param>
        /// <param name="data"><see cref="IMemberData"/> that is returned if the validation succeeds.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
        /// <returns><see langword="true"/> if the validation was a success, <see langword="false"/> otherwise.</returns>
        public abstract bool ValidateAndCreate(
            TSyntax node,
            DefaultParamCompilationData compilation,
            SemanticModel semanticModel,
            TSymbol symbol,
            in TypeParameterContainer typeParameters,
            [NotNullWhen(true)] out TData? data,
            CancellationToken cancellationToken = default
        );

        /// <inheritdoc/>
        [Obsolete("TypeParameterContainer is required for proper analysis. Use ValidateAndCreate with a TypeParameterContainer parameter instead.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
        public override sealed bool ValidateAndCreate(
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
            TSyntax node,
            DefaultParamCompilationData compilation,
            SemanticModel semanticModel,
            TSymbol symbol,
            [NotNullWhen(true)] out TData? data,
            CancellationToken cancellationToken = default
        )
        {
            TypeParameterContainer typeParameters = GetTypeParameters(GetTypeParameterList(node), semanticModel, compilation, cancellationToken);

            return ValidateAndCreate(node, compilation, semanticModel, symbol, in typeParameters, out data, cancellationToken);
        }

        IEnumerable<IMemberData> ICachedGeneratorSyntaxFilter<IDefaultParamTarget>.Filtrate(in CachedGeneratorExecutionContext<IDefaultParamTarget> context)
        {
            return Filtrate(context.CastContext<TData>()).Cast<IMemberData>();
        }

        IEnumerator<IMemberData> ICachedGeneratorSyntaxFilter<IDefaultParamTarget>.GetEnumerator(in CachedGeneratorExecutionContext<IDefaultParamTarget> context)
        {
            IEnumerator<TData> enumerator = GetEnumerator(context.CastContext<TData>());

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

        bool IDefaultParamFilter.GetValidationData(
            CSharpSyntaxNode node,
            DefaultParamCompilationData compilation,
            [NotNullWhen(true)] out SemanticModel? semanticModel,
            [NotNullWhen(true)] out ISymbol? symbol,
            [NotNullWhen(true)] out TypeParameterContainer typeParameters,
            CancellationToken cancellationToken
        )
        {
            if (node is not TSyntax n)
            {
                semanticModel = default;
                symbol = default;
                typeParameters = default;
                return false;
            }

            bool isValid = GetValidationData(n, compilation, out semanticModel, out TSymbol? s, out typeParameters, cancellationToken);
            symbol = s;
            return isValid;
        }

        bool IDefaultParamFilter.ValidateAndCreate(
            CSharpSyntaxNode node,
            DefaultParamCompilationData compilation,
            [NotNullWhen(true)] out IDefaultParamTarget? data,
            CancellationToken cancellationToken
        )
        {
            if (node is not TSyntax n)
            {
                data = default;
                return false;
            }

            bool isValid = ValidateAndCreate(n, compilation, out TData? d, cancellationToken);
            data = d;
            return isValid;
        }

        bool IDefaultParamFilter.ValidateAndCreate(
            CSharpSyntaxNode node,
            DefaultParamCompilationData compilation,
            SemanticModel semanticModel,
            ISymbol symbol,
            in TypeParameterContainer typeParameters,
            [NotNullWhen(true)] out IDefaultParamTarget? data,
            CancellationToken cancellationToken
        )
        {
            if (node is not TSyntax n || symbol is not TSymbol s)
            {
                data = default;
                return false;
            }

            bool isValid = ValidateAndCreate(n, compilation, semanticModel, s, in typeParameters, out TData? d, cancellationToken);
            data = d;
            return isValid;
        }

        bool IDefaultParamFilter.ValidateAndCreate(
            CSharpSyntaxNode node,
            DefaultParamCompilationData compilation,
            [NotNullWhen(true)] out IDefaultParamTarget? data,
            IDiagnosticReceiver diagnosticReceiver,
            CancellationToken cancellationToken
        )
        {
            if (node is not TSyntax n)
            {
                data = default;
                return false;
            }

            bool isValid = ValidateAndCreate(n, compilation, out TData? d, cancellationToken);
            data = d;
            return isValid;
        }

        bool IDefaultParamFilter.ValidateAndCreate(
            CSharpSyntaxNode node,
            DefaultParamCompilationData compilation,
            SemanticModel semanticModel,
            ISymbol symbol,
            in TypeParameterContainer typeParameters,
            [NotNullWhen(true)] out IDefaultParamTarget? data,
            IDiagnosticReceiver diagnosticReceiver,
            CancellationToken cancellationToken
        )
        {
            if (node is not TSyntax n || symbol is not TSymbol s)
            {
                data = default;
                return false;
            }

            bool isValid = ValidateAndCreate(n, compilation, semanticModel, s, in typeParameters, out TData? d, diagnosticReceiver, cancellationToken);
            data = d;
            return isValid;
        }

        bool INodeValidatorWithDiagnostics<IDefaultParamTarget>.ValidateAndCreate(
            CSharpSyntaxNode node,
            ICompilationData compilation,
            [NotNullWhen(true)] out IDefaultParamTarget? data,
            IDiagnosticReceiver diagnosticReceiver,
            CancellationToken cancellationToken
        )
        {
            if (node is not TSyntax n || compilation is not DefaultParamCompilationData c)
            {
                data = default;
                return false;
            }

            bool isValid = ValidateAndCreate(n, c, out TData? d, diagnosticReceiver, cancellationToken);
            data = d;
            return isValid;
        }

        bool INodeValidatorWithDiagnostics<IDefaultParamTarget>.ValidateAndCreate(
            CSharpSyntaxNode node,
            ICompilationData compilation,
            SemanticModel semanticModel,
            ISymbol symbol,
            [NotNullWhen(true)] out IDefaultParamTarget? data,
            IDiagnosticReceiver diagnosticReceiver,
            CancellationToken cancellationToken
        )
        {
            if (node is not TSyntax n || compilation is not DefaultParamCompilationData c || symbol is not TSymbol s)
            {
                data = default;
                return false;
            }

            bool isValid = ValidateAndCreate(n, c, semanticModel, s, out TData? d, diagnosticReceiver, cancellationToken);
            data = d;
            return isValid;
        }

        bool INodeValidator<IDefaultParamTarget>.ValidateAndCreate(
            CSharpSyntaxNode node,
            ICompilationData compilation,
            [NotNullWhen(true)] out IDefaultParamTarget? data,
            CancellationToken cancellationToken
        )
        {
            if (node is not TSyntax n || compilation is not DefaultParamCompilationData c)
            {
                data = default;
                return false;
            }

            bool isValid = ValidateAndCreate(n, c, out TData? d, cancellationToken);
            data = d;
            return isValid;
        }

        bool INodeValidator<IDefaultParamTarget>.ValidateAndCreate(
            CSharpSyntaxNode node,
            ICompilationData compilation,
            SemanticModel semanticModel,
            ISymbol symbol,
            [NotNullWhen(true)] out IDefaultParamTarget? data,
            CancellationToken cancellationToken
        )
        {
            if (node is not TSyntax n || compilation is not DefaultParamCompilationData c || symbol is not TSymbol s)
            {
                data = default;
                return false;
            }

            bool isValid = ValidateAndCreate(n, c, semanticModel, s, out TData? d, cancellationToken);
            data = d;
            return isValid;
        }

        /// <inheritdoc/>
        protected override sealed DefaultParamCompilationData? CreateCompilation(in GeneratorExecutionContext context)
        {
            if (context.Compilation is not CSharpCompilation compilation)
            {
                return null;
            }

            return new DefaultParamCompilationData(compilation);
        }

        /// <summary>
        /// Returns a <see cref="TypeParameterListSyntax"/> associated with the specified <paramref name="node"/>.
        /// </summary>
        /// <param name="node"><typeparamref name="TSyntax"/> to get the <see cref="TypeParameterListSyntax"/> associated with.</param>
        protected abstract TypeParameterListSyntax? GetTypeParameterList(TSyntax node);

        /// <summary>
        /// Determines whether the collected <see cref="TypeParameterContainer"/> is valid for analysis.
        /// </summary>
        /// <param name="typeParameters"><see cref="TypeParameterContainer"/> to check if is valid for analysis.</param>
        /// <param name="node">Current <see cref="CSharpSyntaxNode"/>.</param>
        protected virtual bool TypeParametersAreValid(in TypeParameterContainer typeParameters, TSyntax node)
        {
            return typeParameters.HasDefaultParams;
        }

        private static TypeParameterContainer GetTypeParameters(
            TypeParameterListSyntax? typeParameters,
            SemanticModel semanticModel,
            DefaultParamCompilationData compilation,
            CancellationToken cancellationToken
        )
        {
            if (typeParameters is null)
            {
                return new TypeParameterContainer(null);
            }

            return new TypeParameterContainer(typeParameters.Parameters.Select(p => TypeParameterData.CreateFrom(p, semanticModel, compilation, cancellationToken)));
        }
    }
}