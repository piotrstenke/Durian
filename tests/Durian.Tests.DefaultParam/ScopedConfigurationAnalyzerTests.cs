using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Durian.Configuration;
using Durian.Generator;
using Durian.Generator.DefaultParam;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Durian.Tests.DefaultParam
{
	public sealed class ScopedConfigurationAnalyzerTests : AnalyzerTest<DefaultParamScopedConfigurationAnalyzer>
	{
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
			Assert.True(diagnostics.Any(d => d.Id == DefaultParamDiagnostics.DUR0125_ScopedConfigurationShouldNotBePlacedOnATypeWithoutDefaultParamMembers.Id));
		}
	}
}