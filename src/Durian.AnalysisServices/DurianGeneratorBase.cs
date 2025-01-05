using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using Durian.Analysis.CodeGeneration;
using Durian.Analysis.Logging;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Durian.Analysis;

/// <summary>
/// Base class for all Durian source generators.
/// </summary>
public abstract partial class DurianGeneratorBase : IDurianGenerator, ILoggableSourceGenerator
{
	private readonly LoggingConfiguration? _initialConfig;

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
	[MemberNotNullWhen(true, nameof(_initialConfig))]
#pragma warning disable CS8775 // Member must have a non-null value when exiting in some condition.
	public virtual bool SupportsDynamicLoggingConfiguration => true;
#pragma warning restore CS8775 // Member must have a non-null value when exiting in some condition.

	/// <summary>
	/// Initializes a new instance of the <see cref="DurianGeneratorBase"/> class.
	/// </summary>
	protected DurianGeneratorBase()
	{
		LogHandler = InitLogHandler(null);

		if (SupportsDynamicLoggingConfiguration)
		{
			_initialConfig = LoggingConfiguration.ForGenerator(this);
		}

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
	public override int GetHashCode()
	{
		return InstanceId.GetHashCode();
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc/>
	public override string ToString()
	{
		return $"{GeneratorName} (v. {GeneratorVersion})";
	}

	/// <summary>
	/// Returns a collection of <see cref="ISourceTextProvider"/>s that will be used to generate syntax trees statically during the generator's initialization process.
	/// </summary>
	protected internal virtual IEnumerable<ISourceTextProvider>? GetInitialSources()
	{
		return null;
	}

	/// <summary>
	/// Returns an array of <see cref="DurianModule"/>s representing modules that should be enabled before the current generator pass is executed.
	/// </summary>
	protected internal abstract DurianModule[] GetRequiredModules();

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	/// <param name="disposing">Determines whether this method was called from the <see cref="Dispose()"/> method or object's finalizer.</param>
	protected virtual void Dispose(bool disposing)
	{
		// Do nothing by default.
	}

	/// <summary>
	/// Validates the specified <paramref name="compilation"/>.
	/// </summary>
	/// <param name="compilation"><see cref="CSharpCompilation"/> to validate.</param>
	/// <param name="reportDiagnostic">Action used to report <see cref="Diagnostic"/>s.</param>
	protected internal virtual bool ValidateCompilation(CSharpCompilation compilation, Action<Diagnostic> reportDiagnostic)
	{
		return true;
	}

	/// <summary>
	/// Initializes static syntax trees.
	/// </summary>
	/// <param name="addSource">Action used to add the generated source text.</param>
	/// <param name="cancellationToken">Token used to cancel the operation.</param>
	protected void InitializeStaticTrees(Action<string, SourceText> addSource, CancellationToken cancellationToken)
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
			if (cancellationToken.IsCancellationRequested)
			{
				break;
			}

			string hintName = treeProvider.GetHintName();
			string text = treeProvider.GetText();

			text = AutoGenerated.ApplyHeader(text, generatorName, generatorVersion);
			SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(text, encoding: Encoding.UTF8, cancellationToken: cancellationToken);

			SourceText source = syntaxTree.GetText(cancellationToken);

			addSource(hintName, source);
			LogSource(hintName, syntaxTree, cancellationToken);
		}
	}

