// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Xunit;

namespace Durian.Analysis.Tests.AnalysisUtilities
{
    public sealed class GetGenericName_IEnumerableString_String
    {
        [Fact]
        public void CanReturnGenericNameWithMultipleTypeParameters()
        {
            Assert.True(Analysis.AnalysisUtilities.GetGenericName(new string[] { "T", "U" }, "Test") == "Test<T, U>");
        }

        [Fact]
        public void CanReturnGenericNameWithSingleTypeParameter()
        {
            Assert.True(Analysis.AnalysisUtilities.GetGenericName(new string[] { "T" }, "Test") == "Test<T>");
        }

        [Fact]
        public void ReturnsOnlyGenericPart_When_NameIsNull()
        {
            string genericName = Analysis.AnalysisUtilities.GetGenericName(new string[] { "T" }, null);
            Assert.True(genericName.Length > 1 && genericName[0] == '<' && genericName[^1] == '>');
        }

        [Fact]
        public void ReturnsOnlyName_When_TypeParametersIsEmpty()
        {
            Assert.True(Analysis.AnalysisUtilities.GetGenericName(Array.Empty<string>(), "Test") == "Test");
        }

        [Fact]
        public void ThrowsArgumentNullException_When_TypeParametersIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => Analysis.AnalysisUtilities.GetGenericName(null!, "Test"));
        }
    }
}