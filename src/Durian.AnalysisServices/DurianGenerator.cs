// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Durian.Analysis
{
	/// <summary>
	/// Abstract implementation of the <see cref="IDurianGenerator"/> interface that performs early validation of the input <see cref="GeneratorExecutionContext"/>.
	/// </summary>
	/// <typeparam name="TCompilationData">User-defined type of <see cref="ICompilationData"/> this <see cref="IDurianGenerator"/> operates on.</typeparam>
	/// <typeparam name="TSyntaxReceiver">User-defined type of <see cref="IDurianSyntaxReceiver"/> that provides the <see cref="CSharpSyntaxNode"/>s to perform the generation on.</typeparam>
	/// <typeparam name="TFilter">User-defined type of <see cref="ISyntaxFilter"/> that decides what <see cref="CSharpSyntaxNode"/>s collected by the <see cref="SyntaxReceiver"/> are valid for generation.</typeparam>
	public abstract class DurianGenerator<TCompilationData, TSyntaxReceiver, TFilter> : GeneratorWithFilters<TFilter>, IDurianGenerator
		where TCompilationData : ICompilationData
		where TSyntaxReceiver : IDurianSyntaxReceiver
		where TFilter : IGeneratorSyntaxFilterWithDiagnostics
	{
		private readonly List<CSharpSyntaxTree> _generatedDuringCurrentPass = new(16);

		/// <inheritdoc/>
		public CancellationToken CancellationToken { get; private set; }

		/// <summary>
		/// Determines whether data of this <see cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/> was successfully initialized by the last call to the <see cref="Execute(in GeneratorExecutionContext)"/> method.
		/// </summary>
		[MemberNotNullWhen(true, nameof(TargetCompilation), nameof(SyntaxReceiver), nameof(ParseOptions))]
		public bool HasValidData { get; set; }

		/// <summary>
		/// Determines whether the last execution of the <see cref="Execute(in GeneratorExecutionContext)"/> method was a success.
		/// </summary>
		[MemberNotNullWhen(true, nameof(TargetCompilation), nameof(SyntaxReceiver), nameof(ParseOptions))]
		public new bool IsSuccess
		{
			get => base.IsSuccess;
			private protected set => base.IsSuccess = value;
		}

		/// <inheritdoc cref="IDurianGenerator.ParseOptions"/>
		public CSharpParseOptions? ParseOptions { get; private set; }

		/// <inheritdoc cref="IDurianGenerator.SyntaxReceiver"/>
		public TSyntaxReceiver? SyntaxReceiver { get; private set; }

		/// <inheritdoc cref="IDurianGenerator.TargetCompilation"/>
		public TCompilationData? TargetCompilation { get; private set; }

		string? IDurianGenerator.GeneratorName => GetGeneratorName();
		string? IDurianGenerator.GeneratorVersion => GetGeneratorVersion();
		CSharpParseOptions IDurianGenerator.ParseOptions => ParseOptions!;
		IDurianSyntaxReceiver IDurianGenerator.SyntaxReceiver => SyntaxReceiver!;
		ICompilationData IDurianGenerator.TargetCompilation => TargetCompilation!;

		/// <inheritdoc cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}.DurianGenerator(in ConstructionContext, IHintNameProvider?)"/>
		protected DurianGenerator()
		{
		}

		/// <inheritdoc cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}.DurianGenerator(in ConstructionContext, IHintNameProvider?)"/>
		protected DurianGenerator(in ConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGenerator(in ConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="DurianGenerator(LoggingConfiguration?, IHintNameProvider?)"/>
		protected DurianGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGenerator(LoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}

		/// <summary>
		/// Creates new instance of <typeparamref name="TCompilationData"/>.
		/// </summary>
		/// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
		public abstract TCompilationData? CreateCompilationData(CSharpCompilation compilation);

		/// <summary>
		/// Creates a new <typeparamref name="TSyntaxReceiver"/> to be used during the current generation pass.
		/// </summary>
		public abstract TSyntaxReceiver CreateSyntaxReceiver();

		/// <summary>
		/// Begins the generation.
		/// </summary>
		/// <param name="context">The <see cref="GeneratorInitializationContext"/> to work on.</param>
		public sealed override void Execute(in GeneratorExecutionContext context)
		{
			ResetData();

			if (!InitializeCompilation(in context, out CSharpCompilation? compilation) ||
				context.SyntaxReceiver is not TSyntaxReceiver receiver ||
				!ValidateSyntaxReceiver(receiver)
			)
			{
				return;
			}

			try
			{
				InitializeExecutionData(compilation, receiver, in context);

				if (!HasValidData)
				{
					return;
				}

				Filtrate(in context);
				IsSuccess = true;
			}
			catch (Exception e)
			{
				LogException(e);
				IsSuccess = false;

				if (EnableExceptions)
				{
					throw;
				}
			}
		}

		/// <summary>
		/// Initializes the source generator.
		/// </summary>
		/// <param name="context">The <see cref="GeneratorInitializationContext"/> to work on.</param>
		public override void Initialize(GeneratorInitializationContext context)
		{
			base.Initialize(context);
			context.RegisterForSyntaxNotifications(() => CreateSyntaxReceiver());
		}

		IDurianSyntaxReceiver IDurianGenerator.CreateSyntaxReceiver()
		{
			return CreateSyntaxReceiver();
		}

		/// <inheritdoc/>
		protected sealed override void AddSource(CSharpSyntaxTree syntaxTree, string hintName, in GeneratorPostInitializationContext context)
		{
			base.AddSource(syntaxTree, hintName, context);
		}

		/// <inheritdoc/>
		protected sealed override void AddSource(string source, string hintName, in GeneratorPostInitializationContext context)
		{
			base.AddSource(source, hintName, context);
		}

		/// <summary>
		/// Adds the generated <paramref name="source"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="source">The generated text.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		/// <exception cref="InvalidOperationException"><see cref="HasValidData"/> must be <see langword="true"/> in order to add new source.</exception>
		protected sealed override void AddSource(string source, string hintName, in GeneratorExecutionContext context)
		{
			ThrowIfHasNoValidData();

			CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(source, ParseOptions, encoding: System.Text.Encoding.UTF8, cancellationToken: context.CancellationToken);
			AddSource_Internal(tree, hintName, in context);
		}

		/// <summary>
		/// Adds the generated <paramref name="tree"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="tree">The generated <see cref="CSharpSyntaxTree"/>.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		/// <exception cref="InvalidOperationException"><see cref="HasValidData"/> must be <see langword="true"/> in order to add new source.</exception>
		protected sealed override void AddSource(CSharpSyntaxTree tree, string hintName, in GeneratorExecutionContext context)
		{
			ThrowIfHasNoValidData();
			AddSource_Internal(tree, hintName, in context);
		}

		/// <summary>
		/// Adds the generated <paramref name="text"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="original">The <see cref="CSharpSyntaxNode"/> the source was generated from.</param>
		/// <param name="text">The generated text.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		/// <exception cref="InvalidOperationException"><see cref="HasValidData"/> must be <see langword="true"/> in order to add new source.</exception>
		protected void AddSourceWithOriginal(CSharpSyntaxNode original, string text, string hintName, in GeneratorExecutionContext context)
		{
			ThrowIfHasNoValidData();
			CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(text, ParseOptions, encoding: System.Text.Encoding.UTF8, cancellationToken: context.CancellationToken);
			AddSource_Internal(original, tree, hintName, in context);
		}

		/// <summary>
		/// Adds the generated <paramref name="tree"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="original">The <see cref="CSharpSyntaxNode"/> the source was generated from.</param>
		/// <param name="tree">The generated <see cref="CSharpSyntaxTree"/>.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		/// <exception cref="InvalidOperationException"><see cref="HasValidData"/> must be <see langword="true"/> in order to add new source.</exception>
		protected void AddSourceWithOriginal(CSharpSyntaxNode original, CSharpSyntaxTree tree, string hintName, in GeneratorExecutionContext context)
		{
			ThrowIfHasNoValidData();
			AddSource_Internal(original, tree, hintName, in context);
		}

		/// <summary>
		/// Validates the <paramref name="syntaxReceiver"/>.
		/// </summary>
		/// <param name="syntaxReceiver"><typeparamref name="TSyntaxReceiver"/> to validate.</param>
		protected virtual bool ValidateSyntaxReceiver(TSyntaxReceiver syntaxReceiver)
		{
			return !syntaxReceiver.IsEmpty();
		}

		/// <inheritdoc/>
		protected sealed override void BeforeFiltersWithGeneratedSymbols()
		{
			if (_generatedDuringCurrentPass.Count > 0)
			{
				// Generated sources should be added AFTER all filters that don't include generated symbols were executed to avoid conflicts with SemanticModels.
				foreach (CSharpSyntaxTree generatedTree in _generatedDuringCurrentPass)
				{
					TargetCompilation!.UpdateCompilation(generatedTree);
				}

				_generatedDuringCurrentPass.Clear();
			}
		}

		private protected void AddSource_Internal(CSharpSyntaxTree tree, string hintName, in GeneratorExecutionContext context)
		{
			context.AddSource(hintName, tree.GetText(context.CancellationToken));

			if (IsFilterWithGeneratedSymbols)
			{
				TargetCompilation!.UpdateCompilation(tree);
			}
			else
			{
				_generatedDuringCurrentPass.Add(tree);
			}

			if (LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.Node))
			{
				LogNode_Internal(tree.GetRoot(context.CancellationToken), hintName, NodeOutput.Node);
			}
		}

		private protected void AddSource_Internal(CSharpSyntaxNode original, CSharpSyntaxTree tree, string hintName, in GeneratorExecutionContext context)
		{
			context.AddSource(hintName, tree.GetText(context.CancellationToken));

			if (IsFilterWithGeneratedSymbols)
			{
				TargetCompilation!.UpdateCompilation(tree);
			}
			else
			{
				_generatedDuringCurrentPass.Add(tree);
			}

			if (LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.InputOutput))
			{
				LogInputOutput_Internal(original, tree.GetRoot(context.CancellationToken), hintName, default);
			}
		}

		private protected void InitializeExecutionData(CSharpCompilation currentCompilation, TSyntaxReceiver syntaxReceiver, in GeneratorExecutionContext context)
		{
			TCompilationData? data = CreateCompilationData(currentCompilation);

			if (data is null || data.HasErrors)
			{
				return;
			}

			TargetCompilation = data;
			ParseOptions = context.ParseOptions as CSharpParseOptions ?? CSharpParseOptions.Default;
			SyntaxReceiver = syntaxReceiver;
			CancellationToken = context.CancellationToken;
			HasValidData = true;

			if (EnableDiagnostics)
			{
				DiagnosticReceiver.SetContext(in context);
			}
		}

		private protected void ThrowIfHasNoValidData()
		{
			if (!HasValidData && EnableExceptions)
			{
				throw new InvalidOperationException($"{nameof(HasValidData)} must be true in order to add a new source!");
			}
		}

		private protected void ResetData()
		{
			SyntaxReceiver = default;
			TargetCompilation = default;
			ParseOptions = null!;
			CancellationToken = default;
			IsSuccess = false;
			HasValidData = false;
			FileNameProvider.Reset();
		}
	}

	/// <inheritdoc cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/>
	public abstract class DurianGenerator<TCompilationData, TSyntaxReceiver> : DurianGenerator<TCompilationData, TSyntaxReceiver, IGeneratorSyntaxFilterWithDiagnostics>
		where TCompilationData : ICompilationData
		where TSyntaxReceiver : IDurianSyntaxReceiver
	{
		/// <inheritdoc cref="DurianGenerator{TCompilationData, TSyntaxReceiver}.DurianGenerator(in ConstructionContext, IHintNameProvider?)"/>
		protected DurianGenerator()
		{
		}

		/// <inheritdoc cref="DurianGenerator{TCompilationData, TSyntaxReceiver}.DurianGenerator(in ConstructionContext, IHintNameProvider?)"/>
		protected DurianGenerator(in ConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator{TCompilationData, TSyntaxReceiver}"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGenerator(in ConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="DurianGenerator(LoggingConfiguration?, IHintNameProvider?)"/>
		protected DurianGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator{TCompilationData, TSyntaxReceiver}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGenerator(LoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}
	}

	/// <inheritdoc cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/>
	public abstract class DurianGenerator<TCompilationData> : DurianGenerator<TCompilationData, IDurianSyntaxReceiver, IGeneratorSyntaxFilterWithDiagnostics>
		where TCompilationData : ICompilationData
	{
		/// <inheritdoc cref="DurianGenerator{TCompilationData}.DurianGenerator(in ConstructionContext, IHintNameProvider?)"/>
		protected DurianGenerator()
		{
		}

		/// <inheritdoc cref="DurianGenerator{TCompilationData}.DurianGenerator(in ConstructionContext, IHintNameProvider?)"/>
		protected DurianGenerator(in ConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator{TCompilationData}"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGenerator(in ConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="DurianGenerator(LoggingConfiguration?, IHintNameProvider?)"/>
		protected DurianGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator{TCompilationData}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGenerator(LoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}
	}

	/// <inheritdoc cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/>
	public abstract class DurianGenerator : DurianGenerator<ICompilationData, IDurianSyntaxReceiver, IGeneratorSyntaxFilterWithDiagnostics>
	{
		/// <inheritdoc cref="DurianGenerator(in ConstructionContext, IHintNameProvider?)"/>
		protected DurianGenerator()
		{
		}

		/// <inheritdoc cref="DurianGenerator(in ConstructionContext, IHintNameProvider?)"/>
		protected DurianGenerator(in ConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGenerator(in ConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="DurianGenerator(LoggingConfiguration?, IHintNameProvider?)"/>
		protected DurianGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGenerator(LoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}
	}
}