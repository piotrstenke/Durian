using Xunit;
using Durian.Generator;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.VisualBasic;

namespace Durian.Tests.Core
{
	public sealed class CurrentLanguageTests
	{
		[Fact]
		public async Task Success_When_IsCSharp()
		{
			CSharpCompilation compilation = CSharpCompilation.Create(RoslynUtilities.DefaultCompilationName, null, RoslynUtilities.GetBaseReferences());
			IsCSharpCompilationAnalyzer analyzer = new();
			ImmutableArray<Diagnostic> diagnostics = await analyzer.RunAnalyzer(compilation);
			Assert.Empty(diagnostics);
		}

		[Fact]
		public async Task Error_When_IsVisualBasic()
		{
			VisualBasicCompilation compilation = VisualBasicCompilation.Create(RoslynUtilities.DefaultCompilationName, null, RoslynUtilities.GetBaseReferences());
			IsCSharpCompilationAnalyzer analyzer = new();
			Assert.True(await analyzer.ProducesDiagnostic(compilation, DurianDiagnostics.DUR0004_DurianModulesAreValidOnlyInCSharp));
		}
	}
}
