using Xunit;

namespace Durian.Tests.AnalysisCore.AutoGenerated
{
	public sealed class GetHeader
	{
		internal const string GeneratorName = "SourceGenerator";
		internal const string GeneratorVersion = "1.0.0.0";

		internal const string NoNameOrVersion =
@"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a source generator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
";

		internal const string NoVersion =
@"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the " + GeneratorName + @" class.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
";

		internal const string WithNameAndVersion =
@"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the " + GeneratorName + " class (version " + GeneratorVersion + @").
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
";

		[Fact]
		public void ExcludesGeneratorNameAndVersion_When_GeneratorNameIsNull()
		{
			Assert.True(Durian.AutoGenerated.GetHeader(null, GeneratorVersion) == NoNameOrVersion);
		}

		[Fact]
		public void ExcludesGeneratorNameAndVersion_When_GeneratorNameIsNull_And_OverloadWithoutVersionWasCalled()
		{
			Assert.True(Durian.AutoGenerated.GetHeader(null) == NoNameOrVersion);
		}

		[Fact]
		public void ExcludesGeneratorNameAndVersion_When_GeneratorNameIsNull_And_VersionIsNull()
		{
			Assert.True(Durian.AutoGenerated.GetHeader(null, null) == NoNameOrVersion);
		}

		[Fact]
		public void ExcludesGeneratorNameAndVersion_When_ParameterlessOverloadWasCalled()
		{
			Assert.True(Durian.AutoGenerated.GetHeader() == NoNameOrVersion);
		}

		[Fact]
		public void ExcludesVersion_When_VersionIsNull()
		{
			Assert.True(Durian.AutoGenerated.GetHeader(GeneratorName, null) == NoVersion);
		}

		[Fact]
		public void ExcludesVersion_When_OverloadWithoutVersionWasCalled()
		{
			Assert.True(Durian.AutoGenerated.GetHeader(GeneratorName) == NoVersion);
		}

		[Fact]
		public void IncludesGeneratorNameAndVersion_When_TheyAreNotNull()
		{
			Assert.True(Durian.AutoGenerated.GetHeader(GeneratorName, GeneratorVersion) == WithNameAndVersion);
		}
	}
}
