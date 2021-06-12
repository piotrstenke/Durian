// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Durian.Configuration;
using Durian.Generator;
using Durian.Generator.DefaultParam;
using Microsoft.CodeAnalysis;
using Xunit;
using static Durian.Generator.DefaultParam.DefaultParamDiagnostics;

namespace Durian.Tests.DefaultParam
{
	public sealed class ScopedConfigurationAnalyzerTests : AnalyzerTest<DefaultParamScopedConfigurationAnalyzer>
	{
		[Fact]
		public async Task NoWarning_When_HasScopedConfiguration_And_HasDefaultParamMembers()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(DefaultParamScopedConfigurationAttribute)}]
public partial class Test
{{
	private class Child<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task NoWarning_When_TargetNamespaceIsNamedLikeKeyword_And_HasAtSign()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TargetNamespace)} = ""@int"")]
public partial class Test
{{
	private class Child<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task NoWarning_When_TargetNamespaceIsNull()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TargetNamespace)} = null)]
public partial class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
{{
	private class Child<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task NoWarning_When_TargetNamespaceIsValid()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TargetNamespace)} = ""Durian"")]
public partial class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
{{
	private class Child<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task NoWarning_When_TargetNamespaceIsValid_And_HasDot()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TargetNamespace)} = ""Durian.Core"")]
public partial class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
{{
	private class Child<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Warning_When_HasScopedConfiguration_And_HasNoDefaultParamMembers()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(DefaultParamScopedConfigurationAttribute)}]
public class Test
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0125_ScopedConfigurationShouldNotBePlacedOnATypeWithoutDefaultParamMembers.Id));
		}

		[Fact]
		public async Task Warning_When_TargetNamespaceIsDurianGenerator()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TargetNamespace)} = ""Durian.Generator"")]
public partial class Test
{{
	private class Child<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0127_InvalidTargetNamespace.Id));
		}

		[Fact]
		public async Task Warning_When_TargetNamespaceIsNamedLikeKeyword()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TargetNamespace)} = ""int"")]
public partial class Test
{{
	private class Child<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0127_InvalidTargetNamespace.Id));
		}

		[Fact]
		public async Task Warning_When_TargetNamespaceIsNotValidIdentifier()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TargetNamespace)} = ""15412"")]
public partial class Test
{{
	private class Child<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0127_InvalidTargetNamespace.Id));
		}

		[Fact]
		public async Task Warning_When_TargetNamespaceIsWhitespaceOrEmpty()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TargetNamespace)} = ""   "")]
public partial class Test
{{
	private class Child<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0127_InvalidTargetNamespace.Id));
		}
	}
}
