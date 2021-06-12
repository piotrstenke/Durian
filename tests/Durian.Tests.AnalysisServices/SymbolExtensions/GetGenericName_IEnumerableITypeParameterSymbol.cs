// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Moq;
using Xunit;

namespace Durian.Tests.AnalysisServices.AnalysisUtilities
{
	public sealed class GetGenericName_IEnumerableITypeParameterSymbol
	{
		[Fact]
		public void CanReturnMultipleTypeParameters()
		{
			Mock<ITypeParameterSymbol> parameter1 = new();
			parameter1.Setup(p => p.Name).Returns("T");

			Mock<ITypeParameterSymbol> parameter2 = new();
			parameter2.Setup(p => p.Name).Returns("U");

			Assert.True(new ITypeParameterSymbol[] { parameter1.Object, parameter2.Object }.GetGenericName() == "<T, U>");
		}

		[Fact]
		public void CanReturnSingleTypeParameter()
		{
			Mock<ITypeParameterSymbol> parameter = new();
			parameter.Setup(p => p.Name).Returns("T");

			Assert.True(new ITypeParameterSymbol[] { parameter.Object }.GetGenericName() == "<T>");
		}

		[Fact]
		public void ReturnsEmptyString_When_TypeParametersIsEmpty()
		{
			Assert.True(Array.Empty<ITypeParameterSymbol>().GetGenericName() == string.Empty);
		}

		[Fact]
		public void ThrowsArgumentNullException_When_TypeParametersIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => ((IEnumerable<ITypeParameterSymbol>)null!).GetGenericName());
		}
	}
}
