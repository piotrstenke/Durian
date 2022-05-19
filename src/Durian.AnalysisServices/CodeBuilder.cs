// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Text;
using Durian.Analysis.CodeGeneration;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis
{
	/// <summary>
	/// A wrapper for the <see cref="StringBuilder"/> class that helps generating C# code.
	/// </summary>
	[DebuggerDisplay("{TextBuilder}")]
	public sealed partial class CodeBuilder
	{
		private readonly IDurianGenerator? _generator;
		private readonly CodeBuilderStyleConfiguration _style;
		private int _currentIndent;
		private int _currentLength;

		/// <summary>
		/// Determines whether the last method call has changed state of the builder.
		/// </summary>
		public bool Changed => _currentIndent != TextBuilder.Length;

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
		/// <see cref="IDurianGenerator"/> that created this <see cref="CodeBuilder"/>.
		/// </summary>
		public IDurianGenerator? Generator
		{
			get
			{
				if (_generator is not null)
				{
					return _generator;
				}

				return PassContext?.Generator;
			}
		}

		/// <summary>
		/// Builder that write single keywords.
		/// </summary>
		public KeywordWriter Keyword { get; }

		/// <summary>
		/// <see cref="GeneratorPassContext"/> to use when writing data.
		/// </summary>
		public GeneratorPassContext? PassContext { get; }

		/// <summary>
		/// <see cref="StringBuilder"/> to write the generated code to.
		/// </summary>
		public StringBuilder TextBuilder { get; }

		/// <summary>
		/// Configuration of this code builder.
		/// </summary>
		public CodeBuilderConfiguration Configuration { get; }

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
		/// <param name="generator"><see cref="IDurianGenerator"/> that created this <see cref="CodeBuilder"/>.</param>
		public CodeBuilder(IDurianGenerator? generator)
		{
			TextBuilder = new StringBuilder();
			_generator = generator;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeBuilder"/> class.
		/// </summary>
		/// <param name="generator"><see cref="IDurianGenerator"/> that created this <see cref="CodeBuilder"/>.</param>
		/// <param name="builder"><see cref="StringBuilder"/> to write the data to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
		public CodeBuilder(IDurianGenerator? generator, StringBuilder builder)
		{
			if (builder is null)
			{
				throw new ArgumentNullException(nameof(builder));
			}

			TextBuilder = builder;
			_generator = generator;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeBuilder"/> class.
		/// </summary>
		/// <param name="context"><see cref="GeneratorPassContext"/> to use when writing data.</param>
		public CodeBuilder(GeneratorPassContext? context)
		{
			PassContext = context;
			TextBuilder = new();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeBuilder"/> class.
		/// </summary>
		/// <param name="context"><see cref="GeneratorPassContext"/> to use when writing data.</param>
		/// <param name="builder"><see cref="StringBuilder"/> to write the data to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
		public CodeBuilder(GeneratorPassContext? context, StringBuilder builder)
		{
			if (builder is null)
			{
				throw new ArgumentNullException(nameof(builder));
			}

			TextBuilder = builder;
			PassContext = context;
		}

		/// <summary>
		/// Begins declaration of auto-property accessors.
		/// </summary>
		public CodeBuilder Accessor(AutoPropertyKind kind)
		{
			InitBuilder();

			switch (kind)
			{
				case AutoPropertyKind.GetOnly:
					TextBuilder.Append(" { get; }");
					break;

				case AutoPropertyKind.GetSet:
					TextBuilder.Append(" { get; set; }");
					break;

				case AutoPropertyKind.GetInit:
					TextBuilder.Append(" { get; init; }");
					break;
			}

			return this;
		}

		/// <summary>
		/// Begins declaration of a property or event accessor.
		/// </summary>
		/// <param name="accessor">Kind of accessor to begin declaration of.</param>
		public CodeBuilder Accessor(Accessor accessor)
		{
			return Accessor(accessor, _style.MethodStyle);
		}

		/// <summary>
		/// Begins declaration of a property or event accessor.
		/// </summary>
		/// <param name="accessor">Kind of accessor to begin declaration of.</param>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder Accessor(Accessor accessor, MethodStyle body)
		{
			InitBuilder();

			if(accessor.GetText() is string value)
			{
				TextBuilder.Append(value);
			}

			return MethodBody(body);
		}

		/// <summary>
		/// Begins an attribute list.
		/// </summary>
		public CodeBuilder AttributeList()
		{
			Indent();
			TextBuilder.Append('[');
			return this;
		}

		/// <summary>
		/// Begins an attribute list with an <see cref="AttributeTarget"/>.
		/// </summary>
		/// <param name="target">Target of the attribute.</param>
		public CodeBuilder AttributeList(AttributeTarget target)
		{
			Indent();

			if (target.GetText() is not string text)
			{
				return AttributeList();
			}

			TextBuilder.Append('[');
			TextBuilder.Append(text);
			TextBuilder.Append(':');
			TextBuilder.Append(' ');

			return this;
		}

		/// <summary>
		/// Begins a new scope.
		/// </summary>
		public CodeBuilder BeginBlock()
		{
			InitBuilder();

			NewLine();
			Indent();
			TextBuilder.Append('{');
			NewLine();
			CurrentIndent++;

			return this;
		}

		/// <summary>
		/// Begins a member declaration using the specified raw text.
		/// </summary>
		/// <param name="member">Raw text to write.</param>
		public CodeBuilder Declation(string member)
		{
			InitBuilder();

			TextBuilder.Append(member);
			BeginBlock();

			return this;
		}

		/// <summary>
		/// Begins a parameter list.
		/// </summary>
		public CodeBuilder ParameterList()
		{
			InitBuilder();

			TextBuilder.Append('(');
			return this;
		}

		/// <summary>
		/// Begins a type parameter list.
		/// </summary>
		public CodeBuilder TypeParameterList()
		{
			InitBuilder();

			TextBuilder.Append('<');
			return this;
		}

		/// <summary>
		/// Clears the builder.
		/// </summary>
		public CodeBuilder Clear()
		{
			InitBuilder();

			TextBuilder.Clear();
			return this;
		}

		/// <summary>
		/// Decrements the value of the <see cref="CurrentIndent"/>.
		/// </summary>
		public CodeBuilder DecrementIndent()
		{
			InitBuilder();

			CurrentIndent--;
			return this;
		}

		/// <summary>
		/// Ends all the remaining scope.
		/// </summary>
		public CodeBuilder EndAllBlocks()
		{
			InitBuilder();

			int length = CurrentIndent;

			for (int i = 0; i < length; i++)
			{
				CurrentIndent--;
				Indent();
				TextBuilder.AppendLine("}");
			}

			return this;
		}

		/// <summary>
		/// Ends an attribute list.
		/// </summary>
		public CodeBuilder EndAttributeList()
		{
			InitBuilder();

			TextBuilder.Append(']');
			NewLine();
			return this;
		}

		/// <summary>
		/// Ends the current scope.
		/// </summary>
		public CodeBuilder EndBlock()
		{
			CurrentIndent--;
			Indent();
			TextBuilder.AppendLine("}");

			return this;
		}

		/// <summary>
		/// Ends a parameter list.
		/// </summary>
		public CodeBuilder EndParameterList()
		{
			InitBuilder();

			TextBuilder.Append(')');
			return this;
		}

		/// <summary>
		/// Ends a type parameter list.
		/// </summary>
		public CodeBuilder EndTypeParameterList()
		{
			InitBuilder();

			TextBuilder.Append('>');
			return this;
		}

		/// <summary>
		/// Increments the value of the <see cref="CurrentIndent"/>.
		/// </summary>
		public CodeBuilder IncrementIndent()
		{
			InitBuilder();
			CurrentIndent++;
			return this;
		}

		/// <summary>
		/// Applies indentation according to the value of <see cref="CurrentIndent"/>.
		/// </summary>
		public CodeBuilder Indent()
		{
			Indent(_currentIndent);
			return this;
		}

		/// <summary>
		/// Applies indentation according to the specified <paramref name="value"/>.
		/// </summary>
		/// <param name="value">Indentation level to apply.</param>
		public CodeBuilder Indent(int value)
		{
			InitBuilder();

			for (int i = 0; i < value; i++)
			{
				TextBuilder.Append('\t');
			}

			return this;
		}

		/// <summary>
		/// Writes a new line.
		/// </summary>
		public CodeBuilder NewLine()
		{
			InitBuilder();
			TextBuilder.AppendLine();
			return this;
		}

		/// <summary>
		/// Writes a <paramref name="number"/> of new lines.
		/// </summary>
		/// <param name="number">Number of new lines to write.</param>
		public CodeBuilder NewLine(int number)
		{
			InitBuilder();

			for (int i = 0; i < number; i++)
			{
				TextBuilder.AppendLine();
			}

			return this;
		}

		/// <inheritdoc cref="ParseSyntaxTree(CSharpParseOptions)"/>
		public CSharpSyntaxTree ParseSyntaxTree()
		{
			CSharpParseOptions? options = PassContext?.ParseOptions ?? Generator?.GetCurrentPassContext()?.ParseOptions;
			return ParseSyntaxTree(options);
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

		/// <summary>
		/// Writes a space.
		/// </summary>
		public CodeBuilder Space()
		{
			InitBuilder();

			TextBuilder.Append(' ');
			return this;
		}

		/// <summary>
		/// Writes a <paramref name="number"/> of spaces.
		/// </summary>
		/// <param name="number">Number of spaces to write.</param>
		public CodeBuilder Space(int number)
		{
			InitBuilder();

			for (int i = 0; i < number; i++)
			{
				TextBuilder.Append(' ');
			}

			return this;
		}

		/// <summary>
		/// Writes a tab.
		/// </summary>
		public CodeBuilder Tab()
		{
			InitBuilder();

			TextBuilder.Append('\t');
			return this;
		}

		/// <summary>
		/// Writes a <paramref name="number"/> of tabs.
		/// </summary>
		/// <param name="number">Number of tabs to write.</param>
		public CodeBuilder Tab(int number)
		{
			InitBuilder();

			for (int i = 0; i < number; i++)
			{
				TextBuilder.Append('\t');
			}

			return this;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return TextBuilder.ToString();
		}

		/// <summary>
		/// Appends the <paramref name="value"/> to the <see cref="TextBuilder"/>.
		/// </summary>
		/// <param name="value">Value to append to the <see cref="TextBuilder"/>.</param>
		public CodeBuilder Write(string value)
		{
			InitBuilder();

			TextBuilder.Append(value);
			return this;
		}

		/// <summary>
		/// Writes the file header returned by the <see cref="AutoGenerated.GetHeader(string?, string?)"/> method using values provided by the <see cref="Generator"/>.
		/// </summary>
		/// <remarks>If the <see cref="Generator"/> is <see langword="null"/>, calls the <see cref="AutoGenerated.GetHeader()"/> method instead.</remarks>
		public CodeBuilder WriteHeader()
		{
			InitBuilder();

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
			return this;
		}

		/// <summary>
		/// Writes the file header returned by the <see cref="AutoGenerated.GetHeader(string?)"/> method.
		/// </summary>
		/// <param name="generatorName">Name of generator that created the following code.</param>
		public CodeBuilder WriteHeader(string? generatorName)
		{
			InitBuilder();

			TextBuilder.Append(AutoGenerated.GetHeader(generatorName));
			return this;
		}

		/// <summary>
		/// Writes an accessibility modifier.
		/// </summary>
		/// <param name="accessibility">Accessibility modifier to write.</param>
		public CodeBuilder Accessibility(Accessibility accessibility)
		{
			InitBuilder();

			if (accessibility.GetText() is string keyword)
			{
				TextBuilder.Append(keyword);
				Space();
			}

			return this;
		}

		/// <summary>
		/// Writes nullability marker if the <paramref name="annotation"/> is equal to <see cref="Microsoft.CodeAnalysis.NullableAnnotation.Annotated"/>.
		/// </summary>
		/// <param name="annotation"><see cref="Microsoft.CodeAnalysis.NullableAnnotation"/> to write.</param>
		public CodeBuilder Nullability(NullableAnnotation annotation)
		{
			InitBuilder();

			if (annotation == NullableAnnotation.Annotated)
			{
				TextBuilder.Append('?');
			}

			return this;
		}

		/// <summary>
		/// Begins a method body.
		/// </summary>
		/// <param name="body">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder MethodBody(MethodStyle body)
		{
			InitBuilder();

			switch (body)
			{
				case CodeGeneration.MethodStyle.Block:
					BeginBlock();
					break;

				case CodeGeneration.MethodStyle.Expression:
					TextBuilder.Append(" => ");
					break;

				default:
					TextBuilder.Append(';');
					NewLine();
					break;
			}

			return this;
		}

		/// <summary>
		/// Writes the file header returned by the <see cref="AutoGenerated.GetHeader(string?, string?)"/> method.
		/// </summary>
		/// <param name="generatorName">Name of generator that created the following code.</param>
		/// <param name="version">Version of the generator that created the following code.</param>
		public CodeBuilder WriteHeader(string? generatorName, string? version)
		{
			InitBuilder();

			TextBuilder.Append(AutoGenerated.GetHeader(generatorName, version));
			return this;
		}

		/// <summary>
		/// Writes the specified <paramref name="variance"/>.
		/// </summary>
		/// <param name="variance">Kind of variance to write.</param>
		public CodeBuilder Variance(VarianceKind variance)
		{
			InitBuilder();

			if (variance.GetText() is string value)
			{
				TextBuilder.Append(value);
			}

			return this;
		}

		/// <summary>
		/// Appends a new line character to the <see cref="TextBuilder"/>.
		/// </summary>
		public CodeBuilder WriteLine()
		{
			InitBuilder();

			TextBuilder.AppendLine();
			return this;
		}

		/// <summary>
		/// Appends the <paramref name="value"/> followed by a new line to the <see cref="TextBuilder"/>.
		/// </summary>
		/// <param name="value">Value to append to the <see cref="TextBuilder"/>.</param>v
		public CodeBuilder WriteLine(string value)
		{
			InitBuilder();

			TextBuilder.AppendLine(value);
			return this;
		}

		private void CommaSpace()
		{
			TextBuilder.Append(',');
			Space();
		}

		private void InitBuilder()
		{
			_currentLength = TextBuilder.Length;
		}

		///// <summary>
		///// Writes all usings that are needed by the specified <paramref name="node"/>.
		///// </summary>
		///// <param name="semanticModel">Target <see cref="SemanticModel"/> that is used to get the used namespaces of the specified <paramref name="node"/>.</param>
		///// <param name="node"><see cref="CSharpSyntaxNode"/> to write the used namespaces of.</param>
		///// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		///// <exception cref="ArgumentNullException"><paramref name="semanticModel"/> is <see langword="null"/>. -or- <paramref name="node"/> is <see langword="null"/>.</exception>
		///// <exception cref="InvalidOperationException">
		///// <see cref="Generator"/> returned null <see cref="IGeneratorPassContext"/>. -or-
		///// If both the <see cref="Generator"/> and <see cref="PassContext"/> are <see langword="null"/>,
		///// a <see cref="INamespaceSymbol"/> of the assembly's global namespace must be explicitly provided using one of WriteUsings 3-parameter overloads.
		///// </exception>
		//public void WriteUsings(SemanticModel semanticModel, CSharpSyntaxNode node, CancellationToken cancellationToken = default)
		//{
		//	ICompilationData? targetCompilation;

		//	if (PassContext is not null)
		//	{
		//		targetCompilation = PassContext.TargetCompilation;
		//	}
		//	else if (_generator is not null)
		//	{
		//		targetCompilation = _generator.GetCurrentPassContext()?.TargetCompilation;

		//		if (targetCompilation is null)
		//		{
		//			throw new InvalidOperationException($"Generator returned null {nameof(IGeneratorPassContext)}");
		//		}
		//	}
		//	else
		//	{
		//		throw new InvalidOperationException($"If both the {nameof(Generator)} and {nameof(PassContext)} are null, a {nameof(INamespaceSymbol)} of the assembly's global namespace must be explicitly provided using one of WriteUsings 3-parameter overloads.");
		//	}

		//	WriteUsings_Internal(semanticModel.GetUsedNamespaces(node, targetCompilation, true, cancellationToken));
		//}

		//private void BeginNamespaceDeclaration_Internal(IEnumerable<INamespaceSymbol> namespaces, NamespaceScope type)
		//{
		//	BeginNamespace_Internal(namespaces.JoinNamespaces(), type);
		//}

		//private void BeginNamespace_Internal(string @namespace, NamespaceScope type)
		//{
		//	Indent();
		//	TextBuilder.Append("namespace ").AppendLine(@namespace);

		//	if(type == NamespaceScope.File)
		//	{
		//		TextBuilder.Append(';');
		//	}
		//	else
		//	{
		//		BeginScope();
		//	}
		//}

		//private void BeginTypeDeclaration_Internal(ITypeData type)
		//{
		//	foreach (SyntaxToken modifier in type.Modifiers)
		//	{
		//		TextBuilder.Append(modifier.ValueText);
		//		TextBuilder.Append(' ');
		//	}

		//	TextBuilder.Append(type.Declaration.GetKeyword());
		//	TextBuilder.Append(' ');

		//	TextBuilder.AppendLine(type.Symbol.GetGenericName(GenericSubstitution.Variance));
		//	Indent();
		//	CurrentIndent++;
		//	TextBuilder.AppendLine("{");
		//}

		//private void BeginTypeDeclaration_Internal(BaseTypeDeclarationSyntax type)
		//{
		//	foreach (SyntaxToken modifier in type.Modifiers)
		//	{
		//		TextBuilder.Append(modifier.ValueText);
		//		TextBuilder.Append(' ');
		//	}

		//	TextBuilder.Append(type.GetKeyword());
		//	TextBuilder.Append(' ');

		//	TextBuilder.Append(type.Identifier.ToString());

		//	if (type is TypeDeclarationSyntax t)
		//	{
		//		WriteTypeParameters(t.TypeParameterList);

		//		if (type is RecordDeclarationSyntax record)
		//		{
		//			WriteParameters(record.ParameterList);
		//		}

		//		WriteConstraints(t.ConstraintClauses);
		//	}

		//	TextBuilder.AppendLine();
		//	BeginScope();
		//}

		//private void WriteDeclarationLead_Internal(IMemberData member, IEnumerable<string>? usings, string? generatorName, string? version)
		//{
		//	WriteHeader(generatorName, version);
		//	TextBuilder.AppendLine();

		//	if (usings is not null)
		//	{
		//		int length = TextBuilder.Length;
		//		WriteUsings_Internal(usings);

		//		if (TextBuilder.Length > length)
		//		{
		//			TextBuilder.AppendLine();
		//		}
		//	}

		//	if (member.Symbol.ContainingNamespace is not null && !member.Symbol.ContainingNamespace.IsGlobalNamespace)
		//	{
		//		BeginNamespaceDeclaration_Internal(member.GetContainingNamespaces());
		//	}

		//	WriteParentDeclarations_Internal(member.GetContainingTypes());
		//}

		//private void WriteParentDeclarations_Internal(IEnumerable<ITypeData> types)
		//{
		//	foreach (ITypeData parent in types)
		//	{
		//		Indent();
		//		BeginTypeDeclaration_Internal(parent);
		//	}
		//}

		//private void WriteUsings_Internal(IEnumerable<string> namespaces)
		//{
		//	if (CurrentIndent == 0)
		//	{
		//		foreach (string u in namespaces)
		//		{
		//			TextBuilder.Append("using ").Append(u).AppendLine(";");
		//		}
		//	}
		//	else
		//	{
		//		foreach (string u in namespaces)
		//		{
		//			Indent();
		//			TextBuilder.Append("using ").Append(u).AppendLine(";");
		//		}
		//	}
		//}
	}
}