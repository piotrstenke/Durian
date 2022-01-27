﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis
{
	/// <summary>
	/// A <see cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/> that uses a <see cref="Analysis.CodeBuilder"/> to generate code.
	/// </summary>
	/// <typeparam name="TCompilationData">User-defined type of <see cref="ICompilationData"/> this <see cref="IDurianSourceGenerator"/> operates on.</typeparam>
	/// <typeparam name="TSyntaxReceiver">User-defined type of <see cref="IDurianSyntaxReceiver"/> that provides the <see cref="CSharpSyntaxNode"/>s to perform the generation on.</typeparam>
	/// <typeparam name="TFilter">User-defined type of <see cref="ISyntaxFilter"/> that decides what <see cref="CSharpSyntaxNode"/>s collected by the <see cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}.SyntaxReceiver"/> are valid for generation.</typeparam>
	public abstract class DurianGeneratorWithBuilder<TCompilationData, TSyntaxReceiver, TFilter> : DurianGenerator<TCompilationData, TSyntaxReceiver, TFilter>
		where TCompilationData : class, ICompilationDataWithSymbols
		where TSyntaxReceiver : class, IDurianSyntaxReceiver
		where TFilter : notnull, IGeneratorSyntaxFilterWithDiagnostics
	{
		private readonly string _autoGeneratedAttribute;

		/// <summary>
		/// <see cref="Analysis.CodeBuilder"/> that is used to generate code.
		/// </summary>
		public CodeBuilder CodeBuilder { get; }

		/// <inheritdoc cref="DurianGeneratorWithBuilder(in LoggableGeneratorConstructionContext, IHintNameProvider?)"/>
		protected DurianGeneratorWithBuilder() : this(GeneratorLoggingConfiguration.Default, null)
		{
		}

		/// <inheritdoc cref="DurianGeneratorWithBuilder(in LoggableGeneratorConstructionContext, IHintNameProvider?)"/>
		protected DurianGeneratorWithBuilder(in LoggableGeneratorConstructionContext context) : this(in context, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGeneratorWithBuilder{TCompilationData, TSyntaxReceiver, TFilter}"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableSourceGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGeneratorWithBuilder(in LoggableGeneratorConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
			CodeBuilder = new(this);
			_autoGeneratedAttribute = AutoGenerated.GetGeneratedCodeAttribute(GetGeneratorName(), GetVersion());
		}

		/// <inheritdoc cref="DurianGeneratorWithBuilder(GeneratorLoggingConfiguration?, IHintNameProvider?)"/>
		protected DurianGeneratorWithBuilder(GeneratorLoggingConfiguration? loggingConfiguration) : this(loggingConfiguration, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGeneratorWithBuilder{TCompilationData, TSyntaxReceiver, TFilter}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGeneratorWithBuilder(GeneratorLoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
			CodeBuilder = new(this);
			_autoGeneratedAttribute = AutoGenerated.GetGeneratedCodeAttribute(GetGeneratorName(), GetVersion());
		}

		/// <summary>
		/// Adds the source created using the <see cref="CodeBuilder"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		/// <exception cref="InvalidOperationException"><see cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}.HasValidData"/> must be <see langword="true"/> in order to add new source.</exception>
		protected void AddSource(string hintName, in GeneratorExecutionContext context)
		{
			ThrowIfHasNoValidData();

			CSharpSyntaxTree tree = CodeBuilder.ParseSyntaxTree();
			CodeBuilder.Clear();
			AddSource_Internal(tree, hintName, in context);
		}

		/// <summary>
		/// Adds the source created using the <see cref="CodeBuilder"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="original">The <see cref="CSharpSyntaxNode"/> the source was generated from.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		/// <exception cref="InvalidOperationException"><see cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}.HasValidData"/> must be <see langword="true"/> in order to add new source.</exception>
		protected void AddSourceWithOriginal(CSharpSyntaxNode original, string hintName, in GeneratorExecutionContext context)
		{
			ThrowIfHasNoValidData();
			CSharpSyntaxTree tree = CodeBuilder.ParseSyntaxTree();
			CodeBuilder.Clear();
			AddSource_Internal(original, tree, hintName, in context);
		}

		/// <summary>
		/// Adds the text of the <see cref="CodeBuilder"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorPostInitializationContext"/> to add the source to.</param>
		protected void InitializeSource(string hintName, in GeneratorPostInitializationContext context)
		{
			CSharpSyntaxTree tree = CodeBuilder.ParseSyntaxTree();
			CodeBuilder.Clear();
			InitializeSource(tree, hintName, in context);
		}

		/// <summary>
		/// Writes the <paramref name="generated"/> <see cref="CSharpSyntaxNode"/> and applies all needed code generation attributes.
		/// </summary>
		/// <param name="generated"><see cref="CSharpSyntaxNode"/> that was generated during the current generation pass.</param>
		/// <param name="original"><see cref="IMemberData"/> this <see cref="CSharpSyntaxNode"/> was generated from.</param>
		/// <param name="applyInheritdocIfPossible">Determines whether to apply the <c>&lt;inheritdoc/&gt;</c> tag if the <paramref name="original"/> has a documentation comment.</param>
		protected void WriteGeneratedMember(CSharpSyntaxNode generated, IMemberData original, bool applyInheritdocIfPossible = true)
		{
			string generatedFrom = AutoGenerated.GetDurianGeneratedAttribute(original.Symbol.ToString());

			if (TryGetInheritdoc(original, applyInheritdocIfPossible, out string? inheritdoc))
			{
				WriteGeneratedMember_Internal(generated, generatedFrom, inheritdoc);
			}
			else
			{
				WriteGeneratedMember_Internal(generated, generatedFrom);
			}
		}

		/// <summary>
		/// Writes the <paramref name="generated"/> <see cref="CSharpSyntaxNode"/> and applies all needed code generation attributes.
		/// </summary>
		/// <param name="generated"><see cref="CSharpSyntaxNode"/> that was generated during the current generation pass.</param>
		protected void WriteGeneratedMember(CSharpSyntaxNode generated)
		{
			CodeBuilder.Indent();
			CodeBuilder.AppendLine(_autoGeneratedAttribute);
			CodeBuilder.Indent();
			CodeBuilder.AppendLine(generated.ToString());
		}

		/// <summary>
		/// Writes all the <paramref name="generated"/> <see cref="CSharpSyntaxNode"/>s and applies all needed code generation attributes.
		/// </summary>
		/// <param name="generated"><see cref="CSharpSyntaxNode"/>s that were generated during the current generation pass.</param>
		protected void WriteGeneratedMembers(CSharpSyntaxNode[] generated)
		{
			WriteGeneratedMember(generated[0]);

			int length = generated.Length;
			for (int i = 1; i < length; i++)
			{
				CodeBuilder.AppendLine();
				WriteGeneratedMember(generated[i]);
			}
		}

		/// <summary>
		/// Writes all the <paramref name="generated"/> <see cref="CSharpSyntaxNode"/>s and applies all needed code generation attributes.
		/// </summary>
		/// <param name="generated"><see cref="CSharpSyntaxNode"/>s that were generated during the current generation pass.</param>
		/// <param name="original"><see cref="IMemberData"/> this <see cref="CSharpSyntaxNode"/>s were generated from.</param>
		/// <param name="applyInheritdocIfPossible">Determines whether to apply the <c>&lt;inheritdoc/&gt;</c> tag if the <paramref name="original"/> has a documentation comment.</param>
		protected void WriteGeneratedMembers(CSharpSyntaxNode[] generated, IMemberData original, bool applyInheritdocIfPossible = true)
		{
			if (generated.Length == 0)
			{
				return;
			}

			string generatedFrom = AutoGenerated.GetDurianGeneratedAttribute(original.Symbol.ToString());
			int length = generated.Length;

			if (TryGetInheritdoc(original, applyInheritdocIfPossible, out string? inheritdoc))
			{
				WriteGeneratedMember_Internal(generated[0], generatedFrom, inheritdoc);

				for (int i = 1; i < length; i++)
				{
					CodeBuilder.AppendLine();
					WriteGeneratedMember_Internal(generated[i], generatedFrom, inheritdoc);
				}
			}
			else
			{
				WriteGeneratedMember_Internal(generated[0], generatedFrom);

				for (int i = 1; i < length; i++)
				{
					CodeBuilder.AppendLine();
					WriteGeneratedMember_Internal(generated[i], generatedFrom);
				}
			}
		}

		private static bool TryGetInheritdoc(IMemberData original, bool applyInheritdocIfPossible, [NotNullWhen(true)] out string? inheritdoc)
		{
			if (applyInheritdocIfPossible)
			{
				string? doc = original.GetInheritdocIfHasDocumentation();

				if (!string.IsNullOrWhiteSpace(doc))
				{
					inheritdoc = doc!;
					return true;
				}
			}

			inheritdoc = null;
			return false;
		}

		private void WriteGeneratedMember_Internal(CSharpSyntaxNode generated, string generatedFrom)
		{
			CodeBuilder.Indent();
			CodeBuilder.AppendLine(_autoGeneratedAttribute);
			CodeBuilder.Indent();
			CodeBuilder.AppendLine(generatedFrom);
			CodeBuilder.Indent();
			CodeBuilder.AppendLine(generated.ToString());
		}

		private void WriteGeneratedMember_Internal(CSharpSyntaxNode generated, string generatedFrom, string inheritdoc)
		{
			CodeBuilder.Indent();
			CodeBuilder.AppendLine(inheritdoc);
			WriteGeneratedMember_Internal(generated, generatedFrom);
		}
	}
}