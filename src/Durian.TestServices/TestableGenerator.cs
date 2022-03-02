// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis;
using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;

namespace Durian.TestServices
{
    /// <summary>
    /// <see cref="IDurianGenerator"/> that provides better test-related logging experience.
    /// </summary>
    /// <typeparam name="TCompilationData">User-defined type of <see cref="ICompilationData"/> this <see cref="IDurianGenerator"/> operates on.</typeparam>
    /// <typeparam name="TSyntaxReceiver">User-defined type of <see cref="IDurianSyntaxReceiver"/> that provides the <see cref="CSharpSyntaxNode"/>s to perform the generation on.</typeparam>
    /// <typeparam name="TFilter">User-defined type of <see cref="ISyntaxFilter"/> that decides what <see cref="CSharpSyntaxNode"/>s collected by the <see cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}.SyntaxReceiver"/> are valid for generation.</typeparam>
    public class TestableGenerator<TCompilationData, TSyntaxReceiver, TFilter> : DurianGenerator<TCompilationData, TSyntaxReceiver, TFilter>, ITestableGenerator
        where TCompilationData : ICompilationDataWithSymbols
        where TSyntaxReceiver : IDurianSyntaxReceiver
        where TFilter : IGeneratorSyntaxFilterWithDiagnostics
    {
        private int _analyzerCounter;
        private int _generatorCounter;

        /// <inheritdoc/>
        public override bool EnableDiagnostics
        {
            get => UnderlayingGenerator.EnableDiagnostics;
            set => UnderlayingGenerator.EnableDiagnostics = value;
        }

        /// <inheritdoc/>
        public override bool EnableExceptions
        {
            get => UnderlayingGenerator.EnableExceptions;
            set => UnderlayingGenerator.EnableExceptions = value;
        }

        /// <inheritdoc/>
        public override bool EnableLogging
        {
            get => UnderlayingGenerator.EnableLogging;
            set => UnderlayingGenerator.EnableLogging = value;
        }

        /// <inheritdoc/>
        public override int NumStaticTrees => UnderlayingGenerator.NumStaticTrees;

        /// <summary>
        /// <see cref="IDurianGenerator"/> that is used to actually generate sources.
        /// </summary>
        public DurianGenerator<TCompilationData, TSyntaxReceiver, TFilter> UnderlayingGenerator { get; }

        IDurianGenerator ITestableGenerator.UnderlayingGenerator => UnderlayingGenerator;
        private IGeneratorExecutionFlow Flow => UnderlayingGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestableGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/> class.
        /// </summary>
        /// <param name="generator"><see cref="IDurianGenerator"/> that is used to actually generate sources.</param>
        /// <param name="testName">Name of the currently executing test.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="testName"/> cannot be <see langword="null"/> or empty.</exception>
        public TestableGenerator(DurianGenerator<TCompilationData, TSyntaxReceiver, TFilter> generator, string testName) : base(generator.LoggingConfiguration, new TestNameToFile(testName))
        {
            if (generator is null)
            {
                throw new ArgumentNullException(nameof(generator));
            }

            UnderlayingGenerator = generator;
        }

        /// <inheritdoc/>
        public override TCompilationData? CreateCompilationData(CSharpCompilation compilation)
        {
            if (UnderlayingGenerator.CreateCompilationData(compilation) is TCompilationData c)
            {
                return c;
            }

            return default;
        }

        /// <inheritdoc/>
        public override TSyntaxReceiver CreateSyntaxReceiver()
        {
            return UnderlayingGenerator.CreateSyntaxReceiver();
        }

        /// <inheritdoc/>
        public override FilterContainer<TFilter> GetFilters(IHintNameProvider fileNameProvider)
        {
            return UnderlayingGenerator.GetFilters(fileNameProvider);
        }

        /// <inheritdoc/>
        public override FilterContainer<TFilter>? GetFilters(in GeneratorExecutionContext context)
        {
            return UnderlayingGenerator.GetFilters(in context);
        }

        /// <inheritdoc/>
        public override string? GetGeneratorName()
        {
            return UnderlayingGenerator.GetGeneratorName();
        }

        /// <inheritdoc/>
        public override string? GetGeneratorVersion()
        {
            return UnderlayingGenerator.GetGeneratorVersion();
        }

        /// <inheritdoc/>
        public override IEnumerable<ISourceTextProvider>? GetInitialSources()
        {
            return UnderlayingGenerator.GetInitialSources();
        }

        /// <inheritdoc/>
        public override DurianModule[] GetRequiredModules()
        {
            return UnderlayingGenerator.GetRequiredModules();
        }

        /// <inheritdoc/>
        public override void Initialize(GeneratorInitializationContext context)
        {
            UnderlayingGenerator.Initialize(context);
        }

        /// <inheritdoc/>
        protected override void AfterExecution(in GeneratorExecutionContext context)
        {
            Flow.AfterExecution(in context);
        }

        /// <inheritdoc/>
        protected override void AfterExecutionOfGroup(FilterGroup<TFilter> filterGroup, in GeneratorExecutionContext context)
        {
            Flow.AfterExecutionOfGroup(filterGroup, in context);
            SetAnalyzerMode();
        }

        /// <inheritdoc/>
        protected override void BeforeExecution(in GeneratorExecutionContext context)
        {
            Flow.BeforeExecution(in context);
        }

        /// <inheritdoc/>
        protected override void BeforeExecutionOfGroup(FilterGroup<TFilter> filterGroup, in GeneratorExecutionContext context)
        {
            Flow.BeforeExecutionOfGroup(filterGroup, in context);
            SetGeneratorMode();
        }

        /// <inheritdoc/>
        protected override void BeforeFiltrationOfGroup(FilterGroup<TFilter> filterGroup, in GeneratorExecutionContext context)
        {
            Flow.BeforeFiltrationOfGroup(filterGroup, in context);
            SetAnalyzerMode();
        }

        /// <inheritdoc/>
        protected override void BeforeGeneratedSymbolFiltration(FilterGroup<TFilter> filterGroup, in GeneratorExecutionContext context)
        {
            Flow.BeforeGeneratedSymbolFiltration(filterGroup, in context);
        }

        /// <inheritdoc/>
        protected override bool Generate(IMemberData member, string hintName, in GeneratorExecutionContext context)
        {
            return Flow.Generate(member, hintName, in context);
        }

        /// <inheritdoc/>
        protected override void IterateThroughFilter(TFilter filter, in GeneratorExecutionContext context)
        {
            IEnumerator<IMemberData> iter = filter.GetEnumerator();

            SetAnalyzerMode();

            while (iter.MoveNext())
            {
                SetGeneratorMode();
                GenerateFromData(iter.Current, in context);
                SetAnalyzerMode();
            }
        }

        /// <inheritdoc/>
        protected override bool ValidateCompilation(CSharpCompilation compilation, in GeneratorExecutionContext context)
        {
            return Flow.ValidateCompilation(compilation, in context);
        }

        /// <inheritdoc/>
        protected override bool ValidateSyntaxReceiver(TSyntaxReceiver syntaxReceiver)
        {
            return Flow.ValidateSyntaxReceiver(syntaxReceiver);
        }

        private void SetAnalyzerMode()
        {
            if (FileNameProvider is not TestNameToFile t)
            {
                return;
            }

            _generatorCounter = t.Counter;
            t.Counter = _analyzerCounter;
        }

        private void SetGeneratorMode()
        {
            if (FileNameProvider is not TestNameToFile t)
            {
                return;
            }

            _analyzerCounter = t.Counter;
            t.Counter = _generatorCounter;
        }
    }

