using System.Linq;
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
		public INamedTypeSymbol MethodConfigurationAttribute { get; }
		public IMethodSymbol MethodConfigurationConstructor { get; }
		public override bool HasErrors => Attribute is null || GeneratedCodeAttribute is null || ConfigurationAttribute is null || MethodConfigurationAttribute is null;
		public DefaultParamConfiguration Configuration { get; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public DefaultParamCompilationData(CSharpCompilation compilation) : base(compilation)
		{
			INamedTypeSymbol? attr = compilation.Assembly.GetTypeByMetadataName(DefaultParamAttribute.FullyQualifiedName);
			Attribute = attr!;
			AttributeConstructor = attr?.InstanceConstructors[0]!;
			GeneratedCodeAttribute = compilation.GetTypeByMetadataName("System.CodeDom.Compiler.GeneratedCodeAttribute")!;
			ConfigurationAttribute = compilation.GetTypeByMetadataName(DefaultParamConfigurationAttribute.FullyQualifiedName)!;
			MethodConfigurationAttribute = compilation.GetTypeByMetadataName(DefaultParamMethodConfigurationAttribute.FullyQualifiedName)!;
			MethodConfigurationConstructor = MethodConfigurationAttribute?.InstanceConstructors.FirstOrDefault(ctor => ctor.Parameters.Length == 0)!;

			Configuration = GetConfiguration(compilation, ConfigurationAttribute);
		}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

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
						ApplyNewToGeneratedMembersWithEquivalentSignature = attribute.GetNamedArgumentValue<bool>(DefaultParamConfigurationAttribute.ApplyNewToGeneratedMembersWithEquivalentSignatureProperty),
						CallInsteadOfCopying = attribute.GetNamedArgumentValue<bool>(DefaultParamConfigurationAttribute.CallInsteadOfCopyingProperty)
					};
				}
			}

			return DefaultParamConfiguration.Default;
		}
	}
}
