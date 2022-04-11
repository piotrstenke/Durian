﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Immutable;
using System.Threading.Tasks;
using Durian.Analysis;
using Durian.Generator;
using Durian.Info;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using static Durian.Analysis.DurianDiagnostics;

namespace Durian.Tests
{
	public sealed class TypeImportTests
	{
		[Fact]
		public async Task Error_When_AddedCustomTypeToGeneratorNamespace()
		{
			string input =
$@"namespace Durian.Generator
{{
	class Test
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(new CustomTypesInGeneratorNamespaceAnalyzer(), input), d => d.Id == DUR0005_DoNotAddTypesToGeneratorNamespace.Id);
		}

		[Fact]
		public async Task Error_When_UsesTypeFromGeneratorNamespace()
		{
			string input =
$@"using {DurianStrings.GeneratorNamespace};

[{nameof(DurianGeneratedAttribute)}]
class Test
{{
}}
";
			Assert.Contains(await RunAnalyzer(new TypeImportAnalyzer(), input), d => d.Id == DUR0003_DoNotUseTypeFromDurianGeneratorNamespace.Id);
		}

		[Fact]
		public async Task Error_When_UsesTypeFromGeneratorNamespaceAsAlias()
		{
			string input =
$@"using A = {DurianStrings.GeneratorNamespace}.{nameof(DurianGeneratedAttribute)};

[A]
class Test
{{
}}
";
			Assert.Contains(await RunAnalyzer(new TypeImportAnalyzer(), input), d => d.Id == DUR0003_DoNotUseTypeFromDurianGeneratorNamespace.Id);
		}

		[Fact]
		public async Task Error_When_UsesTypeFromGeneratorNamespaceInFullyQualifiedName()
		{
			string input =
$@"[{DurianStrings.GeneratorNamespace}.{nameof(DurianGeneratedAttribute)}]
class Test
{{
}}
";
			Assert.Contains(await RunAnalyzer(new TypeImportAnalyzer(), input), d => d.Id == DUR0003_DoNotUseTypeFromDurianGeneratorNamespace.Id);
		}

		[Fact]
		public async Task Error_When_UsesTypeFromGeneratorNamespaceInGenericName()
		{
			string input =
$@"using {DurianStrings.GeneratorNamespace};
using System.Collections.Generic;

class Test
{{
	public void Method()
	{{
		List<{nameof(DurianGeneratedAttribute)}> list = new();
	}}
}}
";
			Assert.Contains(await RunAnalyzer(new TypeImportAnalyzer(), input), d => d.Id == DUR0003_DoNotUseTypeFromDurianGeneratorNamespace.Id);
		}

		[Fact]
		public async Task UsingStatementWithGeneratorNamespaceDoesNotProduceError()
		{
			string input =
$@"using {DurianStrings.GeneratorNamespace};

class Test
{{
}}
";
			Assert.Empty(await RunAnalyzer(new TypeImportAnalyzer(), input));
		}

		private static Task<ImmutableArray<Diagnostic>> RunAnalyzer(DiagnosticAnalyzer analyzer, string input)
		{
			return RunAnalyzer(analyzer, input, nameof(DurianModule.Core));
		}

		private static Task<ImmutableArray<Diagnostic>> RunAnalyzer(DiagnosticAnalyzer analyzer, string input, string module)
		{
			string moduleMock =
@$"{AutoGenerated.GetHeader()}

[assembly: {DurianStrings.GeneratorNamespace}.{nameof(EnableModuleAttribute)}({DurianStrings.InfoNamespace}.{nameof(DurianModule)}.{module})]";

			CSharpCompilation compilation = RoslynUtilities.CreateCompilation(new string[] { moduleMock, input });

			return analyzer.RunAnalyzer(compilation);
		}
	}
}