    /// <inheritdoc cref="TestableGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/>
    public class TestableGenerator<TCompilationData, TSyntaxReceiver> : TestableGenerator<TCompilationData, TSyntaxReceiver, IGeneratorSyntaxFilterWithDiagnostics>
        where TCompilationData : ICompilationDataWithSymbols
        where TSyntaxReceiver : IDurianSyntaxReceiver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestableGenerator{TCompilationData, TSyntaxReceiver}"/> class.
        /// </summary>
        /// <param name="generator"><see cref="IDurianGenerator"/> that is used to actually generate sources..</param>
        /// <param name="testName">Name of the currently executing test.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="testName"/> cannot be <see langword="null"/> or empty.</exception>
        public TestableGenerator(DurianGenerator<TCompilationData, TSyntaxReceiver> generator, string testName) : base(generator, testName)
        {
        }
    }

    /// <inheritdoc cref="TestableGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/>
    public class TestableGenerator<TCompilationData> : TestableGenerator<TCompilationData, IDurianSyntaxReceiver, IGeneratorSyntaxFilterWithDiagnostics>
        where TCompilationData : ICompilationDataWithSymbols
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestableGenerator{TCompilationData}"/> class.
        /// </summary>
        /// <param name="generator"><see cref="IDurianGenerator"/> that is used to actually generate sources..</param>
        /// <param name="testName">Name of the currently executing test.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="testName"/> cannot be <see langword="null"/> or empty.</exception>
        public TestableGenerator(DurianGenerator<TCompilationData> generator, string testName) : base(generator, testName)
        {
        }
    }

    /// <inheritdoc cref="TestableGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/>
    public class TestableGenerator : TestableGenerator<ICompilationDataWithSymbols, IDurianSyntaxReceiver, IGeneratorSyntaxFilterWithDiagnostics>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestableGenerator"/> class.
        /// </summary>
        /// <param name="generator"><see cref="IDurianGenerator"/> that is used to actually generate sources..</param>
        /// <param name="testName">Name of the currently executing test.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="testName"/> cannot be <see langword="null"/> or empty.</exception>
        public TestableGenerator(DurianGenerator generator, string testName) : base(generator, testName)
        {
        }
    }
}