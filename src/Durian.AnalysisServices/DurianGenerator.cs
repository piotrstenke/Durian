﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
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
	/// <typeparam name="TContext">Type of <see cref="IGeneratorPassContext"/> this generator uses.</typeparam>
	public abstract class DurianGenerator<TContext> : DurianGeneratorWithContext<TContext> where TContext : GeneratorPassContext
	{
		private readonly IGeneratorServiceContainer _serviceContainer;

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator{TContext}"/> class.
		/// </summary>
		protected DurianGenerator()
		{
			_serviceContainer = InitServices();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator{TContext}"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="DurianGenerator{TContext}"/> is initialized.</param>
		protected DurianGenerator(in GeneratorLogCreationContext context) : base(in context)
		{
			_serviceContainer = InitServices();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator{TContext}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		protected DurianGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
			_serviceContainer = InitServices();
		}

		private IGeneratorServiceContainer InitServices()
		{
			GeneratorServiceContainer services = new();
			ConfigureServices(services);
			return services;
		}

		/// <summary>
		/// Configures services used by this generator.
		/// </summary>
		/// <param name="services">Container of services to configure.</param>
		public virtual void ConfigureServices(IGeneratorServiceContainer services)
		{
			// Do nothing by default.
		}

		/// <inheritdoc/>
		public override void Initialize(GeneratorInitializationContext context)
		{
			base.Initialize(context);
		}

		/// <summary>
		/// Creates a new instance of <see cref="ICompilationData"/> to be used the current generator pass.
		/// </summary>
		/// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
		public abstract ICompilationData? CreateCompilationData(CSharpCompilation compilation);

		/// <inheritdoc/>
		protected internal override void AddSourceCore(CSharpSyntaxTree tree, string hintName, TContext context)
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
		protected internal override void AfterExecution(TContext context)
		{
			context.State = GeneratorState.Success;
		}

		/// <inheritdoc/>
		protected internal override void AfterExecutionOfGroup(IReadOnlyFilterGroup<IGeneratorSyntaxFilter> filterGroup, TContext context)
		{
			context.IsFilterWithGeneratedSymbols = false;
		}

		/// <inheritdoc/>
		protected internal override void BeforeFiltersWithGeneratedSymbols(TContext context)
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
		protected internal sealed override TContext? CreateCurrentPassContext(CSharpCompilation currentCompilation, in GeneratorExecutionContext context)
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

			TContext pass = CreateCurrentPassContext(data, in context);

			IGeneratorServiceResolver services = _serviceContainer.CreateResolver();

			pass._originalContext = context;
			pass.Generator = this;
			pass.CancellationToken = context.CancellationToken;
			pass.Services = services;
			pass.TargetCompilation = data;
			pass.SyntaxReceiver = syntaxReceiver;
			pass.State = GeneratorState.Running;

			if (pass.ParseOptions is null)
			{
				pass.ParseOptions = CSharpParseOptions.Default;
			}

			if (pass.FileNameProvider is null)
			{
				pass.FileNameProvider = new SymbolNameToFile();
			}

			FillContext(pass);

			return pass;
		}

		/// <summary>
		/// Creates a new <typeparamref name="TContext"/> for the current generator pass.
		/// </summary>
		/// <param name="currentCompilation">Current <see cref="ICompilationData"/>.</param>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		protected abstract TContext CreateCurrentPassContext(ICompilationData currentCompilation, in GeneratorExecutionContext context);

		/// <summary>
		/// Fills the specified <paramref name="context"/> with custom data.
		/// </summary>
		/// <param name="context"><typeparamref name="TContext"/> to fill with data.</param>
		protected virtual void FillContext(TContext context)
		{
			// Do nothing by default.
		}

		/// <inheritdoc/>
		protected internal override void OnException(Exception e, TContext context, bool allowLog)
		{
			base.OnException(e, context, allowLog);
			context.State = GeneratorState.Failed;
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

	/// <inheritdoc cref="DurianGenerator{TContext}"/>
	public abstract class DurianGenerator : DurianGenerator<GeneratorPassContext>
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
		protected DurianGenerator(in GeneratorLogCreationContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		protected DurianGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <inheritdoc/>
		protected override GeneratorPassContext CreateCurrentPassContext(ICompilationData currentCompilation, in GeneratorExecutionContext context)
		{
			return new();
		}
	}
}
