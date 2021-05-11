using System;
using System.Diagnostics.CodeAnalysis;
using Durian.Data;
using Durian.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.DefaultParam
{
	/// <summary>
	/// <see cref="CompilationData"/> that contains all <see cref="ISymbol"/>s needed to generate source code using the <see cref="DefaultParamGenerator"/>.
	/// </summary>
	public sealed class DefaultParamCompilationData : CompilationDataWithSymbols
	{
		/// <summary>
		/// <see cref="INamedTypeSymbol"/> of the generated <see cref="DefaultParamAttribute"/>.
		/// </summary>
		public INamedTypeSymbol? MainAttribute { get; private set; }

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> of the generated <see cref="DefaultParamConfigurationAttribute"/>.
		/// </summary>
		public INamedTypeSymbol? ConfigurationAttribute { get; private set; }

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> of the generated <see cref="DefaultParam.DPMethodGenConvention"/>.
		/// </summary>
		public INamedTypeSymbol? DPMethodGenConvention { get; private set; }

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> of the generated <see cref="DefaultParam.DPTypeGenConvention"/>.
		/// </summary>
		public INamedTypeSymbol? DPTypeGenConvention { get; private set; }

		/// <inheritdoc/>
		[MemberNotNullWhen(false, nameof(MainAttribute), nameof(ConfigurationAttribute), nameof(DPMethodGenConvention), nameof(DPTypeGenConvention))]
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

			MainAttribute = Compilation.Assembly.GetTypeByMetadataName(DefaultParamAttribute.FullyQualifiedName);
			ConfigurationAttribute = Compilation.Assembly.GetTypeByMetadataName(DefaultParamConfigurationAttribute.FullyQualifiedName);
			DPTypeGenConvention = Compilation.Assembly.GetTypeByMetadataName(DefaultParam.DPTypeGenConvention.FullName);
			DPMethodGenConvention = Compilation.Assembly.GetTypeByMetadataName(DefaultParam.DPMethodGenConvention.FullName);

			HasErrors = base.HasErrors || MainAttribute is null || ConfigurationAttribute is null || DPTypeGenConvention is null || DPMethodGenConvention is null;
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

			INamedTypeSymbol configurationAttribute = compilation.GetTypeByMetadataName(DefaultParamConfigurationAttribute.FullyQualifiedName)!;
			return GetConfiguration(compilation, configurationAttribute);
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
					return new()
					{
						ApplyNewModifierWhenPossible = attribute.GetNamedArgumentValue<bool>(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossibleProperty),
						MethodConvention = attribute.GetNamedArgumentValue<int>(DefaultParamConfigurationAttribute.MethodConvetionProperty),
						TypeConvention = attribute.GetNamedArgumentValue<int>(DefaultParamConfigurationAttribute.TypeConventionProperty)
					};
				}
			}

			return DefaultParamConfiguration.Default;
		}
	}
}
