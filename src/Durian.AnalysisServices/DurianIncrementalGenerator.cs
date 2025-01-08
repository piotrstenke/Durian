using System;
using System.Text;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Durian.Analysis;

/// <summary>
/// Base class for all Durian source generators implementing the <see cref="IIncrementalGenerator"/> interface.
/// </summary>
public abstract class DurianIncrementalGenerator : DurianGeneratorBase, IIncrementalGenerator
{
	private IHintNameProvider _hintNameProvider;

	/// <summary>
	/// <see cref="IHintNameProvider"/> that creates hint names for the generated source.
	/// </summary>
	/// <exception cref="ArgumentNullException">Value is <see langword="null"/>.</exception>
	public IHintNameProvider HintNameProvider
	{
		get => _hintNameProvider;
		set => _hintNameProvider = value ?? throw new ArgumentNullException(nameof(HintNameProvider));
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DurianIncrementalGenerator"/> class.
	/// </summary>
	protected DurianIncrementalGenerator()
	{
		_hintNameProvider = CreateHintNameProvider();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DurianIncrementalGenerator"/> class.
	/// </summary>
	/// <param name="logHandler">Service that handles log files for this generator.</param>
	/// <exception cref="ArgumentNullException"><paramref name="logHandler"/> is <see langword="null"/>.</exception>
	protected DurianIncrementalGenerator(IGeneratorLogHandler logHandler) : base(logHandler)
	{
		_hintNameProvider = CreateHintNameProvider();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DurianIncrementalGenerator"/> class.
	/// </summary>
	/// <param name="context">Configures how this generator is initialized.</param>
	protected DurianIncrementalGenerator(in GeneratorLogCreationContext context) : base(in context)
	{
		_hintNameProvider = CreateHintNameProvider();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DurianIncrementalGenerator"/> class.
	/// </summary>
	/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
	protected DurianIncrementalGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
	{
		_hintNameProvider = CreateHintNameProvider();
	}

	/// <inheritdoc/>
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		context.RegisterPostInitializationOutput(context => InitializeStaticTrees(context.AddSource, context.CancellationToken));

		IncrementalValueProvider<Compilation> compilation = context.CompilationProvider.Select((compilation, _) => compilation);

		context.RegisterSourceOutput(compilation, (context, compilation) => PrepareForExecution(compilation, (hintName, sourceText, compilation) =>
		{
			context.AddSource(hintName, sourceText);
			return compilation;
		}, context.ReportDiagnostic));

		Register(compilation, context);
	}

	/// <summary>
	/// Adds generator-specific logic to the <paramref name="context"/>.
	/// </summary>
	/// <param name="compilation">Current compilation.</param>
	/// <param name="context">The <see cref="IncrementalGeneratorInitializationContext"/> to register callbacks on</param>
	protected internal abstract void Register(IncrementalValueProvider<Compilation> compilation, IncrementalGeneratorInitializationContext context);

	/// <summary>
	/// Adds the specified <paramref name="source"/> to the <paramref name="context"/>.
	/// </summary>
	/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
	/// <param name="source">A <see cref="string"/> representation of the generated code.</param>
	/// <param name="context"><see cref="SourceProductionContext"/> to add the source to.</param>
	protected virtual void AddSource(string hintName, string source, SourceProductionContext context)
	{
		SyntaxTree tree = CSharpSyntaxTree.ParseText(source, encoding: Encoding.UTF8, cancellationToken: context.CancellationToken);
		AddSource(hintName, tree, context);
	}

	/// <summary>
	/// Adds the specified <paramref name="source"/> to the <paramref name="context"/>.
	/// </summary>
	/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
	/// <param name="source">Source text of the generated code.</param>
	/// <param name="context"><see cref="SourceProductionContext"/> to add the source to.</param>
	protected virtual void AddSource(string hintName, SourceText source, SourceProductionContext context)
	{
		SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString(), encoding: Encoding.UTF8, cancellationToken: context.CancellationToken);
		AddSource(hintName, tree, context);
	}

	/// <summary>
	/// Adds the specified <paramref name="syntaxTree"/> to the <paramref name="context"/>.
	/// </summary>
	/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
	/// <param name="syntaxTree"><see cref="SyntaxTree"/> to add to the <paramref name="context"/>.</param>
	/// <param name="context"><see cref="SourceProductionContext"/> to add the source to.</param>
	protected virtual void AddSource(string hintName, SyntaxTree syntaxTree, SourceProductionContext context)
	{
		context.AddSource(hintName, syntaxTree.GetText(context.CancellationToken));
		LogSource(hintName, syntaxTree, context.CancellationToken);
	}

	private static SymbolNameHintProvider CreateHintNameProvider()
	{
		return new();
	}
}
