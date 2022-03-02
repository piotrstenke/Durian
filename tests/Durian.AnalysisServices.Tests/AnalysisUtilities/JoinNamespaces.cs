// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Xunit;

namespace Durian.Analysis.Tests.AnalysisUtilities
{
    public sealed class JoinNamespaces
    {
        [Fact]
        public void ReturnsEmptyString_When_NamespacesIsEmpty()
        {
            Assert.True(Analysis.AnalysisUtilities.JoinNamespaces(Array.Empty<string>()) == string.Empty);
        }

        [Fact]
        public void ReturnsMultipleNamespaces_When_NamespacesContainsMultipleElements()
        {
            Assert.True(Analysis.AnalysisUtilities.JoinNamespaces(new string[] { "System", "Collections", "Generic" }) == "System.Collections.Generic");
        }

        [Fact]
        public void ReturnsSingleNamespace_When_NamespacesContainsOneElement()
        {
            Assert.True(Analysis.AnalysisUtilities.JoinNamespaces(new string[] { "System" }) == "System");
        }

        [Fact]
        public void ThrowsArgumentNullException_When_NamespacesIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => Analysis.AnalysisUtilities.JoinNamespaces(null!));
        }
    }
}