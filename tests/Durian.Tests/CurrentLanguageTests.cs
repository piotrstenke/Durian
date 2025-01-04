using System.Collections.Immutable;
using System.Threading.Tasks;
using Durian.Analysis;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using Xunit;

namespace Durian.Tests;

public sealed class CurrentLanguageTests
{
	[Fact]
	public async Task Error_When_IsVisualBasic()
	{
		VisualBasicCompilation compilation = VisualBasicCompilation.Create(RoslynUtilities.DefaultCompilationName, null, RoslynUtilities.GetBaseReferences());
		IsCSharpCompilationAnalyzer analyzer = new();
		Assert.True(await analyzer.ProducesDiagnostic(compilation, DurianDiagnostics.DUR0004_DurianModulesAreValidOnlyInCSharp));
	}

	[Fact]
	public async Task Success_When_IsCSharp()
	{
		CSharpCompilation compilation = CSharpCompilation.Create(RoslynUtilities.DefaultCompilationName, null, RoslynUtilities.GetBaseReferences());
		IsCSharpCompilationAnalyzer analyzer = new();
		ImmutableArray<Diagnostic> diagnostics = await analyzer.RunAnalyzer(compilation);
		Assert.Empty(diagnostics);
	}
}
