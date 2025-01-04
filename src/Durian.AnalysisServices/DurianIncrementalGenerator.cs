using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Durian.Analysis;

/// <summary>
/// Base class for all Durian source generators implementing the <see cref="IIncrementalGenerator"/> interface.
/// </summary>
public abstract class DurianIncrementalGenerator : DurianGeneratorBase, IIncrementalGenerator
{
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

		Register(context);
	}

	/// <summary>
	/// Adds generator-specific logic to the <paramref name="context"/>.
	/// </summary>
	/// <param name="context">The <see cref="IncrementalGeneratorInitializationContext"/> to register callbacks on</param>
	protected abstract void Register(IncrementalGeneratorInitializationContext context);

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
}
