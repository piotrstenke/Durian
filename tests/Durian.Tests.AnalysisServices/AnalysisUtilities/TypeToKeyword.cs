using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Durian.Tests.AnalysisServices.AnalysisUtilities
{
	public sealed class TypeToKeyword
	{
		[Fact]
		public void ReturnsEmpty_When_TypeIsNull()
		{
			Assert.True(Durian.Generator.AnalysisUtilities.TypeToKeyword(null) == string.Empty);
		}

		[Theory]
		[ClassData(typeof(KeywordCollection))]
		public void ReturnsCSharpTypeKeyword_When_IsValidDotNetType(string keyword, string expected)
		{
			Assert.True(Durian.Generator.AnalysisUtilities.TypeToKeyword(keyword) == expected);
		}

		[Fact]
		public void ReturnsInput_When_InputIsNotValidDotNetType()
		{
			Assert.True(Durian.Generator.AnalysisUtilities.TypeToKeyword("Type") == "Type");
		}

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
	}
}
