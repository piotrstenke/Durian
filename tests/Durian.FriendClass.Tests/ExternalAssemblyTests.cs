// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Xunit;
using Durian.TestServices;
using Durian.Configuration;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using static Durian.Analysis.FriendClass.FriendClassDiagnostics;
using System.Reflection;
using System;

namespace Durian.Analysis.FriendClass.Tests
{
	public sealed class ExternalAssemblyTests
	{
		[Fact]
		public async Task Error_When_DoesNotAllowExternalAssembly_And_HasInternalsVisibleTo_And_IsNotFriend()
		{
			const string externalAssembly = "external";

			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(""{externalAssembly}"")]
[{nameof(FriendClassConfigurationAttribute)}({nameof(FriendClassConfigurationAttribute.AllowsExternalAssembly)} = false)]
[{nameof(FriendClassAttribute)}(typeof(Other))]
public class Test
{{
	internal static string Name {{ get; }}
}}

class Other
{{
}}
";
			string ext =
$@"public class External
{{
	void Main()
	{{
		string name = Test.Name;
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAccessAnalyzerAsync(input, ext, externalAssembly, true);
			Assert.Contains(diagnostics, d => d.Id == DUR0302_MemberCannotBeAccessedOutsideOfFriendClass.Id);
		}

		[Fact]
		public async Task Error_When_TypeIsOutsideOfCurrentAssembly_And_AllowsExternalAssembly_AndDoesNotHaveAssociatedAllowsVisibleTo()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(""another"")]
[{nameof(FriendClassConfigurationAttribute)}({nameof(FriendClassConfigurationAttribute.AllowsExternalAssembly)} = true)]
[{nameof(FriendClassAttribute)}(typeof(External))]
public class Test
{{
	internal static string Name {{ get; }}
}}

";
			string ext =
$@"public class External
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunDeclarationAnalyzerAsync(input, ext, "external", false);
			Assert.Contains(diagnostics, d => d.Id == DUR0310_InternalsVisibleToNotFound.Id);
		}

		[Fact]
		public async Task Error_When_TypeIsOutsideOfCurrentAssembly_And_DoesNotAllowExternalAssembly()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(FriendClassConfigurationAttribute)}({nameof(FriendClassConfigurationAttribute.AllowsExternalAssembly)} = false)]
[{nameof(FriendClassAttribute)}(typeof(External))]
public class Test
{{
	internal static string Name {{ get; }}
}}
";
			string ext =
$@"public class External
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunDeclarationAnalyzerAsync(input, ext, "external", false);
			Assert.Contains(diagnostics, d => d.Id == DUR0301_TargetTypeIsOutsideOfAssembly.Id);
		}

		[Fact]
		public async Task Error_When_TypeIsOutsideOfCurrentAssembly_And_DoesNotAllowExternalAssembly_And_HasInternalsVisibleTo()
		{
			const string externalAssembly = "external";

			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(""{externalAssembly}"")]
[{nameof(FriendClassConfigurationAttribute)}({nameof(FriendClassConfigurationAttribute.AllowsExternalAssembly)} = false)]
[{nameof(FriendClassAttribute)}(typeof(External))]
public class Test
{{
	internal static string Name {{ get; }}
}}
";
			string ext =
$@"public class External
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunDeclarationAnalyzerAsync(input, ext, externalAssembly, false);
			Assert.Contains(diagnostics, d => d.Id == DUR0301_TargetTypeIsOutsideOfAssembly.Id);
		}

		[Fact]
		public async Task Success_When_TypeIsOutsideOfCurrentAssembly_And_AllowsExternalAssembly_And_HasAssociatedAllowsVisibleToAttribute()
		{
			const string externalAssembly = "external";

			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(""{externalAssembly}"")]
[{nameof(FriendClassConfigurationAttribute)}({nameof(FriendClassConfigurationAttribute.AllowsExternalAssembly)} = true)]
[{nameof(FriendClassAttribute)}(typeof(External))]
public class Test
{{
	internal static string Name {{ get; }}
}}
";
			string ext =
$@"public class External
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunDeclarationAnalyzerAsync(input, ext, externalAssembly, false);
			Assert.Empty(diagnostics);
		}

		private static Task<ImmutableArray<Diagnostic>> RunAccessAnalyzerAsync(string input, string externalCode, string externalAssemblyName, bool refInputOrExternal)
		{
			FriendClassAccessAnalyzer analyzer = new();
			return RunAnalyzerAsync(analyzer, input, externalCode, externalAssemblyName, refInputOrExternal);
		}

		private static async Task<ImmutableArray<Diagnostic>> RunAnalyzerAsync(DiagnosticAnalyzer analyzer, string input, string externalCode, string externalAssemblyName, bool refInputOrExternal)
		{
			CSharpCompilation first = refInputOrExternal ? GetInputCompilation() : GetExternalCompilation();

			using MemoryStream stream = new();

			EmitResult emit = first.Emit(stream);

			if (!emit.Success)
			{
				throw new InvalidOperationException("Emit failed!");
			}

			MetadataReference reference = MetadataReference.CreateFromImage(stream.ToArray());
			CSharpCompilation second = refInputOrExternal ? GetExternalCompilation() : GetInputCompilation();
			second = second.AddReferences(reference);

			AnalysisResult result = await second
				.WithAnalyzers(ImmutableArray.Create(analyzer))
				.GetAnalysisResultAsync(default);

			return result.GetAllDiagnostics(analyzer);

			CSharpCompilation GetInputCompilation()
			{
				return RoslynUtilities
					.CreateBaseCompilation()
					.AddSyntaxTrees(CSharpSyntaxTree.ParseText(input, encoding: Encoding.UTF8));
			}

			CSharpCompilation GetExternalCompilation()
			{
				return RoslynUtilities
					.CreateBaseCompilation()
					.WithAssemblyName(externalAssemblyName)
					.AddSyntaxTrees(CSharpSyntaxTree.ParseText(externalCode, encoding: Encoding.UTF8));
			}
		}

		private static Task<ImmutableArray<Diagnostic>> RunDeclarationAnalyzerAsync(string input, string externalCode, string externalAssemblyName, bool refInputOrExternal)
		{
			FriendClassDeclarationAnalyzer analyzer = new();
			return RunAnalyzerAsync(analyzer, input, externalCode, externalAssemblyName, refInputOrExternal);
		}
	}
}
