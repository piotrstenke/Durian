// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using Durian.Analysis.CodeGeneration;
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
	public abstract partial class DurianGeneratorBase : IDurianGenerator
	{
		/// <inheritdoc/>
		public abstract string GeneratorName { get; }

		/// <inheritdoc/>
		public abstract string GeneratorVersion { get; }

		/// <summary>
		/// Unique identifier of the current instance.
		/// </summary>
		public Guid InstanceId { get; }

		/// <inheritdoc cref="IGeneratorLogHandler.LoggingConfiguration"/>
		public LoggingConfiguration LoggingConfiguration => LogHandler.LoggingConfiguration;

		/// <inheritdoc/>
		public IGeneratorLogHandler LogHandler { get; }

		/// <inheritdoc/>
		public virtual int NumStaticTrees => GetInitialSources().Count();

		/// <inheritdoc/>
		public virtual bool SupportsDynamicLoggingConfiguration => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGeneratorBase"/> class.
		/// </summary>
		protected DurianGeneratorBase()
		{
			LogHandler = InitLogHandler(null);
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
			LogHandler = InitLogHandler(LoggingConfiguration.ForGenerator(this, in context));
			InstanceId = Guid.NewGuid();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGeneratorBase"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		protected DurianGeneratorBase(LoggingConfiguration? loggingConfiguration)
		{
			LogHandler = InitLogHandler(loggingConfiguration);
			InstanceId = Guid.NewGuid();
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <inheritdoc/>
		public virtual bool Execute(in GeneratorExecutionContext context)
		{
			return PrepareForExecution(in context, out _);
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

		void ISourceGenerator.Execute(GeneratorExecutionContext context)
		{
			Execute(in context);
		}

		IGeneratorPassContext? IDurianGenerator.GetCurrentPassContext()
		{
			return GetCurrentPassContextCore();
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

		/// <summary>
		/// Adds the specified <paramref name="source"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="source">A <see cref="string"/> representation of the generated code.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorPostInitializationContext"/> to add the source to.</param>
		protected virtual void AddSource(string source, string hintName, in GeneratorPostInitializationContext context)
		{
			SyntaxTree tree = CSharpSyntaxTree.ParseText(source, encoding: Encoding.UTF8);
			AddSource(tree, hintName, in context);
		}

		/// <summary>
		/// Adds the specified <paramref name="syntaxTree"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="syntaxTree"><see cref="SyntaxTree"/> to add to the <paramref name="context"/>.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorPostInitializationContext"/> to add the source to.</param>
		protected virtual void AddSource(SyntaxTree syntaxTree, string hintName, in GeneratorPostInitializationContext context)
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
			SyntaxTree tree = CSharpSyntaxTree.ParseText(source, context.ParseOptions as CSharpParseOptions, encoding: Encoding.UTF8);
			AddSource(tree, hintName, in context);
		}

		/// <summary>
		/// Adds the specified <paramref name="syntaxTree"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="syntaxTree"><see cref="SyntaxTree"/> to add to the <paramref name="context"/>.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		protected virtual void AddSource(SyntaxTree syntaxTree, string hintName, in GeneratorExecutionContext context)
		{
			context.AddSource(hintName, syntaxTree.GetText(context.CancellationToken));
			LogSource(hintName, syntaxTree, context.CancellationToken);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing">Determines whether this method was called from the <see cref="Dispose()"/> method or object's finalizer.</param>
		protected virtual void Dispose(bool disposing)
		{
			// Do nothing by default.
		}

		/// <inheritdoc cref="IDurianGenerator.GetCurrentPassContext"/>
		protected virtual IGeneratorPassContext? GetCurrentPassContextCore()
		{
			return null;
		}

		private protected bool PrepareForExecution(in GeneratorExecutionContext context, [NotNullWhen(true)] out CSharpCompilation? compilation)
		{
			DurianModule[]? modules;

			if (SupportsDynamicLoggingConfiguration && LogHandler is DynamicGeneratorLogHandler logHandler)
			{
				if (FindDynamicLoggingConfiguration(in context, out modules) is LoggingConfiguration config)
				{
					logHandler.LoggingConfiguration = config;
				}
				else
				{
					logHandler.LoggingConfiguration = LoggingConfiguration.Default;
				}
			}
			else
			{
				modules = default;
			}

			return InitializeCompilation(in context, modules, out compilation);
		}

		private static LoggingConfiguration CreateDynamicLoggingConfiguration(AttributeData attribute)
		{
			LoggingConfigurationAttribute attr = new();

			if (attribute.TryGetNamedArgumentValue(nameof(EnableLoggingAttribute.EnableExceptions), out bool enableExceptions))
			{
				attr.EnableExceptions = enableExceptions;
			}

			if (attribute.TryGetNamedArgumentEnumValue(nameof(EnableLoggingAttribute.DefaultNodeOutput), out NodeOutput defaultNodeOutput))
			{
				attr.DefaultNodeOutput = defaultNodeOutput;
			}

			if (attribute.TryGetNamedArgumentValue(nameof(EnableLoggingAttribute.SupportsDiagnostics), out bool supportsDiagnostics))
			{
				attr.SupportsDiagnostics = supportsDiagnostics;
			}

			if (attribute.TryGetNamedArgumentEnumValue(nameof(EnableLoggingAttribute.SupportedLogs), out GeneratorLogs supportedLogs))
			{
				attr.SupportedLogs = supportedLogs;
			}

			if (attribute.TryGetNamedArgumentValue(nameof(EnableLoggingAttribute.LogDirectory), out string? logDirectory))
			{
				attr.LogDirectory = logDirectory;
			}

			if (attribute.TryGetNamedArgumentValue(nameof(EnableLoggingAttribute.RelativeToDefault), out bool relativeToDefault))
			{
				attr.RelativeToDefault = relativeToDefault;
			}

			return LoggingConfiguration.FromAttribute(attr as ILoggingConfigurationAttribute);
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
				compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(
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

		private LoggingConfiguration? FindDynamicLoggingConfiguration(in GeneratorExecutionContext context, out DurianModule[]? modules)
		{
			Compilation compilation = context.Compilation;

			if (compilation.GetTypeByMetadataName(typeof(EnableLoggingAttribute).ToString()) is not INamedTypeSymbol enableLoggingAttribute)
			{
				modules = default;
				return default;
			}

			ImmutableArray<AttributeData> assemblyAttributes = compilation.Assembly
				.GetAttributes()
				.Where(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, enableLoggingAttribute))
				.ToImmutableArray();

			if (assemblyAttributes.Length == 0)
			{
				modules = default;
				return default;
			}

			modules = GetRequiredModules();

			if (modules.Length == 0)
			{
				return default;
			}

			string[] moduleNames = new string[modules.Length];

			for (int i = 0; i < modules.Length; i++)
			{
				moduleNames[i] = ModuleIdentity.GetName(modules[i]);
			}

			foreach (AttributeData attr in assemblyAttributes)
			{
				TypedConstant argument = attr.GetConstructorArgument(0);

				if (!AnalysisUtilities.TryRetrieveModuleName(argument, out string? moduleName))
				{
					continue;
				}

				if (Array.IndexOf(moduleNames, moduleName) != -1)
				{
					LoggingConfiguration config = CreateDynamicLoggingConfiguration(attr);
					LoggingConfiguration.RegisterDynamic(moduleName, config);
					return config;
				}
			}

			return default;
		}

		private bool InitializeCompilation(in GeneratorExecutionContext context, DurianModule[]? modules, [NotNullWhen(true)] out CSharpCompilation? compilation)
		{
			if (IsValidCSharpCompilation(in context, out CSharpCompilation? c) && ValidateCompilation(c, in context))
			{
				modules ??= GetRequiredModules();
				EnableModules(ref c, in context, modules);

				compilation = c;
				return true;
			}

			compilation = default;
			return false;
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

		private IGeneratorLogHandler InitLogHandler(LoggingConfiguration? configuration)
		{
			if (SupportsDynamicLoggingConfiguration)
			{
				return new DynamicGeneratorLogHandler(configuration);
			}

			return new GeneratorLogHandler(configuration);
		}

		private void LogSource(string hintName, SyntaxTree syntaxTree, CancellationToken cancellationToken)
		{
			LogHandler.LogNode(syntaxTree.GetRoot(cancellationToken), hintName, NodeOutput.Node);
		}
	}
}
