// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Durian.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Manager
{
	/// <summary>
	/// <see cref="AnalyzerManager"/> that can execute source generators.
	/// </summary>
	public abstract class GeneratorManager : AnalyzerManager, ISourceGenerator
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorManager"/> class.
		/// </summary>
		protected GeneratorManager()
		{
		}

		/// <summary>
		/// Attempts to return a function that creates a new <see cref="ISyntaxReceiver"/> to be used during the generator execution pass.
		/// </summary>
		/// <param name="syntaxReceiverCreator">Function that creates a new <see cref="ISyntaxReceiver"/> to be used during the generator execution pass.</param>
		public virtual bool TryGetSyntaxReceiverCreator([NotNullWhen(true)] out SyntaxReceiverCreator? syntaxReceiverCreator)
		{
			syntaxReceiverCreator = null;
			return false;
		}

		/// <inheritdoc/>
		public virtual void Execute(GeneratorExecutionContext context)
		{
			if (!ShouldAnalyze(context.Compilation))
			{
				return;
			}

			foreach (ISourceGenerator generator in GetGenerators())
			{
				if (generator is IDurianGenerator durianGenerator)
				{
					durianGenerator.Execute(in context);
				}
				else
				{
					generator.Execute(context);
				}
			}
		}

		/// <summary>
		/// Returns a collection of <see cref="ISourceGenerator"/>s to execute.
		/// </summary>
		public abstract IEnumerable<ISourceGenerator> GetGenerators();

		/// <inheritdoc/>
		public void Initialize(GeneratorInitializationContext context)
		{
			if (TryGetSyntaxReceiverCreator(out SyntaxReceiverCreator? syntaxReceiverCreator))
			{
				context.RegisterForSyntaxNotifications(syntaxReceiverCreator);
			}
		}

		/// <summary>
		/// Determines whether the specified <see cref="CSharpCompilation"/> is valid for analysis.
		/// </summary>
		/// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
		public bool ShouldAnalyze(Compilation compilation)
		{
			if (compilation is CSharpCompilation c)
			{
				return ShouldAnalyze(c);
			}

			return false;
		}
	}
}