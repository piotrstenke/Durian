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
	public sealed class TypeConfigurationAnalyzerTests : AnalyzerTest<GenericSpecializationConfigurationAnalyzer>
	{
		private const string _allowAttr = nameof(AllowSpecializationAttribute);
		private const string _configAttr = nameof(GenericSpecializationConfigurationAttribute);
		private const string _interfaceName = nameof(GenericSpecializationConfigurationAttribute.InterfaceName);
		private const string _templateName = nameof(GenericSpecializationConfigurationAttribute.TemplateName);

		[Fact]
		public async Task Error_When_InterfaceNameIsEmptyOrWhiteSpaceOnly()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[{_configAttr}({_interfaceName} = """")]
public class Test
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0203_SpecifiedNameIsNotAValidIdentifier.Id));
		}

		[Fact]
		public async Task Error_When_InterfaceNameIsInvalidIdentifier()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[{_configAttr}({_interfaceName} = ""1_abc"")]
public class Test
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0203_SpecifiedNameIsNotAValidIdentifier.Id));
		}

		[Fact]
		public async Task Error_When_InterfaceNameIsNamedLikeKeyword()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[{_configAttr}({_interfaceName} = ""int"")]
public class Test
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0203_SpecifiedNameIsNotAValidIdentifier.Id));
		}

		[Fact]
		public async Task Error_When_InterfaceNameIsNamedLikeTargetClass_And_HasAllowSpecializationAttribute()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{_allowAttr}]
[{_configAttr}({_interfaceName} = ""Test"")]
public class Test<T>
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0222_TargetNameCannotBeTheSameAsContainingClass.Id));
		}

		[Fact]
		public async Task Error_When_TemplateNameIsEmptyOrWhiteSpaceOnly()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[{_configAttr}({_templateName} = """")]
public class Test
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0203_SpecifiedNameIsNotAValidIdentifier.Id));
		}

		[Fact]
		public async Task Error_When_TemplateNameIsInvalidIdentifier()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[{_configAttr}({_templateName} = ""1_abc"")]
public class Test
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0203_SpecifiedNameIsNotAValidIdentifier.Id));
		}

		[Fact]
		public async Task Error_When_TemplateNameIsNamedLikeKeyword()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[{_configAttr}({_templateName} = ""int"")]
public class Test
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0203_SpecifiedNameIsNotAValidIdentifier.Id));
		}

		[Fact]
		public async Task Error_When_TemplateNameIsNamedLikeTargetClass_And_HasAllowSpecializationAttribute()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{_allowAttr}]
[{_configAttr}({_templateName} = ""Test"")]
public class Test<T>
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0222_TargetNameCannotBeTheSameAsContainingClass.Id));
		}

		[Fact]
		public async Task NoError_When_InterfaceNameIsNamedLikeKeywordAndHasAtSign()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[{_configAttr}({_interfaceName} = ""@int"")]
public class Test
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.False(diagnostics.Any(d => d.Id == DUR0203_SpecifiedNameIsNotAValidIdentifier.Id));
		}

		[Fact]
		public async Task NoError_When_InterfaceNameIsNamedLikeTargetClass()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[{_configAttr}({_interfaceName} = ""Test"")]
public class Test
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.False(diagnostics.Any(d => d.Id == DUR0203_SpecifiedNameIsNotAValidIdentifier.Id));
		}

		[Fact]
		public async Task NoError_When_InterfaceNameIsNamedLikeTargetClass_And_IsGeneric()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[{_configAttr}({_interfaceName} = ""Test"")]
public class Test<T>
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.False(diagnostics.Any(d => d.Id == DUR0203_SpecifiedNameIsNotAValidIdentifier.Id));
		}

		[Fact]
		public async Task NoError_When_InterfaceNameIsNull()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[{_configAttr}({_interfaceName} = null)]
public class Test
{{
}}
";

			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.False(diagnostics.Any(d => d.Id == DUR0203_SpecifiedNameIsNotAValidIdentifier.Id));
		}

		[Fact]
		public async Task NoError_When_TemplateNameIsNamedLikeKeywordAndHasAtSign()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[{_configAttr}({_templateName} = ""@int"")]
public class Test
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.False(diagnostics.Any(d => d.Id == DUR0203_SpecifiedNameIsNotAValidIdentifier.Id));
		}

		[Fact]
		public async Task NoError_When_TemplateNameIsNamedLikeTargetClass()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[{_configAttr}({_templateName} = ""Test"")]
public class Test
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.False(diagnostics.Any(d => d.Id == DUR0203_SpecifiedNameIsNotAValidIdentifier.Id));
		}

		[Fact]
		public async Task NoError_When_TemplateNameIsNamedLikeTargetClass_And_IsGeneric()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[{_configAttr}({_templateName} = ""Test"")]
public class Test<T>
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.False(diagnostics.Any(d => d.Id == DUR0203_SpecifiedNameIsNotAValidIdentifier.Id));
		}

		[Fact]
		public async Task NoError_When_TemplateNameIsNull()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[{_configAttr}({_templateName} = null)]
public class Test
{{
}}
";

			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.False(diagnostics.Any(d => d.Id == DUR0203_SpecifiedNameIsNotAValidIdentifier.Id));
		}

		[Fact]
		public async Task NoWarning_When_HasSpecializedMembers()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{_configAttr}]
public class Test
{{
	[{_allowAttr}]
	public class Child<T>
	{{
	}}
}}
";

			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Warning_When_HasNoMembersWithAllowSpecializationAttribute()
		{
			string input =
$@"using {DurianStrings.ConfigurationNamespace};

[{_configAttr}]
public class Test
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0204_DoNotSpecifyConfigurationAttributeOnMemberWithNoSpecializations.Id));
		}
	}
}
