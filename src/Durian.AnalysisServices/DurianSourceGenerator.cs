﻿using System;
using Durian.Analysis.Data;
using Durian.Analysis.Filtering;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis;

/// <summary>
/// Abstract implementation of the <see cref="IDurianGenerator"/> interface that performs early validation of the input <see cref="GeneratorExecutionContext"/>.
/// </summary>
/// <typeparam name="TContext">Type of <see cref="IGeneratorPassContext"/> this generator uses.</typeparam>
public abstract class DurianSourceGenerator<TContext> : DurianGeneratorWithContext<TContext> where TContext : GeneratorPassContext
{
	private readonly IGeneratorServiceContainer _serviceContainer;

	/// <summary>
	/// Initializes a new instance of the <see cref="DurianSourceGenerator{TContext}"/> class.
	/// </summary>
	protected DurianSourceGenerator()
	{
		_serviceContainer = InitServices();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DurianSourceGenerator{TContext}"/> class.
	/// </summary>
	/// <param name="context">Configures how this <see cref="DurianSourceGenerator{TContext}"/> is initialized.</param>
	protected DurianSourceGenerator(in GeneratorLogCreationContext context) : base(in context)
	{
		_serviceContainer = InitServices();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DurianSourceGenerator{TContext}"/> class.
	/// </summary>
	/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
	protected DurianSourceGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
	{
		_serviceContainer = InitServices();
	}

	/// <summary>
	/// Configures services used by this generator.
	/// </summary>
	/// <param name="services">Container of services to configure.</param>
	protected virtual void ConfigureServices(IGeneratorServiceContainer services)
	{
		// Do nothing by default.
	}

	/// <summary>
	/// Creates a new instance of <see cref="ICompilationData"/> to be used the current generator pass.
	/// </summary>
	/// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
	protected internal abstract ICompilationData? CreateCompilationData(CSharpCompilation compilation);

	/// <inheritdoc/>
	protected internal override void AddSourceCore(string hintName, SyntaxTree tree, TContext context)
	{
		base.AddSourceCore(hintName, tree, context);

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
			foreach (SyntaxTree generatedTree in context.GenerationQueue)
			{
				context.TargetCompilation.UpdateCompilation(generatedTree);
			}

			context.GenerationQueue.Clear();
		}

		context.IsFilterWithGeneratedSymbols = true;
	}

	/// <inheritdoc/>
	protected internal sealed override TContext? CreateCurrentPassContext(CSharpCompilation currentCompilation, GeneratorExecutionContext context)
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

		TContext pass = CreateCurrentPassContext(data, context);

		IGeneratorServiceResolver services = _serviceContainer.CreateResolver();

		pass._originalContext = context;
		pass.Generator = this;
		pass.CancellationToken = context.CancellationToken;
		pass.Services = services;
		pass.TargetCompilation = data;
		pass.SyntaxReceiver = syntaxReceiver;
		pass.State = GeneratorState.Running;

		pass.ParseOptions ??= context.ParseOptions;
		pass.FileNameProvider ??= new SymbolNameHintProvider();

		FillContext(pass);

		return pass;
	}

	/// <inheritdoc/>
	protected internal override void OnException(Exception e, TContext context, bool allowLog)
	{
		base.OnException(e, context, allowLog);
		context.State = GeneratorState.Failed;
	}

	/// <summary>
	/// Creates a new <typeparamref name="TContext"/> for the current generator pass.
	/// </summary>
	/// <param name="currentCompilation">Current <see cref="ICompilationData"/>.</param>
	/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
	protected abstract TContext CreateCurrentPassContext(ICompilationData currentCompilation, GeneratorExecutionContext context);

	/// <summary>
	/// Fills the specified <paramref name="context"/> with custom data.
	/// </summary>
	/// <param name="context"><typeparamref name="TContext"/> to fill with data.</param>
	protected virtual void FillContext(TContext context)
	{
		// Do nothing by default.
	}

	/// <summary>
	/// Validates the <paramref name="syntaxReceiver"/>.
	/// </summary>
	/// <param name="syntaxReceiver"><see cref="IDurianSyntaxReceiver"/> to validate.</param>
	protected virtual bool ValidateSyntaxReceiver(IDurianSyntaxReceiver syntaxReceiver)
	{
		return !syntaxReceiver.IsEmpty();
	}

	private GeneratorServiceContainer InitServices()
	{
		GeneratorServiceContainer services = new();
		ConfigureServices(services);
		return services;
	}
}

/// <inheritdoc cref="DurianSourceGenerator{TContext}"/>
public abstract class DurianSourceGenerator : DurianSourceGenerator<GeneratorPassContext>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DurianSourceGenerator"/> class.
	/// </summary>
	protected DurianSourceGenerator()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DurianSourceGenerator"/> class.
	/// </summary>
	/// <param name="context">Configures how this <see cref="DurianSourceGenerator"/> is initialized.</param>
	protected DurianSourceGenerator(in GeneratorLogCreationContext context) : base(in context)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DurianSourceGenerator"/> class.
	/// </summary>
	/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
	protected DurianSourceGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
	{
	}

	/// <inheritdoc/>
	protected override GeneratorPassContext CreateCurrentPassContext(ICompilationData currentCompilation, GeneratorExecutionContext context)
	{
		return new();
	}
}
