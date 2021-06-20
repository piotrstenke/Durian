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
		private readonly string _defaultParamAttribute = typeof(DefaultParamAttribute).ToString();
		private readonly string _defaultParamConfigurationAttribute = typeof(DefaultParamConfigurationAttribute).ToString();
		private readonly string _defaultParamScopedConfigurationAttribute = typeof(DefaultParamScopedConfigurationAttribute).ToString();
		private readonly string _dpMethodConvention = typeof(DPMethodConvention).ToString();
		private readonly string _dpTypeConvention = typeof(DPTypeConvention).ToString();

		/// <summary>
		/// <see cref="DefaultParamConfiguration"/> created from the <see cref="DefaultParamScopedConfigurationAttribute"/> defined on the <see cref="CompilationData.Compilation"/>'s main assembly. -or- <see cref="DefaultParamConfiguration.Default"/> if no <see cref="DefaultParamScopedConfigurationAttribute"/> was found.
		/// </summary>
		public DefaultParamConfiguration Configuration { get; }

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> of the <see cref="DefaultParamConfigurationAttribute"/>.
		/// </summary>
		public INamedTypeSymbol? ConfigurationAttribute { get; private set; }

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> of the <see cref="Configuration.DPMethodConvention"/>.
		/// </summary>
		public INamedTypeSymbol? DPMethodConvention { get; private set; }

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> of the <see cref="Configuration.DPTypeConvention"/>.
		/// </summary>
		public INamedTypeSymbol? DPTypeConvention { get; private set; }

		/// <inheritdoc/>
		[MemberNotNullWhen(false, nameof(MainAttribute), nameof(ConfigurationAttribute), nameof(ScopedConfigurationAttribute), nameof(DPMethodConvention), nameof(DPTypeConvention))]
		public override bool HasErrors
		{
			get => base.HasErrors;
			protected set => base.HasErrors = value;
		}

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> of the <see cref="DefaultParamAttribute"/>.
		/// </summary>
		public INamedTypeSymbol? MainAttribute { get; private set; }

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> of the <see cref="DefaultParamScopedConfigurationAttribute"/>.
		/// </summary>
		public INamedTypeSymbol? ScopedConfigurationAttribute { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamCompilationData"/> class.
		/// </summary>
		/// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public DefaultParamCompilationData(CSharpCompilation compilation) : base(compilation)
		{
			Configuration = GetConfiguration(compilation, ScopedConfigurationAttribute);
		}

		/// <summary>
		/// Creates a new instance of <see cref="DefaultParamConfiguration"/> based on the <see cref="DefaultParamScopedConfigurationAttribute"/> defined on the <paramref name="compilation"/>'s main assembly or <see cref="DefaultParamConfiguration.Default"/> if no <see cref="DefaultParamScopedConfigurationAttribute"/> was found.
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

			MainAttribute = Compilation.GetTypeByMetadataName(_defaultParamAttribute);
			ConfigurationAttribute = Compilation.GetTypeByMetadataName(_defaultParamConfigurationAttribute);
			ScopedConfigurationAttribute = Compilation.GetTypeByMetadataName(_defaultParamScopedConfigurationAttribute);
			DPTypeConvention = Compilation.GetTypeByMetadataName(_dpTypeConvention);
			DPMethodConvention = Compilation.GetTypeByMetadataName(_dpMethodConvention);

			HasErrors =
				base.HasErrors ||
				MainAttribute is null ||
				ConfigurationAttribute is null ||
				ScopedConfigurationAttribute is null ||
				DPTypeConvention is null ||
				DPMethodConvention is null;
		}

		private static DefaultParamConfiguration GetConfiguration(CSharpCompilation compilation, INamedTypeSymbol? configurationAttribute)
		{
			if (configurationAttribute is null)
			{
				return DefaultParamConfiguration.Default;
			}

			foreach (AttributeData attribute in compilation.Assembly.GetAttributes())
			{
				if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, configurationAttribute))
				{
					bool applyNewModififer = !attribute.TryGetNamedArgumentValue(nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible), out bool value) || value;
					DPMethodConvention methodCon = (DPMethodConvention)DefaultParamUtilities.GetValidConventionEnumValue(attribute.GetNamedArgumentValue<int>(nameof(DefaultParamScopedConfigurationAttribute.MethodConvention)));
					DPTypeConvention typeCon = (DPTypeConvention)DefaultParamUtilities.GetValidConventionEnumValue(attribute.GetNamedArgumentValue<int>(nameof(DefaultParamScopedConfigurationAttribute.TypeConvention)));
					string? @namespace = attribute.GetNamedArgumentValue<string>(nameof(DefaultParamScopedConfigurationAttribute.TargetNamespace));

					return new()
					{
						ApplyNewModifierWhenPossible = applyNewModififer,
						MethodConvention = methodCon,
						TypeConvention = typeCon,
						TargetNamespace = @namespace
					};
				}
			}

			return DefaultParamConfiguration.Default;
		}
	}
}
