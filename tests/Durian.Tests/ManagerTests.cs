// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Durian.Manager;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Durian.Info;
using System.Collections.Generic;
using System.Linq;

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
		public async Task GeneratesEnableModuleAttributesForAllModules()
		{
			CSharpCompilation compilation = RoslynUtilities.CreateBaseCompilation();
			DisabledModuleAnalyzer analyzer = new();
			await analyzer.RunAnalyzer(compilation);

			SingletonGeneratorTestResult result = GeneratorTest.RunGenerator("", analyzer);
			DurianModule[] modules = ModuleIdentity.GetAllModulesAsEnums();

			Assert.True(result.IsGenerated);
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
