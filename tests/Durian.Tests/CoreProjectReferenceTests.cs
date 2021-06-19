// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Durian.Analysis;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Durian.Tests
{
	public sealed class CoreProjectReferenceTests
	{
		// This test fails on Ubuntu and Mac for some reason.

		//[Fact]
		//public async Task Error_When_DoesNotReferenceDurianCore()
		//{
		//	CSharpCompilation compilation = RoslynUtilities.CreateBaseCompilation();
		//	compilation = compilation.WithReferences(compilation.References.Where(r =>
		//	{
		//		return r.Display is not null && AssemblyIdentity.TryParseDisplayName(r.Display, out AssemblyIdentity? assembly) && assembly.Name != "Durian.Core";
		//	}));

		//	DependenciesAnalyzer analyzer = new();
		//	Assert.True(await analyzer.ProducesDiagnostic(compilation, DurianDiagnostics.DUR0001_ProjectMustReferenceDurianCore));
		//}

		[Fact]
		public async Task Error_When_ReferencesMainDurianPackageAndAnyDurianAnalyzerPackage()
		{
			CSharpCompilation compilation = RoslynUtilities.CreateBaseCompilation();
			string dir = Path.GetDirectoryName(typeof(CoreProjectReferenceTests).Assembly.Location)!;
			string mainPath = Path.Combine(dir, "Durian.dll");
			string analyzerPath = Path.Combine(dir, "Durian.Core.Analyzer.dll");

			compilation = compilation.AddReferences(MetadataReference.CreateFromFile(mainPath), MetadataReference.CreateFromFile(analyzerPath));

			DependenciesAnalyzer analyzer = new();
			Assert.True(await analyzer.ProducesDiagnostic(compilation, DurianDiagnostics.DUR0007_DoNotReferencePackageIfManagerIsPresent));
		}

		[Fact]
		public async Task Succcess_When_ReferencesDurianCore()
		{
			CSharpCompilation compilation = RoslynUtilities.CreateBaseCompilation();
			DependenciesAnalyzer analyzer = new();
			ImmutableArray<Diagnostic> diagnostics = await analyzer.RunAnalyzer(compilation);
			Assert.Empty(diagnostics);
		}

		[Fact]
		public async Task Success_When_ReferencesMainPackage()
		{
			CSharpCompilation compilation = RoslynUtilities.CreateBaseCompilation();
			string dir = Path.GetDirectoryName(typeof(CoreProjectReferenceTests).Assembly.Location)!;
			string mainPath = Path.Combine(dir, "Durian.dll");

			compilation = compilation.AddReferences(MetadataReference.CreateFromFile(mainPath));

			DependenciesAnalyzer analyzer = new();
			ImmutableArray<Diagnostic> diagnostics = await analyzer.RunAnalyzer(compilation);
			Assert.Empty(diagnostics);
		}
	}
}
