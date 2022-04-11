// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Threading;

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// <c>DefaultParam</c>-specific <see cref="GeneratorPassContext"/>.
	/// </summary>
	public sealed class DefaultParamPassContext : GeneratorPassBuilderContext
	{
		/// <summary>
		/// Rewrites nodes marked with the <c>DefaultParamAttribute</c>.
		/// </summary>
		public DefaultParamRewriter Rewriter { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamPassContext"/> class.
		/// </summary>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		/// <param name="parseOptions"><see cref="CSharpParseOptions"/> that will be used to parse any added sources.</param>
		internal DefaultParamPassContext(IHintNameProvider? fileNameProvider = default, CSharpParseOptions? parseOptions = default) : base(fileNameProvider, parseOptions)
		{
			Rewriter = new();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamPassContext"/> class.
		/// </summary>
		/// <param name="originalContext"><see cref="GeneratorExecutionContext"/> created for the current generator pass.</param>
		/// <param name="generator"><see cref="IDurianGenerator"/> this context was created for.</param>
		/// <param name="targetCompilation"><see cref="ICompilationData"/> this <see cref="IDurianGenerator"/> operates on.</param>
		/// <param name="syntaxReceiver"><see cref="IDurianSyntaxReceiver"/> that provides the <see cref="SyntaxNode"/>es that will take part in the generation.</param>
		/// <param name="parseOptions"><see cref="CSharpParseOptions"/> that will be used to parse any added sources.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		/// <param name="services">Container of services that can be resolved during the current generator pass.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that can be checked to see if the generation should be canceled.</param>
		public DefaultParamPassContext(
			in GeneratorExecutionContext originalContext,
			IDurianGenerator generator,
			ICompilationData targetCompilation,
			IDurianSyntaxReceiver syntaxReceiver,
			CSharpParseOptions parseOptions,
			IHintNameProvider fileNameProvider,
			IGeneratorServiceContainer services,
			CancellationToken cancellationToken = default
		) : base(originalContext, generator, targetCompilation, syntaxReceiver, parseOptions, fileNameProvider, services, cancellationToken)
		{
			Rewriter = new();
		}
	}
}
