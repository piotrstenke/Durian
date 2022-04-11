// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using Durian.Analysis.Extensions;
using Durian.Analysis.Logging;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Durian.Analysis
{
	/// <summary>
	/// Base class for all Durian generators.
	/// </summary>
	public abstract class DurianGeneratorBase : IDurianGenerator
	{
		#region Diagnostics copied from Durian.Core.Analyzer

#pragma warning disable IDE1006 // Naming Styles
		private static readonly DiagnosticDescriptor DUR0001_ProjectMustReferenceDurianCore = new(
#pragma warning restore IDE1006 // Naming Styles
#pragma warning disable RS2008 // Enable analyzer release tracking
			id: "DUR0001",
#pragma warning restore RS2008 // Enable analyzer release tracking
			title: "Projects with any Durian analyzer must reference the Durian.Core package",
			messageFormat: "Projects with any Durian analyzer must reference the Durian.Core package",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: GlobalInfo.Repository + "/tree/master/docs/Core/DUR0001.md",
			isEnabledByDefault: true
		);

#pragma warning disable IDE1006 // Naming Styles
		private static readonly DiagnosticDescriptor DUR0004_DurianModulesAreValidOnlyInCSharp = new(
#pragma warning restore IDE1006 // Naming Styles
#pragma warning disable RS2008 // Enable analyzer release tracking
			id: "DUR0004",
#pragma warning restore RS2008 // Enable analyzer release tracking
			title: "Durian modules can be used only in C#",
			messageFormat: "Durian modules can be used only in C#",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: GlobalInfo.Repository + "/tree/master/docs/Core/DUR0004.md",
			isEnabledByDefault: true
		);

		#endregion Diagnostics copied from Durian.Core.Analyzer

		/// <summary>
		/// Unique identifier of the current instance.
		/// </summary>
		public Guid InstanceId { get; }

		/// <inheritdoc/>
		public IGeneratorLogHandler LogHandler { get; }

		/// <inheritdoc cref="IGeneratorLogHandler.LoggingConfiguration"/>
		public LoggingConfiguration LoggingConfiguration => LogHandler.LoggingConfiguration;

		/// <inheritdoc/>
		public virtual int NumStaticTrees => GetInitialSources().Count();

		/// <inheritdoc/>
		public abstract string GeneratorName { get; }

		/// <inheritdoc/>
		public abstract string GeneratorVersion { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGeneratorBase"/> class.
		/// </summary>
		protected DurianGeneratorBase()
		{
			LogHandler = new GeneratorLogHandler();
			InstanceId = Guid.NewGuid();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGeneratorBase"/> class.
		/// </summary>
		/// <param name="logHandler">Service that handles log files for this generator.</param>
		/// <exception cref="ArgumentNullException"><paramref name="logHandler"/> is <see langword="null"/>.</exception>
		protected DurianGeneratorBase(IGeneratorLogHandler logHandler)
		{
			if (logHandler is null)
			{
				throw new ArgumentNullException(nameof(logHandler));
			}

			LogHandler = logHandler;
			InstanceId = Guid.NewGuid();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGeneratorBase"/> class.
		/// </summary>
		/// <param name="context">Configures how this generator is initialized.</param>
		protected DurianGeneratorBase(in GeneratorLogCreationContext context)
		{
			LogHandler = new GeneratorLogHandler(LoggingConfiguration.CreateForGenerator(this, in context));
			InstanceId = Guid.NewGuid();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGeneratorBase"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		protected DurianGeneratorBase(LoggingConfiguration? loggingConfiguration)
		{
			LogHandler = new GeneratorLogHandler(loggingConfiguration);
			InstanceId = Guid.NewGuid();
		}

		/// <inheritdoc/>
		public virtual bool Execute(in GeneratorExecutionContext context)
		{
			return InitializeCompilation(in context, out _);
		}

		/// <inheritdoc cref="IDurianGenerator.GetCurrentPassContext"/>
		public IGeneratorPassContext? GetCurrentPassContext()
		{
			return GetCurrentPassContextCore();
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return InstanceId.GetHashCode();
		}

		/// <summary>
		/// Returns a collection of <see cref="ISourceTextProvider"/>s that will be used to generate syntax trees statically during the generator's initialization process.
		/// </summary>
		public virtual IEnumerable<ISourceTextProvider>? GetInitialSources()
		{
			return null;
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/>s representing modules that should be enabled before the current generator pass is executed.
		/// </summary>
		public abstract DurianModule[] GetRequiredModules();

		/// <inheritdoc/>
		public virtual void Initialize(GeneratorInitializationContext context)
		{
			context.RegisterForPostInitialization(InitializeStaticTrees);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{GeneratorName} (v. {GeneratorVersion})";
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
			CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(source, (CSharpParseOptions)context.ParseOptions, encoding: Encoding.UTF8);
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

		/// <inheritdoc cref="IDurianGenerator.GetCurrentPassContext"/>
		protected virtual IGeneratorPassContext? GetCurrentPassContextCore()
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
			if (IsValidCSharpCompilation(in context, out CSharpCompilation? c) && ValidateCompilation(c, in context))
			{
				DurianModule[] modules = GetRequiredModules();
				EnableModules(ref c, in context, modules);

				compilation = c;
				return true;
			}

			compilation = default;
			return false;
		}

		/// <summary>
		/// Validates the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to validate.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to report <see cref="Diagnostic"/>s to.</param>
		protected internal virtual bool ValidateCompilation(CSharpCompilation compilation, in GeneratorExecutionContext context)
		{
			return true;
		}

		void ISourceGenerator.Execute(GeneratorExecutionContext context)
		{
			Execute(in context);
		}

		IGeneratorPassContext? IDurianGenerator.GetCurrentPassContext()
		{
			return GetCurrentPassContextCore();
		}

		private static void EnableModules(ref CSharpCompilation compilation, in GeneratorExecutionContext context, DurianModule[] modules)
		{
			AttributeData[] attributes = ModuleUtilities.GetInstancesOfEnableAttribute(compilation);
			bool[] enabled = new bool[modules.Length];

			foreach (AttributeData attribute in attributes)
			{
				if (!attribute.TryGetConstructorArgumentValue(0, out int value))
				{
					continue;
				}

				DurianModule module = (DurianModule)value;

				for (int i = 0; i < modules.Length; i++)
				{
					if (modules[i] == module)
					{
						enabled[i] = true;
					}
				}
			}

			for (int i = 0; i < modules.Length; i++)
			{
				if (enabled[i])
				{
					continue;
				}

				DurianModule module = modules[i];

				string source = AutoGenerated.ApplyHeader($"[assembly: global::{DurianStrings.GeneratorNamespace}.EnableModule({DurianStrings.InfoNamespace}.{nameof(DurianModule)}.{module})]\r\n");

				context.AddSource($"__EnableModule__{module}", SourceText.From(source, Encoding.UTF8));
				compilation = compilation.AddSyntaxTrees((CSharpSyntaxTree)CSharpSyntaxTree.ParseText(
					source,
					context.ParseOptions as CSharpParseOptions,
					encoding: Encoding.UTF8,
					cancellationToken: context.CancellationToken)
				);
			}
		}

		private static bool HasValidReferences(Compilation compilation, out bool hasCoreAnalyzer)
		{
			bool foundAnalyzer = false;
			bool currentValue = false;

			foreach (AssemblyIdentity assembly in compilation.ReferencedAssemblyNames)
			{
				if (assembly.Name == "Durian")
				{
					hasCoreAnalyzer = true;
					return true;
				}

				if (assembly.Name == "Durian.Core")
				{
					if (foundAnalyzer)
					{
						hasCoreAnalyzer = true;
						return true;
					}

					currentValue = true;
					continue;
				}

				if (assembly.Name == "Durian.Core.Analyzer")
				{
					if (currentValue)
					{
						hasCoreAnalyzer = true;
						return true;
					}

					foundAnalyzer = true;
				}
			}

			hasCoreAnalyzer = false;
			return currentValue;
		}

		private void InitializeStaticTrees(GeneratorPostInitializationContext context)
		{
			IEnumerable<ISourceTextProvider>? syntaxTreeProviders = GetInitialSources();

			if (syntaxTreeProviders is null)
			{
				return;
			}

			string? generatorName = GeneratorName;
			string? generatorVersion = GeneratorVersion;

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

		private static bool IsValidCSharpCompilation(in GeneratorExecutionContext context, [NotNullWhen(true)] out CSharpCompilation? compilation)
		{
			bool isValid = true;

			if (!HasValidReferences(context.Compilation, out bool hasCoreAnalyzer))
			{
				if (!hasCoreAnalyzer)
				{
					context.ReportDiagnostic(Diagnostic.Create(DUR0001_ProjectMustReferenceDurianCore, Location.None));
				}

				isValid = false;
			}

			if (context.Compilation is CSharpCompilation c)
			{
				compilation = c;
				return isValid;
			}
			else if (!hasCoreAnalyzer)
			{
				context.ReportDiagnostic(Diagnostic.Create(DUR0004_DurianModulesAreValidOnlyInCSharp, Location.None));
			}

			compilation = default;
			return false;
		}

		private void LogSource(string hintName, SyntaxTree syntaxTree, CancellationToken cancellationToken)
		{
			LogHandler.LogNode(syntaxTree.GetRoot(cancellationToken), hintName, NodeOutput.Node);
		}
	}
}
