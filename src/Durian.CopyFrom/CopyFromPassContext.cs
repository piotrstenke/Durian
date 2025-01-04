using System.Text.RegularExpressions;
using System.Threading;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.CopyFrom
{
	/// <summary>
	/// <c>CopyFrom</c>-specific <see cref="GeneratorPassContext"/>.
	/// </summary>
	public sealed class CopyFromPassContext : GeneratorPassBuilderContext
	{
		/// <summary>
		/// Queue that contains <see cref="ICopyFromMember"/> waiting for their dependencies to be generated.
		/// </summary>
		public DependencyQueue DependencyQueue { get; } = new();

		/// <inheritdoc cref="GeneratorPassContext.Generator"/>
		public new CopyFromGenerator Generator => (base.Generator as CopyFromGenerator)!;

		/// <summary>
		/// Provides <see cref="Regex"/> for a given pattern.
		/// </summary>
		public RegexProvider RegexProvider { get; } = new();

		/// <summary>
		/// <see cref="CopyFrom.SymbolRegistry"/> that contains all the symbols that were already generated.
		/// </summary>
		public SymbolRegistry SymbolRegistry { get; } = new();

		/// <inheritdoc cref="GeneratorPassContext.SyntaxReceiver"/>
		public new CopyFromSyntaxReceiver SyntaxReceiver => (base.SyntaxReceiver as CopyFromSyntaxReceiver)!;

		/// <inheritdoc cref="GeneratorPassContext.TargetCompilation"/>
		public new CopyFromCompilationData TargetCompilation => (base.TargetCompilation as CopyFromCompilationData)!;

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromPassContext"/> class.
		/// </summary>
		/// <param name="originalContext"><see cref="GeneratorExecutionContext"/> created for the current generator pass.</param>
		/// <param name="generator"><see cref="CopyFromGenerator"/> this context was created for.</param>
		/// <param name="targetCompilation"><see cref="CopyFromCompilationData"/> this <see cref="CopyFromGenerator"/> operates on.</param>
		/// <param name="syntaxReceiver"><see cref="CopyFromSyntaxReceiver"/> that provides the <see cref="SyntaxNode"/>es that will take part in the generation.</param>
		/// <param name="parseOptions"><see cref="CSharpParseOptions"/> that will be used to parse any added sources.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		/// <param name="services">Container of services that can be resolved during the current generator pass.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that can be checked to see if the generation should be canceled.</param>
		public CopyFromPassContext(
			in GeneratorExecutionContext originalContext,
			CopyFromGenerator generator,
			CopyFromCompilationData targetCompilation,
			CopyFromSyntaxReceiver syntaxReceiver,
			CSharpParseOptions parseOptions,
			IHintNameProvider fileNameProvider,
			IGeneratorServiceResolver services,
			CancellationToken cancellationToken = default
		) : base(originalContext, generator, targetCompilation, syntaxReceiver, parseOptions, fileNameProvider, services, cancellationToken)
		{
		}

		internal CopyFromPassContext(IHintNameProvider? fileNameProvider = default, CSharpParseOptions? parseOptions = default) : base(fileNameProvider, parseOptions)
		{
		}
	}
}
