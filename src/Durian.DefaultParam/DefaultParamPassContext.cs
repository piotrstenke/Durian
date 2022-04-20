// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Threading;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// <c>DefaultParam</c>-specific <see cref="GeneratorPassContext"/>.
	/// </summary>
	public sealed class DefaultParamPassContext : GeneratorPassBuilderContext
	{
		/// <inheritdoc cref="GeneratorPassContext.Generator"/>
		public new DefaultParamGenerator Generator => (base.Generator as DefaultParamGenerator)!;

		/// <summary>
		/// Rewrites nodes marked with the <c>DefaultParamAttribute</c>.
		/// </summary>
		public DefaultParamRewriter Rewriter { get; }

		/// <inheritdoc cref="GeneratorPassContext.SyntaxReceiver"/>
		public new DefaultParamSyntaxReceiver SyntaxReceiver => (base.SyntaxReceiver as DefaultParamSyntaxReceiver)!;

		/// <inheritdoc cref="GeneratorPassContext.TargetCompilation"/>
		public new DefaultParamCompilationData TargetCompilation => (base.TargetCompilation as DefaultParamCompilationData)!;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamPassContext"/> class.
		/// </summary>
		/// <param name="originalContext"><see cref="GeneratorExecutionContext"/> created for the current generator pass.</param>
		/// <param name="generator"><see cref="DefaultParamGenerator"/> this context was created for.</param>
		/// <param name="targetCompilation"><see cref="DefaultParamCompilationData"/> this <see cref="DefaultParamGenerator"/> operates on.</param>
		/// <param name="syntaxReceiver"><see cref="DefaultParamSyntaxReceiver"/> that provides the <see cref="SyntaxNode"/>es that will take part in the generation.</param>
		/// <param name="parseOptions"><see cref="CSharpParseOptions"/> that will be used to parse any added sources.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		/// <param name="services">Container of services that can be resolved during the current generator pass.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that can be checked to see if the generation should be canceled.</param>
		public DefaultParamPassContext(
			in GeneratorExecutionContext originalContext,
			DefaultParamGenerator generator,
			DefaultParamCompilationData targetCompilation,
			DefaultParamSyntaxReceiver syntaxReceiver,
			CSharpParseOptions parseOptions,
			IHintNameProvider fileNameProvider,
			IGeneratorServiceResolver services,
			CancellationToken cancellationToken = default
		) : base(originalContext, generator, targetCompilation, syntaxReceiver, parseOptions, fileNameProvider, services, cancellationToken)
		{
			Rewriter = new();
		}

		internal DefaultParamPassContext(IHintNameProvider? fileNameProvider = default, CSharpParseOptions? parseOptions = default) : base(fileNameProvider, parseOptions)
		{
			Rewriter = new();
		}
	}
}
