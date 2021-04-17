using Xunit;

namespace Durian.Tests.AnalysisCore.AnalysisUtilities
{
	public sealed class ConvertFullyQualifiedNameToXml
	{
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

		[Fact]
		public void ConvertsParent_When_ParentIsGeneric()
		{
			Assert.True(Execute("Parent<T>.Child") == "Parent{T}.Child");
		}

		[Fact]
		public void ConvertsChild_When_ChildIsGeneric()
		{
			Assert.True(Execute("Parent.Child<T>") == "Parent.Child{T}");
		}

		[Fact]
		public void ConvertsParentAndChild_When_BothAreGeneric()
		{
			Assert.True(Execute("Parent<T>.Child<U>") == "Parent{T}.Child{U}");
		}

		private static string Execute(string? input)
		{
			return Durian.AnalysisUtilities.ConvertFullyQualifiedNameToXml(input);
		}
	}
}
