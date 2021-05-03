using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Durian.Data;
using Durian.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.DefaultParam
{
	public sealed class DefaultParamCompilationData : CompilationDataWithSymbols
	{
		public INamedTypeSymbol? Attribute { get; private set; }
		public IMethodSymbol? AttributeConstructor { get; private set; }
		public INamedTypeSymbol? ConfigurationAttribute { get; private set; }
		public INamedTypeSymbol? MethodConfigurationAttribute { get; private set; }
		public IMethodSymbol? MethodConfigurationConstructor { get; private set; }

		[MemberNotNullWhen(false, nameof(Attribute), nameof(AttributeConstructor), nameof(ConfigurationAttribute), nameof(MethodConfigurationAttribute), nameof(MethodConfigurationConstructor))]
		public override bool HasErrors { get; protected set; }

		public DefaultParamConfiguration Configuration { get; }

		public DefaultParamCompilationData(CSharpCompilation compilation) : base(compilation)
		{
			Configuration = GetConfiguration(compilation, ConfigurationAttribute);
		}

		public override void Reset()
		{
			base.Reset();

			INamedTypeSymbol? attr = Compilation.Assembly.GetTypeByMetadataName(DefaultParamAttribute.FullyQualifiedName);
			Attribute = attr;
			AttributeConstructor = attr?.InstanceConstructors[0]!;
			ConfigurationAttribute = Compilation.Assembly.GetTypeByMetadataName(DefaultParamConfigurationAttribute.FullyQualifiedName);
			MethodConfigurationAttribute = Compilation.Assembly.GetTypeByMetadataName(DefaultParamMethodConfigurationAttribute.FullyQualifiedName);
			MethodConfigurationConstructor = MethodConfigurationAttribute?.InstanceConstructors.FirstOrDefault(ctor => ctor.Parameters.Length == 0);

			HasErrors = base.HasErrors || Attribute is null || AttributeConstructor is null || ConfigurationAttribute is null || MethodConfigurationAttribute is null || MethodConfigurationConstructor is null;
		}

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
