// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using Durian.Generator.Data;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Tests.AnalysisServices.SyntaxNodeExtensions
{
	public sealed class GetMemberData : CompilationTest
	{
		[Fact]
		public void ReturnsValidClassData_When_IsClass()
		{
			Assert.True(ValidateMember<ClassData, ClassDeclarationSyntax>("class Test { }"));
		}

		[Fact]
		public void ReturnsValidDelegateData_When_IsDelegate()
		{
			Assert.True(ValidateMember<DelegateData, DelegateDeclarationSyntax>("delegate void Test(int a);"));
		}

		[Fact]
		public void ReturnsValidEventData_When_HasMultipleEventFieldsInSingleDeclaration()
		{
			EventFieldDeclarationSyntax e = GetNode<EventFieldDeclarationSyntax>("class Test { event System.Action OnInit, OnExit; }");
			SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(e.SyntaxTree);
			IEventSymbol symbol1 = (semanticModel.GetDeclaredSymbol(e.Declaration.Variables[0]) as IEventSymbol)!;
			IEventSymbol symbol2 = (semanticModel.GetDeclaredSymbol(e.Declaration.Variables[1]) as IEventSymbol)!;

			Generator.Data.EventData? data = e.GetMemberData(Compilation) as Generator.Data.EventData;
			Generator.Data.EventData[]? declaredEvents = data?.GetUnderlayingEvents().ToArray();

			Assert.True(ValidateMember(data, e, symbol1) && declaredEvents!.Length == 2 && ValidateMember(declaredEvents[1], e, symbol2));
		}

		[Fact]
		public void ReturnsValidEventData_When_IsEventProperty()
		{
			Assert.True(ValidateMember<Generator.Data.EventData, EventDeclarationSyntax>("class Test { event System.Action OnInit {add {} remove{} } }"));
		}

		[Fact]
		public void ReturnsValidEventData_When_IsSingleEventField()
		{
			EventFieldDeclarationSyntax e = GetNode<EventFieldDeclarationSyntax>("class Test { event System.Action OnInit; }");
			SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(e.SyntaxTree);
			IEventSymbol symbol = (semanticModel.GetDeclaredSymbol(e.Declaration.Variables[0]) as IEventSymbol)!;
			Generator.Data.EventData? data = e.GetMemberData(Compilation) as Generator.Data.EventData;

			Assert.True(ValidateMember(data, e, symbol) && data!.GetUnderlayingEvents().Count() == 1);
		}

		[Fact]
		public void ReturnsValidFieldData_When_HasMultipleFieldsInSingleDeclaration()
		{
			FieldDeclarationSyntax field = GetNode<FieldDeclarationSyntax>("class Test { int Age = 0, Index = 2 }");
			SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(field.SyntaxTree);
			IFieldSymbol symbol1 = (semanticModel.GetDeclaredSymbol(field.Declaration.Variables[0]) as IFieldSymbol)!;
			IFieldSymbol symbol2 = (semanticModel.GetDeclaredSymbol(field.Declaration.Variables[1]) as IFieldSymbol)!;

			Generator.Data.FieldData? data = field.GetMemberData(Compilation) as Generator.Data.FieldData;
			Generator.Data.FieldData[]? declaredFields = data?.GetUnderlayingFields().ToArray();

			Assert.True(ValidateMember(data, field, symbol1) && declaredFields!.Length == 2 && ValidateMember(declaredFields[1], field, symbol2));
		}

		[Fact]
		public void ReturnsValidFieldData_When_IsSingleField()
		{
			FieldDeclarationSyntax field = GetNode<FieldDeclarationSyntax>("class Test { int Age = 0; }");
			SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(field.SyntaxTree);
			IFieldSymbol symbol = (semanticModel.GetDeclaredSymbol(field.Declaration.Variables[0]) as IFieldSymbol)!;
			Generator.Data.FieldData? data = field.GetMemberData(Compilation) as Generator.Data.FieldData;

			Assert.True(ValidateMember(data, field, symbol) && data!.GetUnderlayingFields().Count() == 1);
		}

		[Fact]
		public void ReturnsValidInterfaceData_When_IsInterface()
		{
			Assert.True(ValidateMember<InterfaceData, InterfaceDeclarationSyntax>("interface Test { }"));
		}

		[Fact]
		public void ReturnsValidMemberData_When_IsUnknownMemberType()
		{
			Assert.True(ValidateMember<Generator.Data.MemberData, MemberDeclarationSyntax>("class Test { int this[int index] { get => 0 }}"));
		}

		[Fact]
		public void ReturnsValidMethodData_When_IsMethod()
		{
			Assert.True(ValidateMember<Generator.Data.MethodData, MethodDeclarationSyntax>("class Test { void Method() { } }"));
		}

		[Fact]
		public void ReturnsValidPropertyData_When_IsProperty()
		{
			Assert.True(ValidateMember<PropertyData, PropertyDeclarationSyntax>("class Test { string Name {get;set;}}"));
		}

		[Fact]
		public void ReturnsValidRecordData_When_IsRecord()
		{
			Assert.True(ValidateMember<RecordData, RecordDeclarationSyntax>("record Test(string name);"));
		}

		[Fact]
		public void ReturnsValidStructData_When_IsStruct()
		{
			Assert.True(ValidateMember<StructData, StructDeclarationSyntax>("struct Test { }"));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_CompilationIsNull()
		{
			MemberDeclarationSyntax member = GetNode<ClassDeclarationSyntax>("class Test { }");
			Assert.Throws<ArgumentNullException>(() => member.GetMemberData(null!));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_MemberIsNull()
		{
			MemberDeclarationSyntax member = null!;
			Assert.Throws<ArgumentNullException>(() => member.GetMemberData(Compilation));
		}

		private bool ValidateMember<TData, TDeclaration>(string src)
			where TData : class, IMemberData
			where TDeclaration : MemberDeclarationSyntax
		{
			TDeclaration decl = GetNode<TDeclaration>(src);
			TData? data = decl.GetMemberData(Compilation) as TData;
			SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(decl.SyntaxTree, true);
			ISymbol symbol = semanticModel.GetDeclaredSymbol(decl)!;

			return ValidateMember(data, decl, symbol);
		}

		private bool ValidateMember(IMemberData? member, CSharpSyntaxNode node, ISymbol symbol)
		{
			return
				member is not null &&
				member.ParentCompilation == Compilation &&
				SymbolEqualityComparer.Default.Equals(member.Symbol, symbol) &&
				member.Declaration.IsEquivalentTo(node);
		}
	}
}
