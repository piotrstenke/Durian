using System.Threading.Tasks;
using Durian.Analysis;
using Durian.Analysis.Logging;
using Durian.Info;
using Durian.TestServices;
using Xunit;
using static Durian.Analysis.DurianDiagnostics;

namespace Durian.Tests;

public sealed class EnableLoggingAttributeTests : AnalyzerTest<EnableLoggingAttributeAnalyzer>
{
	[Fact]
	public async Task Success_When_HasSingleAttribute()
	{
		string input =
$@"using {DurianStrings.MainNamespace}.Analysis.Logging;
using {DurianStrings.InfoNamespace};

[assembly: {nameof(EnableLoggingAttribute)}({nameof(DurianModule)}.{nameof(DurianModule.Core)})]
";
		Assert.Empty(await RunAnalyzer(input));
	}

	[Fact]
	public async Task Success_When_HasMultipleAttributes()
	{
		string input =
$@"using {DurianStrings.MainNamespace}.Analysis.Logging;
using {DurianStrings.InfoNamespace};

[assembly: {nameof(EnableLoggingAttribute)}({nameof(DurianModule)}.{nameof(DurianModule.Core)})]
[assembly: {nameof(EnableLoggingAttribute)}({nameof(DurianModule)}.{nameof(DurianModule.DefaultParam)})]
";
		Assert.Empty(await RunAnalyzer(input));
	}

	[Fact]
	public async Task Error_When_HasSameDurianModule()
	{
		string input =
$@"using {DurianStrings.MainNamespace}.Analysis.Logging;
using {DurianStrings.InfoNamespace};

[assembly: {nameof(EnableLoggingAttribute)}({nameof(DurianModule)}.{nameof(DurianModule.Core)})]
[assembly: {nameof(EnableLoggingAttribute)}({nameof(DurianModule)}.{nameof(DurianModule.Core)})]
";
		Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0010_DuplicateEnableLogging.Id);
	}

	[Fact]
	public async Task Error_When_HasSameDurianModuleWithDifferentProperties()
	{
		string input =
$@"using {DurianStrings.MainNamespace}.Analysis.Logging;
using {DurianStrings.InfoNamespace};

[assembly: {nameof(EnableLoggingAttribute)}({nameof(DurianModule)}.{nameof(DurianModule.Core)})]
[assembly: {nameof(EnableLoggingAttribute)}({nameof(DurianModule)}.{nameof(DurianModule.Core)}, {nameof(EnableLoggingAttribute.EnableExceptions)} = true)]
";
		Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0010_DuplicateEnableLogging.Id);
	}

	[Fact]
	public async Task Error_When_HasSameModuleName()
	{
		string input =
$@"using {DurianStrings.MainNamespace}.Analysis.Logging;

[assembly: {nameof(EnableLoggingAttribute)}(""core"")]
[assembly: {nameof(EnableLoggingAttribute)}(""core"")]
";
		Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0010_DuplicateEnableLogging.Id);
	}

	[Fact]
	public async Task Error_When_HasSameModuleNameWithDifferentProperties()
	{
		string input =
$@"using {DurianStrings.MainNamespace}.Analysis.Logging;

[assembly: {nameof(EnableLoggingAttribute)}(""core"")]
[assembly: {nameof(EnableLoggingAttribute)}(""core"", {nameof(EnableLoggingAttribute.EnableExceptions)} = true)]
";
		Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0010_DuplicateEnableLogging.Id);
	}

	[Fact]
	public async Task Error_When_ResolvesToSameName()
	{
		string input =
$@"using {DurianStrings.MainNamespace}.Analysis.Logging;
using {DurianStrings.InfoNamespace};

[assembly: {nameof(EnableLoggingAttribute)}({nameof(DurianModule)}.{nameof(DurianModule.Core)})]
[assembly: {nameof(EnableLoggingAttribute)}(""Core"")]
";
		Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0010_DuplicateEnableLogging.Id);
	}

	[Fact]
	public async Task Error_When_ResolvesToSameNameWithDifferentProperties()
	{
		string input =
$@"using {DurianStrings.MainNamespace}.Analysis.Logging;
using {DurianStrings.InfoNamespace};

[assembly: {nameof(EnableLoggingAttribute)}({nameof(DurianModule)}.{nameof(DurianModule.Core)})]
[assembly: {nameof(EnableLoggingAttribute)}(""Core"", {nameof(EnableLoggingAttribute.EnableExceptions)} = true)]
";
		Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0010_DuplicateEnableLogging.Id);
	}
}
