using System;
using Xunit;

namespace Durian.Tests.AnalysisCore.AnalysisUtilities
{
	public sealed class GetGenericName_IEnumerableString_String
	{
		[Fact]
		public void ThrowsArgumentNullException_When_TypeParametersIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => Durian.Generator.AnalysisUtilities.GetGenericName(null!, "Test"));
		}

		[Fact]
		public void ReturnsOnlyGenericPart_When_NameIsNull()
		{
			string genericName = Durian.Generator.AnalysisUtilities.GetGenericName(new string[] { "T" }, null);
			Assert.True(genericName.Length > 1 && genericName[0] == '<' && genericName[^1] == '>');
		}

		[Fact]
		public void ReturnsOnlyName_When_TypeParametersIsEmpty()
		{
			Assert.True(Durian.Generator.AnalysisUtilities.GetGenericName(Array.Empty<string>(), "Test") == "Test");
		}

		[Fact]
		public void CanReturnGenericNameWithSingleTypeParameter()
		{
			Assert.True(Durian.Generator.AnalysisUtilities.GetGenericName(new string[] { "T" }, "Test") == "Test<T>");
		}

		[Fact]
		public void CanReturnGenericNameWithMultipleTypeParameters()
		{
			Assert.True(Durian.Generator.AnalysisUtilities.GetGenericName(new string[] { "T", "U" }, "Test") == "Test<T, U>");
		}
	}
}
