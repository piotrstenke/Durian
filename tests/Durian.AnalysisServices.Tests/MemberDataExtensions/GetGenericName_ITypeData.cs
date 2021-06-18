// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Analysis.Extensions;
using Durian.TestServices;
using Xunit;

namespace Durian.Analysis.Tests.MemberDataExtensions
{
	public sealed class GetGenericName_ITypeData : CompilationTest
	{
		[Fact]
		public void IsSuccess_When_HasMultipleTypeParameters()
		{
			Assert.True(GetClass("class Test<T, U> { }").GetGenericName() == "Test<T, U>");
		}

		[Fact]
		public void IsSuccess_When_HasOneTypeParameter()
		{
			Assert.True(GetClass("class Test<T> { }").GetGenericName() == "Test<T>");
		}

		[Fact]
		public void ReturnsTypeName_When_IsNotGeneric()
		{
			Assert.True(GetClass("class Test { }").GetGenericName() == "Test");
		}

		[Fact]
		public void ThrowsArgumentNullException_When_TypeIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => GetClass(null).GetGenericName());
		}
	}
}
