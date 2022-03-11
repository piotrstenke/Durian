// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Xunit;
using Durian.TestServices;

using static Durian.Analysis.CopyFrom.CopyFromDiagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Durian.Analysis.CopyFrom.Tests
{
	public sealed class PatternTests : AnalyzerTest<CopyFromAnalyzer>
	{
		[Fact]
		public async Task Warning_When_HasPattern_And_PatternIsNull()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target""), {PatternAttributeProvider.TypeName}(null, """")]
	partial void Method();

	void Target()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0214_InvalidPatternAttributeSpecified.Id);
		}

		[Fact]
		public async Task Warning_When_HasPattern_And_ReplacementIsNull()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target""), {PatternAttributeProvider.TypeName}(""\w+"", null)]
	partial void Method();

	void Target()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0214_InvalidPatternAttributeSpecified.Id);
		}

		[Fact]
		public async Task Warning_When_PatternIsRedundant()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{PatternAttributeProvider.TypeName}(""\w+"", """")]
	partial void Method();
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0215_RedundantPatternAttribute.Id);
		}

		[Fact]
		public async Task Warning_When_SamePatternAlreadySpecified()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	[{PatternAttributeProvider.TypeName}(""\w+"", """")]
	[{PatternAttributeProvider.TypeName}(""\w+"", ""xyz"")]
	partial void Method();
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0216_EquivalentPatternAttribute.Id);
		}

		protected override IEnumerable<ISourceTextProvider>? GetInitialSources()
		{
			return CopyFromGenerator.GetSourceProviders();
		}
	}
}