	/// <summary>
	/// Prepares then generator for a generation pass.
	/// </summary>
	/// <param name="compilation">Current compilation.</param>
	/// <param name="addSource">Action used to add new source to the compilation.</param>
	/// <param name="reportDiagnostic">Action used to report diagnostics.</param>
	protected CSharpCompilation? PrepareForExecution(Compilation compilation, Func<string, SourceText, CSharpCompilation, CSharpCompilation> addSource, Action<Diagnostic> reportDiagnostic)
	{
		DurianModule[]? modules;

		if (SupportsDynamicLoggingConfiguration && LogHandler is DynamicGeneratorLogHandler logHandler)
		{
			if (FindDynamicLoggingConfiguration(compilation, out modules) is LoggingConfiguration config)
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

		return InitializeCompilation(compilation, modules, addSource, reportDiagnostic);
	}

	/// <summary>
	/// Logs the specified <paramref name="syntaxTree"/>.
	/// </summary>
	/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
	/// <param name="syntaxTree"><see cref="SyntaxTree"/> to log.</param>
	/// <param name="cancellationToken"><see cref="CancellationToken"/> used to cancel the operation.</param>
	protected void LogSource(string hintName, SyntaxTree syntaxTree, CancellationToken cancellationToken)
	{
		LogHandler.LogNode(syntaxTree.GetRoot(cancellationToken), hintName, NodeOutput.Node);
	}

	private LoggingConfiguration CreateDynamicLoggingConfiguration(AttributeData attribute)
	{
		LoggingConfigurationAttribute attr = new();
		bool isChanged = false;

		if (attribute.TryGetNamedArgumentValue(nameof(EnableLoggingAttribute.EnableExceptions), out bool enableExceptions))
		{
			attr.EnableExceptions = enableExceptions;
			isChanged = true;
		}

		if (attribute.TryGetNamedArgumentEnumValue(nameof(EnableLoggingAttribute.DefaultNodeOutput), out NodeOutput defaultNodeOutput))
		{
			attr.DefaultNodeOutput = defaultNodeOutput;
			isChanged = true;
		}

		if (attribute.TryGetNamedArgumentValue(nameof(EnableLoggingAttribute.SupportsDiagnostics), out bool supportsDiagnostics))
		{
			attr.SupportsDiagnostics = supportsDiagnostics;
			isChanged = true;
		}

		if (attribute.TryGetNamedArgumentEnumValue(nameof(EnableLoggingAttribute.SupportedLogs), out GeneratorLogs supportedLogs))
		{
			attr.SupportedLogs = supportedLogs;
			isChanged = true;
		}

		if (attribute.TryGetNamedArgumentValue(nameof(EnableLoggingAttribute.LogDirectory), out string? logDirectory))
		{
			attr.LogDirectory = logDirectory;
			isChanged = true;
		}

		if (attribute.TryGetNamedArgumentValue(nameof(EnableLoggingAttribute.RelativeToDefault), out bool relativeToDefault))
		{
			attr.RelativeToDefault = relativeToDefault;
			isChanged = true;
		}

		attr.RelativeToGlobal = false;

		if (isChanged)
		{
			if (logDirectory is null && !attr.RelativeToDefault)
			{
				return LoggingConfiguration.Default;
			}

			return LoggingConfiguration.FromAttribute(attr);
		}

		return _initialConfig!;
	}

	private IGeneratorLogHandler InitLogHandler(LoggingConfiguration? configuration)
	{
		if (SupportsDynamicLoggingConfiguration)
		{
			return new DynamicGeneratorLogHandler(configuration);
		}

		return new GeneratorLogHandler(configuration);
	}

	private static CSharpCompilation EnableModules(CSharpCompilation compilation, Func<string, SourceText, CSharpCompilation, CSharpCompilation> addSource, DurianModule[] modules)
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

			compilation = addSource($"__EnableModule__{module}", SourceText.From(source, Encoding.UTF8), compilation);
		}

		return compilation;
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

	private bool IsValidCSharpCompilation(Compilation compilation, Action<Diagnostic> reportDiagnostic)
	{
		bool isValid = true;

		if (!HasValidReferences(compilation, out bool hasCoreAnalyzer))
		{
			if (!hasCoreAnalyzer)
			{
				reportDiagnostic(Diagnostic.Create(DUR0001_ProjectMustReferenceDurianCore, Location.None));
			}

			isValid = false;
		}

		if (compilation is CSharpCompilation c)
		{
			if(!isValid)
			{
				return false;
			}

			return ValidateCompilation(c, reportDiagnostic);
		}

		if (!hasCoreAnalyzer)
		{
			reportDiagnostic(Diagnostic.Create(DUR0004_DurianModulesAreValidOnlyInCSharp, Location.None));
		}

		return false;
	}

	private LoggingConfiguration? FindDynamicLoggingConfiguration(Compilation compilation, out DurianModule[]? modules)
	{
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

	private CSharpCompilation? InitializeCompilation(Compilation compilation, DurianModule[]? modules, Func<string, SourceText, CSharpCompilation, CSharpCompilation> addSource, Action<Diagnostic> reportDiagnostic)
	{
		if (IsValidCSharpCompilation(compilation, reportDiagnostic))
		{
			CSharpCompilation c = (compilation as CSharpCompilation)!;
			modules ??= GetRequiredModules();
			return EnableModules(c, addSource, modules);
		}

		return null;
	}
}
