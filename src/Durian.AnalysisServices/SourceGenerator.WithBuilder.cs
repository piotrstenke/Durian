﻿using System;
using System.Diagnostics.CodeAnalysis;
using Durian.Generator.Data;
using Durian.Generator.Extensions;
using Durian.Generator.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Generator
{
	public abstract partial class SourceGenerator<TCompilationData, TSyntaxReceiver, TFilter> where TCompilationData : class, ICompilationData
		where TSyntaxReceiver : class, IDurianSyntaxReceiver
		where TFilter : notnull, IGeneratorSyntaxFilterWithDiagnostics
	{
		/// <summary>
		/// A <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/> that uses a <see cref="Generator.CodeBuilder"/> to generate code.
		/// </summary>
		public abstract class WithBuilder : SourceGenerator<TCompilationData, TSyntaxReceiver, TFilter>
		{
			private readonly string _autoGeneratedAttribute;

			/// <summary>
			/// <see cref="Generator.CodeBuilder"/> that is used to generate code.
			/// </summary>
			public CodeBuilder CodeBuilder { get; }

			/// <summary>
			/// Initializes a new instance of the <see cref="WithBuilder"/> class.
			/// </summary>
			/// <param name="checkForConfigurationAttribute">Determines whether to try to create a <see cref="GeneratorLoggingConfiguration"/> based on one of the logging attributes.
			/// <para>See: <see cref="GeneratorLoggingConfigurationAttribute"/>, <see cref="DefaultGeneratorLoggingConfigurationAttribute"/></para></param>
			/// <param name="enableLoggingIfSupported">Determines whether to enable logging for this <see cref="WithBuilder"/> instance if logging is supported.</param>
			/// <param name="enableDiagnosticsIfSupported">Determines whether to set <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}.EnableDiagnostics"/> to <see langword="true"/> if <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}.SupportsDiagnostics"/> is <see langword="true"/>.</param>
			/// <param name="enableExceptionsIfDebug">Determines whether to set <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}.EnableExceptions"/> to <see langword="true"/> if the DEBUG symbol is present and the initial value of <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}.EnableExceptions"/> is <see langword="false"/>.</param>
			protected WithBuilder(bool checkForConfigurationAttribute, bool enableLoggingIfSupported = true, bool enableDiagnosticsIfSupported = true, bool enableExceptionsIfDebug = true) : base(checkForConfigurationAttribute, enableLoggingIfSupported, enableDiagnosticsIfSupported, enableExceptionsIfDebug)
			{
				CodeBuilder = new(this);
				_autoGeneratedAttribute = AutoGenerated.GetGeneratedCodeAttribute(GetGeneratorName(), GetVersion());
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="WithBuilder"/> class.
			/// </summary>
			/// <param name="checkForConfigurationAttribute">Determines whether to try to create a <see cref="GeneratorLoggingConfiguration"/> based on one of the logging attributes.
			/// <para>See: <see cref="GeneratorLoggingConfigurationAttribute"/>, <see cref="DefaultGeneratorLoggingConfigurationAttribute"/></para></param>
			/// <param name="enableLoggingIfSupported">Determines whether to enable logging for this <see cref="WithBuilder"/> instance if logging is supported.</param>
			/// <param name="enableDiagnosticsIfSupported">Determines whether to set <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}.EnableDiagnostics"/> to <see langword="true"/> if <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}.SupportsDiagnostics"/> is <see langword="true"/>.</param>
			/// <param name="enableExceptionsIfDebug">Determines whether to set <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}.EnableExceptions"/> to <see langword="true"/> if the DEBUG symbol is present and the initial value of <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}.EnableExceptions"/> is <see langword="false"/>.</param>
			/// <param name="fileNameProvider">Creates names for generated files.</param>
			/// <exception cref="ArgumentNullException"><paramref name="fileNameProvider"/> is <see langword="null"/>.</exception>
			protected WithBuilder(bool checkForConfigurationAttribute, bool enableLoggingIfSupported, bool enableDiagnosticsIfSupported, bool enableExceptionsIfDebug, IFileNameProvider fileNameProvider) : base(checkForConfigurationAttribute, enableLoggingIfSupported, enableDiagnosticsIfSupported, enableExceptionsIfDebug, fileNameProvider)
			{
				CodeBuilder = new(this);
				_autoGeneratedAttribute = AutoGenerated.GetGeneratedCodeAttribute(GetGeneratorName(), GetVersion());
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="WithBuilder"/> class.
			/// </summary>
			/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
			protected WithBuilder(GeneratorLoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
			{
				CodeBuilder = new(this);
				_autoGeneratedAttribute = AutoGenerated.GetGeneratedCodeAttribute(GetGeneratorName(), GetVersion());
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="WithBuilder"/> class.
			/// </summary>
			/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
			/// <param name="fileNameProvider">Creates names for generated files.</param>
			/// <exception cref="ArgumentNullException"><paramref name="fileNameProvider"/> is <see langword="null"/>.</exception>
			protected WithBuilder(GeneratorLoggingConfiguration? loggingConfiguration, IFileNameProvider fileNameProvider) : base(loggingConfiguration, fileNameProvider)
			{
				CodeBuilder = new(this);
				_autoGeneratedAttribute = AutoGenerated.GetGeneratedCodeAttribute(GetGeneratorName(), GetVersion());
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

				if (TryGetInheritdoc(original, applyInheritdocIfPossible, out string? inheritdoc))
				{
					WriteGeneratedMember_Internal(generated[0], generatedFrom, inheritdoc);

					int length = generated.Length;
					for (int i = 1; i < length; i++)
					{
						CodeBuilder.AppendLine();
						WriteGeneratedMember_Internal(generated[i], generatedFrom, inheritdoc);
					}
				}
				else
				{
					WriteGeneratedMember_Internal(generated[0], generatedFrom);

					int length = generated.Length;
					for (int i = 1; i < length; i++)
					{
						CodeBuilder.AppendLine();
						WriteGeneratedMember_Internal(generated[i], generatedFrom);
					}
				}
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
			/// Adds the source created using the <see cref="CodeBuilder"/> to the <paramref name="context"/>.
			/// </summary>
			/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
			/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
			/// <exception cref="InvalidOperationException"><see cref="HasValidData"/> must be <see langword="true"/> in order to add new source.</exception>
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
			/// <exception cref="InvalidOperationException"><see cref="HasValidData"/> must be <see langword="true"/> in order to add new source.</exception>
			protected void AddSourceWithOriginal(CSharpSyntaxNode original, string hintName, in GeneratorExecutionContext context)
			{
				ThrowIfHasNoValidData();
				CSharpSyntaxTree tree = CodeBuilder.ParseSyntaxTree();
				CodeBuilder.Clear();
				AddSource_Internal(original, tree, hintName, in context);
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
		}
	}
}
