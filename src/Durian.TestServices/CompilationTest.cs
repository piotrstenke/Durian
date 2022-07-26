// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Durian.Analysis;
using Durian.Analysis.Data;
using Durian.Analysis.Data.FromSource;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.TestServices
{
	/// <summary>
	/// An abstract class that provides methods that retrieve <see cref="IMemberData"/>of various types, <see cref="ISymbol"/>s or <see cref="CSharpSyntaxNode"/>s directly from an input <see cref="string"/>. Useful when unit testing a <see cref="ISourceGenerator"/>.
	/// </summary>
	public abstract class CompilationTest
	{
		/// <summary>
		/// A <see cref="TestableCompilationData"/> that can be used during the test.
		/// </summary>
		public TestableCompilationData Compilation { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CompilationTest"/> class.
		/// </summary>
		protected CompilationTest()
		{
			Compilation = TestableCompilationData.Create();
			AddInitialSources();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CompilationTest"/> class.
		/// </summary>
		/// <param name="sources">An array of <see cref="string"/>s to be used as initial sources of <see cref="CSharpSyntaxTree"/>s for the <see cref="Compilation"/>.</param>
		protected CompilationTest(params string[]? sources)
		{
			Compilation = TestableCompilationData.Create(sources);
			AddInitialSources();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CompilationTest"/> class.
		/// </summary>
		/// <param name="compilation">An instance of <see cref="TestableCompilationData"/> to share between all tests in this class.</param>
		/// <param name="addInitialSources">Determines whether to add sources created using the <see cref="GetInitialSources()"/> method to the <paramref name="compilation"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		protected CompilationTest(TestableCompilationData compilation, bool addInitialSources = true)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			Compilation = compilation;

			if (addInitialSources)
			{
				AddInitialSources();
			}
		}

		/// <summary>
		/// Adds sources created by the <see cref="GetInitialSources()"/> method to the target <paramref name="compilation"/>.
		/// </summary>
		protected void AddInitialSources(ref CSharpCompilation compilation)
		{
			IEnumerable<ISourceTextProvider>? sourceTexts = GetInitialSources();

			if (sourceTexts is not null)
			{
				IEnumerable<CSharpSyntaxTree> syntaxTrees = sourceTexts.Select(text => (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(text.GetText(), encoding: Encoding.UTF8));

				compilation = compilation.AddSyntaxTrees(syntaxTrees);
			}
		}

		/// <summary>
		/// Creates a new <see cref="ClassData"/> from the specified <paramref name="source"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="index">Index at which the <see cref="ClassData"/> should be returned. Can be thought of as a number of <see cref="ClassDeclarationSyntax"/>es to skip before creating a valid <see cref="IMemberData"/>.</param>
		/// <returns>
		/// A new <see cref="ClassData"/> created from a <see cref="ClassDeclarationSyntax"/> found at the specified index in the parsed <see cref="CSharpSyntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="ClassDeclarationSyntax"/> exists.
		/// </returns>
		protected ClassData? GetClass(string? source, int index = 0)
		{
			return Compilation.GetMemberData<ClassDeclarationSyntax>(source, index) as ClassData;
		}

		/// <summary>
		/// Creates a new <see cref="ConstructorData"/> from the specified <paramref name="source"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="index">Index at which the <see cref="ConstructorData"/> should be returned. Can be thought of as a number of <see cref="ConstructorDeclarationSyntax"/>es to skip before creating a valid <see cref="IMemberData"/>.</param>
		/// <returns>
		/// A new <see cref="ConstructorData"/> created from a <see cref="ConstructorDeclarationSyntax"/> found at the specified index in the parsed <see cref="CSharpSyntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="ConstructorDeclarationSyntax"/> exists.
		/// </returns>
		protected ConstructorData? GetConstructor(string? source, int index = 0)
		{
			return Compilation.GetMemberData<ConstructorDeclarationSyntax>(source, index) as ConstructorData;
		}

		/// <summary>
		/// Creates a new <see cref="ConversionOperatorData"/> from the specified <paramref name="source"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="index">Index at which the <see cref="ConversionOperatorData"/> should be returned. Can be thought of as a number of <see cref="ConversionOperatorDeclarationSyntax"/>es to skip before creating a valid <see cref="IMemberData"/>.</param>
		/// <returns>
		/// A new <see cref="ConversionOperatorData"/> created from a <see cref="ConversionOperatorDeclarationSyntax"/> found at the specified index in the parsed <see cref="CSharpSyntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="ConversionOperatorDeclarationSyntax"/> exists.
		/// </returns>
		protected ConversionOperatorData? GetConversionOperator(string? source, int index = 0)
		{
			return Compilation.GetMemberData<ConversionOperatorDeclarationSyntax>(source, index) as ConversionOperatorData;
		}

		/// <summary>
		/// Creates a new <see cref="DelegateData"/> from the specified <paramref name="source"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="index">Index at which the <see cref="DelegateData"/> should be returned. Can be thought of as a number of <see cref="DelegateDeclarationSyntax"/>es to skip before creating a valid <see cref="IMemberData"/>.</param>
		/// <returns>
		/// A new <see cref="DelegateData"/> created from a <see cref="DelegateDeclarationSyntax"/> found at the specified index in the parsed <see cref="CSharpSyntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="DelegateDeclarationSyntax"/> exists.
		/// </returns>
		protected DelegateData? GetDelegate(string? source, int index = 0)
		{
			return Compilation.GetMemberData<DelegateDeclarationSyntax>(source, index) as DelegateData;
		}

		/// <summary>
		/// Creates a new <see cref="DestructorData"/> from the specified <paramref name="source"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="index">Index at which the <see cref="DestructorData"/> should be returned. Can be thought of as a number of <see cref="DestructorDeclarationSyntax"/>es to skip before creating a valid <see cref="IMemberData"/>.</param>
		/// <returns>
		/// A new <see cref="DestructorData"/> created from a <see cref="DestructorDeclarationSyntax"/> found at the specified index in the parsed <see cref="CSharpSyntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="DestructorDeclarationSyntax"/> exists.
		/// </returns>
		protected DestructorData? GetDestructor(string? source, int index = 0)
		{
			return Compilation.GetMemberData<DestructorDeclarationSyntax>(source, index) as DestructorData;
		}

		/// <summary>
		/// Creates a new <see cref="EnumData"/> from the specified <paramref name="source"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="index">Index at which the <see cref="EnumData"/> should be returned. Can be thought of as a number of <see cref="EnumDeclarationSyntax"/>es to skip before creating a valid <see cref="IMemberData"/>.</param>
		/// <returns>
		/// A new <see cref="EnumData"/> created from a <see cref="EnumDeclarationSyntax"/> found at the specified index in the parsed <see cref="CSharpSyntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="EnumDeclarationSyntax"/> exists.
		/// </returns>
		protected EnumData? GetEnum(string? source, int index = 0)
		{
			return Compilation.GetMemberData<EnumDeclarationSyntax>(source, index) as EnumData;
		}

		/// <summary>
		/// Creates a new <see cref="EventData"/> from the specified <paramref name="source"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="index">Index at which the <see cref="EventData"/> should be returned. Can be thought of as a number of <see cref="EventFieldDeclarationSyntax"/>es to skip before creating a valid <see cref="IMemberData"/>.</param>
		/// <returns>
		/// A new <see cref="EventData"/> created from a <see cref="EventFieldDeclarationSyntax"/> found at the specified index in the parsed <see cref="CSharpSyntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="EventFieldDeclarationSyntax"/> exists.
		/// </returns>
		protected EventData? GetEventField(string? source, int index = 0)
		{
			return Compilation.GetMemberData<EventFieldDeclarationSyntax>(source, index) as EventData;
		}

		/// <summary>
		/// Creates a new <see cref="EventData"/> from the specified <paramref name="source"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="index">Index at which the <see cref="EventData"/> should be returned. Can be thought of as a number of <see cref="EventDeclarationSyntax"/>es to skip before creating a valid <see cref="IMemberData"/>.</param>
		/// <returns>
		/// A new <see cref="EventData"/> created from a <see cref="EventDeclarationSyntax"/> found at the specified index in the parsed <see cref="CSharpSyntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="EventDeclarationSyntax"/> exists.
		/// </returns>
		protected EventData? GetEventProperty(string? source, int index = 0)
		{
			return Compilation.GetMemberData<EventDeclarationSyntax>(source, index) as EventData;
		}

		/// <summary>
		/// Creates a new <see cref="FieldData"/> from the specified <paramref name="source"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="index">Index at which the <see cref="FieldData"/> should be returned. Can be thought of as a number of <see cref="FieldDeclarationSyntax"/>es to skip before creating a valid <see cref="IMemberData"/>.</param>
		/// <returns>
		/// A new <see cref="FieldData"/> created from a <see cref="FieldDeclarationSyntax"/> found at the specified index in the parsed <see cref="CSharpSyntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="FieldDeclarationSyntax"/> exists.
		/// </returns>
		protected FieldData? GetField(string? source, int index = 0)
		{
			return Compilation.GetMemberData<FieldDeclarationSyntax>(source, index) as FieldData;
		}

		/// <summary>
		/// Creates a new <see cref="IndexerData"/> from the specified <paramref name="source"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="index">Index at which the <see cref="IndexerData"/> should be returned. Can be thought of as a number of <see cref="IndexerDeclarationSyntax"/>es to skip before creating a valid <see cref="IMemberData"/>.</param>
		/// <returns>
		/// A new <see cref="IndexerData"/> created from a <see cref="IndexerDeclarationSyntax"/> found at the specified index in the parsed <see cref="CSharpSyntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="IndexerDeclarationSyntax"/> exists.
		/// </returns>
		protected IndexerData? GetIndexer(string? source, int index = 0)
		{
			return Compilation.GetMemberData<IndexerDeclarationSyntax>(source, index) as IndexerData;
		}

		/// <summary>
		/// Returns a collection of <see cref="ISourceTextProvider"/>s that create initial sources.
		/// </summary>
		protected virtual IEnumerable<ISourceTextProvider>? GetInitialSources()
		{
			return null;
		}

		/// <summary>
		/// Creates a new <see cref="InterfaceData"/> from the specified <paramref name="source"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="index">Index at which the <see cref="InterfaceData"/> should be returned. Can be thought of as a number of <see cref="InterfaceDeclarationSyntax"/>es to skip before creating a valid <see cref="IMemberData"/>.</param>
		/// <returns>
		/// A new <see cref="InterfaceData"/> created from a <see cref="InterfaceDeclarationSyntax"/> found at the specified index in the parsed <see cref="CSharpSyntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="InterfaceDeclarationSyntax"/> exists.
		/// </returns>
		protected InterfaceData? GetInterface(string? source, int index = 0)
		{
			return Compilation.GetMemberData<InterfaceDeclarationSyntax>(source, index) as InterfaceData;
		}

		/// <summary>
		/// Creates a new <see cref="MemberData"/> from the specified <paramref name="source"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="index">Index at which the <see cref="MemberData"/> should be returned. Can be thought of as a number of <see cref="MemberDeclarationSyntax"/>es to skip before creating a valid <see cref="IMemberData"/>.</param>
		/// <returns>
		/// A new <see cref="MemberData"/> created from a <see cref="MemberDeclarationSyntax"/> found at the specified index in the parsed <see cref="CSharpSyntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="MemberDeclarationSyntax"/> exists.
		/// </returns>
		protected MemberData? GetMember(string? source, int index = 0)
		{
			return Compilation.GetMemberData<MemberDeclarationSyntax>(source, index) as MemberData;
		}

		/// <summary>
		/// Creates a new <see cref="MethodData"/> from the specified <paramref name="source"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="index">Index at which the <see cref="MethodData"/> should be returned. Can be thought of as a number of <see cref="MethodDeclarationSyntax"/>es to skip before creating a valid <see cref="IMemberData"/>.</param>
		/// <returns>
		/// A new <see cref="MethodData"/> created from a <see cref="MethodDeclarationSyntax"/> found at the specified index in the parsed <see cref="CSharpSyntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="MethodDeclarationSyntax"/> exists.
		/// </returns>
		protected MethodData? GetMethod(string? source, int index = 0)
		{
			return Compilation.GetMemberData<MethodDeclarationSyntax>(source, index) as MethodData;
		}

		/// <inheritdoc cref="TestableCompilationData.GetNode{TNode}(string?, int)"/>
		protected TNode? GetNode<TNode>(string? source, int index = 0) where TNode : CSharpSyntaxNode
		{
			return Compilation.GetNode<TNode>(source, index);
		}

		/// <inheritdoc cref="TestableCompilationData.GetNode{TNode}(CSharpSyntaxTree?, int)"/>
		protected TNode? GetNode<TNode>(CSharpSyntaxTree? syntaxTree, int index = 0) where TNode : CSharpSyntaxNode
		{
			return Compilation.GetNode<TNode>(syntaxTree, index);
		}

		/// <summary>
		/// Creates a new <see cref="OperatorData"/> from the specified <paramref name="source"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="index">Index at which the <see cref="OperatorData"/> should be returned. Can be thought of as a number of <see cref="OperatorDeclarationSyntax"/>es to skip before creating a valid <see cref="IMemberData"/>.</param>
		/// <returns>
		/// A new <see cref="OperatorData"/> created from a <see cref="OperatorDeclarationSyntax"/> found at the specified index in the parsed <see cref="CSharpSyntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="OperatorDeclarationSyntax"/> exists.
		/// </returns>
		protected OperatorData? GetOperator(string? source, int index = 0)
		{
			return Compilation.GetMemberData<OperatorDeclarationSyntax>(source, index) as OperatorData;
		}

		/// <summary>
		/// Creates a new <see cref="PropertyData"/> from the specified <paramref name="source"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="index">Index at which the <see cref="PropertyData"/> should be returned. Can be thought of as a number of <see cref="PropertyDeclarationSyntax"/>es to skip before creating a valid <see cref="IMemberData"/>.</param>
		/// <returns>
		/// A new <see cref="PropertyData"/> created from a <see cref="PropertyDeclarationSyntax"/> found at the specified index in the parsed <see cref="CSharpSyntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="PropertyDeclarationSyntax"/> exists.
		/// </returns>
		protected PropertyData? GetProperty(string? source, int index = 0)
		{
			return Compilation.GetMemberData<PropertyDeclarationSyntax>(source, index) as PropertyData;
		}

		/// <summary>
		/// Creates a new <see cref="RecordData"/> from the specified <paramref name="source"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="index">Index at which the <see cref="RecordData"/> should be returned. Can be thought of as a number of <see cref="RecordDeclarationSyntax"/>es to skip before creating a valid <see cref="IMemberData"/>.</param>
		/// <returns>
		/// A new <see cref="RecordData"/> created from a <see cref="RecordDeclarationSyntax"/> found at the specified index in the parsed <see cref="CSharpSyntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="RecordDeclarationSyntax"/> exists.
		/// </returns>
		protected RecordData? GetRecord(string? source, int index = 0)
		{
			return Compilation.GetMemberData<RecordDeclarationSyntax>(source, index) as RecordData;
		}

		/// <summary>
		/// Creates a new <see cref="StructData"/> from the specified <paramref name="source"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="index">Index at which the <see cref="StructData"/> should be returned. Can be thought of as a number of <see cref="StructDeclarationSyntax"/>es to skip before creating a valid <see cref="IMemberData"/>.</param>
		/// <returns>
		/// A new <see cref="StructData"/> created from a <see cref="StructDeclarationSyntax"/> found at the specified index in the parsed <see cref="CSharpSyntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="StructDeclarationSyntax"/> exists.
		/// </returns>
		protected StructData? GetStruct(string? source, int index = 0)
		{
			return Compilation.GetMemberData<StructDeclarationSyntax>(source, index) as StructData;
		}

		/// <inheritdoc cref="TestableCompilationData.GetSymbol{TSymbol}(CSharpSyntaxNode)"/>
		protected TSymbol? GetSymbol<TSymbol>(CSharpSyntaxNode node) where TSymbol : class, ISymbol
		{
			return Compilation.GetSymbol<TSymbol>(node);
		}

		/// <inheritdoc cref="TestableCompilationData.GetSymbol{TSymbol, TNode}(string?, int)"/>
		protected TSymbol? GetSymbol<TSymbol, TNode>(string? source, int index = 0) where TSymbol : class, ISymbol where TNode : CSharpSyntaxNode
		{
			return Compilation.GetSymbol<TSymbol, TNode>(source, index);
		}

		/// <inheritdoc cref="TestableCompilationData.GetSymbol{TSymbol, TNode}(CSharpSyntaxTree?, int)"/>
		protected TSymbol? GetSymbol<TSymbol, TNode>(CSharpSyntaxTree? syntaxTree, int index = 0) where TSymbol : class, ISymbol where TNode : CSharpSyntaxNode
		{
			return Compilation.GetSymbol<TSymbol, TNode>(syntaxTree, index);
		}

		/// <summary>
		/// Creates a new <see cref="TypeData{TDeclaration}"/> from the specified <paramref name="source"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="index">Index at which the <see cref="TypeData{TDeclaration}"/> should be returned. Can be thought of as a number of <see cref="CSharpSyntaxNode"/>es of type <typeparamref name="TDeclaration"/> to skip before creating a valid <see cref="IMemberData"/>.</param>
		/// <returns>
		/// A new <see cref="TypeData{TDeclaration}"/> created from a <see cref="CSharpSyntaxNode"/>es of type <typeparamref name="TDeclaration"/> found at the specified index in the parsed <see cref="CSharpSyntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="CSharpSyntaxNode"/> of type <typeparamref name="TDeclaration"/> exists.
		/// </returns>
		protected TypeData<TDeclaration>? GetType<TDeclaration>(string? source, int index = 0) where TDeclaration : TypeDeclarationSyntax
		{
			return Compilation.GetMemberData<TDeclaration>(source, index) as TypeData<TDeclaration>;
		}

		private void AddInitialSources()
		{
			CSharpCompilation newCompilation = Compilation.OriginalCompilation;
			AddInitialSources(ref newCompilation);
			Compilation.OriginalCompilation = newCompilation;
		}
	}
}
