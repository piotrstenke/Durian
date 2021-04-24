using Durian.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Tests
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
		/// <param name="sources">An array of <see cref="string"/>s to be used as initial sources of <see cref="CSharpSyntaxTree"/>s for the <see cref="Compilation"/>.</param>
		protected CompilationTest(params string[]? sources)
		{
			Compilation = TestableCompilationData.Create(sources);
		}

		/// <inheritdoc cref="TestableCompilationData.GetNode{TNode}(string?, int)"/>
		protected TNode GetNode<TNode>(string? source, int index = 0) where TNode : CSharpSyntaxNode
		{
			return Compilation.GetNode<TNode>(source, index)!;
		}

		/// <inheritdoc cref="TestableCompilationData.GetNode{TNode}(CSharpSyntaxTree?, int)"/>
		protected TNode GetNode<TNode>(CSharpSyntaxTree? syntaxTree, int index = 0) where TNode : CSharpSyntaxNode
		{
			return Compilation.GetNode<TNode>(syntaxTree, index)!;
		}

		/// <inheritdoc cref="TestableCompilationData.GetSymbol{TSymbol}(CSharpSyntaxNode)"/>
		protected TSymbol GetSymbol<TSymbol>(CSharpSyntaxNode node) where TSymbol : class, ISymbol
		{
			return Compilation.GetSymbol<TSymbol>(node)!;
		}

		/// <inheritdoc cref="TestableCompilationData.GetSymbol{TSymbol, TNode}(string?, int)"/>
		protected TSymbol GetSymbol<TSymbol, TNode>(string? source, int index = 0) where TSymbol : class, ISymbol where TNode : CSharpSyntaxNode
		{
			return Compilation.GetSymbol<TSymbol, TNode>(source, index)!;
		}

		/// <inheritdoc cref="TestableCompilationData.GetSymbol{TSymbol, TNode}(CSharpSyntaxTree?, int)"/>
		protected TSymbol GetSymbol<TSymbol, TNode>(CSharpSyntaxTree? syntaxTree, int index = 0) where TSymbol : class, ISymbol where TNode : CSharpSyntaxNode
		{
			return Compilation.GetSymbol<TSymbol, TNode>(syntaxTree, index)!;
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
		protected ClassData GetClass(string? source, int index = 0)
		{
			return (Compilation.GetMemberData<ClassDeclarationSyntax>(source, index) as ClassData)!;
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
		protected StructData GetStruct(string? source, int index = 0)
		{
			return (Compilation.GetMemberData<StructDeclarationSyntax>(source, index) as StructData)!;
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
		protected InterfaceData GetInterface(string? source, int index = 0)
		{
			return (Compilation.GetMemberData<InterfaceDeclarationSyntax>(source, index) as InterfaceData)!;
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
		protected RecordData GetRecord(string? source, int index = 0)
		{
			return (Compilation.GetMemberData<RecordDeclarationSyntax>(source, index) as RecordData)!;
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
		protected MemberData GetMember(string? source, int index = 0)
		{
			return (Compilation.GetMemberData<MemberDeclarationSyntax>(source, index) as MemberData)!;
		}

		/// <summary>
		/// Creates a new <see cref="TypeData"/> from the specified <paramref name="source"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="index">Index at which the <see cref="TypeData"/> should be returned. Can be thought of as a number of <see cref="TypeDeclarationSyntax"/>es to skip before creating a valid <see cref="IMemberData"/>.</param>
		/// <returns>
		/// A new <see cref="TypeData"/> created from a <see cref="TypeDeclarationSyntax"/> found at the specified index in the parsed <see cref="CSharpSyntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="TypeDeclarationSyntax"/> exists.
		/// </returns>
		protected TypeData GetType(string? source, int index = 0)
		{
			return (Compilation.GetMemberData<TypeDeclarationSyntax>(source, index) as TypeData)!;
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
		protected FieldData GetField(string? source, int index = 0)
		{
			return (Compilation.GetMemberData<FieldDeclarationSyntax>(source, index) as FieldData)!;
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
		protected PropertyData GetProperty(string? source, int index = 0)
		{
			return (Compilation.GetMemberData<PropertyDeclarationSyntax>(source, index) as PropertyData)!;
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
		protected EventData GetEventProperty(string? source, int index = 0)
		{
			return (Compilation.GetMemberData<EventDeclarationSyntax>(source, index) as EventData)!;
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
		protected EventData GetEventField(string? source, int index = 0)
		{
			return (Compilation.GetMemberData<EventFieldDeclarationSyntax>(source, index) as EventData)!;
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
		protected DelegateData GetDelegate(string? source, int index = 0)
		{
			return (Compilation.GetMemberData<DelegateDeclarationSyntax>(source, index) as DelegateData)!;
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
		protected MethodData GetMethod(string? source, int index = 0)
		{
			return (Compilation.GetMemberData<MethodDeclarationSyntax>(source, index) as MethodData)!;
		}
	}
}
