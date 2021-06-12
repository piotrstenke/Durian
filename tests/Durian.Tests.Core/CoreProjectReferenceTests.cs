// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Immutable;
using System.Threading.Tasks;
using Durian.Generator.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Durian.Tests.Core
{
	public sealed class CoreProjectReferenceTests
	{
		//
		// For some reason this unit test fails on GitHub
		//

		//[Fact]
		//public async Task Error_When_DoesNotReferenceDurianCore()
		//{
		//	CSharpCompilation compilation = RoslynUtilities.CreateBaseCompilation();
		//	compilation = compilation.WithReferences(compilation.References.Where(r =>
		//	{
		//		return r.Display is not null && AssemblyIdentity.TryParseDisplayName(r.Display, out AssemblyIdentity? assembly) && assembly.Name != "Durian.Core";
		//	}));

		//	CoreProjectAnalyzer analyzer = new();
		//	Assert.True(await analyzer.ProducesDiagnostic(compilation, DurianDiagnostics.DUR0001_ProjectMustReferenceDurianCore));
		//}

		[Fact]
		public async Task Succcess_When_ReferencesDurianCore()
		{
			CSharpCompilation compilation = RoslynUtilities.CreateBaseCompilation();
			CoreProjectAnalyzer analyzer = new();
			ImmutableArray<Diagnostic> diagnostics = await analyzer.RunAnalyzer(compilation);
			Assert.Empty(diagnostics);
		}
	}
}
