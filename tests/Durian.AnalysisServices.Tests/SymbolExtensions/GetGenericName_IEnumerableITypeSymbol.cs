// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Moq;
using Xunit;

namespace Durian.Analysis.Tests.AnalysisUtilities
{
	public sealed class GetGenericName_IEnumerableITypeSymbol
	{
		[Fact]
		public void CanReturnGenericNameWithArrayArgument()
		{
			Mock<INamedTypeSymbol> element = new();
			element.Setup(e => e.Name).Returns("int");
			element.Setup(e => e.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			element.Setup(e => e.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			element.Setup(e => e.IsValueType).Returns(true);

			Mock<IArrayTypeSymbol> array = new();
			array.Setup(a => a.ElementType).Returns(element.Object);
			array.Setup(a => a.Rank).Returns(1);

			Assert.True(RunWithSymbol(array.Object, "int[]"));
		}

		[Fact]
		public void CanReturnGenericNameWithArrayOfArraysArgument()
		{
			Mock<INamedTypeSymbol> element = new();
			element.Setup(e => e.Name).Returns("int");
			element.Setup(e => e.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			element.Setup(e => e.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			element.Setup(e => e.IsValueType).Returns(true);

			Mock<IArrayTypeSymbol> childArray = new();
			childArray.Setup(a => a.ElementType).Returns(element.Object);
			childArray.Setup(a => a.Rank).Returns(1);

			Mock<IArrayTypeSymbol> array = new();
			array.Setup(a => a.ElementType).Returns(childArray.Object);
			array.Setup(a => a.Rank).Returns(1);

			Assert.True(RunWithSymbol(array.Object, "int[][]"));
		}

		[Fact]
		public void CanReturnGenericNameWithArrayOfArraysOfGenerics()
		{
			Mock<INamedTypeSymbol> arg = new();
			arg.Setup(a => a.Name).Returns("int");
			arg.Setup(a => a.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			arg.Setup(a => a.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			arg.Setup(a => a.IsValueType).Returns(true);

			Mock<ITypeParameterSymbol> param = new();
			param.Setup(p => p.Name).Returns("T");

			Mock<INamedTypeSymbol> type = new();
			type.Setup(t => t.Name).Returns("List");
			type.Setup(t => t.TypeParameters).Returns(ImmutableArray.Create(param.Object));
			type.Setup(t => t.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>(arg.Object));

			Mock<IArrayTypeSymbol> childArray = new();
			childArray.Setup(a => a.ElementType).Returns(type.Object);
			childArray.Setup(a => a.Rank).Returns(1);

			Mock<IArrayTypeSymbol> array = new();
			array.Setup(a => a.ElementType).Returns(childArray.Object);
			array.Setup(a => a.Rank).Returns(1);

			Assert.True(RunWithSymbol(array.Object, "List<int>[][]"));
		}

		[Fact]
		public void CanReturnGenericNameWithArrayOfGenerics()
		{
			Mock<INamedTypeSymbol> arg = new();
			arg.Setup(a => a.Name).Returns("int");
			arg.Setup(a => a.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			arg.Setup(a => a.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			arg.Setup(a => a.IsValueType).Returns(true);

			Mock<ITypeParameterSymbol> param = new();
			param.Setup(p => p.Name).Returns("T");

			Mock<INamedTypeSymbol> type = new();
			type.Setup(t => t.Name).Returns("List");
			type.Setup(t => t.TypeParameters).Returns(ImmutableArray.Create(param.Object));
			type.Setup(t => t.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>(arg.Object));

			Mock<IArrayTypeSymbol> array = new();
			array.Setup(a => a.ElementType).Returns(type.Object);
			array.Setup(a => a.Rank).Returns(1);

			Assert.True(RunWithSymbol(array.Object, "List<int>[]"));
		}

		[Fact]
		public void CanReturnGenericNameWithArrayOfMultidimensionalArrays()
		{
			Mock<INamedTypeSymbol> element = new();
			element.Setup(e => e.Name).Returns("int");
			element.Setup(e => e.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			element.Setup(e => e.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			element.Setup(e => e.IsValueType).Returns(true);

			Mock<IArrayTypeSymbol> childArray = new();
			childArray.Setup(a => a.ElementType).Returns(element.Object);
			childArray.Setup(a => a.Rank).Returns(2);

			Mock<IArrayTypeSymbol> array = new();
			array.Setup(a => a.ElementType).Returns(childArray.Object);
			array.Setup(a => a.Rank).Returns(1);

			Assert.True(RunWithSymbol(array.Object, "int[][,]"));
		}

		[Fact]
		public void CanReturnGenericNameWithArrayOfMultidimensionalArraysOfArrays()
		{
			Mock<INamedTypeSymbol> element = new();
			element.Setup(e => e.Name).Returns("int");
			element.Setup(e => e.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			element.Setup(e => e.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			element.Setup(e => e.IsValueType).Returns(true);

			Mock<IArrayTypeSymbol> lowestArray = new();
			lowestArray.Setup(a => a.ElementType).Returns(element.Object);
			lowestArray.Setup(a => a.Rank).Returns(1);

			Mock<IArrayTypeSymbol> childArray = new();
			childArray.Setup(a => a.ElementType).Returns(lowestArray.Object);
			childArray.Setup(a => a.Rank).Returns(2);

			Mock<IArrayTypeSymbol> array = new();
			array.Setup(a => a.ElementType).Returns(childArray.Object);
			array.Setup(a => a.Rank).Returns(1);

			Assert.True(RunWithSymbol(array.Object, "int[][,][]"));
		}

		[Fact]
		public void CanReturnGenericNameWithArrayOfMultidimensionalArraysOfMultidimensionalArrays()
		{
			Mock<INamedTypeSymbol> element = new();
			element.Setup(e => e.Name).Returns("int");
			element.Setup(e => e.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			element.Setup(e => e.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			element.Setup(e => e.IsValueType).Returns(true);

			Mock<IArrayTypeSymbol> lowestArray = new();
			lowestArray.Setup(a => a.ElementType).Returns(element.Object);
			lowestArray.Setup(a => a.Rank).Returns(2);

			Mock<IArrayTypeSymbol> childArray = new();
			childArray.Setup(a => a.ElementType).Returns(lowestArray.Object);
			childArray.Setup(a => a.Rank).Returns(3);

			Mock<IArrayTypeSymbol> array = new();
			array.Setup(a => a.ElementType).Returns(childArray.Object);
			array.Setup(a => a.Rank).Returns(1);

			Assert.True(RunWithSymbol(array.Object, "int[][,,][,]"));
		}

		[Fact]
		public void CanReturnGenericNameWithArrayOfNullableArrays()
		{
			Mock<INamedTypeSymbol> element = new();
			element.Setup(e => e.Name).Returns("int");
			element.Setup(e => e.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			element.Setup(e => e.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			element.Setup(e => e.IsValueType).Returns(true);

			Mock<IArrayTypeSymbol> childArray = new();
			childArray.Setup(a => a.ElementType).Returns(element.Object);
			childArray.Setup(a => a.NullableAnnotation).Returns(NullableAnnotation.Annotated);
			childArray.Setup(a => a.Rank).Returns(1);

			Mock<IArrayTypeSymbol> array = new();
			array.Setup(a => a.ElementType).Returns(childArray.Object);
			array.Setup(a => a.Rank).Returns(1);

			Assert.True(RunWithSymbol(array.Object, "int[]?[]"));
		}

		[Fact]
		public void CanReturnGenericNameWithArrayOfNullables()
		{
			Mock<INamedTypeSymbol> element = new();
			element.Setup(e => e.Name).Returns("int");
			element.Setup(e => e.NullableAnnotation).Returns(NullableAnnotation.Annotated);
			element.Setup(e => e.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			element.Setup(e => e.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			element.Setup(e => e.IsValueType).Returns(true);

			Mock<IArrayTypeSymbol> array = new();
			array.Setup(a => a.ElementType).Returns(element.Object);
			array.Setup(a => a.Rank).Returns(1);

			Assert.True(RunWithSymbol(array.Object, "int?[]"));
		}

		[Fact]
		public void CanReturnGenericNameWithDynamicArgument()
		{
			Mock<IDynamicTypeSymbol> type = new();
			type.Setup(t => t.Name).Returns("dynamic");

			Assert.True(RunWithSymbol(type.Object, "dynamic"));
		}

		[Fact]
		public void CanReturnGenericNameWithGenericArgument()
		{
			Mock<INamedTypeSymbol> arg = new();
			arg.Setup(a => a.Name).Returns("int");
			arg.Setup(a => a.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			arg.Setup(a => a.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			arg.Setup(a => a.IsValueType).Returns(true);

			Mock<ITypeParameterSymbol> param = new();
			param.Setup(p => p.Name).Returns("T");

			Mock<INamedTypeSymbol> type = new();
			type.Setup(t => t.Name).Returns("List");
			type.Setup(t => t.TypeParameters).Returns(ImmutableArray.Create(param.Object));
			type.Setup(t => t.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>(arg.Object));

			Assert.True(RunWithSymbol(type.Object, "List<int>"));
		}

		[Fact]
		public void CanReturnGenericNameWithJaggedArrayOfNullables()
		{
			Mock<INamedTypeSymbol> element = new();
			element.Setup(e => e.Name).Returns("int");
			element.Setup(e => e.NullableAnnotation).Returns(NullableAnnotation.Annotated);
			element.Setup(e => e.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			element.Setup(e => e.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			element.Setup(e => e.IsValueType).Returns(true);

			Mock<IArrayTypeSymbol> childArray = new();
			childArray.Setup(a => a.ElementType).Returns(element.Object);
			childArray.Setup(a => a.Rank).Returns(1);

			Mock<IArrayTypeSymbol> array = new();
			array.Setup(a => a.ElementType).Returns(childArray.Object);
			array.Setup(a => a.Rank).Returns(1);

			Assert.True(RunWithSymbol(array.Object, "int?[][]"));
		}

		[Fact]
		public void CanReturnGenericNameWithMultidimensionalArrayOfMultidimensionalArraysOfArrays()
		{
			Mock<INamedTypeSymbol> element = new();
			element.Setup(e => e.Name).Returns("int");
			element.Setup(e => e.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			element.Setup(e => e.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			element.Setup(e => e.IsValueType).Returns(true);

			Mock<IArrayTypeSymbol> lowestArray = new();
			lowestArray.Setup(a => a.ElementType).Returns(element.Object);
			lowestArray.Setup(a => a.Rank).Returns(1);

			Mock<IArrayTypeSymbol> childArray = new();
			childArray.Setup(a => a.ElementType).Returns(lowestArray.Object);
			childArray.Setup(a => a.Rank).Returns(3);

			Mock<IArrayTypeSymbol> array = new();
			array.Setup(a => a.ElementType).Returns(childArray.Object);
			array.Setup(a => a.Rank).Returns(2);

			Assert.True(RunWithSymbol(array.Object, "int[,][,,][]"));
		}

		[Fact]
		public void CanReturnGenericNameWithMultimensionalArray()
		{
			Mock<INamedTypeSymbol> element = new();
			element.Setup(e => e.Name).Returns("int");
			element.Setup(e => e.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			element.Setup(e => e.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			element.Setup(e => e.IsValueType).Returns(true);

			Mock<IArrayTypeSymbol> array = new();
			array.Setup(a => a.ElementType).Returns(element.Object);
			array.Setup(a => a.Rank).Returns(2);

			Assert.True(RunWithSymbol(array.Object, "int[,]"));
		}

		[Fact]
		public void CanReturnGenericNameWithMultimensionalArray_When_RankIsGreaterThan2()
		{
			Mock<INamedTypeSymbol> element = new();
			element.Setup(e => e.Name).Returns("int");
			element.Setup(e => e.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			element.Setup(e => e.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			element.Setup(e => e.IsValueType).Returns(true);

			Mock<IArrayTypeSymbol> array = new();
			array.Setup(a => a.ElementType).Returns(element.Object);
			array.Setup(a => a.Rank).Returns(3);

			Assert.True(RunWithSymbol(array.Object, "int[,,]"));
		}

		[Fact]
		public void CanReturnGenericNameWithMultimensionalArrayOfArrays()
		{
			Mock<INamedTypeSymbol> element = new();
			element.Setup(e => e.Name).Returns("int");
			element.Setup(e => e.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			element.Setup(e => e.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			element.Setup(e => e.IsValueType).Returns(true);

			Mock<IArrayTypeSymbol> childArray = new();
			childArray.Setup(a => a.ElementType).Returns(element.Object);
			childArray.Setup(a => a.Rank).Returns(1);
			Mock<IArrayTypeSymbol> array = new();
			array.Setup(a => a.ElementType).Returns(childArray.Object);
			array.Setup(a => a.Rank).Returns(2);

			Assert.True(RunWithSymbol(array.Object, "int[,][]"));
		}

		[Fact]
		public void CanReturnGenericNameWithMultipleTypeArguments()
		{
			Mock<INamedTypeSymbol> arg1 = new();
			arg1.Setup(p => p.Name).Returns("int");
			arg1.Setup(p => p.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			arg1.Setup(p => p.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			arg1.Setup(a => a.IsValueType).Returns(true);

			Mock<INamedTypeSymbol> arg2 = new();
			arg2.Setup(p => p.Name).Returns("string");
			arg2.Setup(p => p.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			arg2.Setup(p => p.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());

			Assert.True(new ITypeSymbol[] { arg1.Object, arg2.Object }.GetGenericName() == "<int, string>");
		}

		[Fact]
		public void CanReturnGenericNameWithMutlidimensionalArrayOfArraysOfMultidimensionalArrays()
		{
			Mock<INamedTypeSymbol> element = new();
			element.Setup(e => e.Name).Returns("int");
			element.Setup(e => e.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			element.Setup(e => e.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			element.Setup(e => e.IsValueType).Returns(true);

			Mock<IArrayTypeSymbol> lowestArray = new();
			lowestArray.Setup(a => a.ElementType).Returns(element.Object);
			lowestArray.Setup(a => a.Rank).Returns(3);

			Mock<IArrayTypeSymbol> childArray = new();
			childArray.Setup(a => a.ElementType).Returns(lowestArray.Object);
			childArray.Setup(a => a.Rank).Returns(1);

			Mock<IArrayTypeSymbol> array = new();
			array.Setup(a => a.ElementType).Returns(childArray.Object);
			array.Setup(a => a.Rank).Returns(2);

			Assert.True(RunWithSymbol(array.Object, "int[,][][,,]"));
		}

		[Fact]
		public void CanReturnGenericNameWithNullableArgument()
		{
			Mock<INamedTypeSymbol> type = new();
			type.Setup(t => t.Name).Returns("int");
			type.Setup(t => t.NullableAnnotation).Returns(NullableAnnotation.Annotated);
			type.Setup(t => t.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			type.Setup(t => t.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			type.Setup(t => t.IsValueType).Returns(true);

			Assert.True(RunWithSymbol(type.Object, "int?"));
		}

		[Fact]
		public void CanReturnGenericNameWithNullableArray()
		{
			Mock<INamedTypeSymbol> element = new();
			element.Setup(e => e.Name).Returns("int");
			element.Setup(e => e.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			element.Setup(e => e.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			element.Setup(e => e.IsValueType).Returns(true);

			Mock<IArrayTypeSymbol> array = new();
			array.Setup(a => a.ElementType).Returns(element.Object);
			array.Setup(a => a.NullableAnnotation).Returns(NullableAnnotation.Annotated);
			array.Setup(a => a.Rank).Returns(1);

			Assert.True(RunWithSymbol(array.Object, "int[]?"));
		}

		[Fact]
		public void CanReturnGenericNameWithNullableArrayOfArrays()
		{
			Mock<INamedTypeSymbol> element = new();
			element.Setup(e => e.Name).Returns("int");
			element.Setup(e => e.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			element.Setup(e => e.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			element.Setup(e => e.IsValueType).Returns(true);

			Mock<IArrayTypeSymbol> childArray = new();
			childArray.Setup(a => a.ElementType).Returns(element.Object);
			childArray.Setup(a => a.Rank).Returns(1);

			Mock<IArrayTypeSymbol> array = new();
			array.Setup(a => a.ElementType).Returns(childArray.Object);
			array.Setup(a => a.NullableAnnotation).Returns(NullableAnnotation.Annotated);
			array.Setup(a => a.Rank).Returns(1);

			Assert.True(RunWithSymbol(array.Object, "int[][]?"));
		}

		[Fact]
		public void CanReturnGenericNameWithNullableArrayOfNullableArrays()
		{
			Mock<INamedTypeSymbol> element = new();
			element.Setup(e => e.Name).Returns("int");
			element.Setup(e => e.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			element.Setup(e => e.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			element.Setup(e => e.IsValueType).Returns(true);

			Mock<IArrayTypeSymbol> childArray = new();
			childArray.Setup(a => a.ElementType).Returns(element.Object);
			childArray.Setup(a => a.NullableAnnotation).Returns(NullableAnnotation.Annotated);
			childArray.Setup(a => a.Rank).Returns(1);

			Mock<IArrayTypeSymbol> array = new();
			array.Setup(a => a.ElementType).Returns(childArray.Object);
			array.Setup(a => a.NullableAnnotation).Returns(NullableAnnotation.Annotated);
			array.Setup(a => a.Rank).Returns(1);

			Assert.True(RunWithSymbol(array.Object, "int[]?[]?"));
		}

		[Fact]
		public void CanReturnGenericNameWithNullableArrayOfNullableArraysOfNullables()
		{
			Mock<INamedTypeSymbol> element = new();
			element.Setup(e => e.Name).Returns("int");
			element.Setup(e => e.NullableAnnotation).Returns(NullableAnnotation.Annotated);
			element.Setup(e => e.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			element.Setup(e => e.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			element.Setup(e => e.IsValueType).Returns(true);

			Mock<IArrayTypeSymbol> childArray = new();
			childArray.Setup(a => a.ElementType).Returns(element.Object);
			childArray.Setup(a => a.NullableAnnotation).Returns(NullableAnnotation.Annotated);
			childArray.Setup(a => a.Rank).Returns(1);

			Mock<IArrayTypeSymbol> array = new();
			array.Setup(a => a.ElementType).Returns(childArray.Object);
			array.Setup(a => a.NullableAnnotation).Returns(NullableAnnotation.Annotated);
			array.Setup(a => a.Rank).Returns(1);

			Assert.True(RunWithSymbol(array.Object, "int?[]?[]?"));
		}

		[Fact]
		public void CanReturnGenericNameWithOneTypeArgument()
		{
			Mock<INamedTypeSymbol> type = new();
			type.Setup(t => t.Name).Returns("int");
			type.Setup(t => t.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			type.Setup(t => t.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			type.Setup(t => t.IsValueType).Returns(true);

			Assert.True(RunWithSymbol(type.Object, "int"));
		}

		[Fact]
		public void CanReturnGenericNamwWithMultidimensionalArrayOfNullables()
		{
			Mock<INamedTypeSymbol> element = new();
			element.Setup(e => e.Name).Returns("int");
			element.Setup(e => e.NullableAnnotation).Returns(NullableAnnotation.Annotated);
			element.Setup(e => e.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			element.Setup(e => e.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			element.Setup(e => e.IsValueType).Returns(true);

			Mock<IArrayTypeSymbol> array = new();
			array.Setup(a => a.ElementType).Returns(element.Object);
			array.Setup(a => a.Rank).Returns(2);

			Assert.True(RunWithSymbol(array.Object, "int?[,]"));
		}

		[Fact]
		public void ReplacesDotNetNullableTypeWithQuestionMark()
		{
			Mock<INamedTypeSymbol> arg = new();
			arg.Setup(a => a.Name).Returns("int");
			arg.Setup(a => a.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			arg.Setup(a => a.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			arg.Setup(a => a.IsValueType).Returns(true);

			Mock<ITypeParameterSymbol> param = new();
			param.Setup(p => p.Name).Returns("T");

			Mock<INamedTypeSymbol> type = new();
			type.Setup(t => t.Name).Returns("Nullable");
			type.Setup(t => t.TypeParameters).Returns(ImmutableArray.Create(param.Object));
			type.Setup(t => t.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>(arg.Object));
			type.Setup(t => t.IsValueType).Returns(true);

			Assert.True(RunWithSymbol(type.Object, "int?"));
		}

		[Fact]
		public void ReplacesDotNetPrimitiveWithCSharpKeyword()
		{
			Mock<INamedTypeSymbol> arg = new();
			arg.Setup(a => a.Name).Returns("Int32");
			arg.Setup(a => a.TypeParameters).Returns(ImmutableArray.Create<ITypeParameterSymbol>());
			arg.Setup(a => a.TypeArguments).Returns(ImmutableArray.Create<ITypeSymbol>());
			arg.Setup(a => a.IsValueType).Returns(true);

			Assert.True(RunWithSymbol(arg.Object, "int"));
		}

		[Fact]
		public void ReturnsEmptyString_When_TypeArgumentsIsEmpty()
		{
			Assert.True(Array.Empty<ITypeSymbol>().GetGenericName() == string.Empty);
		}

		[Fact]
		public void ReturnsParameterGenericName_When_TypeArgumentsIsIEnumerableITypeParameterSymbol()
		{
			Mock<ITypeParameterSymbol> parameter = new();
			parameter.Setup(p => p.Name).Returns("T");

			ITypeParameterSymbol[] parameters = { parameter.Object };
			Assert.True(((ITypeSymbol[])parameters).GetGenericName() == "<T>");
		}

		[Fact]
		public void ThrowsArgumentNullException_When_TypeArgumentsIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => ((IEnumerable<ITypeSymbol>)null!).GetGenericName());
		}

		private static bool RunWithSymbol(ITypeSymbol symbol, string expectedArgName)
		{
			return new ITypeSymbol[] { symbol }.GetGenericName() == $"<{expectedArgName}>";
		}
	}
}