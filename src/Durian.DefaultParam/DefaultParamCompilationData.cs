// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Durian.Configuration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// <see cref="CompilationData"/> that contains all <see cref="ISymbol"/>s needed to generate source code using the <see cref="DefaultParamGenerator"/>.
	/// </summary>
	public sealed class DefaultParamCompilationData : CompilationDataWithSymbols
	{
		/// <summary>
		/// <see cref="INamedTypeSymbol"/> of the <see cref="Durian.DefaultParamAttribute"/>.
		/// </summary>
		public INamedTypeSymbol? DefaultParamAttribute { get; private set; }

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> of the <see cref="Configuration.DefaultParamConfigurationAttribute"/>.
		/// </summary>
		public INamedTypeSymbol? DefaultParamConfigurationAttribute { get; private set; }

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> of the <see cref="Configuration.DefaultParamScopedConfigurationAttribute"/>.
		/// </summary>
		public INamedTypeSymbol? DefaultParamScopedConfigurationAttribute { get; private set; }

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> of the <see cref="Configuration.DPMethodConvention"/>.
		/// </summary>
		public INamedTypeSymbol? DPMethodConvention { get; private set; }

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> of the <see cref="Configuration.DPTypeConvention"/>.
		/// </summary>
		public INamedTypeSymbol? DPTypeConvention { get; private set; }

		/// <summary>
		/// <see cref="DefaultParamConfiguration"/> created from the <see cref="Configuration.DefaultParamScopedConfigurationAttribute"/>
		/// defined on the <see cref="CompilationData.Compilation"/>'s main assembly. -or-
		/// <see cref="DefaultParamConfiguration.Default"/> if no <see cref="Configuration.DefaultParamScopedConfigurationAttribute"/> was found.
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
		/// <see cref="Configuration.DefaultParamScopedConfigurationAttribute"/> defined on the
		/// <paramref name="compilation"/>'s main assembly or <see cref="DefaultParamConfiguration.Default"/>
		/// if no <see cref="Configuration.DefaultParamScopedConfigurationAttribute"/> was found.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the <see cref="DefaultParamConfiguration"/> for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static DefaultParamConfiguration GetConfiguration(CSharpCompilation compilation)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			INamedTypeSymbol? configurationAttribute = compilation.GetTypeByMetadataName(typeof(DefaultParamScopedConfigurationAttribute).ToString());
			return GetConfiguration(compilation, configurationAttribute);
		}

		/// <inheritdoc/>
		public override void Reset()
		{
			base.Reset();

			DefaultParamAttribute = Compilation.GetTypeByMetadataName(typeof(DefaultParamAttribute).ToString());
			DefaultParamConfigurationAttribute = Compilation.GetTypeByMetadataName(typeof(DefaultParamConfigurationAttribute).ToString());
			DefaultParamScopedConfigurationAttribute = Compilation.GetTypeByMetadataName(typeof(DefaultParamScopedConfigurationAttribute).ToString());
			DPTypeConvention = Compilation.GetTypeByMetadataName(typeof(DPTypeConvention).ToString());
			DPMethodConvention = Compilation.GetTypeByMetadataName(typeof(DPMethodConvention).ToString());

			HasErrors =
				base.HasErrors ||
				DefaultParamAttribute is null ||
				DefaultParamConfigurationAttribute is null ||
				DefaultParamScopedConfigurationAttribute is null ||
				DPTypeConvention is null ||
				DPMethodConvention is null;
		}

		private static DefaultParamConfiguration BuildConfiguration(AttributeData attribute)
		{
			bool applyNewModififer = !attribute.TryGetNamedArgumentValue(
				nameof(Configuration.DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible),
				out bool value) ||
				value;

			DPMethodConvention methodCon = (DPMethodConvention)GetValidConventionEnumValue(
				attribute.GetNamedArgumentValue<int>(
					nameof(Configuration.DefaultParamScopedConfigurationAttribute.MethodConvention)));

			DPTypeConvention typeCon = (DPTypeConvention)GetValidConventionEnumValue(
				attribute.GetNamedArgumentValue<int>(nameof(
					Configuration.DefaultParamScopedConfigurationAttribute.TypeConvention)));

			string? @namespace = attribute.GetNamedArgumentValue<string>(
				nameof(Configuration.DefaultParamScopedConfigurationAttribute.TargetNamespace));

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
