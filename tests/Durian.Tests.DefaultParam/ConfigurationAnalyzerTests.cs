using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Durian.Configuration;
using Durian.Generator;
using Durian.Generator.DefaultParam;
using Microsoft.CodeAnalysis;
using Xunit;
using static Durian.Generator.DefaultParam.DefaultParamDiagnostics;

namespace Durian.Tests.DefaultParam
{
	public sealed class ConfigurationAnalyzerTests : AnalyzerTest<DefaultParamConfigurationAnalyzer>
	{
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
			Assert.True(diagnostics.Any(d => d.Id == DUR0111_DefaultParamConfigurationAttributeCannotBeAppliedToMembersWithoutDefaultParamAttribute.Id));
		}

		[Fact]
		public async Task Error_When_ConfigurationIsOnLocalFunction()
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
			Assert.True(diagnostics.Any(d => d.Id == DUR0115_DefaultParamConfigurationIsNotValidOnThisTypeOfMethod.Id));
		}

		[Fact]
		public async Task Error_When_MethodIsInInterface()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial interface ITest 
{{
	[{nameof(DefaultParamConfigurationAttribute)}()]
	void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>();
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0115_DefaultParamConfigurationIsNotValidOnThisTypeOfMethod.Id));
		}

		[Fact]
		public async Task Error_When_MethodIsExplicitImplementation()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

interface ITest 
{{
	void Method<[DefaultParam(typeof(string))]T>();
}}

class Test : ITest
{{
	[{nameof(DefaultParamConfigurationAttribute)}]
	void ITest.Method<[DefaultParam(typeof(string))]T>()
	{{
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0115_DefaultParamConfigurationIsNotValidOnThisTypeOfMethod.Id));
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
			Assert.True(diagnostics.Any(d => d.Id == DUR0111_DefaultParamConfigurationAttributeCannotBeAppliedToMembersWithoutDefaultParamAttribute.Id));
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
			Assert.True(diagnostics.Any(d => d.Id == DUR0111_DefaultParamConfigurationAttributeCannotBeAppliedToMembersWithoutDefaultParamAttribute.Id));
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
			Assert.True(diagnostics.Any(d => d.Id == DUR0113_MethodConventionShouldNotBeUsedOnMembersOtherThanMethods.Id));
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
			Assert.True(diagnostics.Any(d => d.Id == DUR0113_MethodConventionShouldNotBeUsedOnMembersOtherThanMethods.Id));
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
			Assert.True(diagnostics.Any(d => d.Id == DUR0112_TypeConvetionShouldNotBeUsedOnMembersOtherThanTypes.Id));
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
			Assert.True(diagnostics.Any(d => d.Id == DUR0112_TypeConvetionShouldNotBeUsedOnMembersOtherThanTypes.Id));
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
		public async Task Error_When_IsInheritOnStruct()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Inherit)})]
public struct Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>()
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0117_InheritTypeConventionCannotBeUsedOnStructOrSealedType.Id));
		}

		[Fact]
		public async Task Error_When_IsInheritOnSealedType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Inherit)})]
public sealed class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>()
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0117_InheritTypeConventionCannotBeUsedOnStructOrSealedType.Id));
		}

		[Fact]
		public async Task Error_When_IsInheritOnTypeWithNoAccessibleConstructors()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Inherit)})]
public class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
{{
	private Test()
	{{
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0123_InheritTypeConventionCannotBeUsedOnTypeWithNoAccessibleConstructor.Id));
		}

		[Fact]
		public async Task Warning_When_IsNotChildType_And_UsesApplyNewModifierProperty()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]
public class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0124_ApplyNewModifierShouldNotBeUsedWhenIsNotChildOfType.Id));
		}
	}
}