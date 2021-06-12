// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Immutable;
using System.Threading.Tasks;
using Durian.Generator.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using Xunit;

namespace Durian.Tests.Core
{
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
		public async Task Error_WhenIsLowerThanCSharp9()
		{
			SyntaxTree tree = CSharpSyntaxTree.ParseText("", options: new CSharpParseOptions(Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp8));

			CSharpCompilation compilation = CSharpCompilation.Create(RoslynUtilities.DefaultCompilationName, new SyntaxTree[] { tree }, RoslynUtilities.GetBaseReferences());

			IsCSharpCompilationAnalyzer analyzer = new();
			Assert.True(await analyzer.ProducesDiagnostic(compilation, DurianDiagnostics.DUR0006_ProjectMustUseCSharp9));
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
}
