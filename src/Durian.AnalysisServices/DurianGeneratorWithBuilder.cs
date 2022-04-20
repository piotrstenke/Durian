﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis
{
	/// <summary>
	/// A <see cref="DurianGenerator"/> that uses a <see cref="CodeBuilder"/> to generate code.
	/// </summary>
	/// <typeparam name="TContext">Type of <see cref="IGeneratorPassContext"/> this generator uses.</typeparam>
	public abstract class DurianGeneratorWithBuilder<TContext> : DurianGenerator<TContext> where TContext : GeneratorPassBuilderContext
	{
		private readonly string _autoGeneratedAttribute;

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGeneratorWithBuilder{TContext}"/> class.
		/// </summary>
		protected DurianGeneratorWithBuilder() : this(null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGeneratorWithBuilder{TContext}"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="DurianGeneratorWithBuilder{TContext}"/> is initialized.</param>
		protected DurianGeneratorWithBuilder(in GeneratorLogCreationContext context) : base(in context)
		{
			_autoGeneratedAttribute = AutoGenerated.GetGeneratedCodeAttribute(GeneratorName, GeneratorVersion);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGeneratorWithBuilder{TContext}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		protected DurianGeneratorWithBuilder(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
			_autoGeneratedAttribute = AutoGenerated.GetGeneratedCodeAttribute(GeneratorName, GeneratorVersion);
		}

		private protected static string NodeToString(CSharpSyntaxNode node)
		{
			return node.WithoutTrivia().ToFullString();
		}

		private protected static bool TryGetInheritdoc(IMemberData original, GenerateDocumentation applyInheritdoc, [NotNullWhen(true)] out string? inheritdoc)
		{
			switch (applyInheritdoc)
			{
				case GenerateDocumentation.Always:
					inheritdoc = AutoGenerated.GetInheritdoc(original.GetXmlFullyQualifiedName());
					return true;

				case GenerateDocumentation.WhenPossible:

					string? doc = original.Symbol.GetInheritdocIfHasDocumentation();

					if (!string.IsNullOrWhiteSpace(doc))
					{
						inheritdoc = doc!;
						return true;
					}

					break;
			}

			inheritdoc = null;
			return false;
		}

		private protected static bool TryGetInheritdoc(ISymbol original, GenerateDocumentation applyInheritdoc, [NotNullWhen(true)] out string? inheritdoc)
		{
			switch (applyInheritdoc)
			{
				case GenerateDocumentation.Always:
					inheritdoc = AutoGenerated.GetInheritdoc(original.GetXmlFullyQualifiedName());
					return true;

				case GenerateDocumentation.WhenPossible:

					string? doc = original.GetInheritdocIfHasDocumentation();

					if (!string.IsNullOrWhiteSpace(doc))
					{
						inheritdoc = doc!;
						return true;
					}

					break;
			}

			inheritdoc = null;
			return false;
		}

		/// <summary>
		/// Adds the source created using the <see cref="CodeBuilder"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><typeparamref name="TContext"/> to add the source to.</param>
		protected void AddSource(string hintName, TContext context)
		{
			CSharpSyntaxTree tree = context.CodeBuilder.ParseSyntaxTree();
			context.CodeBuilder.Clear();
			AddSource_Internal(tree, hintName, context);
		}

		/// <summary>
		/// Adds the source created using the <see cref="CodeBuilder"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="original">The <see cref="CSharpSyntaxNode"/> the source was generated from.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><typeparamref name="TContext"/> to add the source to.</param>
		protected void AddSourceWithOriginal(CSharpSyntaxNode original, string hintName, TContext context)
		{
			CSharpSyntaxTree tree = context.CodeBuilder.ParseSyntaxTree();
			context.CodeBuilder.Clear();
			AddSource_Internal(original, tree, hintName, context);
		}

		/// <summary>
		/// Used to convert the specified <see cref="ISymbol"/> to <see cref="string"/> when applying the <see cref="Generator.DurianGeneratedAttribute"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to convert.</param>
		protected virtual string SymbolToString(ISymbol symbol)
		{
			if (symbol is IMethodSymbol method)
			{
				if (method.MethodKind == MethodKind.StaticConstructor)
				{
					return method.GetParentTypesString(false) + ".static " + method.ContainingType.Name + "()";
				}

				if (method.MethodKind == MethodKind.ExplicitInterfaceImplementation && method.ExplicitInterfaceImplementations.Length > 0)
				{
					IMethodSymbol interfaceMethod = method.ExplicitInterfaceImplementations[0];
					string interfaceName = interfaceMethod.ContainingType.GetGenericName();
					string methodName = method.GetParentTypesString(false) + '.' + interfaceMethod.GetGenericName(GenericSubstitution.ParameterList);
					return $"({interfaceName}){methodName}";
				}
			}

			return symbol.ToString();
		}

		/// <summary>
		/// Writes the <paramref name="generated"/> <see cref="CSharpSyntaxNode"/> and applies all needed code generation attributes.
		/// </summary>
		/// <param name="generated"><see cref="CSharpSyntaxNode"/> that was generated during the current generation pass.</param>
		/// <param name="original"><see cref="IMemberData"/> this <see cref="CSharpSyntaxNode"/> was generated from.</param>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		/// <param name="applyInheritdoc">Determines when to apply the <c>&lt;inheritdoc/&gt;</c> tag.</param>
		protected void WriteGeneratedMember(CSharpSyntaxNode generated, IMemberData original, TContext context, GenerateDocumentation applyInheritdoc = GenerateDocumentation.WhenPossible)
		{
			WriteGeneratedMember(NodeToString(generated), original, context, applyInheritdoc);
		}

		/// <summary>
		/// Writes the <paramref name="generated"/> <see cref="string"/> and applies all needed code generation attributes.
		/// </summary>
		/// <param name="generated"><see cref="string"/> that was generated during the current generation pass.</param>
		/// <param name="original"><see cref="IMemberData"/> this <see cref="string"/> was generated from.</param>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		/// <param name="applyInheritdoc">Determines when to apply the <c>&lt;inheritdoc/&gt;</c> tag.</param>
		protected void WriteGeneratedMember(string generated, IMemberData original, TContext context, GenerateDocumentation applyInheritdoc = GenerateDocumentation.WhenPossible)
		{
			string generatedFrom = AutoGenerated.GetDurianGeneratedAttribute(SymbolToString(original.Symbol));

			if (TryGetInheritdoc(original, applyInheritdoc, out string? inheritdoc))
			{
				WriteGeneratedMember_Internal(generated, generatedFrom, inheritdoc, context);
			}
			else
			{
				WriteGeneratedMember_Internal(generated, generatedFrom, context);
			}
		}

		/// <summary>
		/// Writes the <paramref name="generated"/> <see cref="CSharpSyntaxNode"/> and applies all needed code generation attributes.
		/// </summary>
		/// <param name="generated"><see cref="CSharpSyntaxNode"/> that was generated during the current generation pass.</param>
		/// <param name="original"><see cref="ISymbol"/> this <see cref="CSharpSyntaxNode"/> was generated from.</param>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		/// <param name="applyInheritdoc">Determines when to apply the <c>&lt;inheritdoc/&gt;</c> tag.</param>
		protected void WriteGeneratedMember(CSharpSyntaxNode generated, ISymbol original, TContext context, GenerateDocumentation applyInheritdoc = GenerateDocumentation.WhenPossible)
		{
			WriteGeneratedMember(NodeToString(generated), original, context, applyInheritdoc);
		}

		/// <summary>
		/// Writes the <paramref name="generated"/> <see cref="string"/> and applies all needed code generation attributes.
		/// </summary>
		/// <param name="generated"><see cref="string"/> that was generated during the current generation pass.</param>
		/// <param name="original"><see cref="ISymbol"/> this <see cref="string"/> was generated from.</param>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		/// <param name="applyInheritdoc">Determines when to apply the <c>&lt;inheritdoc/&gt;</c> tag.</param>
		protected void WriteGeneratedMember(string generated, ISymbol original, TContext context, GenerateDocumentation applyInheritdoc = GenerateDocumentation.WhenPossible)
		{
			string generatedFrom = AutoGenerated.GetDurianGeneratedAttribute(SymbolToString(original));

			if (TryGetInheritdoc(original, applyInheritdoc, out string? inheritdoc))
			{
				WriteGeneratedMember_Internal(generated, generatedFrom, inheritdoc, context);
			}
			else
			{
				WriteGeneratedMember_Internal(generated, generatedFrom, context);
			}
		}

		/// <summary>
		/// Writes the <paramref name="generated"/> <see cref="CSharpSyntaxNode"/> and applies all needed code generation attributes.
		/// </summary>
		/// <param name="generated"><see cref="CSharpSyntaxNode"/> that was generated during the current generation pass.</param>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		protected void WriteGeneratedMember(CSharpSyntaxNode generated, TContext context)
		{
			WriteGeneratedMember(NodeToString(generated), context);
		}

		/// <summary>
		/// Writes the <paramref name="generated"/> <see cref="string"/> and applies all needed code generation attributes.
		/// </summary>
		/// <param name="generated"><see cref="string"/> that was generated during the current generation pass.</param>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		protected void WriteGeneratedMember(string generated, TContext context)
		{
			WriteGeneratedMember_Internal(generated, AutoGenerated.GetDurianGeneratedAttribute(), context);
		}

		/// <summary>
		/// Writes the <paramref name="generated"/> <see cref="CSharpSyntaxNode"/> and applies all needed code generation attributes.
		/// </summary>
		/// <param name="generated"><see cref="CSharpSyntaxNode"/> that was generated during the current generation pass.</param>
		/// <param name="generatedFrom">Name of member this <see cref="CSharpSyntaxNode"/> was generated from.</param>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		protected void WriteGeneratedMember(CSharpSyntaxNode generated, string? generatedFrom, TContext context)
		{
			WriteGeneratedMember(NodeToString(generated), generatedFrom, context);
		}

		/// <summary>
		/// Writes the <paramref name="generated"/> <see cref="string"/> and applies all needed code generation attributes.
		/// </summary>
		/// <param name="generated"><see cref="string"/> that was generated during the current generation pass.</param>
		/// <param name="generatedFrom">Name of member this <see cref="string"/> was generated from.</param>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		protected void WriteGeneratedMember(string generated, string? generatedFrom, TContext context)
		{
			WriteGeneratedMember_Internal(generated, AutoGenerated.GetDurianGeneratedAttribute(generatedFrom), context);
		}

		/// <summary>
		/// Writes the <paramref name="generated"/> <see cref="CSharpSyntaxNode"/> and applies all needed code generation attributes.
		/// </summary>
		/// <param name="generated"><see cref="CSharpSyntaxNode"/> that was generated during the current generation pass.</param>
		/// <param name="generatedFrom">Name of member this <see cref="CSharpSyntaxNode"/> was generated from.</param>
		/// <param name="inheritdoc">Text to put in the 'inheritdoc' tag.</param>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		protected void WriteGeneratedMember(CSharpSyntaxNode generated, string? generatedFrom, string? inheritdoc, TContext context)
		{
			WriteGeneratedMember(NodeToString(generated), generatedFrom, inheritdoc, context);
		}

		/// <summary>
		/// Writes the <paramref name="generated"/> <see cref="string"/> and applies all needed code generation attributes.
		/// </summary>
		/// <param name="generated"><see cref="string"/> that was generated during the current generation pass.</param>
		/// <param name="generatedFrom">Name of member this <see cref="string"/> was generated from.</param>
		/// <param name="inheritdoc">Text to put in the 'inheritdoc' tag.</param>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		protected void WriteGeneratedMember(string generated, string? generatedFrom, string? inheritdoc, TContext context)
		{
			WriteGeneratedMember_Internal(generated, AutoGenerated.GetDurianGeneratedAttribute(generatedFrom), AutoGenerated.GetInheritdoc(inheritdoc), context);
		}

		/// <summary>
		/// Writes all the <paramref name="generated"/> <see cref="CSharpSyntaxNode"/>s and applies all needed code generation attributes.
		/// </summary>
		/// <param name="generated"><see cref="CSharpSyntaxNode"/>s that were generated during the current generation pass.</param>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		protected void WriteGeneratedMembers(CSharpSyntaxNode[] generated, TContext context)
		{
			WriteGeneratedMembers(ConvertString(generated), context);
		}

		/// <summary>
		/// Writes all the <paramref name="generated"/> <see cref="string"/>s and applies all needed code generation attributes.
		/// </summary>
		/// <param name="generated"><see cref="string"/>s that were generated during the current generation pass.</param>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		protected void WriteGeneratedMembers(string[] generated, TContext context)
		{
			WriteGeneratedMember(generated[0], context);

			int length = generated.Length;
			for (int i = 1; i < length; i++)
			{
				context.CodeBuilder.WriteLine();
				WriteGeneratedMember(generated[i], context);
			}
		}

		/// <summary>
		/// Writes all the <paramref name="generated"/> <see cref="CSharpSyntaxNode"/>s and applies all needed code generation attributes.
		/// </summary>
		/// <param name="generated"><see cref="CSharpSyntaxNode"/>s that were generated during the current generation pass.</param>
		/// <param name="original"><see cref="IMemberData"/> this <see cref="CSharpSyntaxNode"/>s were generated from.</param>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		/// <param name="applyInheritdoc">Determines when to apply the <c>&lt;inheritdoc/&gt;</c> tag.</param>
		protected void WriteGeneratedMembers(CSharpSyntaxNode[] generated, IMemberData original, TContext context, GenerateDocumentation applyInheritdoc = GenerateDocumentation.WhenPossible)
		{
			WriteGeneratedMembers(ConvertString(generated), original, context, applyInheritdoc);
		}

		/// <summary>
		/// Writes all the <paramref name="generated"/> <see cref="string"/>s and applies all needed code generation attributes.
		/// </summary>
		/// <param name="generated"><see cref="string"/>s that were generated during the current generation pass.</param>
		/// <param name="original"><see cref="IMemberData"/> this <see cref="string"/>s were generated from.</param>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		/// <param name="applyInheritdoc">Determines when to apply the <c>&lt;inheritdoc/&gt;</c> tag.</param>
		protected void WriteGeneratedMembers(string[] generated, IMemberData original, TContext context, GenerateDocumentation applyInheritdoc = GenerateDocumentation.WhenPossible)
		{
			if (generated.Length == 0)
			{
				return;
			}

			string generatedFrom = AutoGenerated.GetDurianGeneratedAttribute(SymbolToString(original.Symbol));
			int length = generated.Length;

			if (TryGetInheritdoc(original, applyInheritdoc, out string? inheritdoc))
			{
				WriteGeneratedMember_Internal(generated[0], generatedFrom, inheritdoc, context);

				for (int i = 1; i < length; i++)
				{
					context.CodeBuilder.WriteLine();
					WriteGeneratedMember_Internal(generated[i], generatedFrom, inheritdoc, context);
				}
			}
			else
			{
				WriteGeneratedMember_Internal(generated[0], generatedFrom, context);

				for (int i = 1; i < length; i++)
				{
					context.CodeBuilder.WriteLine();
					WriteGeneratedMember_Internal(generated[i], generatedFrom, context);
				}
			}
		}

		/// <summary>
		/// Writes all the <paramref name="generated"/> <see cref="CSharpSyntaxNode"/>s and applies all needed code generation attributes.
		/// </summary>
		/// <param name="generated"><see cref="CSharpSyntaxNode"/>s that were generated during the current generation pass.</param>
		/// <param name="original"><see cref="ISymbol"/> this <see cref="CSharpSyntaxNode"/>s were generated from.</param>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		/// <param name="applyInheritdoc">Determines when to apply the <c>&lt;inheritdoc/&gt;</c> tag.</param>
		protected void WriteGeneratedMembers(CSharpSyntaxNode[] generated, ISymbol original, TContext context, GenerateDocumentation applyInheritdoc = GenerateDocumentation.WhenPossible)
		{
			WriteGeneratedMembers(ConvertString(generated), original, context, applyInheritdoc);
		}

		/// <summary>
		/// Writes all the <paramref name="generated"/> <see cref="string"/>s and applies all needed code generation attributes.
		/// </summary>
		/// <param name="generated"><see cref="string"/>s that were generated during the current generation pass.</param>
		/// <param name="original"><see cref="ISymbol"/> this <see cref="string"/>s were generated from.</param>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		/// <param name="applyInheritdoc">Determines when to apply the <c>&lt;inheritdoc/&gt;</c> tag.</param>
		protected void WriteGeneratedMembers(string[] generated, ISymbol original, TContext context, GenerateDocumentation applyInheritdoc = GenerateDocumentation.WhenPossible)
		{
			if (generated.Length == 0)
			{
				return;
			}

			string generatedFrom = AutoGenerated.GetDurianGeneratedAttribute(SymbolToString(original));
			int length = generated.Length;

			if (TryGetInheritdoc(original, applyInheritdoc, out string? inheritdoc))
			{
				WriteGeneratedMember_Internal(generated[0], generatedFrom, inheritdoc, context);

				for (int i = 1; i < length; i++)
				{
					context.CodeBuilder.WriteLine();
					WriteGeneratedMember_Internal(generated[i], generatedFrom, inheritdoc, context);
				}
			}
			else
			{
				WriteGeneratedMember_Internal(generated[0], generatedFrom, context);

				for (int i = 1; i < length; i++)
				{
					context.CodeBuilder.WriteLine();
					WriteGeneratedMember_Internal(generated[i], generatedFrom, context);
				}
			}
		}

		private protected void WriteGeneratedMember_Internal(string generated, string generatedFrom, TContext context)
		{
			context.CodeBuilder.Indent();
			context.CodeBuilder.WriteLine(_autoGeneratedAttribute);
			context.CodeBuilder.Indent();
			context.CodeBuilder.WriteLine(generatedFrom);
			context.CodeBuilder.Indent();
			context.CodeBuilder.WriteLine(generated);
			context.CodeBuilder.WriteLine();
		}

		private protected void WriteGeneratedMember_Internal(string generated, string generatedFrom, string inheritdoc, TContext context)
		{
			context.CodeBuilder.Indent();
			context.CodeBuilder.WriteLine(inheritdoc);
			WriteGeneratedMember_Internal(generated, generatedFrom, context);
		}

		private static string[] ConvertString(CSharpSyntaxNode[] nodes)
		{
			return nodes.Select(n => NodeToString(n)).ToArray();
		}
	}

	/// <inheritdoc cref="DurianGeneratorWithBuilder{TContext}"/>
	public abstract class DurianGeneratorWithBuilder : DurianGeneratorWithBuilder<GeneratorPassBuilderContext>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGeneratorWithBuilder"/> class.
		/// </summary>
		protected DurianGeneratorWithBuilder()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGeneratorWithBuilder"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="DurianGeneratorWithBuilder"/> is initialized.</param>
		protected DurianGeneratorWithBuilder(in GeneratorLogCreationContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGeneratorWithBuilder"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		protected DurianGeneratorWithBuilder(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <inheritdoc/>
		protected override GeneratorPassBuilderContext CreateCurrentPassContext(ICompilationData currentCompilation, in GeneratorExecutionContext context)
		{
			return new GeneratorPassBuilderContext();
		}
	}
}
