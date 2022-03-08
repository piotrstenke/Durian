// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Durian.Analysis.DefaultParam
{
    /// <summary>
    /// <see cref="CompilationData"/> that contains all <see cref="ISymbol"/>s needed to generate source code using the <see cref="DefaultParamGenerator"/>.
    /// </summary>
    public sealed class DefaultParamCompilationData : CompilationDataWithSymbols
    {
        /// <summary>
        /// <see cref="INamedTypeSymbol"/> of the <c>Durian.DefaultParamAttribute</c> class.
        /// </summary>
        public INamedTypeSymbol? DefaultParamAttribute { get; private set; }

        /// <summary>
        /// <see cref="INamedTypeSymbol"/> of the <c>Durian.Configuration.DefaultParamConfigurationAttribute</c> class.
        /// </summary>
        public INamedTypeSymbol? DefaultParamConfigurationAttribute { get; private set; }

        /// <summary>
        /// <see cref="INamedTypeSymbol"/> of the <c>Durian.Configuration.DefaultParamScopedConfigurationAttribute</c> class.
        /// </summary>
        public INamedTypeSymbol? DefaultParamScopedConfigurationAttribute { get; private set; }

        /// <summary>
        /// <see cref="INamedTypeSymbol"/> of the <c>Durian.Configuration.DPMethodConvention</c> enum.
        /// </summary>
        public INamedTypeSymbol? DPMethodConvention { get; private set; }

        /// <summary>
        /// <see cref="INamedTypeSymbol"/> of the <c>Durian.Configuration.DPTypeConvention</c> enum.
        /// </summary>
        public INamedTypeSymbol? DPTypeConvention { get; private set; }

        /// <summary>
        /// <see cref="DefaultParamConfiguration"/> created from the <c>Durian.Configuration.DefaultParamScopedConfigurationAttribute</c>
        /// defined on the <see cref="CompilationData.Compilation"/>'s main assembly. -or-
        /// <see cref="DefaultParamConfiguration.Default"/> if no <c>Durian.Configuration.DefaultParamScopedConfigurationAttribute</c> was found.
        /// </summary>
        public DefaultParamConfiguration GlobalConfiguration { get; }

        /// <inheritdoc/>
        [MemberNotNullWhen(false, nameof(DefaultParamAttribute), nameof(DefaultParamConfigurationAttribute), nameof(DefaultParamScopedConfigurationAttribute), nameof(DPMethodConvention), nameof(DPTypeConvention))]
        public override bool HasErrors
        {
            get => base.HasErrors;
            protected set => base.HasErrors = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultParamCompilationData"/> class.
        /// </summary>
        /// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
        public DefaultParamCompilationData(CSharpCompilation compilation) : base(compilation)
        {
            GlobalConfiguration = GetConfiguration(compilation, DefaultParamScopedConfigurationAttribute);
        }

        /// <summary>
        /// Creates a new instance of <see cref="DefaultParamConfiguration"/> based on the
        /// <c>Durian.Configuration.DefaultParamScopedConfigurationAttribute</c> defined on the
        /// <paramref name="compilation"/>'s main assembly or <see cref="DefaultParamConfiguration.Default"/>
        /// if no <c>Durian.Configuration.DefaultParamScopedConfigurationAttribute</c> was found.
        /// </summary>
        /// <param name="compilation"><see cref="CSharpCompilation"/> to get the <see cref="DefaultParamConfiguration"/> for.</param>
        /// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
        public static DefaultParamConfiguration GetConfiguration(CSharpCompilation compilation)
        {
            if (compilation is null)
            {
                throw new ArgumentNullException(nameof(compilation));
            }

            INamedTypeSymbol? configurationAttribute = compilation.GetTypeByMetadataName(DefaultParamScopedConfigurationAttributeProvider.FullName);
            return GetConfiguration(compilation, configurationAttribute);
        }

        /// <inheritdoc/>
        public override void Reset()
        {
            base.Reset();

            DefaultParamAttribute = IncludeType(DefaultParamAttributeProvider.FullName);
            DefaultParamConfigurationAttribute = IncludeType(DefaultParamConfigurationAttributeProvider.FullName);
            DefaultParamScopedConfigurationAttribute = IncludeType(DefaultParamScopedConfigurationAttributeProvider.FullName);
            DPTypeConvention = IncludeType(DPTypeConventionProvider.FullName);
            DPMethodConvention = IncludeType(DPMethodConventionProvider.FullName);
        }

        private static DefaultParamConfiguration BuildConfiguration(AttributeData attribute)
        {
            bool applyNewModififer = !attribute.TryGetNamedArgumentValue(
                DefaultParamConfigurationAttributeProvider.ApplyNewModifierWhenPossible,
                out bool value)
                || value;

            MethodConvention methodCon = (MethodConvention)GetValidConventionEnumValue(
                attribute.GetNamedArgumentValue<int>(DefaultParamConfigurationAttributeProvider.MethodConvention));

            TypeConvention typeCon = (TypeConvention)GetValidConventionEnumValue(
                attribute.GetNamedArgumentValue<int>(DefaultParamConfigurationAttributeProvider.TypeConvention));

            string? @namespace = attribute.GetNamedArgumentValue<string>(DefaultParamConfigurationAttributeProvider.TargetNamespace);

            return new()
            {
                ApplyNewModifierWhenPossible = applyNewModififer,
                MethodConvention = methodCon,
                TypeConvention = typeCon,
                TargetNamespace = @namespace
            };
        }

        private static DefaultParamConfiguration GetConfiguration(CSharpCompilation compilation, INamedTypeSymbol? configurationAttribute)
        {
            if (configurationAttribute is null)
            {
                return DefaultParamConfiguration.Default;
            }

            foreach (AttributeData attribute in compilation.Assembly.GetAttributes())
            {
                if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, configurationAttribute))
                {
                    continue;
                }

                DefaultParamConfiguration config = BuildConfiguration(attribute);

                if (DefaultParamScopedConfigurationAnalyzer.AnalyzeConfiguration(config))
                {
                    return config;
                }

                break;
            }

            return DefaultParamConfiguration.Default;
        }

        private static int GetValidConventionEnumValue(int value)
        {
            if (value == 1)
            {
                return value;
            }

            return 0;
        }
    }
}