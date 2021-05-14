using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Durian.Configuration;
using Durian.Generator;
using Durian.Generator.DefaultParam;
using Microsoft.CodeAnalysis;
using Xunit;
using Desc = Durian.Generator.DefaultParam.DefaultParamDiagnostics;

namespace Durian.Tests.DefaultParam
{
	public sealed class ConfigurationAnalyzerTests : DefaultParamAnalyzerTest<DefaultParamConfigurationAnalyzer>
	{
		[Fact]
		public async Task NoDiagnostics_When_IsGlobalConfiguration()
		{
			string input =
@$"using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamConfigurationAttribute)}()]
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
	[{nameof(DefaultParamConfigurationAttribute)}()]
	public static void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>()
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
	[{nameof(DefaultParamConfigurationAttribute)}()]
	public delegate void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>();
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

[{nameof(DefaultParamConfigurationAttribute)}()]
partial class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
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
	[{nameof(DefaultParamConfigurationAttribute)}()]
	public static void Method<T>()
	{{
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0111_DefaultParamConfigurationAttributeCannotBeAppliedToMembersWithoutDefaultParamAttribute.Id));
		}

		[Fact]
		public async Task Warning_When_ConfigurationIsOnLocalFunction()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test 
{{
	public static void Method()
	{{
		[{nameof(DefaultParamConfigurationAttribute)}()]
		static void Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>()
		{{
		}}
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0115_DefaultParamConfigurationIsNotValidOnThisTypeOfMethod.Id));
		}

		[Fact]
		public async Task Warning_When_ConfigurationIsOnNonDefaultParamType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(DefaultParamConfigurationAttribute)}()]
partial class Test<T>
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0111_DefaultParamConfigurationAttributeCannotBeAppliedToMembersWithoutDefaultParamAttribute.Id));
		}

		[Fact]
		public async Task Warning_When_ConfigurationIsOnNonDefaultParamDelegate()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test 
{{
	[{nameof(DefaultParamConfigurationAttribute)}()]
	public delegate void Method<T>();
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0111_DefaultParamConfigurationAttributeCannotBeAppliedToMembersWithoutDefaultParamAttribute.Id));
		}

		[Fact]
		public async Task Warning_When_HasNewModifierPropertyOnType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]
partial class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0114_NewModifierPropertyShouldNotBeUsedOnMembers.Id));
		}

		[Fact]
		public async Task Warning_When_HasNewModifierPropertyOnMethod()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test 
{{
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]
	public static void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>()
	{{
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0114_NewModifierPropertyShouldNotBeUsedOnMembers.Id));
		}

		[Fact]
		public async Task Warning_When_HasNewModifierPropertyOnDelegate()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test 
{{
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]
	public delegate void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>();
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0114_NewModifierPropertyShouldNotBeUsedOnMembers.Id));
		}

		[Fact]
		public async Task NoDiagnostics_When_HasNewModifierPropertyInGlobalConfig()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]
";

			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Warning_When_MethodConventionOnType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.MethodConvention)} = {nameof(DPMethodConvention)}.{nameof(DPMethodConvention.Call)})]
partial class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0113_MethodConventionShouldNotBeUsedOnMembersOtherThanMethods.Id));
		}

		[Fact]
		public async Task Warning_When_MethodConventionOnDelegate()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test 
{{
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.MethodConvention)} = {nameof(DPMethodConvention)}.{nameof(DPMethodConvention.Call)})]
	public delegate void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>();
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0113_MethodConventionShouldNotBeUsedOnMembersOtherThanMethods.Id));
		}

		[Fact]
		public async Task NoDiagnostics_When_MethodConventionOnMethod()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test 
{{
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.MethodConvention)} = {nameof(DPMethodConvention)}.{nameof(DPMethodConvention.Call)})]
	public static void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>()
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

[assembly: {nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.MethodConvention)} = {nameof(DPMethodConvention)}.{nameof(DPMethodConvention.Call)})]
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
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Inherit)})]
	public static void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>()
	{{
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0112_TypeConvetionShouldNotBeUsedOnMethodsOrDelegates.Id));
		}

		[Fact]
		public async Task Warning_When_TypeConventionOnDelegate()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test 
{{
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Inherit)})]
	public delegate void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>();
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0112_TypeConvetionShouldNotBeUsedOnMethodsOrDelegates.Id));
		}


		[Fact]
		public async Task NoDiagnostics_When_TypeConventionOnType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Inherit)})]
partial class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
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

[assembly: {nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.MethodConvention)} = {nameof(DPMethodConvention)}.{nameof(DPMethodConvention.Call)})]
";

			Assert.Empty(await RunAnalyzerAsync(input));
		}
	}
}