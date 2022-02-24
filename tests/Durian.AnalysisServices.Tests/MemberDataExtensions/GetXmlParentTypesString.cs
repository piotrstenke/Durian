// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Analysis.Extensions;
using Durian.TestServices;
using Xunit;

namespace Durian.Analysis.Tests.MemberDataExtensions
{
	public sealed class GetXmlParentTypesString : CompilationTest
	{
		[Fact]
		public void IsSuccess_When_HasMultipleParentTypes()
		{
			Assert.True(GetClass("class Parent { class Child1 { class Child2 { } } }", 2)!.GetXmlParentTypesString() == "Parent.Child1.Child2");
		}

		[Fact]
		public void IsSuccess_When_IsGeneric()
		{
			Assert.True(GetClass("class Parent { class Child<T> { } }", 1)!.GetXmlParentTypesString() == "Parent.Child{T}");
		}

		[Fact]
		public void IsSuccess_When_IsGeneric_And_HasMultipleTypeParameters()
		{
			Assert.True(GetClass("class Parent { class Child<T, U> { } }", 1)!.GetXmlParentTypesString() == "Parent.Child{T, U}");
		}

		[Fact]
		public void IsSuccess_When_IsGeneric_And_ParentIsGeneric()
		{
			Assert.True(GetClass("class Parent<T> { class Child<U> { } }", 1)!.GetXmlParentTypesString() == "Parent{T}.Child{U}");
		}

		[Fact]
		public void IsSuccess_When_ParentIsGeneric()
		{
			Assert.True(GetClass("class Parent<T> { class Child { } }", 1)!.GetXmlParentTypesString() == "Parent{T}.Child");
		}

		[Fact]
		public void ReturnsMemberName_When_HasNoParentTypes()
		{
			Assert.True(GetClass("class Test { }")!.GetXmlParentTypesString() == "Test");
		}

		[Fact]
		public void ReturnsNameWithParameters_When_IsGenericMethod_And_IncludeParametersIsTrue()
		{
			Assert.True(GetMethod("class Test { void Method<T>(int a) { } }")!.GetXmlParentTypesString(true) == "Test.Method{T}(int)");
		}

		[Fact]
		public void ReturnsNameWithParameters_When_IsGenericMethod_And_ParentIsGeneric_And_IncludeParametersIsTrue()
		{
			Assert.True(GetMethod("class Test<T> { void Method<U>(U a, int b) { } }")!.GetXmlParentTypesString(true) == "Test{T}.Method{U}(U, int)");
		}

		[Fact]
		public void ReturnsNameWithParameters_When_IsMethod_And_IncludeParametersIsTrue()
		{
			Assert.True(GetMethod("class Test { void Method(int a) { } }")!.GetXmlParentTypesString(true) == "Test.Method(int)");
		}

		[Fact]
		public void ThrowsArgumentNullException_When_MemberIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => GetClass(null)!.GetXmlParentTypesString());
		}
	}
}