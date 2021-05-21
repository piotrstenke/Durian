using System;
using Xunit;

namespace Durian.Tests.AnalysisServices.AnalysisUtilities
{
	public sealed class GetGenericName_IEnumerableString
	{
		[Fact]
		public void ThrowsArgumentNullException_When_TypeParametersIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => Durian.Generator.AnalysisUtilities.GetGenericName(null!));
		}

		[Fact]
		public void ReturnsEmptyString_When_TypeParametersIsEmpty()
		{
			Assert.True(Durian.Generator.AnalysisUtilities.GetGenericName(Array.Empty<string>()) == string.Empty);
		}

		[Fact]
		public void CanReturnSingleTypeParameter()
		{
			Assert.True(Durian.Generator.AnalysisUtilities.GetGenericName(new string[] { "T" }) == "<T>");
		}

		[Fact]
		public void CanReturnMultipleTypeParameters()
		{
			Assert.True(Durian.Generator.AnalysisUtilities.GetGenericName(new string[] { "T", "U" }) == "<T, U>");
		}
	}
}
