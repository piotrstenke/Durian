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
using static Durian.Analysis.FriendClass.FriendClassDiagnostics;

namespace Durian.Analysis.FriendClass.Tests
{
	public sealed class ExternalAssemblyTests
	{
		[Fact]
		public async Task Error_When_DoesNotAllowExternalAssembly_And_HasInternalsVisibleTo_And_IsNotFriend()
		{
			const string externalAssembly = "external";

			string input =
$@"using {DurianStrings.ConfigurationNamespace};
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(""{externalAssembly}"")]
[{nameof(FriendClassConfigurationAttribute)}({nameof(FriendClassConfigurationAttribute.AllowsExternalAssembly)} = false)]
class Test
{{
	internal static string Name {{ get; }}
}}
";
			string ext =
$@"public class Other
{{
	void Main()
	{{
		string name = Test.Name;
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAccessAnalyzerAsync(input, ext, externalAssembly);
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
[{nameof(FriendClassAttribute)}(typeof(Other))]
class Test
{{
	internal static string Name {{ get; }}
}}
";
			string ext =
$@"public class Other
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunDeclarationAnalyzerAsync(input, ext, "external");
			Assert.Contains(diagnostics, d => d.Id == DUR0310_InternalsVisibleToNotFound.Id);
		}

		[Fact]
		public async Task Error_When_TypeIsOutsideOfCurrentAssembly_And_DoesNotAllowExternalAssembly()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(FriendClassConfigurationAttribute)}({nameof(FriendClassConfigurationAttribute.AllowsExternalAssembly)} = false)]
[{nameof(FriendClassAttribute)}(typeof(Other))]
class Test
{{
	internal static string Name {{ get; }}
}}
";
			string ext =
$@"public class Other
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunDeclarationAnalyzerAsync(input, ext, "external");
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
[{nameof(FriendClassAttribute)}(typeof(Other))]
class Test
{{
	internal static string Name {{ get; }}
}}
";
			string ext =
$@"public class Other
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunDeclarationAnalyzerAsync(input, ext, externalAssembly);
			Assert.Contains(diagnostics, d => d.Id == DUR0301_TargetTypeIsOutsideOfAssembly.Id);
		}

		[Fact]
		public async Task Error_When_TypeIsOutsideOfCurrentAssembly_And_DoesNotAllowExternalAssembly_And_HasInternalsVisibleTo_And_TriesToAccessInternalMember()
		{
			const string externalAssembly = "external";

			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(""{externalAssembly}"")]
[{nameof(FriendClassConfigurationAttribute)}({nameof(FriendClassConfigurationAttribute.AllowsExternalAssembly)} = false)]
[{nameof(FriendClassAttribute)}(typeof(Other))]
class Test
{{
	internal static string Name {{ get; }}
}}
";
			string ext =
$@"public class Other
{{
	void Main()
	{{
		string name = Test.Name;
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAccessAnalyzerAsync(input, ext, externalAssembly);
			Assert.Contains(diagnostics, d => d.Id == DUR0313_MemberCannotBeAccessedInExternalAssembly.Id);
		}

		[Fact]
		public async Task Success_When_AllowsExternalAssembly_And_FriendIsInExternalAssembly_And_FriendTriesToAccessInternalMember()
		{
			const string externalAssembly = "external";

			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(""{externalAssembly}"")]
[{nameof(FriendClassConfigurationAttribute)}({nameof(FriendClassConfigurationAttribute.AllowsExternalAssembly)} = true)]
[{nameof(FriendClassAttribute)}(typeof(Other))]
class Test
{{
	internal static string Name {{ get; }}
}}
";
			string ext =
$@"public class Other
{{
	void Main()
	{{
		string name = Test.Name;
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAccessAnalyzerAsync(input, ext, externalAssembly);
			Assert.Empty(diagnostics);
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
[{nameof(FriendClassAttribute)}(typeof(Other))]
class Test
{{
	internal static string Name {{ get; }}
}}
";
			string ext =
$@"public class Other
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunDeclarationAnalyzerAsync(input, ext, externalAssembly);
			Assert.Empty(diagnostics);
		}

		private static Task<ImmutableArray<Diagnostic>> RunAccessAnalyzerAsync(string input, string externalCode, string externalAssemblyName)
		{
			FriendClassAccessAnalyzer analyzer = new();
			return RunAnalyzerAsync(analyzer, input, externalCode, externalAssemblyName);
		}

		private static async Task<ImmutableArray<Diagnostic>> RunAnalyzerAsync(DiagnosticAnalyzer analyzer, string input, string externalCode, string externalAssemblyName)
		{
			CSharpCompilation external = RoslynUtilities
				.CreateBaseCompilation()
				.WithAssemblyName(externalAssemblyName)
				.AddSyntaxTrees(CSharpSyntaxTree.ParseText(externalCode, encoding: Encoding.UTF8));

			CSharpCompilation current = RoslynUtilities
				.CreateBaseCompilation()
				.AddSyntaxTrees(CSharpSyntaxTree.ParseText(input, encoding: Encoding.UTF8));

			using MemoryStream stream = new();

			current.Emit(stream);
			MetadataReference reference = MetadataReference.CreateFromStream(stream);
			external = external.AddReferences(reference);

			using MemoryStream stream2 = new();

			AnalysisResult result = await external
				.WithAnalyzers(ImmutableArray.Create(analyzer))
				.GetAnalysisResultAsync(default);

			return result.GetAllDiagnostics(analyzer);
		}

		private static Task<ImmutableArray<Diagnostic>> RunDeclarationAnalyzerAsync(string input, string externalCode, string externalAssemblyName)
		{
			FriendClassDeclarationAnalyzer analyzer = new();
			return RunAnalyzerAsync(analyzer, input, externalCode, externalAssemblyName);
		}
	}
}
