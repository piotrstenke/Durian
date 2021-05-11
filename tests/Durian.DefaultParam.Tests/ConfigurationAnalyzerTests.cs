using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Durian.DefaultParam;
using Microsoft.CodeAnalysis;
using Xunit;
using Desc = Durian.DefaultParam.DefaultParamDiagnostics.Descriptors;

namespace Durian.Tests.DefaultParam
{
	public sealed class ConfigurationAnalyzerTests : DefaultParamAnalyzerTest<DefaultParamConfigurationAnalyzer>
	{
		public ConfigurationAnalyzerTests(DefaultParamCompilationFixture fixture) : base(fixture)
		{
		}

		[Fact]
		public async Task NoDiagnostics_When_IsGlobalConfiguration()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamConfigurationAttribute.AttributeName}()]
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task NoDiagnostics_When_IsDefaultParamMethod()
		{
			string input = 
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test 
{{
	[{DefaultParamConfigurationAttribute.AttributeName}()]
	public static void Method<[{DefaultParamAttribute.AttributeName}(typeof(string))]T>()
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task NoDiagnostics_When_IsDefaultParamDelegate()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test 
{{
	[{DefaultParamConfigurationAttribute.AttributeName}()]
	public delegate void Method<[{DefaultParamAttribute.AttributeName}(typeof(string))]T>();
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task NoDiagnostics_When_IsDefaultParamType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamConfigurationAttribute.AttributeName}()]
partial class Test<[{DefaultParamAttribute.AttributeName}(typeof(string))]T>
{{
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Warning_When_ConfigurationIsOnNonDefaultParamMethod()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test 
{{
	[{DefaultParamConfigurationAttribute.AttributeName}()]
	public static void Method<T>()
	{{
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DefaultParamConfigurationAttributeCannotBeAppliedToMembersWithoutDefaultParamAttribute.Id));
		}

		[Fact]
		public async Task Warning_When_ConfigurationIsOnNonDefaultParamType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamConfigurationAttribute.AttributeName}()]
partial class Test<T>
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DefaultParamConfigurationAttributeCannotBeAppliedToMembersWithoutDefaultParamAttribute.Id));
		}

		[Fact]
		public async Task Warning_When_ConfigurationIsOnNonDefaultParamDelegate()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test 
{{
	[{DefaultParamConfigurationAttribute.AttributeName}()]
	public delegate void Method<T>();
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DefaultParamConfigurationAttributeCannotBeAppliedToMembersWithoutDefaultParamAttribute.Id));
		}

		[Fact]
		public async Task Warning_When_HasNewModifierPropertyOnType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossibleProperty} = true)]
partial class Test<[{DefaultParamAttribute.AttributeName}(typeof(string))]T>
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DefaultParamPropertyShouldNotBeUsedOnMembersOfType.Id));
		}

		[Fact]
		public async Task Warning_When_HasNewModifierPropertyOnMethod()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test 
{{
	[{DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossibleProperty} = true)]
	public static void Method<[{DefaultParamAttribute.AttributeName}(typeof(string))]T>()
	{{
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DefaultParamPropertyShouldNotBeUsedOnMembersOfType.Id));
		}

		[Fact]
		public async Task Warning_When_HasNewModifierPropertyOnDelegate()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test 
{{
	[{DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossibleProperty} = true)]
	public delegate void Method<[{DefaultParamAttribute.AttributeName}(typeof(string))]T>();
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DefaultParamPropertyShouldNotBeUsedOnMembersOfType.Id));
		}

		[Fact]
		public async Task NoDiagnostics_When_HasNewModifierPropertyInGlobalConfig()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossibleProperty} = true)]
";

			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Warning_When_MethodConventionOnType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.MethodConvetionProperty} = {DPMethodGenConvention.Name}.{nameof(DPMethodGenConvention.Call)})]
partial class Test<[{DefaultParamAttribute.AttributeName}(typeof(string))]T>
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DefaultParamPropertyShouldNotBeUsedOnMembersOfType.Id));
		}

		[Fact]
		public async Task Warning_When_MethodConventionOnDelegate()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test 
{{
	[{DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.MethodConvetionProperty} = {DPMethodGenConvention.Name}.{nameof(DPMethodGenConvention.Call)})]
	public delegate void Method<[{DefaultParamAttribute.AttributeName}(typeof(string))]T>();
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DefaultParamPropertyShouldNotBeUsedOnMembersOfType.Id));
		}

		[Fact]
		public async Task NoDiagnostics_When_MethodConventionOnMethod()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test 
{{
	[{DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.MethodConvetionProperty} = {DPMethodGenConvention.Name}.{nameof(DPMethodGenConvention.Call)})]
	public static void Method<[{DefaultParamAttribute.AttributeName}(typeof(string))]T>()
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task NoDiagnostics_When_MethodConvetionPropertyInGlobalConfig()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.MethodConvetionProperty} = {DPMethodGenConvention.Name}.{nameof(DPMethodGenConvention.Call)})]
";

			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Warning_When_TypeConventionOnMethod()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test 
{{
	[{DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.TypeConventionProperty} = {DPTypeGenConvention.Name}.{nameof(DPTypeGenConvention.Inherit)})]
	public static void Method<[{DefaultParamAttribute.AttributeName}(typeof(string))]T>()
	{{
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DefaultParamPropertyShouldNotBeUsedOnMembersOfType.Id));
		}

		[Fact]
		public async Task Warning_When_TypeConventionOnDelegate()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test 
{{
	[{DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.TypeConventionProperty} = {DPTypeGenConvention.Name}.{nameof(DPTypeGenConvention.Inherit)})]
	public delegate void Method<[{DefaultParamAttribute.AttributeName}(typeof(string))]T>();
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DefaultParamPropertyShouldNotBeUsedOnMembersOfType.Id));
		}


		[Fact]
		public async Task NoDiagnostics_When_TypeConventionOnType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.TypeConventionProperty} = {DPTypeGenConvention.Name}.{nameof(DPTypeGenConvention.Inherit)})]
partial class Test<[{DefaultParamAttribute.AttributeName}(typeof(string))]T>
{{
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task NoDiagnostics_When_TypeConvetionPropertyInGlobalConfig()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.MethodConvetionProperty} = {DPMethodGenConvention.Name}.{nameof(DPMethodGenConvention.Call)})]
";

			Assert.Empty(await RunAnalyzerAsync(input));
		}
	}
}