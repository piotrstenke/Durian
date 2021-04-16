using Durian.Data;
using Durian.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.DefaultParam
{
	public sealed class DefaultParamCompilationData : CompilationData
	{
		public INamedTypeSymbol Attribute { get; }
		public IMethodSymbol AttributeConstructor { get; }
		public INamedTypeSymbol GeneratedCodeAttribute { get; }
		public INamedTypeSymbol ConfigurationAttribute { get; }
		public override bool HasErrors => Attribute is null || GeneratedCodeAttribute is null || ConfigurationAttribute is null;
		public DefaultParamConfiguration Configuration { get; }

		public DefaultParamCompilationData(CSharpCompilation compilation) : base(compilation)
		{
			INamedTypeSymbol? attr = compilation.Assembly.GetTypeByMetadataName(DefaultParamAttribute.FullyQualifiedName);
			Attribute = attr!;
			AttributeConstructor = attr?.InstanceConstructors[0]!;
			GeneratedCodeAttribute = compilation.GetTypeByMetadataName("System.CodeDom.Compiler.GeneratedCodeAttribute")!;
			ConfigurationAttribute = compilation.GetTypeByMetadataName(DefaultParamConfigurationAttribute.FullyQualifiedName)!;

			Configuration = GetConfiguration(compilation, ConfigurationAttribute);
		}

		public static DefaultParamConfiguration GetConfiguration(CSharpCompilation compilation)
		{
			if (compilation is null)
			{
				return DefaultParamConfiguration.Default;
			}

			INamedTypeSymbol configurationAttribute = compilation.GetTypeByMetadataName(DefaultParamConfigurationAttribute.FullyQualifiedName)!;
			return GetConfiguration(compilation, configurationAttribute);
		}

		private static DefaultParamConfiguration GetConfiguration(CSharpCompilation compilation, INamedTypeSymbol configurationAttribute)
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
						AllowOverridingOfDefaultParamValues = attribute.GetNamedArgumentValue<bool>(DefaultParamConfigurationAttribute.AllowOverridingOfDefaultParamValuesProperty),
						AllowAddingDefaultParamToNewParameters = attribute.GetNamedArgumentValue<bool>(DefaultParamConfigurationAttribute.AllowAddingDefaultParamToNewParametersProperty),
						ApplyNewToGeneratedMembersWithEquivalentSignature = attribute.GetNamedArgumentValue<bool>(DefaultParamConfigurationAttribute.ApplyNewToGeneratedMembersWithEquivalentSignatureProperty)
					};
				}
			}

			return DefaultParamConfiguration.Default;
		}
	}
}
