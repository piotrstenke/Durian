using System.Threading.Tasks;
using Durian.Analysis;
using Durian.TestServices;
using Xunit;
using static Durian.Analysis.DurianDiagnostics;

namespace Durian.Tests
{
	public sealed class PartialNameTests : AnalyzerTest<PartialNameAnalyzer>
	{
		[Fact]
		public async Task Success_When_HasMultiplePartialNames()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{nameof(PartialNameAttribute)}(""value"")]
[{nameof(PartialNameAttribute)}(""other"")]
partial class Test
{{
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_HasMultiplePartialParts()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{nameof(PartialNameAttribute)}(""value"")]
partial class Test
{{
}}

[{nameof(PartialNameAttribute)}(""other"")]
partial class Test
{{
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_HasOnePartialPart()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{nameof(PartialNameAttribute)}(""value"")]
partial class Test
{{
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Warning_When_HasDuplicateNames()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{nameof(PartialNameAttribute)}(""value"")]
[{nameof(PartialNameAttribute)}(""value"")]
partial class Test
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0009_DuplicatePartialPart.Id);
		}

		[Fact]
		public async Task Warning_When_HasDuplicateNamesOnDifferentParts()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{nameof(PartialNameAttribute)}(""value"")]
partial class Test
{{
}}

[{nameof(PartialNameAttribute)}(""value"")]
partial class Test
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0009_DuplicatePartialPart.Id);
		}

		[Fact]
		public async Task Warning_When_IsNotPartial()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{nameof(PartialNameAttribute)}(""value"")]
class Test
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0006_PartialNameAttributeNotOnPartial.Id);
		}
	}
}
