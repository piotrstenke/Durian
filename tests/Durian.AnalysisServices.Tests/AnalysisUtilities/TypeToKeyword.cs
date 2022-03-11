// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Durian.Analysis.Tests.AnalysisUtilities
{
	public sealed class TypeToKeyword
	{
		private sealed class KeywordCollection : IEnumerable<object[]>
		{
			public IEnumerator<object[]> GetEnumerator()
			{
				string[] keywords = Utilities.GetCSharpTypeKeywords();
				string[] types = Utilities.GetDotNetTypeWithKeywords();

				int length = types.Length;

				for (int i = 0; i < length; i++)
				{
					yield return new object[] { types[i], keywords[i] };
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		[Theory]
		[ClassData(typeof(KeywordCollection))]
		public void ReturnsCSharpTypeKeyword_When_IsValidDotNetType(string keyword, string expected)
		{
			Assert.True(Analysis.AnalysisUtilities.TypeToKeyword(keyword) == expected);
		}

		[Fact]
		public void ReturnsEmpty_When_TypeIsNull()
		{
			Assert.True(Analysis.AnalysisUtilities.TypeToKeyword(null) == string.Empty);
		}

		[Fact]
		public void ReturnsInput_When_InputIsNotValidDotNetType()
		{
			Assert.True(Analysis.AnalysisUtilities.TypeToKeyword("Type") == "Type");
		}
	}
}
