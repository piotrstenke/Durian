using System;
using Xunit;

namespace Durian.Tests.AnalysisCore.AnalysisUtilities
{
	public sealed class GetGenericName_IEnumerableString
	{
		[Fact]
		public void ThrowsArgumentNullException_When_TypeParametersIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => Durian.AnalysisUtilities.GetGenericName(null!));
		}

		[Fact]
		public void ReturnsEmptyString_When_TypeParametersIsEmpty()
		{
			Assert.True(Durian.AnalysisUtilities.GetGenericName(Array.Empty<string>()) == string.Empty);
		}

		[Fact]
		public void CanReturnSingleTypeParameter()
		{
			Assert.True(Durian.AnalysisUtilities.GetGenericName(new string[] { "T" }) == "<T>");
		}

		[Fact]
		public void CanReturnMultipleTypeParameters()
		{
			Assert.True(Durian.AnalysisUtilities.GetGenericName(new string[] { "T", "U" }) == "<T, U>");
		}
	}
}
