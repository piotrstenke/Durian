// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Durian.Analysis
{
	/// <summary>
	/// A wrapper for the <see cref="StringBuilder"/> class that helps generating C# code.
	/// </summary>
	[DebuggerDisplay("{TextBuilder}")]
	public sealed class CodeBuilder
	{
		private int _currentIndent;

		/// <summary>
		/// Current indentation level.
		/// </summary>
		public int CurrentIndent
		{
			get => _currentIndent;
			set
			{
				if (value < 0)
				{
					_currentIndent = 0;
				}
				else
				{
					_currentIndent = value;
				}
			}
		}

		/// <summary>
		/// The <see cref="IDurianGenerator"/> this <see cref="CodeBuilder"/> is used by.
		/// </summary>
		public IDurianGenerator? Generator { get; }

		/// <summary>
		/// <see cref="StringBuilder"/> this <see cref="CodeBuilder"/> writes to.
		/// </summary>
		public StringBuilder TextBuilder { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeBuilder"/> class.
		/// </summary>
		public CodeBuilder()
		{
			TextBuilder = new StringBuilder();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeBuilder"/> class.
		/// </summary>
		/// <param name="capacity">Capacity of the internal <see cref="StringBuilder"/>.</param>
		public CodeBuilder(int capacity)
		{
			TextBuilder = new StringBuilder(capacity);
		}

		/// <inheritdoc cref="CodeBuilder(IDurianGenerator?, StringBuilder)"/>
		public CodeBuilder(StringBuilder builder)
		{
			if (builder is null)
			{
				throw new ArgumentNullException(nameof(builder));
			}

			TextBuilder = builder;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeBuilder"/> class.
		/// </summary>
		/// <param name="sourceGenerator">The <see cref="IDurianGenerator"/> this <see cref="CodeBuilder"/> is used by.</param>
		public CodeBuilder(IDurianGenerator sourceGenerator)
		{
			TextBuilder = new StringBuilder();
			Generator = sourceGenerator;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeBuilder"/> class.
		/// </summary>
		/// <param name="sourceGenerator">The <see cref="IDurianGenerator"/> this <see cref="CodeBuilder"/> is used by.</param>
		/// <param name="builder"><see cref="StringBuilder"/> to write the data to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
		public CodeBuilder(IDurianGenerator sourceGenerator, StringBuilder builder)
		{
			TextBuilder = builder;
			Generator = sourceGenerator;
		}

		/// <summary>
		/// Appends the <paramref name="value"/> to the <see cref="TextBuilder"/>.
		/// </summary>
		/// <param name="value">Value to append to the <see cref="TextBuilder"/>.</param>
		public void Write(string value)
		{
			TextBuilder.Append(value);
		}


		/// <summary>
		/// Appends a new line character to the <see cref="TextBuilder"/>.
		/// </summary>
		public void WriteLine()
		{
			TextBuilder.AppendLine();
		}

		/// <summary>
		/// Appends the <paramref name="value"/> followed by a new line to the <see cref="TextBuilder"/>.
		/// </summary>
		/// <param name="value">Value to append to the <see cref="TextBuilder"/>.</param>v
		public void WriteLine(string value)
		{
			TextBuilder.AppendLine(value);
		}

		/// <summary>
		/// Begins a member declaration using the specified raw text.
		/// </summary>
		/// <param name="member">Raw text to write.</param>
		public void BeginDeclation(string member)
		{
			TextBuilder?.AppendLine(member);
			BeginScope();
		}

		/// <summary>
		/// Begins a new scope.
		/// </summary>
		public void BeginScope()
		{
			Indent();
			TextBuilder.AppendLine("{");
			CurrentIndent++;
		}

		/// <inheritdoc cref="BeginMethodDeclaration(MethodData, bool, bool)"/>
		public void BeginMethodDeclaration(MethodData method, bool blockOrExpression)
		{
			BeginMethodDeclaration(method, blockOrExpression, false);
		}

		/// <summary>
		/// Writes declaration of a method.
		/// </summary>
		/// <param name="method"><see cref="MethodData"/> that contains all the needed info about the target method.</param>
		/// <param name="blockOrExpression">
		/// Determines whether to begin a block body ('{') or an expression body ('=>').
		/// <see langword="true"/> for block, <see langword="false"/> for expression.
		/// </param>
		/// <param name="includeTrivia">Determines whether to include trivia of the <paramref name="method"/></param>
		/// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
		public void BeginMethodDeclaration(MethodData method, bool blockOrExpression, bool includeTrivia)
		{
			if (method is null)
			{
				throw new ArgumentNullException(nameof(method));
			}

			BeginMethodDeclaration_Internal(method.Declaration, blockOrExpression, includeTrivia);
		}

		/// <inheritdoc cref="BeginMethodDeclaration(MethodDeclarationSyntax, bool, bool)"/>
		public void BeginMethodDeclaration(MethodDeclarationSyntax method, bool blockOrExpression)
		{
			BeginMethodDeclaration(method, blockOrExpression, false);
		}

		/// <summary>
		/// Writes declaration of a method.
		/// </summary>
		/// <param name="method"><see cref="MethodDeclarationSyntax"/> to copy the method signature from.</param>
		/// <param name="blockOrExpression">
		/// Determines whether to begin a block body ('{') or an expression body ('=>').
		/// <see langword="true"/> for block, <see langword="false"/> for expression.
		/// </param>
		/// <param name="includeTrivia">Determines whether to include trivia of the <paramref name="method"/></param>
		/// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
		public void BeginMethodDeclaration(MethodDeclarationSyntax method, bool blockOrExpression, bool includeTrivia)
		{
			if (method is null)
			{
				throw new ArgumentNullException(nameof(method));
			}

			BeginMethodDeclaration_Internal(method, blockOrExpression, includeTrivia);
		}

		/// <summary>
		/// Writes declaration of a namespace using the specified collection of <paramref name="namespaces"/>.
		/// </summary>
		/// <param name="namespaces">A collection of <see cref="INamespaceSymbol"/>s to write the names of.</param>
		public void BeginNamespaceDeclaration(IEnumerable<INamespaceSymbol> namespaces)
		{
			if (namespaces is null)
			{
				throw new ArgumentNullException(nameof(namespaces));
			}

			BeginNamespaceDeclaration_Internal(namespaces);
		}

		/// <summary>
		/// Writes declaration of a namespace using the specified collection of <paramref name="namespaces"/>.
		/// </summary>
		/// <param name="namespaces">A collection of namespace names s to write.</param>
		public void BeginNamespaceDeclaration(IEnumerable<string> namespaces)
		{
			if (namespaces is null)
			{
				throw new ArgumentNullException(nameof(namespaces));
			}

			BeginNamespaceDeclaration_Internal(AnalysisUtilities.JoinNamespaces(namespaces));
		}

		/// <summary>
		/// Writes declaration of the specified <paramref name="namespace"/>.
		/// </summary>
		/// <param name="namespace">Name of namespace to begin the declaration of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="namespace"/> is <see langword="null"/>.</exception>
		public void BeginNamespaceDeclaration(string @namespace)
		{
			if (@namespace is null)
			{
				throw new ArgumentNullException(nameof(@namespace));
			}

			BeginNamespaceDeclaration_Internal(@namespace);
		}

		/// <summary>
		/// Writes declaration of the parent namespace of the specified <paramref name="member"/>.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to write the full namespace it is declared in.</param>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>.</exception>
		public void BeginNamespaceDeclarationOf(IMemberData member)
		{
			if (member is null)
			{
				throw new ArgumentNullException(nameof(member));
			}

			BeginNamespaceDeclaration_Internal(member.GetContainingNamespaces());
		}

		/// <summary>
		/// Writes declaration of the parent namespace of the specified <paramref name="member"/>.
		/// </summary>
		/// <param name="member"><see cref="ISymbol"/> to write the full namespace it is declared in.</param>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>.</exception>
		public void BeginNamespaceDeclarationOf(ISymbol member)
		{
			if (member is null)
			{
				throw new ArgumentNullException(nameof(member));
			}

			BeginNamespaceDeclaration_Internal(member.GetContainingNamespaces(false));
		}

		/// <summary>
		/// Writes declaration of the parent namespace of the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to write the full namespace it is declared in.</param>
		/// <exception cref="ArgumentNullException"><paramref name="node"/> is <see langword="null"/>.</exception>
		public void BeginNamespaceDeclarationOf(CSharpSyntaxNode node)
		{
			if (node is null)
			{
				throw new ArgumentNullException(nameof(node));
			}

			BeginNamespaceDeclaration_Internal(AnalysisUtilities.JoinNamespaces(node.GetParentNamespaces()));
		}

		/// <summary>
		/// Writes declaration of a type.
		/// </summary>
		/// <param name="type"><see cref="ITypeData"/> that contains all the needed info about the target type.</param>
		/// <returns>An <see cref="int"/> that represents the modified indentation level.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		public void BeginTypeDeclaration(ITypeData type)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			BeginTypeDeclaration_Internal(type);
		}

		/// <inheritdoc cref="BeginTypeDeclaration(TypeDeclarationSyntax, bool)"/>
		public void BeginTypeDeclaration(TypeDeclarationSyntax type)
		{
			BeginTypeDeclaration(type, false);
		}

		/// <summary>
		/// Writes declaration of a type.
		/// </summary>
		/// <param name="type"><see cref="TypeDeclarationSyntax"/> to convert to a <see cref="string"/> and append to the <see cref="TextBuilder"/>.</param>
		/// <param name="includeTrivia">Determines whether to include trivia of the <paramref name="type"/></param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		public void BeginTypeDeclaration(TypeDeclarationSyntax type, bool includeTrivia)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			BeginTypeDeclaration_Internal(type, includeTrivia);
		}

		/// <summary>
		/// Clears the builder.
		/// </summary>
		public void Clear()
		{
			TextBuilder.Clear();
		}

		/// <summary>
		/// Decrements the value of the <see cref="CurrentIndent"/>.
		/// </summary>
		public void DecrementIndent()
		{
			CurrentIndent--;
		}

		/// <summary>
		/// Ends all the remaining scope.
		/// </summary>
		public void EndAllScopes()
		{
			int length = CurrentIndent;

			for (int i = 0; i < length; i++)
			{
				CurrentIndent--;
				Indent();
				TextBuilder.AppendLine("}");
			}
		}

		/// <summary>
		/// Ends the current scope.
		/// </summary>
		public void EndScope()
		{
			CurrentIndent--;
			Indent();
			TextBuilder.AppendLine("}");
		}

		/// <summary>
		/// Increments the value of the <see cref="CurrentIndent"/>.
		/// </summary>
		public void IncrementIndent()
		{
			CurrentIndent++;
		}

		/// <summary>
		/// Applies indentation according to the value of <see cref="CurrentIndent"/>.
		/// </summary>
		public void Indent()
		{
			Indent(_currentIndent);
		}

		/// <summary>
		/// Applies indentation according to the specified <paramref name="value"/>.
		/// </summary>
		/// <param name="value">Indentation level to apply.</param>
		public void Indent(int value)
		{
			for (int i = 0; i < value; i++)
			{
				TextBuilder.Append('\t');
			}
		}

		/// <inheritdoc cref="ParseSyntaxTree(CSharpParseOptions)"/>
		public CSharpSyntaxTree ParseSyntaxTree()
		{
			return ParseSyntaxTree(Generator?.ParseOptions);
		}

		/// <summary>
		/// Creates new <see cref="CSharpSyntaxTree"/> based on the contents of the <see cref="TextBuilder"/>.
		/// </summary>
		/// <param name="options"><see cref="CSharpParseOptions"/> to use when parsing the <see cref="CSharpSyntaxTree"/>.</param>
		public CSharpSyntaxTree ParseSyntaxTree(CSharpParseOptions? options)
		{
			return (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(
				text: TextBuilder.ToString(),
				options: options,
				encoding: Encoding.UTF8
			);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return TextBuilder.ToString();
		}

		/// <summary>
		/// Writes the attribute text.
		/// </summary>
		/// <param name="attributeName">Name of the attribute to write.</param>
		/// <param name="args">Arguments of the attribute.</param>
		public void WriteAttribute(string? attributeName, params object[] args)
		{
			TextBuilder.Append('[').Append(attributeName ?? string.Empty);

			if (args is not null)
			{
				TextBuilder.Append('(');

				foreach (object arg in args)
				{
					TextBuilder.Append(arg).Append(", ");
				}

				TextBuilder.Remove(TextBuilder.Length - 2, 2);
				TextBuilder.Append(')');
			}

			TextBuilder.Append(']');
		}

		/// <inheritdoc cref="WriteDeclarationLead(IMemberData, IEnumerable{string}, string, string)"/>
		public void WriteDeclarationLead(IMemberData member, IEnumerable<string> usings, string? generatorName)
		{
			WriteDeclarationLead(member, usings, generatorName, null);
		}

		/// <summary>
		/// Writes all the text that is needed before the actual member declaration, that is: the 'auto-generated' header, usings, namespace and parent types declarations.
		/// </summary>
		/// <param name="member">The <see cref="IMemberData"/> that contains all needed info, such as the <see cref="SemanticModel"/> or the <see cref="ISymbol"/>.</param>
		/// <param name="usings">A collection of usings to apply.</param>
		/// <param name="generatorName">Name of generator that created the following code.</param>
		/// <param name="version">Version of the generator.</param>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>. -or- <paramref name="usings"/> is <see langword="null"/>.</exception>
		public void WriteDeclarationLead(IMemberData member, IEnumerable<string> usings, string? generatorName, string? version)
		{
			if (member is null)
			{
				throw new ArgumentNullException(nameof(member));
			}

			if (usings is null)
			{
				throw new ArgumentNullException(nameof(usings));
			}

			WriteDeclarationLead_Internal(member, usings, generatorName, version);
		}

		/// <summary>
		/// Writes the file header returned by the <see cref="AutoGenerated.GetHeader(string?, string?)"/> method using values provided by the <see cref="Generator"/>.
		/// </summary>
		/// <remarks>If the <see cref="Generator"/> is <see langword="null"/>, calls the <see cref="AutoGenerated.GetHeader()"/> method instead.</remarks>
		public void WriteHeader()
		{
			string text;

			if (Generator is null)
			{
				text = AutoGenerated.GetHeader();
			}
			else
			{
				text = AutoGenerated.GetHeader(Generator.GeneratorName, Generator.GeneratorVersion);
			}

			TextBuilder.Append(text);
		}

		/// <summary>
		/// Writes the file header returned by the <see cref="AutoGenerated.GetHeader(string?)"/> method.
		/// </summary>
		/// <param name="generatorName">Name of generator that created the following code.</param>
		public void WriteHeader(string? generatorName)
		{
			TextBuilder.Append(AutoGenerated.GetHeader(generatorName));
		}

		/// <summary>
		/// Writes the file header returned by the <see cref="AutoGenerated.GetHeader(string?, string?)"/> method.
		/// </summary>
		/// <param name="generatorName">Name of generator that created the following code.</param>
		/// <param name="version">Version of the generator that created the following code.</param>
		public void WriteHeader(string? generatorName, string? version)
		{
			TextBuilder.Append(AutoGenerated.GetHeader(generatorName, version));
		}

		/// <summary>
		/// Writes declarations of the specified <paramref name="member"/>'s parent types.
		/// </summary>
		/// <param name="member">Target member.</param>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>.</exception>
		public void WriteParentDeclarations(IMemberData member)
		{
			if (member is null)
			{
				throw new ArgumentNullException(nameof(member));
			}

			WriteParentDeclarations_Internal(member.GetContainingTypes());
		}

		/// <summary>
		/// Writes declarations of all the parent <paramref name="types"/>.
		/// </summary>
		/// <param name="types">A collection of parent <see cref="ITypeData"/>s to write the declarations of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="types"/> is <see langword="null"/>.</exception>
		public void WriteParentDeclarations(IEnumerable<ITypeData> types)
		{
			if (types is null)
			{
				throw new ArgumentNullException(nameof(types));
			}

			WriteParentDeclarations_Internal(types);
		}

		/// <summary>
		/// Writes all usings that are needed by the specified <paramref name="member"/>.
		/// </summary>
		/// <param name="member">Target <see cref="IMemberData"/> to write the used namespaces of.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>.</exception>
		public void WriteUsings(IMemberData member, CancellationToken cancellationToken = default)
		{
			if (member is null)
			{
				throw new ArgumentNullException(nameof(member));
			}

			WriteUsings_Internal(member.SemanticModel.GetUsedNamespaces(member.Declaration, member.ParentCompilation, true, cancellationToken));
		}

		/// <summary>
		/// Writes all usings that are needed by the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="semanticModel">Target <see cref="SemanticModel"/> that is used to get the used namespaces of the specified <paramref name="node"/>.</param>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to write the used namespaces of.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <exception cref="ArgumentNullException"><paramref name="semanticModel"/> is <see langword="null"/>. -or- <paramref name="node"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">
		/// If the <see cref="Generator"/> property is <see langword="null"/>,
		/// a <see cref="INamespaceSymbol"/> of the assembly's global namespace must be explicitly provided using one of WriteUsings 3-parameter overloads.
		/// </exception>
		public void WriteUsings(SemanticModel semanticModel, CSharpSyntaxNode node, CancellationToken cancellationToken = default)
		{
			if (Generator is null)
			{
				throw new InvalidOperationException($"If the {nameof(Generator)} property is null, a {nameof(INamespaceSymbol)} of the assembly's global namespace must be explicitly provided using one of WriteUsings 3-parameter overloads.");
			}

			WriteUsings_Internal(semanticModel.GetUsedNamespaces(node, Generator.TargetCompilation, true, cancellationToken));
		}

		/// <summary>
		/// Writes all usings that are needed by the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="semanticModel">Target <see cref="SemanticModel"/> that is used to get the used namespaces of the specified <paramref name="node"/>.</param>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to write the used namespaces of.</param>
		/// <param name="compilationData"><see cref="ICompilationData"/> the specified <paramref name="node"/> is defined in.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <exception cref="ArgumentNullException"><paramref name="semanticModel"/> is <see langword="null"/>. -or- <paramref name="node"/> is <see langword="null"/>. -or- <paramref name="compilationData"/> is <see langword="null"/>.</exception>
		public void WriteUsings(SemanticModel semanticModel, CSharpSyntaxNode node, ICompilationData compilationData, CancellationToken cancellationToken = default)
		{
			WriteUsings_Internal(semanticModel.GetUsedNamespaces(node, compilationData, true, cancellationToken));
		}

		/// <summary>
		/// Writes all usings that are needed by the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="semanticModel">Target <see cref="SemanticModel"/> that is used to get the used namespaces of the specified <paramref name="node"/>.</param>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to write the used namespaces of.</param>
		/// <param name="compilation"><see cref="CSharpCompilation"/> the specified <paramref name="node"/> is defined in.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <exception cref="ArgumentNullException"><paramref name="semanticModel"/> is <see langword="null"/>. -or- <paramref name="node"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		public void WriteUsings(SemanticModel semanticModel, CSharpSyntaxNode node, CSharpCompilation compilation, CancellationToken cancellationToken = default)
		{
			WriteUsings_Internal(semanticModel.GetUsedNamespaces(node, compilation, true, cancellationToken));
		}

		/// <summary>
		/// Writes all usings that are needed by the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="semanticModel">Target <see cref="SemanticModel"/> that is used to get the used namespaces of the specified <paramref name="node"/>.</param>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to write the used namespaces of.</param>
		/// <param name="assembly"><see cref="IAssemblySymbol"/> the specified <paramref name="node"/> is defined in.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <exception cref="ArgumentNullException"><paramref name="semanticModel"/> is <see langword="null"/>. -or- <paramref name="node"/> is <see langword="null"/>. -or- <paramref name="assembly"/> is <see langword="null"/>.</exception>
		public void WriteUsings(SemanticModel semanticModel, CSharpSyntaxNode node, IAssemblySymbol assembly, CancellationToken cancellationToken = default)
		{
			WriteUsings_Internal(semanticModel.GetUsedNamespaces(node, assembly, true, cancellationToken));
		}

		/// <summary>
		/// Writes all usings that are needed by the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="semanticModel">Target <see cref="SemanticModel"/> that is used to get the used namespaces of the specified <paramref name="node"/>.</param>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to write the used namespaces of.</param>
		/// <param name="globalNamespace"><see cref="INamespaceSymbol"/> that represents the assembly's global namespace.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <exception cref="ArgumentNullException"><paramref name="semanticModel"/> is <see langword="null"/>. -or- <paramref name="node"/> is <see langword="null"/>. -or- <paramref name="globalNamespace"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="globalNamespace"/> is not an actual global namespace.</exception>
		public void WriteUsings(SemanticModel semanticModel, CSharpSyntaxNode node, INamespaceSymbol globalNamespace, CancellationToken cancellationToken = default)
		{
			WriteUsings_Internal(semanticModel.GetUsedNamespaces(node, globalNamespace, true, cancellationToken));
		}

		/// <summary>
		/// Writes usings for all the specified <paramref name="namespaces"/>.
		/// </summary>
		/// <param name="namespaces">A collection of namespaces to write.</param>
		/// <exception cref="ArgumentNullException"> <paramref name="namespaces"/> is <see langword="null"/>.</exception>
		public void WriteUsings(IEnumerable<string> namespaces)
		{
			if (namespaces is null)
			{
				throw new ArgumentNullException(nameof(namespaces));
			}

			WriteUsings_Internal(namespaces);
		}

		/// <summary>
		/// Writes usings for all the specified <paramref name="namespaces"/>.
		/// </summary>
		/// <param name="namespaces">A collection of namespaces to write.</param>
		/// <exception cref="ArgumentNullException"> <paramref name="namespaces"/> is <see langword="null"/>.</exception>
		public void WriteUsings(IEnumerable<INamespaceSymbol> namespaces)
		{
			if (namespaces is null)
			{
				throw new ArgumentNullException(nameof(namespaces));
			}

			WriteUsings_Internal(namespaces.Select(n => n.Name).Where(n => n != string.Empty));
		}

		private static string GetDeclarationText(MemberDeclarationSyntax declaration, bool includeTrivia)
		{
			if (includeTrivia)
			{
				MemberDeclarationSyntax decl = declaration
					.WithLeadingTrivia(declaration.GetLeadingTrivia())
					.WithTrailingTrivia(declaration.GetTrailingTrivia());

				return decl.ToFullString();
			}

			return declaration.ToString();
		}

		private void BeginMethodDeclaration_Internal(MethodDeclarationSyntax method, bool blockOrExpression, bool includeTrivia)
		{
			TextBuilder.Append(GetDeclarationText(SyntaxFactory.MethodDeclaration(method.ReturnType, method.Identifier), includeTrivia));

			if (blockOrExpression)
			{
				TextBuilder.AppendLine();
				Indent();
				CurrentIndent++;
				TextBuilder.AppendLine("{");
			}
			else
			{
				TextBuilder.Append(" => ");
			}
		}

		private void BeginNamespaceDeclaration_Internal(IEnumerable<INamespaceSymbol> namespaces)
		{
			BeginNamespaceDeclaration_Internal(namespaces.JoinNamespaces());
		}

		private void BeginNamespaceDeclaration_Internal(string @namespace)
		{
			Indent();
			TextBuilder.Append("namespace ").AppendLine(@namespace);
			Indent();
			TextBuilder.AppendLine("{");
			CurrentIndent++;
		}

		private void BeginTypeDeclaration_Internal(ITypeData type)
		{
			Indent();

			foreach (SyntaxToken modifier in type.Modifiers)
			{
				TextBuilder.Append(modifier.ValueText);
				TextBuilder.Append(' ');
			}

			TextBuilder.Append(type.Declaration.GetKeyword());
			TextBuilder.Append(' ');

			TextBuilder.AppendLine(type.Symbol.GetGenericName(GenericSubstitution.Variance));
			Indent();
			CurrentIndent++;
			TextBuilder.AppendLine("{");
		}

		private void BeginTypeDeclaration_Internal(TypeDeclarationSyntax type, bool includeTrivia)
		{
			TextBuilder.Append(GetDeclarationText(SyntaxFactory.TypeDeclaration(type.Kind(), type.Identifier), includeTrivia));
		}

		private void WriteDeclarationLead_Internal(IMemberData member, IEnumerable<string> usings, string? generatorName, string? version)
		{
			WriteHeader(generatorName, version);
			TextBuilder.AppendLine();
			string[] namespaces = usings.ToArray();

			if (namespaces.Length > 0)
			{
				WriteUsings_Internal(usings);
				TextBuilder.AppendLine();
			}

			if (member.Symbol.ContainingNamespace is not null && !member.Symbol.ContainingNamespace.IsGlobalNamespace)
			{
				BeginNamespaceDeclaration_Internal(member.GetContainingNamespaces());
			}

			WriteParentDeclarations_Internal(member.GetContainingTypes());
		}

		private void WriteParentDeclarations_Internal(IEnumerable<ITypeData> types)
		{
			foreach (ITypeData parent in types)
			{
				BeginTypeDeclaration_Internal(parent);
			}
		}

		private void WriteUsings_Internal(IEnumerable<string> namespaces)
		{
			if (CurrentIndent == 0)
			{
				foreach (string u in namespaces)
				{
					TextBuilder.Append("using ").Append(u).AppendLine(";");
				}
			}
			else
			{
				foreach (string u in namespaces)
				{
					Indent();
					TextBuilder.Append("using ").Append(u).AppendLine(";");
				}
			}
		}
	}
}