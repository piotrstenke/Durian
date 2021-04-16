using System;
using System.Collections.Generic;
using Durian.Extensions;
using Microsoft.CodeAnalysis;
using Moq;
using Xunit;

namespace Durian.Tests.CorePackage.AnalysisUtilities
{
	public sealed class GetGenericName_IEnumerableITypeParameterSymbol_String
	{
		[Fact]
		public void ThrowsArgumentNullException_When_TypeParametersIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => ((IEnumerable<ITypeParameterSymbol>)null!).GetGenericName("Test"));
		}

		[Fact]
		public void ReturnsOnlyGenericPart_When_NameIsNull()
		{
			string genericName = new ITypeParameterSymbol[] { GetValidTypeParameter() }.GetGenericName(null);
			Assert.True(genericName.Length > 1 && genericName[0] == '<' && genericName[^1] == '>');
		}

		[Fact]
		public void ReturnsOnlyName_When_TypeParametersIsEmpty()
		{
			Assert.True(Array.Empty<ITypeParameterSymbol>().GetGenericName("Test") == "Test");
		}

		[Fact]
		public void CanReturnGenericNameWithOneTypeParameter()
		{
			Assert.True(new ITypeParameterSymbol[] { GetValidTypeParameter() }.GetGenericName("Test") == "Test<T>");
		}

		[Fact]
		public void CanReturnGenericNameWithMultipleTypeParameters()
		{
			Mock<ITypeParameterSymbol> mock = new();
			mock.Setup(p => p.Name).Returns("U");

			ITypeParameterSymbol parameter1 = GetValidTypeParameter();
			ITypeParameterSymbol parameter2 = mock.Object;

			Assert.True(new ITypeParameterSymbol[] { parameter1, parameter2 }.GetGenericName("Test") == "Test<T, U>");
		}

		private static ITypeParameterSymbol GetValidTypeParameter()
		{
			Mock<ITypeParameterSymbol> parameter = new();
			parameter.Setup(p => p.Name).Returns("T");
			return parameter.Object;
		}
	}
}
