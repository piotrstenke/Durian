using System;
using Durian.Generator.Extensions;
using Xunit;

namespace Durian.Tests.AnalysisCore.MemberDataExtensions
{
	public sealed class GetGenericName_IMemberData : CompilationTest
	{
		[Fact]
		public void ThrowsArgumentNullException_When_MemberIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => GetClass(null).GetGenericName());
		}

		[Fact]
		public void ReturnsMemberName_When_MemberIsNotTypeOrDelegateOrMethod()
		{
			Assert.True(GetField("class Test { int Field; }").GetGenericName() == "Field");
		}

		[Fact]
		public void ReturnsMemberName_When_MemberIsNotGeneric()
		{
			Assert.True(GetMethod("class Test { void Method() { } }").GetGenericName() == "Method");
		}

		[Fact]
		public void IsSuccess_When_HasOneTypeParameter()
		{
			Assert.True(GetMethod("class Test { void Method<T>() { } }").GetGenericName() == "Method<T>");
		}

		[Fact]
		public void IsSuccess_When_HasMultipleTypeParameters()
		{
			Assert.True(GetMethod("class Test { void Method<T, U>() { } }").GetGenericName() == "Method<T, U>");
		}

		[Fact]
		public void ReturnsNameWithParameters_When_MemberIsMethod_And_IncludeParametersIsTrue_When_HasSingleTypeParameter()
		{
			Assert.True(GetMethod("class Test { void Method<T>(int a, int b) { } }").GetGenericName(true) == "Method<T>(int, int)");
		}

		[Fact]
		public void ReturnsNameWithParameters_When_MemberIsMethod_And_IncludeParametersIsTrue_When_HasMultipleTypeParameters()
		{
			Assert.True(GetMethod("class Test { void Method<T, U>(int a, int b) { } }").GetGenericName(true) == "Method<T, U>(int, int)");
		}

		[Fact]
		public void IsSuccess_When_GenericParameterIsArgument_and_IncludeParametersIsTrue()
		{
			Assert.True(GetMethod("class Test { void Method<T>(T a) { } }").GetGenericName(true) == "Method<T>(T)");
		}
	}
}