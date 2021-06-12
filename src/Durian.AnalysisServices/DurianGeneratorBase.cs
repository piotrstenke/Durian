// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using Durian.Generator.Logging;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Durian.Generator
{
	/// <summary>
	/// Base class for all Durian generators.
	/// </summary>
	[DebuggerDisplay("Name = {GetGeneratorName()}, Version = {GetVersion()}")]
	public abstract class DurianGeneratorBase : LoggableSourceGenerator
	{
		private ReadonlyContextualDiagnosticReceiver<GeneratorExecutionContext>? _diagnosticReceiver;
		private IHintNameProvider _fileNameProvider;

		/// <summary>
		/// A <see cref="IDiagnosticReceiver"/> that is used to report diagnostics.
		/// </summary>
		/// <remarks>Can be set only if <see cref="SupportsDiagnostics"/> is <see langword="true"/>.</remarks>
		/// <exception cref="InvalidOperationException">
		/// <see cref="DiagnosticReceiver"/> cannot be set if <see cref="SupportsDiagnostics"/> is <see langword="false"/>. -or-
		/// <see cref="DiagnosticReceiver"/> cannot be set to <see langword="null"/> if <see cref="SupportsDiagnostics"/> is <see langword="true"/>.
		/// </exception>
		[DisallowNull]
		public ReadonlyContextualDiagnosticReceiver<GeneratorExecutionContext>? DiagnosticReceiver
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
				else
				{
					_diagnosticReceiver = value;
				}
			}
		}

		/// <inheritdoc/>
		/// <exception cref="InvalidOperationException"><see cref="EnableDiagnostics"/> cannot be set to <see langword="true"/> if <see cref="SupportsDiagnostics"/> is <see langword="false"/>.</exception>
		[MemberNotNullWhen(true, nameof(DiagnosticReceiver))]
		public virtual bool EnableDiagnostics
		{
			get => LoggingConfiguration.EnableDiagnostics;
			set => LoggingConfiguration.EnableDiagnostics = value;
		}

		/// <inheritdoc cref="GeneratorLoggingConfiguration.EnableExceptions"/>
		public virtual bool EnableExceptions
		{
			get => LoggingConfiguration.EnableExceptions;
			set => LoggingConfiguration.EnableExceptions = value;
		}

		/// <inheritdoc cref="GeneratorLoggingConfiguration.EnableLogging"/>
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
		public LoggableGeneratorDiagnosticReceiver LogReceiver { get; }

		/// <inheritdoc/>
		[MemberNotNullWhen(true, nameof(DiagnosticReceiver))]
		public virtual bool SupportsDiagnostics => LoggingConfiguration.SupportsDiagnostics;

		/// <inheritdoc cref="DurianGeneratorBase(in LoggableGeneratorConstructionContext, IHintNameProvider)"/>
		protected DurianGeneratorBase() : this(GeneratorLoggingConfiguration.Default, null)
		{
		}

		/// <inheritdoc cref="DurianGeneratorBase(in LoggableGeneratorConstructionContext, IHintNameProvider)"/>
		protected DurianGeneratorBase(in LoggableGeneratorConstructionContext context) : this(in context, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGeneratorBase"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableSourceGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGeneratorBase(in LoggableGeneratorConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context)
		{
			if (SupportsDiagnostics)
			{
				_diagnosticReceiver = DiagnosticReceiverFactory.SourceGenerator();
			}

			_fileNameProvider = fileNameProvider ?? new SymbolNameToFile();
			LogReceiver = new(this);
		}

		/// <inheritdoc cref="DurianGeneratorBase(GeneratorLoggingConfiguration?, IHintNameProvider?)"/>
		protected DurianGeneratorBase(GeneratorLoggingConfiguration? loggingConfiguration) : this(loggingConfiguration, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGeneratorBase"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGeneratorBase(GeneratorLoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration)
		{
			if (SupportsDiagnostics)
			{
				_diagnosticReceiver = DiagnosticReceiverFactory.SourceGenerator();
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
			// Do nothing.
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{GetGeneratorName()} (v. {GetVersion()})";
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/>s representing modules that should be enabled before the current generator pass is executed.
		/// </summary>
		protected abstract DurianModule[] GetEnabledModules();

		/// <summary>
		/// Returns name of this <see cref="IDurianSourceGenerator"/>.
		/// </summary>
		protected virtual string GetGeneratorName()
		{
			return nameof(DurianGenerator);
		}

		/// <summary>
		/// Returns an array of <see cref="CSharpSyntaxTree"/>s that are generated statically at the generation's start.
		/// </summary>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		protected virtual (CSharpSyntaxTree tree, string hintName)[]? GetStaticSyntaxTrees(CancellationToken cancellationToken)
		{
			return null;
		}

		/// <summary>
		/// Returns version of this <see cref="IDurianSourceGenerator"/>.
		/// </summary>
		protected virtual string GetVersion()
		{
			return "1.0.0";
		}

		/// <inheritdoc/>
		protected bool InitializeCompilation(in GeneratorExecutionContext context, [NotNullWhen(true)] out CSharpCompilation? compilation)
		{
			if (context.Compilation is not CSharpCompilation c || c.LanguageVersion < LanguageVersion.CSharp9 || !HasReferenceToCoreProject(c))
			{
				compilation = null;
				return false;
			}

			DurianModule[] modules = GetEnabledModules();
			EnableModules(ref c, in context, modules);
			InitializeStaticTrees(ref c, in context);

			compilation = c;
			return true;
		}

		/// <summary>
		/// Adds the specified <paramref name="source"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="source">A <see cref="string"/> representation of the generated code.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorPostInitializationContext"/> to add the source to.</param>
		protected void InitializeSource(string source, string hintName, in GeneratorPostInitializationContext context)
		{
			CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(source);
			InitializeSource(tree, hintName, in context);
		}

		/// <summary>
		/// Adds the specified <paramref name="tree"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="tree"><see cref="CSharpSyntaxTree"/> to add to the <paramref name="context"/>.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorPostInitializationContext"/> to add the source to.</param>
		protected void InitializeSource(CSharpSyntaxTree tree, string hintName, in GeneratorPostInitializationContext context)
		{
			context.AddSource(hintName, tree.GetText(context.CancellationToken));

			if (LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.Node))
			{
				LogNode_Internal(tree.GetRoot(context.CancellationToken), hintName);
			}
		}

		private static void EnableModules(ref CSharpCompilation compilation, in GeneratorExecutionContext context, DurianModule[] modules)
		{
			foreach (DurianModule module in modules)
			{
				if (!ModuleUtilities.IsEnabled(module, compilation))
				{
					string source = AutoGenerated.ApplyHeader($"[assembly: global::{DurianStrings.GeneratorNamespace}.{nameof(EnableModuleAttribute)}({DurianStrings.InfoNamespace}.{nameof(DurianModule)}.{module})]\r\n");
					context.AddSource($"__enabled__{module}", SourceText.From(source, Encoding.UTF8));
					compilation = compilation.AddSyntaxTrees((CSharpSyntaxTree)CSharpSyntaxTree.ParseText(source, context.ParseOptions as CSharpParseOptions, encoding: Encoding.UTF8, cancellationToken: context.CancellationToken));
				}
			}
		}

		private static bool HasReferenceToCoreProject(CSharpCompilation compilation)
		{
			return compilation.ReferencedAssemblyNames.Any(r => r.Name == "Durian.Core");
		}

		private void InitializeStaticTrees(ref CSharpCompilation compilation, in GeneratorExecutionContext context)
		{
			(CSharpSyntaxTree tree, string hintName)[]? trees = GetStaticSyntaxTrees(context.CancellationToken);

			if (trees is null)
			{
				return;
			}

			foreach ((CSharpSyntaxTree tree, string hintName) in trees)
			{
				context.AddSource(hintName, tree.GetText(context.CancellationToken));
				compilation = compilation.AddSyntaxTrees(tree);

				if (LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.Node))
				{
					LogNode_Internal(tree.GetRoot(context.CancellationToken), hintName);
				}
			}
		}
	}
}
