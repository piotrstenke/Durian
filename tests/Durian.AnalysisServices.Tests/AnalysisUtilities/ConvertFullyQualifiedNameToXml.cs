// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Xunit;

namespace Durian.Analysis.Tests.AnalysisUtilities
{
	public sealed class ConvertFullyQualifiedNameToXml
	{
		[Fact]
		public void ConvertsChild_When_ChildIsGeneric()
		{
			Assert.True(Execute("Parent.Child<T>") == "Parent.Child{T}");
		}

		[Fact]
		public void ConvertsParent_When_ParentIsGeneric()
		{
			Assert.True(Execute("Parent<T>.Child") == "Parent{T}.Child");
		}

		[Fact]
		public void ConvertsParentAndChild_When_BothAreGeneric()
		{
			Assert.True(Execute("Parent<T>.Child<U>") == "Parent{T}.Child{U}");
		}

		[Fact]
		public void ReturnsEmpty_When_InputIsNull()
		{
			Assert.True(Execute(null) == string.Empty);
		}

		[Fact]
		public void ReturnsName_When_HasNoParent()
		{
			Assert.True(Execute("Child") == "Child");
		}

		[Fact]
		public void ReturnsNameAndParent_When_HasParent()
		{
			Assert.True(Execute("Parent.Child") == "Parent.Child");
		}

		private static string Execute(string? input)
		{
			return Analysis.AnalysisUtilities.ConvertFullyQualifiedNameToXml(input);
		}
	}
}
