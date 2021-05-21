using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Durian.Tests.AnalysisServices.AnalysisUtilities
{
	public sealed class KeywordToType
	{
		[Fact]
		public void ReturnsEmpty_When_KeywordIsNull()
		{
			Assert.True(Durian.Generator.AnalysisUtilities.KeywordToType(null) == string.Empty);
		}

		[Theory]
		[ClassData(typeof(KeywordCollection))]
		public void ReturnsDotNetType_When_IsValidCSharpTypeKeyword(string keyword, string expected)
		{
			Assert.True(Durian.Generator.AnalysisUtilities.KeywordToType(keyword) == expected);
		}

		[Fact]
		public void ReturnsInput_When_InputIsNotCSharpTypeKeyword()
		{
			Assert.True(Durian.Generator.AnalysisUtilities.KeywordToType("type") == "type");
		}

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
	}
}
