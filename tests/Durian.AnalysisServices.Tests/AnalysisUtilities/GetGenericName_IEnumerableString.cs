// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Xunit;

namespace Durian.Analysis.Tests.AnalysisUtilities
{
	public sealed class GetGenericName_IEnumerableString
	{
		[Fact]
		public void CanReturnMultipleTypeParameters()
		{
			Assert.True(Analysis.AnalysisUtilities.GetGenericName(new string[] { "T", "U" }) == "<T, U>");
		}

		[Fact]
		public void CanReturnSingleTypeParameter()
		{
			Assert.True(Analysis.AnalysisUtilities.GetGenericName(new string[] { "T" }) == "<T>");
		}

		[Fact]
		public void ReturnsEmptyString_When_TypeParametersIsEmpty()
		{
			Assert.True(Analysis.AnalysisUtilities.GetGenericName(Array.Empty<string>()) == string.Empty);
		}

		[Fact]
		public void ThrowsArgumentNullException_When_TypeParametersIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => Analysis.AnalysisUtilities.GetGenericName(null!));
		}
	}
}
