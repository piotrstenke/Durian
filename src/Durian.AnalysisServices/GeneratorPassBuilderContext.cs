﻿using System.Threading;
using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis;

/// <summary>
/// <see cref="GeneratorPassContext"/> that also provides a <see cref="Analysis.CodeBuilder"/>.
/// </summary>
public class GeneratorPassBuilderContext : GeneratorPassContext
{
	/// <summary>
	/// <see cref="Analysis.CodeBuilder"/> used to generate the source code.
	/// </summary>
	public CodeBuilder CodeBuilder { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GeneratorPassBuilderContext"/> class.
	/// </summary>
	/// <param name="originalContext"><see cref="GeneratorExecutionContext"/> created for the current generator pass.</param>
	/// <param name="generator"><see cref="ILoggableSourceGenerator"/> this context was created for.</param>
	/// <param name="targetCompilation"><see cref="ICompilationData"/> this generator operates on.</param>
	/// <param name="syntaxReceiver"><see cref="IDurianSyntaxReceiver"/> that provides the <see cref="SyntaxNode"/>es that will take part in the generation.</param>
	/// <param name="parseOptions"><see cref="ParseOptions"/> that will be used to parse any added sources.</param>
	/// <param name="fileNameProvider">Creates names for generated files.</param>
	/// <param name="services">Container of services that can be resolved during the current generator pass.</param>
	/// <param name="cancellationToken"><see cref="CancellationToken"/> that can be checked to see if the generation should be canceled.</param>
	public GeneratorPassBuilderContext(
		GeneratorExecutionContext originalContext,
		ILoggableSourceGenerator generator,
		ICompilationData targetCompilation,
		IDurianSyntaxReceiver syntaxReceiver,
		ParseOptions parseOptions,
		IHintNameProvider fileNameProvider,
		IGeneratorServiceResolver services,
		CancellationToken cancellationToken = default
	) : base(originalContext, generator, targetCompilation, syntaxReceiver, parseOptions, fileNameProvider, services, cancellationToken)
	{
		CodeBuilder = new();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="GeneratorPassBuilderContext"/> class.
	/// </summary>
	/// <param name="fileNameProvider">Creates names for generated files.</param>
	/// <param name="parseOptions"><see cref="ParseOptions"/> that will be used to parse any added sources.</param>
	protected internal GeneratorPassBuilderContext(IHintNameProvider? fileNameProvider = default, ParseOptions? parseOptions = default) : base(fileNameProvider, parseOptions)
	{
		CodeBuilder = new();
	}
}
