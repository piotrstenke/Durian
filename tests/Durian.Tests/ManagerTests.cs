// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Durian.Info;
using Durian.Manager;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Durian.Tests
{
	public sealed class ManagerTests
	{
		[Fact]
		public async Task DisablesModule_When_HasDisableModuleAttribute()
		{
			string input = "[assembly: Durian.DisableModule(Durian.Info.DurianModule.DefaultParam)]";
			CSharpCompilation compilation = RoslynUtilities.CreateBaseCompilation();
			compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(input));
			DisabledModuleAnalyzer analyzer = new();
			await analyzer.RunAnalyzer(compilation);

			Assert.False(DisabledModuleAnalyzer.IsEnabled(DurianModule.DefaultParam));
		}

		[Fact]
		public async Task DoesNotGenerateEnableModuleAttributeForDisabledModule()
		{
			StringBuilder builder = new();

			foreach (DurianModule module in ModuleIdentity.GetAllModulesAsEnums().Where(m => m != DurianModule.DefaultParam))
			{
				builder
					.Append("[assembly: Durian.Generator.EnableModule(Durian.Info.")
					.Append(nameof(DurianModule))
					.Append('.')
					.Append(module.ToString())
					.AppendLine(")]");
			}

			CSharpSyntaxTree expected = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(builder.ToString());

			CSharpCompilation compilation = RoslynUtilities.CreateBaseCompilation();
			compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("[assembly: Durian.DisableModule(Durian.Info.DurianModule.DefaultParam)]"));
			DisabledModuleAnalyzer analyzer = new();
			await analyzer.RunAnalyzer(compilation);

			SingletonGeneratorTestResult result = GeneratorTest.RunGenerator("", analyzer);

			Assert.True(result.IsGenerated);
			Assert.NotNull(result.SyntaxTree);
			Assert.True(result.SyntaxTree!.IsEquivalentTo(expected));
		}

		[Fact]
		public async Task GeneratesEnableModuleAttributesForAllModules()
		{
			StringBuilder builder = new();

			foreach (DurianModule module in ModuleIdentity.GetAllModulesAsEnums())
			{
				builder
					.Append("[assembly: Durian.Generator.EnableModule(Durian.Info.")
					.Append(nameof(DurianModule))
					.Append('.')
					.Append(module.ToString())
					.AppendLine(")]");
			}

			CSharpSyntaxTree expected = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(builder.ToString());

			CSharpCompilation compilation = RoslynUtilities.CreateBaseCompilation();
			DisabledModuleAnalyzer analyzer = new();
			await analyzer.RunAnalyzer(compilation);

			SingletonGeneratorTestResult result = GeneratorTest.RunGenerator("", analyzer);

			Assert.True(result.IsGenerated);
			Assert.NotNull(result.SyntaxTree);
			Assert.True(result.SyntaxTree!.IsEquivalentTo(expected));
		}

		[Fact]
		public async Task ModulesAreEnabledByDefault()
		{
			CSharpCompilation compilation = RoslynUtilities.CreateBaseCompilation();
			DisabledModuleAnalyzer analyzer = new();
			await analyzer.RunAnalyzer(compilation);

			Assert.True(ModuleIdentity.GetAllModulesAsEnums().All(module => DisabledModuleAnalyzer.IsEnabled(module)));
		}
	}
}
