using System;
using System.Diagnostics.CodeAnalysis;
using Durian.Generator.Data;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Durian.Configuration;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// <see cref="CompilationData"/> that contains all <see cref="ISymbol"/>s needed to generate source code using the <see cref="DefaultParamGenerator"/>.
	/// </summary>
	public sealed class DefaultParamCompilationData : CompilationDataWithSymbols
	{
		private readonly string _dpMethodConvention = typeof(DPMethodConvention).ToString();
		private readonly string _dpTypeConvention = typeof(DPTypeConvention).ToString();
		private readonly string _defaultParamAttribute = typeof(DefaultParamAttribute).ToString();
		private readonly string _defaultParamConfigurationAttribute = typeof(DefaultParamConfigurationAttribute).ToString();

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> of the <see cref="DefaultParamAttribute"/>.
		/// </summary>
		public INamedTypeSymbol? MainAttribute { get; private set; }

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
		[MemberNotNullWhen(false, nameof(MainAttribute), nameof(ConfigurationAttribute), nameof(DPMethodConvention), nameof(DPTypeConvention))]
		public override bool HasErrors { get; protected set; }

		/// <summary>
		/// <see cref="DefaultParamConfiguration"/> created from the <see cref="DefaultParamConfigurationAttribute"/> defined on the <see cref="CompilationData.Compilation"/>'s main assembly.
		/// </summary>
		public DefaultParamConfiguration Configuration { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamCompilationData"/> class.
		/// </summary>
		/// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public DefaultParamCompilationData(CSharpCompilation compilation) : base(compilation)
		{
			Configuration = GetConfiguration(compilation, ConfigurationAttribute);
		}

		/// <inheritdoc/>
		public override void Reset()
		{
			base.Reset();

			MainAttribute = Compilation.GetTypeByMetadataName(_defaultParamAttribute);
			ConfigurationAttribute = Compilation.GetTypeByMetadataName(_defaultParamConfigurationAttribute);
			DPTypeConvention = Compilation.GetTypeByMetadataName(_dpTypeConvention);
			DPMethodConvention = Compilation.GetTypeByMetadataName(_dpMethodConvention);

			HasErrors = base.HasErrors || MainAttribute is null || ConfigurationAttribute is null || DPTypeConvention is null || DPMethodConvention is null;
		}

		/// <summary>
		/// Creates a new instance of <see cref="DefaultParamConfiguration"/> based on the <see cref="DefaultParamConfigurationAttribute"/> defined on the specified <paramref name="compilation"/>'s main assembly.
		/// </summary>
		/// <param name="compilation"></param>
		/// <returns>A new instance of <see cref="DefaultParamConfiguration"/> based on the <see cref="DefaultParamConfigurationAttribute"/> defined on the specified <paramref name="compilation"/>'s main assembly. -or- <see cref="DefaultParamConfiguration.Default"/> if <paramref name="compilation"/> is <see langword="null"/> or the <see cref="DefaultParamConfigurationAttribute"/> was not found in the <paramref name="compilation"/>.</returns>
		public static DefaultParamConfiguration GetConfiguration(CSharpCompilation? compilation)
		{
			if (compilation is null)
			{
				return DefaultParamConfiguration.Default;
			}

			INamedTypeSymbol configurationAttribute = compilation.GetTypeByMetadataName(typeof(DefaultParamConfigurationAttribute).ToString())!;
			return GetConfiguration(compilation, configurationAttribute);
		}

		private static DefaultParamConfiguration GetConfiguration(CSharpCompilation compilation, INamedTypeSymbol? configurationAttribute)
		{
			if (configurationAttribute is null)
			{
				return DefaultParamConfiguration.Default;
			}

			const string applyNew = nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible);
			const string methodConvention = nameof(DefaultParamConfigurationAttribute.MethodConvention);
			const string typeConvention = nameof(DefaultParamConfigurationAttribute.TypeConvention);

			foreach (AttributeData attribute in compilation.Assembly.GetAttributes())
			{
				if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, configurationAttribute))
				{
					return new()
					{
						ApplyNewModifierWhenPossible = attribute.GetNamedArgumentValue<bool>(applyNew),
						MethodConvention = (DPMethodConvention)attribute.GetNamedArgumentValue<int>(methodConvention),
						TypeConvention = (DPTypeConvention)attribute.GetNamedArgumentValue<int>(typeConvention)
					};
				}
			}

			return DefaultParamConfiguration.Default;
		}
	}
}
