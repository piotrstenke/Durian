using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Durian.Analysis;

/// <summary>
/// Base class for all Durian source generators implementing the <see cref="ISourceGenerator"/> interface.
/// </summary>
public abstract class DurianBasicGenerator : DurianGeneratorBase, ISourceGenerator
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DurianBasicGenerator"/> class.
	/// </summary>
	protected DurianBasicGenerator()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DurianBasicGenerator"/> class.
	/// </summary>
	/// <param name="logHandler">Service that handles log files for this generator.</param>
	/// <exception cref="ArgumentNullException"><paramref name="logHandler"/> is <see langword="null"/>.</exception>
	protected DurianBasicGenerator(IGeneratorLogHandler logHandler) : base(logHandler)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DurianBasicGenerator"/> class.
	/// </summary>
	/// <param name="context">Configures how this generator is initialized.</param>
	protected DurianBasicGenerator(in GeneratorLogCreationContext context) : base(in context)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DurianBasicGenerator"/> class.
	/// </summary>
	/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
	protected DurianBasicGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
	{
	}

	/// <inheritdoc cref="ISourceGenerator.Execute(GeneratorExecutionContext)"/>
	public virtual bool Execute(GeneratorExecutionContext context)
	{
		return PrepareForExecution(context, out _);
	}

	/// <summary>
	/// Returns data used during the current generator pass.
	/// </summary>
	public virtual IGeneratorPassContext? GetCurrentPassContext()
	{
		return GetCurrentPassContextCore();
	}

	/// <inheritdoc/>
	public virtual void Initialize(GeneratorInitializationContext context)
	{
		context.RegisterForPostInitialization(context => InitializeStaticTrees(context.AddSource, context.CancellationToken));
	}

	void ISourceGenerator.Execute(GeneratorExecutionContext context)
	{
		Execute(context);
	}

	/// <summary>
	/// Returns data used during the current generator pass.
	/// </summary>
	protected virtual IGeneratorPassContext? GetCurrentPassContextCore()
	{
		return null;
	}

	/// <summary>
	/// Prepares then generator for a generation pass.
	/// </summary>
	/// <param name="context">Current execution context.</param>
	/// <param name="c">Returned compilation.</param>
	protected bool PrepareForExecution(GeneratorExecutionContext context, [NotNullWhen(true)] out CSharpCompilation? c)
	{
		Compilation compilation = context.Compilation;

		c = PrepareForExecution(compilation, (hintName, sourceText, compilation) =>
		{
			context.AddSource(hintName, sourceText);

			return compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(
				sourceText.ToString(),
				context.ParseOptions as CSharpParseOptions,
				encoding: Encoding.UTF8,
				cancellationToken: context.CancellationToken)
			);
		}, context.ReportDiagnostic);

		return c is not null;
	}

	/// <summary>
	/// Adds the specified <paramref name="source"/> to the <paramref name="context"/>.
	/// </summary>
	/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
	/// <param name="source">A <see cref="string"/> representation of the generated code.</param>
	/// <param name="context"><see cref="GeneratorPostInitializationContext"/> to add the source to.</param>
	protected virtual void AddSource(string hintName, string source, GeneratorPostInitializationContext context)
	{
		SyntaxTree tree = CSharpSyntaxTree.ParseText(source, encoding: Encoding.UTF8, cancellationToken: context.CancellationToken);
		AddSource(hintName, tree,context );
	}

	/// <summary>
	/// Adds the specified <paramref name="source"/> to the <paramref name="context"/>.
	/// </summary>
	/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
	/// <param name="source">Source text of the generated code.</param>
	/// <param name="context"><see cref="GeneratorPostInitializationContext"/> to add the source to.</param>
	protected virtual void AddSource(string hintName, SourceText source, GeneratorPostInitializationContext context)
	{
		SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString(), encoding: Encoding.UTF8, cancellationToken: context.CancellationToken);
		AddSource(hintName, tree, context);
	}

	/// <summary>
	/// Adds the specified <paramref name="syntaxTree"/> to the <paramref name="context"/>.
	/// </summary>
	/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
	/// <param name="syntaxTree"><see cref="SyntaxTree"/> to add to the <paramref name="context"/>.</param>
	/// <param name="context"><see cref="GeneratorPostInitializationContext"/> to add the source to.</param>
	protected virtual void AddSource(string hintName, SyntaxTree syntaxTree, GeneratorPostInitializationContext context)
	{
		context.AddSource(hintName, syntaxTree.GetText(context.CancellationToken));
		LogSource(hintName, syntaxTree, context.CancellationToken);
	}

	/// <summary>
	/// Adds the specified <paramref name="source"/> to the <paramref name="context"/>.
	/// </summary>
	/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
	/// <param name="source">A <see cref="string"/> representation of the generated code.</param>
	/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
	protected virtual void AddSource(string hintName, string source, GeneratorExecutionContext context)
	{
		SyntaxTree tree = CSharpSyntaxTree.ParseText(source, context.ParseOptions as CSharpParseOptions, encoding: Encoding.UTF8, cancellationToken: context.CancellationToken);
		AddSource(hintName, tree, context);
	}

	/// <summary>
	/// Adds the specified <paramref name="source"/> to the <paramref name="context"/>.
	/// </summary>
	/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
	/// <param name="source">Source text of the generated code.</param>
	/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
	protected virtual void AddSource(string hintName, SourceText source, GeneratorExecutionContext context)
	{
		SyntaxTree tree = CSharpSyntaxTree.ParseText(source.ToString(), context.ParseOptions as CSharpParseOptions, encoding: Encoding.UTF8, cancellationToken: context.CancellationToken);
		AddSource(hintName, tree, context);
	}

	/// <summary>
	/// Adds the specified <paramref name="syntaxTree"/> to the <paramref name="context"/>.
	/// </summary>
	/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
	/// <param name="syntaxTree"><see cref="SyntaxTree"/> to add to the <paramref name="context"/>.</param>
	/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
	protected virtual void AddSource(string hintName, SyntaxTree syntaxTree, GeneratorExecutionContext context)
	{
		context.AddSource(hintName, syntaxTree.GetText(context.CancellationToken));
		LogSource(hintName, syntaxTree, context.CancellationToken);
	}
}
