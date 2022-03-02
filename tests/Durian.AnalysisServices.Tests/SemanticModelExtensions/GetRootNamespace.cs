// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using System;
using Xunit;

namespace Durian.Analysis.Tests.SemanticModelExtensions
{
    public sealed class GetRootNamespace : CompilationTest
    {
        [Fact]
        public void ReturnsGlobalNamespace_When_IsInGlobalNamespace()
        {
            Assert.True(SymbolEqualityComparer.Default.Equals(
                Execute("class Test { }"),
                Compilation.CurrentCompilation.Assembly.GlobalNamespace
            ));
        }

        [Fact]
        public void ReturnsParentNamespace_When_HasParentNamespace()
        {
            Assert.True(Execute("namespace N { class Test { } }").Name == "N");
        }

        [Fact]
        public void ReturnsRootNamespace_When_IsInNestedNamespace()
        {
            Assert.True(Execute("namespace N1.N2 { class Test { } }").Name == "N1");
        }

        [Fact]
        public void ReturnsRootNamespace_When_IsInsideType()
        {
            Assert.True(Execute("namespace N1.N2 { class Parent { class Child { } } }", 1).Name == "N1");
        }

        [Fact]
        public void ThrowsArgumentException_When_GlobalNamespaceIsNotActuallyGlobal()
        {
            IMemberData member = GetClass("namespace N { class Test { } }")!;
            Assert.Throws<ArgumentException>(() => member.SemanticModel.GetContainingNamespaces(member.Declaration, member.Symbol.ContainingNamespace));
        }

        [Fact]
        public void ThrowsArgumentNullException_When_AssemblySymbolIsNull()
        {
            IMemberData member = GetClass("class Test { }")!;
            Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetContainingNamespaces(member.Declaration, assembly: null!));
        }

        [Fact]
        public void ThrowsArgumentNullException_When_CompilationDataIsNull()
        {
            IMemberData member = GetClass("class Test { }")!;
            Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetContainingNamespaces(member.Declaration, compilationData: null!));
        }

        [Fact]
        public void ThrowsArgumentNullException_When_CompilationIsNull()
        {
            IMemberData member = GetClass("class Test { }")!;
            Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetContainingNamespaces(member.Declaration, compilation: null!));
        }

        [Fact]
        public void ThrowsArgumentNullException_When_GlobalNamespaceIsNull()
        {
            IMemberData member = GetClass("class Test { }")!;
            Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetContainingNamespaces(member.Declaration, globalNamespace: null!));
        }

        [Fact]
        public void ThrowsArgumentNullException_When_SemanticModelIsNull()
        {
            IMemberData member = GetClass("class Test { }")!;
            SemanticModel? semanticModel = null;
            Assert.Throws<ArgumentNullException>(() => semanticModel!.GetContainingNamespaces(member.Declaration, Compilation));
        }

        [Fact]
        public void ThrowsArgumentNullException_When_SyntaxNodeIsNull()
        {
            IMemberData member = GetClass("class Test { }")!;
            Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetContainingNamespaces(null!, Compilation));
        }

        private INamespaceSymbol Execute(string src, int index = 0)
        {
            IMemberData member = GetClass(src, index)!;
            return member.SemanticModel.GetRootNamespace(member.Declaration, Compilation);
        }
    }
}