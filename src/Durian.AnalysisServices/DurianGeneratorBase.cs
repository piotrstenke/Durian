// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using Durian.Analysis.Logging;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis
{
	/// <summary>
	/// Base class for all Durian generators.
	/// </summary>
	public abstract class DurianGeneratorBase : LoggableGenerator
	{
		private Analysis.ReadonlyContextualDiagnosticReceiver<GeneratorExecutionContext>? _diagnosticReceiver;

		private IHintNameProvider _fileNameProvider;

		/// <summary>
		/// A <see cref="IDiagnosticReceiver"/> that is used to report diagnostics.
		/// </summary>
		/// <remarks>Can be set only if <see cref="SupportsDiagnostics"/> is <see langword="false"/>.</remarks>
		/// <exception cref="InvalidOperationException">
		/// <see cref="DiagnosticReceiver"/> cannot be set if <see cref="SupportsDiagnostics"/> is <see langword="false"/>. -or-
		/// <see cref="DiagnosticReceiver"/> cannot be set to <see langword="null"/> if <see cref="SupportsDiagnostics"/> is <see langword="true"/>.
		/// </exception>
		public new Analysis.ReadonlyContextualDiagnosticReceiver<GeneratorExecutionContext>? DiagnosticReceiver
		{
			get => _diagnosticReceiver;
			set
			{
				if (!SupportsDiagnostics)
				{
					throw new InvalidOperationException($"{nameof(DiagnosticReceiver)} cannot be set if {nameof(SupportsDiagnostics)} is false!");
				}

				if (value is null)
				{
					if (SupportsDiagnostics)
					{
						throw new InvalidOperationException($"{nameof(DiagnosticReceiver)} cannot be set to null if {nameof(SupportsDiagnostics)} is true!");
					}
				}

				_diagnosticReceiver = value;
			}
		}

		/// <inheritdoc cref="LoggingConfiguration.EnableDiagnostics"/>
		[MemberNotNullWhen(true, nameof(DiagnosticReceiver))]
		public virtual bool EnableDiagnostics
		{
			get => LoggingConfiguration.EnableDiagnostics;
			set => LoggingConfiguration.EnableDiagnostics = value;
		}

		/// <inheritdoc cref="LoggingConfiguration.EnableExceptions"/>
		public virtual bool EnableExceptions
		{
			get => LoggingConfiguration.EnableExceptions;
			set => LoggingConfiguration.EnableExceptions = value;
		}

		/// <inheritdoc cref="LoggingConfiguration.EnableLogging"/>
		public virtual bool EnableLogging
		{
			get => LoggingConfiguration.EnableLogging;
			set => LoggingConfiguration.EnableLogging = value;
		}

		/// <summary>
		/// Creates names for generated files.
		/// </summary>
		/// <exception cref="ArgumentNullException"><see cref="FileNameProvider"/> cannot be <see langword="null"/>.</exception>
		public IHintNameProvider FileNameProvider
		{
			get => _fileNameProvider;
			set
			{
				if (value is null)
				{
					throw new ArgumentNullException(nameof(FileNameProvider));
				}

				_fileNameProvider = value;
			}
		}

		/// <summary>
		/// A <see cref="IDiagnosticReceiver"/> that is used to create log files outside of this <see cref="ISourceGenerator"/>.
		/// </summary>
		public DiagnosticReceiver LogReceiver { get; }

		/// <inheritdoc cref="LoggingConfiguration.SupportsDiagnostics"/>
		[MemberNotNullWhen(true, nameof(DiagnosticReceiver))]
		public bool SupportsDiagnostics
		{
			get => LoggingConfiguration.SupportsDiagnostics;
			set => LoggingConfiguration.SupportsDiagnostics = value;
		}

		/// <inheritdoc cref="DurianGeneratorBase(in ConstructionContext, IHintNameProvider)"/>
		protected DurianGeneratorBase() : this(LoggingConfiguration.Default, null)
		{
		}

		/// <inheritdoc cref="DurianGeneratorBase(in ConstructionContext, IHintNameProvider)"/>
		protected DurianGeneratorBase(in ConstructionContext context) : this(in context, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGeneratorBase"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGeneratorBase(in ConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context)
		{
			if (SupportsDiagnostics)
			{
				_diagnosticReceiver = Analysis.DiagnosticReceiverFactory.SourceGenerator();
			}

			_fileNameProvider = fileNameProvider ?? new SymbolNameToFile();
			LogReceiver = new(this);
		}

		/// <inheritdoc cref="DurianGeneratorBase(LoggingConfiguration?, IHintNameProvider?)"/>
		protected DurianGeneratorBase(LoggingConfiguration? loggingConfiguration) : this(loggingConfiguration, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGeneratorBase"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGeneratorBase(LoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration)
		{
			if (SupportsDiagnostics)
			{
				_diagnosticReceiver = Analysis.DiagnosticReceiverFactory.SourceGenerator();
			}

			_fileNameProvider = fileNameProvider ?? new SymbolNameToFile();
			LogReceiver = new(this);
		}

		/// <inheritdoc/>
		public override void Execute(in GeneratorExecutionContext context)
		{
			InitializeCompilation(in context, out _);
		}

		/// <inheritdoc/>
		public override void Initialize(GeneratorInitializationContext context)
		{
			context.RegisterForPostInitialization(InitializeStaticTrees);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{GetGeneratorName()} (v. {GetGeneratorVersion()})";
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/>s representing modules that should be enabled before the current generator pass is executed.
		/// </summary>
		protected abstract DurianModule[] GetEnabledModules();

		/// <summary>
		/// Returns name of this <see cref="IDurianGenerator"/>.
		/// </summary>
		protected virtual string? GetGeneratorName()
		{
			return null;
		}

		/// <summary>
		/// Returns a collection of <see cref="ISourceTextProvider"/>s that will be used to generate syntax trees statically during the generator's initialization process.
		/// </summary>
		protected virtual IEnumerable<ISourceTextProvider>? GetStaticSyntaxTrees()
		{
			return null;
		}

		/// <summary>
		/// Returns version of this <see cref="IDurianGenerator"/>.
		/// </summary>
		protected virtual string? GetGeneratorVersion()
		{
			return null;
		}

		/// <summary>
		/// Validates and initializes a <see cref="CSharpCompilation"/> provided by the <paramref name="context"/>.
		/// </summary>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> that provides a <see cref="CSharpCompilation"/> to validate and initialize.</param>
		/// <param name="compilation">The validated and initialized <see cref="CSharpCompilation"/>.</param>
		/// <returns><see langword="true"/> if the <paramref name="compilation"/> was successfully validated and initialized, <see langword="false"/> otherwise.</returns>
		protected bool InitializeCompilation(in GeneratorExecutionContext context, [NotNullWhen(true)] out CSharpCompilation? compilation)
		{
			if (context.Compilation is not CSharpCompilation c || !ValidateCompilation(c, in context))
			{
				compilation = null;
				return false;
			}

			compilation = c;
			return true;
		}

		/// <summary>
		/// Adds the specified <paramref name="source"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="source">A <see cref="string"/> representation of the generated code.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorPostInitializationContext"/> to add the source to.</param>
		protected virtual void AddSource(string source, string hintName, in GeneratorPostInitializationContext context)
		{
			CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(source, encoding: Encoding.UTF8);
			AddSource(tree, hintName, in context);
		}

		/// <summary>
		/// Adds the specified <paramref name="syntaxTree"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="syntaxTree"><see cref="CSharpSyntaxTree"/> to add to the <paramref name="context"/>.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorPostInitializationContext"/> to add the source to.</param>
		protected virtual void AddSource(CSharpSyntaxTree syntaxTree, string hintName, in GeneratorPostInitializationContext context)
		{
			context.AddSource(hintName, syntaxTree.GetText(context.CancellationToken));
			LogSource(hintName, syntaxTree, context.CancellationToken);
		}

		/// <summary>
		/// Adds the specified <paramref name="source"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="source">A <see cref="string"/> representation of the generated code.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		protected virtual void AddSource(string source, string hintName, in GeneratorExecutionContext context)
		{
			CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(source, encoding: Encoding.UTF8);
			AddSource(tree, hintName, in context);
		}

		/// <summary>
		/// Adds the specified <paramref name="syntaxTree"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="syntaxTree"><see cref="CSharpSyntaxTree"/> to add to the <paramref name="context"/>.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		protected virtual void AddSource(CSharpSyntaxTree syntaxTree, string hintName, in GeneratorExecutionContext context)
		{
			context.AddSource(hintName, syntaxTree.GetText(context.CancellationToken));
			LogSource(hintName, syntaxTree, context.CancellationToken);
		}

		/// <summary>
		/// Validates the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to validate.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to report <see cref="Diagnostic"/>s to.</param>
		protected virtual bool ValidateCompilation(CSharpCompilation compilation, in GeneratorExecutionContext context)
		{
			return true;
		}

		private void InitializeStaticTrees(GeneratorPostInitializationContext context)
		{
			IEnumerable<ISourceTextProvider>? syntaxTreeProviders = GetStaticSyntaxTrees();

			if (syntaxTreeProviders is null)
			{
				return;
			}

			string? generatorName = GetGeneratorName();
			string? generatorVersion = GetGeneratorVersion();

			foreach (ISourceTextProvider treeProvider in syntaxTreeProviders)
			{
				if (context.CancellationToken.IsCancellationRequested)
				{
					break;
				}

				string hintName = treeProvider.GetHintName();
				string text = treeProvider.GetText();

				text = AutoGenerated.ApplyHeader(text, generatorName, generatorVersion);
				SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(text, encoding: Encoding.UTF8);

				context.AddSource(hintName, text);
				LogSource(hintName, syntaxTree, context.CancellationToken);
			}
		}

		private void LogSource(string hintName, SyntaxTree syntaxTree, CancellationToken cancellationToken)
		{
			if (LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.Node))
			{
				LogNode_Internal(syntaxTree.GetRoot(cancellationToken), hintName);
			}
		}
	}
}