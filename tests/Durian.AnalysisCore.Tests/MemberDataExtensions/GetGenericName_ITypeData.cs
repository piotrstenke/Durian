using System;
using Durian.Extensions;
using Xunit;

namespace Durian.Tests.AnalysisCore.MemberDataExtensions
{
	public sealed class GetGenericName_ITypeData : CompilationTest
	{
		[Fact]
		public void ThrowsArgumentNullException_When_TypeIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => GetClass(null).GetGenericName());
		}

		[Fact]
		public void ReturnsTypeName_When_IsNotGeneric()
		{
			Assert.True(GetClass("class Test { }").GetGenericName() == "Test");
		}

		[Fact]
		public void IsSuccess_When_HasOneTypeParameter()
		{
			Assert.True(GetClass("class Test<T> { }").GetGenericName() == "Test<T>");
		}

		[Fact]
		public void IsSuccess_When_HasMultipleTypeParameters()
		{
			Assert.True(GetClass("class Test<T, U> { }").GetGenericName() == "Test<T, U>");
		}
	}
}