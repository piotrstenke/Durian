// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Analysis.Extensions;
using Durian.TestServices;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Analysis.Tests.SyntaxNodeExtensions
{
	public sealed class GetTypeParameterList : CompilationTest
	{
		[Fact]
		public void ReturnsList_When_IsDelegate()
		{
			DelegateDeclarationSyntax del = GetNode<DelegateDeclarationSyntax>("delegate void Test<T>();")!;
			Assert.True(del.GetTypeParameterList() == del.TypeParameterList);
		}

		[Fact]
		public void ReturnsList_When_IsMethod()
		{
			MethodDeclarationSyntax method = GetNode<MethodDeclarationSyntax>("class Test { void Method<T>() { } }")!;
			Assert.True(method.GetTypeParameterList() == method.TypeParameterList);
		}

		[Fact]
		public void ReturnsList_When_IsType()
		{
			ClassDeclarationSyntax type = GetNode<ClassDeclarationSyntax>("class Test<T> { }")!;
			Assert.True(type.GetTypeParameterList() == type.TypeParameterList);
		}

		[Fact]
		public void ReturnsNull_When_DoesNotSupportTypeParameters()
		{
			MemberDeclarationSyntax member = GetNode<PropertyDeclarationSyntax>("class Test { string Name { get; set; } }")!;
			Assert.True(member.GetTypeParameterList() is null);
		}
	}
}
