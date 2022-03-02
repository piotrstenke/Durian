// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Extensions;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using Xunit;

namespace Durian.Analysis.Tests.SymbolExtensions
{
    public sealed class GetModifiers_IEnumerableTypeDeclarationSyntax : CompilationTest
    {
        [Fact]
        public void CanReturnMultipleModifiers()
        {
            StructDeclarationSyntax decl = GetNode<StructDeclarationSyntax>("class Parent { protected internal unsafe readonly ref partial struct Test { }}")!;
            SyntaxToken[] tokens = new TypeDeclarationSyntax[] { decl }.GetModifiers().ToArray();

            Assert.True(
                tokens.Any(t => t.IsKind(SyntaxKind.ProtectedKeyword)) &&
                tokens.Any(t => t.IsKind(SyntaxKind.InternalKeyword)) &&
                tokens.Any(t => t.IsKind(SyntaxKind.UnsafeKeyword)) &&
                tokens.Any(t => t.IsKind(SyntaxKind.ReadOnlyKeyword)) &&
                tokens.Any(t => t.IsKind(SyntaxKind.RefKeyword)) &&
                tokens.Any(t => t.IsKind(SyntaxKind.PartialKeyword))
            );
        }

        [Fact]
        public void CanReturnSingleModifier()
        {
            ClassDeclarationSyntax decl = GetNode<ClassDeclarationSyntax>("internal class Test { }")!;
            SyntaxToken[] tokens = new TypeDeclarationSyntax[] { decl }.GetModifiers().ToArray();

            Assert.True(tokens.Length == 1 && tokens[0].IsKind(SyntaxKind.InternalKeyword));
        }

        [Fact]
        public void DoesNotReturnIdenticalModifiers()
        {
            ClassDeclarationSyntax decl1 = GetNode<ClassDeclarationSyntax>("public sealed partial class Test { }")!;
            ClassDeclarationSyntax decl2 = GetNode<ClassDeclarationSyntax>("public sealed partial class Test { }")!;
            SyntaxToken[] tokens = new TypeDeclarationSyntax[] { decl1, decl2 }.GetModifiers().ToArray();

            Assert.True(
                tokens.Length == 3 &&
                tokens.Any(t => t.IsKind(SyntaxKind.PublicKeyword)) &&
                tokens.Any(t => t.IsKind(SyntaxKind.SealedKeyword)) &&
                tokens.Any(t => t.IsKind(SyntaxKind.PartialKeyword))
            );
        }

        [Fact]
        public void ReturnsAllModifiers_When_IsPartial()
        {
            ClassDeclarationSyntax decl1 = GetNode<ClassDeclarationSyntax>("public partial class Test { }")!;
            ClassDeclarationSyntax decl2 = GetNode<ClassDeclarationSyntax>("partial sealed class Test { }")!;
            SyntaxToken[] tokens = new TypeDeclarationSyntax[] { decl1, decl2 }.GetModifiers().ToArray();

            Assert.True(
                tokens.Any(t => t.IsKind(SyntaxKind.PublicKeyword)) &&
                tokens.Any(t => t.IsKind(SyntaxKind.SealedKeyword)) &&
                tokens.Any(t => t.IsKind(SyntaxKind.PartialKeyword))
            );
        }

        [Fact]
        public void ReturnsEmpty_When_DeclHasNoModifiers()
        {
            ClassDeclarationSyntax decl = GetNode<ClassDeclarationSyntax>("class Test { }")!;
            SyntaxToken[] tokens = new TypeDeclarationSyntax[] { decl }.GetModifiers().ToArray();
            Assert.True(tokens.Length == 0);
        }

        [Fact]
        public void ThrowsArgumentNullException_When_DeclIsNull()
        {
            INamedTypeSymbol symbol = null!;
            Assert.Throws<ArgumentNullException>(() => symbol.GetModifiers());
        }
    }
}