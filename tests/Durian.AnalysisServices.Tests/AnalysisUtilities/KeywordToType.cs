// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Durian.Analysis.Tests.AnalysisUtilities
{
	public sealed class KeywordToType
	{
		private sealed class KeywordCollection : IEnumerable<object[]>
		{
			public IEnumerator<object[]> GetEnumerator()
			{
				string[] keywords = Utilities.GetCSharpTypeKeywords();
				string[] types = Utilities.GetDotNetTypeWithKeywords();

				int length = keywords.Length;

				for (int i = 0; i < length; i++)
				{
					yield return new object[] { keywords[i], types[i] };
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		[Theory]
		[ClassData(typeof(KeywordCollection))]
		public void ReturnsDotNetType_When_IsValidCSharpTypeKeyword(string keyword, string expected)
		{
			Assert.True(Analysis.AnalysisUtilities.KeywordToType(keyword) == expected);
		}

		[Fact]
		public void ReturnsEmpty_When_KeywordIsNull()
		{
			Assert.True(Analysis.AnalysisUtilities.KeywordToType(null) == string.Empty);
		}

		[Fact]
		public void ReturnsInput_When_InputIsNotCSharpTypeKeyword()
		{
			Assert.True(Analysis.AnalysisUtilities.KeywordToType("type") == "type");
		}
	}
}