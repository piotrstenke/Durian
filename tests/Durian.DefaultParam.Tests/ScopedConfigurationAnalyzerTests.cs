using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Xunit;
using static Durian.Analysis.DefaultParam.DefaultParamDiagnostics;

namespace Durian.Analysis.DefaultParam.Tests
{
	public sealed class ScopedConfigurationAnalyzerTests : AnalyzerTest<DefaultParamScopedConfigurationAnalyzer>
	{
		[Fact]
		public async Task NoWarning_When_HasScopedConfiguration_And_HasDefaultParamMembers()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamScopedConfigurationAttributeProvider.TypeName}]
public partial class Test
{{
	private class Child<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task NoWarning_When_TargetNamespaceIsNamedLikeKeyword_And_HasAtSign()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TargetNamespace} = ""@int"")]
public partial class Test
{{
	private class Child<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task NoWarning_When_TargetNamespaceIsNull()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TargetNamespace} = null)]
public partial class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
{{
	private class Child<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task NoWarning_When_TargetNamespaceIsValid()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TargetNamespace} = ""Durian"")]
public partial class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
{{
	private class Child<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task NoWarning_When_TargetNamespaceIsValid_And_HasDot()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TargetNamespace} = ""Durian.Core"")]
public partial class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
{{
	private class Child<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Warning_When_HasScopedConfiguration_And_HasNoDefaultParamMembers()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamScopedConfigurationAttributeProvider.TypeName}]
public class Test
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzer(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0125_ScopedConfigurationShouldNotBePlacedOnATypeWithoutDefaultParamMembers.Id));
		}

		[Fact]
		public async Task Warning_When_TargetNamespaceIsDurianGenerator()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TargetNamespace} = ""Durian.Generator"")]
public partial class Test
{{
	private class Child<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
	{{
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzer(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0127_InvalidTargetNamespace.Id));
		}

		[Fact]
		public async Task Warning_When_TargetNamespaceIsNamedLikeKeyword()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TargetNamespace} = ""int"")]
public partial class Test
{{
	private class Child<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
	{{
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzer(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0127_InvalidTargetNamespace.Id));
		}

		[Fact]
		public async Task Warning_When_TargetNamespaceIsNotValidIdentifier()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TargetNamespace} = ""15412"")]
public partial class Test
{{
	private class Child<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
	{{
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzer(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0127_InvalidTargetNamespace.Id));
		}

		[Fact]
		public async Task Warning_When_TargetNamespaceIsWhitespaceOrEmpty()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TargetNamespace} = ""   "")]
public partial class Test
{{
	private class Child<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
	{{
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzer(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0127_InvalidTargetNamespace.Id));
		}

		protected override IEnumerable<ISourceTextProvider>? GetInitialSources()
		{
			return DefaultParamGenerator.GetSourceProviders();
		}
	}
}
