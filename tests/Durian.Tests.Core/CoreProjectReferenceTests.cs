using System;
using Xunit;
using Durian.Generator;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Operations;
using System.Threading;
using System.IO;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Linq;

namespace Durian.Tests.Core
{
	public sealed class CoreProjectReferenceTests
	{
		[Fact]
		public async Task Error_When_DoesNotReferenceDurianCore()
		{
			CSharpCompilation compilation = RoslynUtilities.CreateBaseCompilation();
			compilation = compilation.WithReferences(compilation.References.Where(r =>
			{
				return r.Display is not null && AssemblyIdentity.TryParseDisplayName(r.Display, out AssemblyIdentity? assembly) && assembly.Name != "Durian.Core";
			}));

			CoreProjectAnalyzer analyzer = new();
			Assert.True(await analyzer.ProducesDiagnostic(compilation, DurianDiagnostics.DUR0001_ProjectMustReferenceDurianCore));
		}

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
