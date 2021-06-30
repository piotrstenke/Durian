// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Durian.Configuration;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Xunit;
using static Durian.Analysis.GenericSpecialization.GenSpecDiagnostics;

namespace Durian.Analysis.GenericSpecialization.Tests
{
	public sealed class AssemblyConfigurationAnalyzerTests : AnalyzerTest<GenericSpecializationConfigurationAnalyzer>
	{
		private const string _configAttr = nameof(GenericSpecializationConfigurationAttribute);
		private const string _interfaceName = nameof(GenericSpecializationConfigurationAttribute.InterfaceName);
		private const string _templateName = nameof(GenericSpecializationConfigurationAttribute.TemplateName);

		[Fact]
		public async Task Error_When_InterfaceNameIsEmptyOrWhiteSpaceOnly()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[assembly: {_configAttr}({_templateName} = "" "")]
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0203_SpecifiedNameIsNotAValidIdentifier.Id));
		}

		[Fact]
		public async Task Error_When_InterfaceNameIsInvalidIdentifier()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[assembly: {_configAttr}({_templateName} = ""1_abc"")]
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0203_SpecifiedNameIsNotAValidIdentifier.Id));
		}

		[Fact]
		public async Task Error_When_InterfaceNameIsNamedLikeKeyword()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[assembly: {_configAttr}({_interfaceName} = ""int"")]
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0203_SpecifiedNameIsNotAValidIdentifier.Id));
		}

		[Fact]
		public async Task Error_When_TemplateNameIsEmptyOrWhiteSpaceOnly()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[assembly: {_configAttr}({_templateName} = "" "")]
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0203_SpecifiedNameIsNotAValidIdentifier.Id));
		}

		[Fact]
		public async Task Error_When_TemplateNameIsInvalidIdentifier()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[assembly: {_configAttr}({_templateName} = ""1_abc"")]
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0203_SpecifiedNameIsNotAValidIdentifier.Id));
		}

		[Fact]
		public async Task Error_When_TemplateNameIsNamedLikeKeyword()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[assembly: {_configAttr}({_templateName} = ""int"")]
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0203_SpecifiedNameIsNotAValidIdentifier.Id));
		}

		[Fact]
		public async Task NoError_When_InterfaceNameIsNamedLikeKeywordAndHasAtSign()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[assembly: {_configAttr}({_interfaceName} = ""@int"")]
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task NoError_When_InterfaceNameIsNull()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[assembly: {_configAttr}({_interfaceName} = null)]
";

			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task NoError_When_TemplateNameIsNamedLikeKeywordAndHasAtSign()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[assembly: {_configAttr}({_templateName} = ""@int"")]
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task NoError_When_TemplateNameIsNull()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[assembly: {_configAttr}({_templateName} = null)]
";

			Assert.Empty(await RunAnalyzerAsync(input));
		}
	}
}
