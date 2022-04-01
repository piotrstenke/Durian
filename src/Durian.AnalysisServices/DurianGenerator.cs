// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Filters;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis
{
	/// <summary>
	/// Abstract implementation of the <see cref="IDurianGenerator"/> interface that performs early validation of the input <see cref="GeneratorExecutionContext"/>.
	/// </summary>
	public abstract class DurianGenerator : DurianGeneratorWithContext<GeneratorPassContext>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator"/> class.
		/// </summary>
		protected DurianGenerator()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="DurianGenerator"/> is initialized.</param>
		protected DurianGenerator(in ConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		protected DurianGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Configures services to be used during the current generator pass.
		/// </summary>
		/// <param name="services">Container of services to configure.</param>
		public virtual void ConfigureServices(IGeneratorServiceContainer services)
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Creates a new instance of <see cref="ICompilationData"/> to be used the current generator pass.
		/// </summary>
		/// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
		public abstract ICompilationData? CreateCompilationData(CSharpCompilation compilation);

		/// <inheritdoc/>
		protected override void AddSourceCore(CSharpSyntaxTree tree, string hintName, GeneratorPassContext context)
		{
			base.AddSourceCore(tree, hintName, context);

			if (context.IsFilterWithGeneratedSymbols)
			{
				context.TargetCompilation.UpdateCompilation(tree);
			}
			else
			{
				context.GenerationQueue.Add(tree);
			}
		}

		/// <inheritdoc/>
		protected override void AfterExecutionOfGroup(IReadOnlyFilterGroup<IGeneratorSyntaxFilter> filterGroup, GeneratorPassContext context)
		{
			context.IsFilterWithGeneratedSymbols = false;
		}

		/// <inheritdoc/>
		protected override void BeforeFiltersWithGeneratedSymbols(GeneratorPassContext context)
		{
			if (context.GenerationQueue.Count > 0)
			{
				// Generated sources should be added AFTER all filters that don't include generated symbols were executed to avoid conflicts with SemanticModels.
				foreach (CSharpSyntaxTree generatedTree in context.GenerationQueue)
				{
					context.TargetCompilation.UpdateCompilation(generatedTree);
				}

				context.GenerationQueue.Clear();
			}

			context.IsFilterWithGeneratedSymbols = true;
		}

		/// <inheritdoc/>
		protected sealed override GeneratorPassContext? CreateCurrentPassContext(CSharpCompilation currentCompilation, in GeneratorExecutionContext context)
		{
			if (context.SyntaxReceiver is not IDurianSyntaxReceiver syntaxReceiver || !ValidateSyntaxReceiver(syntaxReceiver))
			{
				return default;
			}

			ICompilationData? data = CreateCompilationData(currentCompilation);

			if (data is null || data.HasErrors)
			{
				return default;
			}

			IGeneratorServiceContainer services = new GeneratorServiceContainer();

			ConfigureServices(services);

			GeneratorPassContext pass = CreateCurrentPassContext(data, in context);

			pass.CancellationToken = context.CancellationToken;
			pass.Services = services;
			pass.DiagnosticReceiver = EnableDiagnostics ? DiagnosticReceiver.Factory.SourceGenerator(in context) : default;
			pass.LogReceiver = EnableLogging ? new LoggableDiagnosticReceiver(this) : default;
			pass.TargetCompilation = data;
			pass.Generator = this;
			pass.SyntaxReceiver = syntaxReceiver;
			pass._originalContext = context;

			return pass;
		}

		/// <summary>
		/// Creates a new <see cref="GeneratorPassContext"/> for the current generator pass.
		/// </summary>
		/// <param name="currentCompilation">Current <see cref="ICompilationData"/>.</param>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		protected virtual GeneratorPassContext CreateCurrentPassContext(ICompilationData currentCompilation, in GeneratorExecutionContext context)
		{
			return new GeneratorPassContext();
		}

		/// <summary>
		/// Validates the <paramref name="syntaxReceiver"/>.
		/// </summary>
		/// <param name="syntaxReceiver"><see cref="IDurianSyntaxReceiver"/> to validate.</param>
		protected virtual bool ValidateSyntaxReceiver(IDurianSyntaxReceiver syntaxReceiver)
		{
			return !syntaxReceiver.IsEmpty();
		}
	}
}
