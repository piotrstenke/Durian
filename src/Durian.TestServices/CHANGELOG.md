﻿## Realease 1.0.0

### Added

 - Durian.Tests.AnalyzerTest`1
 - Durian.Tests.AnalyzerTest'1.RunAnalyzerAsync(System.String input, Boolean addToCompilation)
 - Durian.Tests.AnalyzerTest'1.RunAnalyzerAsync(T analyzer, System.String input, Boolean addToCompilation)
 - Durian.Tests.AnalyzerTest'1()
 - Durian.Tests.AnalyzerTest'1(System.String[] sources)
 - Durian.Tests.AnalyzerTest'1(Durian.Tests.TestableCompilationData compilation)
 - Durian.Tests.AttributeTargetGeneratorTestResult
 - Durian.Tests.AttributeTargetGeneratorTestResult.CompareAttribute(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree expected)
 - Durian.Tests.AttributeTargetGeneratorTestResult.CompareAttribute(System.String expected)
 - Durian.Tests.AttributeTargetGeneratorTestResult.CompareSource(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree expected)
 - Durian.Tests.AttributeTargetGeneratorTestResult.CompareSource(System.String expected)
 - Durian.Tests.AttributeTargetGeneratorTestResult.Create(Microsoft.CodeAnalysis.GeneratorDriver generatorDriver, Microsoft.CodeAnalysis.CSharp.CSharpCompilation inputCompilation, Microsoft.CodeAnalysis.CSharp.CSharpCompilation outputCompilation)
 - Durian.Tests.AttributeTargetGeneratorTestResult(Microsoft.CodeAnalysis.GeneratorDriver generatorDriver, Microsoft.CodeAnalysis.CSharp.CSharpCompilation inputCompilation, Microsoft.CodeAnalysis.CSharp.CSharpCompilation outputCompilation)
 - Durian.Tests.AttributeTargetGeneratorTestResult.get_InputCompilation
 - Durian.Tests.AttributeTargetGeneratorTestResult.get_OutputCompilation
 - Durian.Tests.AttributeTargetGeneratorTestResult.get_Exception
 - Durian.Tests.AttributeTargetGeneratorTestResult.get_Generator
 - Durian.Tests.AttributeTargetGeneratorTestResult.get_Diagnostics
 - Durian.Tests.AttributeTargetGeneratorTestResult.get_Attribute
 - Durian.Tests.AttributeTargetGeneratorTestResult.get_Source
 - Durian.Tests.AttributeTargetGeneratorTestResult.get_IsGenerated
 - Durian.Tests.CompilationTest
 - Durian.Tests.CompilationTest.GetNode'1(System.String source, Int32 index)
 - Durian.Tests.CompilationTest.GetNode'1(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree syntaxTree, Int32 index)
 - Durian.Tests.CompilationTest.GetSymbol'1(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode node)
 - Durian.Tests.CompilationTest.GetSymbol'2(System.String source, Int32 index)
 - Durian.Tests.CompilationTest.GetSymbol'2(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree syntaxTree, Int32 index)
 - Durian.Tests.CompilationTest.GetClass(System.String source, Int32 index)
 - Durian.Tests.CompilationTest.GetStruct(System.String source, Int32 index)
 - Durian.Tests.CompilationTest.GetInterface(System.String source, Int32 index)
 - Durian.Tests.CompilationTest.GetRecord(System.String source, Int32 index)
 - Durian.Tests.CompilationTest.GetMember(System.String source, Int32 index)
 - Durian.Tests.CompilationTest.GetType(System.String source, Int32 index)
 - Durian.Tests.CompilationTest.GetField(System.String source, Int32 index)
 - Durian.Tests.CompilationTest.GetProperty(System.String source, Int32 index)
 - Durian.Tests.CompilationTest.GetEventProperty(System.String source, Int32 index)
 - Durian.Tests.CompilationTest.GetEventField(System.String source, Int32 index)
 - Durian.Tests.CompilationTest.GetDelegate(System.String source, Int32 index)
 - Durian.Tests.CompilationTest.GetMethod(System.String source, Int32 index)
 - Durian.Tests.CompilationTest()
 - Durian.Tests.CompilationTest(System.String[] sources)
 - Durian.Tests.CompilationTest(Durian.Tests.TestableCompilationData compilation)
 - Durian.Tests.CompilationTest.get_Compilation
 - Durian.Tests.DiagnosticAnalyzerExtensions
 - Durian.Tests.DiagnosticAnalyzerExtensions.RunAnalyzer(Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer analyzer, Microsoft.CodeAnalysis.Compilation compilation)
 - Durian.Tests.DiagnosticAnalyzerExtensions.RunAnalyzer(Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer analyzer, System.String source)
 - Durian.Tests.DiagnosticAnalyzerExtensions.RunAnalyzer(Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer analyzer, Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree tree)
 - Durian.Tests.DiagnosticAnalyzerExtensions.ProducesDiagnostic(Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer analyzer, Microsoft.CodeAnalysis.Compilation compilation, Microsoft.CodeAnalysis.DiagnosticDescriptor descriptor)
 - Durian.Tests.DiagnosticAnalyzerExtensions.ProducesDiagnostic(Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer analyzer, Microsoft.CodeAnalysis.Compilation compilation, System.String id)
 - Durian.Tests.DurianGeneratorProxy
 - Durian.Tests.DurianGeneratorProxy.Release()
 - Durian.Tests.DurianGeneratorProxy.Initialize(Microsoft.CodeAnalysis.GeneratorInitializationContext context)
 - Durian.Tests.DurianGeneratorProxy.CreateSyntaxReceiver()
 - Durian.Tests.DurianGeneratorProxy.GetStaticSyntaxTrees(System.Threading.CancellationToken cancellationToken)
 - Durian.Tests.DurianGeneratorProxy.GetFilters(Microsoft.CodeAnalysis.GeneratorExecutionContext& context)
 - Durian.Tests.DurianGeneratorProxy.CreateCompilationData(Microsoft.CodeAnalysis.CSharp.CSharpCompilation compilation)
 - Durian.Tests.DurianGeneratorProxy.Generate(Durian.Generator.Data.IMemberData member, System.String hintName, Microsoft.CodeAnalysis.GeneratorExecutionContext& context)
 - Durian.Tests.DurianGeneratorProxy.GetEnabledModules()
 - Durian.Tests.DurianGeneratorProxy.ValidateSyntaxReceiver(Durian.Generator.IDurianSyntaxReceiver syntaxReceiver)
 - Durian.Tests.DurianGeneratorProxy.GetGeneratorName()
 - Durian.Tests.DurianGeneratorProxy.BeforeExecution(Microsoft.CodeAnalysis.GeneratorExecutionContext& context)
 - Durian.Tests.DurianGeneratorProxy.AfterExecution(Microsoft.CodeAnalysis.GeneratorExecutionContext& context)
 - Durian.Tests.DurianGeneratorProxy.BeforeExecutionOfGroup(Durian.Generator.FilterGroup'1[Durian.Generator.IGeneratorSyntaxFilterWithDiagnostics] filterGroup, Microsoft.CodeAnalysis.GeneratorExecutionContext& context)
 - Durian.Tests.DurianGeneratorProxy.AfterExecutionOfGroup(Durian.Generator.FilterGroup'1[Durian.Generator.IGeneratorSyntaxFilterWithDiagnostics] filterGroup, Microsoft.CodeAnalysis.GeneratorExecutionContext& context)
 - Durian.Tests.DurianGeneratorProxy.BeforeFiltrationOfGroup(Durian.Generator.FilterGroup'1[Durian.Generator.IGeneratorSyntaxFilterWithDiagnostics] filterGroup, Microsoft.CodeAnalysis.GeneratorExecutionContext& context)
 - Durian.Tests.DurianGeneratorProxy.BeforeFiltrationAndExecutionOfFiltersWithGeneratedSymbols(Durian.Generator.FilterGroup'1[Durian.Generator.IGeneratorSyntaxFilterWithDiagnostics] filterGroup, Microsoft.CodeAnalysis.GeneratorExecutionContext& context)
 - Durian.Tests.DurianGeneratorProxy.IterateThroughFilter(Durian.Generator.IGeneratorSyntaxFilterWithDiagnostics filter, Microsoft.CodeAnalysis.GeneratorExecutionContext& context)
 - Durian.Tests.DurianGeneratorProxy.GetVersion()
 - Durian.Tests.DurianGeneratorProxy()
 - Durian.Tests.DurianGeneratorProxy(Durian.Generator.Logging.LoggableGeneratorConstructionContext& context)
 - Durian.Tests.DurianGeneratorProxy(Durian.Generator.Logging.LoggableGeneratorConstructionContext& context, Durian.Generator.Logging.IFileNameProvider fileNameProvider)
 - Durian.Tests.DurianGeneratorProxy(Durian.Generator.Logging.GeneratorLoggingConfiguration loggingConfiguration)
 - Durian.Tests.DurianGeneratorProxy(Durian.Generator.Logging.GeneratorLoggingConfiguration loggingConfiguration, Durian.Generator.Logging.IFileNameProvider fileNameProvider)
 - Durian.Tests.DurianGeneratorProxy.get_ExecutionContext
 - Durian.Tests.DurianGeneratorProxy.get_InitializationContext
 - Durian.Tests.DurianGeneratorProxy.add_OnCreateSyntaxReceiver
 - Durian.Tests.DurianGeneratorProxy.remove_OnCreateSyntaxReceiver
 - Durian.Tests.DurianGeneratorProxy.add_OnCreateCompilationData
 - Durian.Tests.DurianGeneratorProxy.remove_OnCreateCompilationData
 - Durian.Tests.DurianGeneratorProxy.add_OnGenerate
 - Durian.Tests.DurianGeneratorProxy.remove_OnGenerate
 - Durian.Tests.DurianGeneratorProxy.add_OnInitialize
 - Durian.Tests.DurianGeneratorProxy.remove_OnInitialize
 - Durian.Tests.DurianGeneratorProxy.add_OnBeforeExecution
 - Durian.Tests.DurianGeneratorProxy.remove_OnBeforeExecution
 - Durian.Tests.DurianGeneratorProxy.add_OnAfterExecution
 - Durian.Tests.DurianGeneratorProxy.remove_OnAfterExecution
 - Durian.Tests.DurianGeneratorProxy.add_OnBeforeFiltrationOfGroup
 - Durian.Tests.DurianGeneratorProxy.remove_OnBeforeFiltrationOfGroup
 - Durian.Tests.DurianGeneratorProxy.add_OnBeforeExecutionOfGroup
 - Durian.Tests.DurianGeneratorProxy.remove_OnBeforeExecutionOfGroup
 - Durian.Tests.DurianGeneratorProxy.add_OnAfterExecutinoOfGroup
 - Durian.Tests.DurianGeneratorProxy.remove_OnAfterExecutinoOfGroup
 - Durian.Tests.DurianGeneratorProxy.add_OnBeforeFiltrationAndExecutionOfFiltersWithGeneratedSymbols
 - Durian.Tests.DurianGeneratorProxy.remove_OnBeforeFiltrationAndExecutionOfFiltersWithGeneratedSymbols
 - Durian.Tests.DurianGeneratorProxy.add_OnGetVersion
 - Durian.Tests.DurianGeneratorProxy.remove_OnGetVersion
 - Durian.Tests.DurianGeneratorProxy.add_OnGetGeneratorName
 - Durian.Tests.DurianGeneratorProxy.remove_OnGetGeneratorName
 - Durian.Tests.DurianGeneratorProxy.add_OnValidateSyntaxReceiver
 - Durian.Tests.DurianGeneratorProxy.remove_OnValidateSyntaxReceiver
 - Durian.Tests.DurianGeneratorProxy.add_OnGetStaticTrees
 - Durian.Tests.DurianGeneratorProxy.remove_OnGetStaticTrees
 - Durian.Tests.DurianGeneratorProxy.add_OnGetFilters
 - Durian.Tests.DurianGeneratorProxy.remove_OnGetFilters
 - Durian.Tests.DurianGeneratorProxy.add_OnIterateThroughFilter
 - Durian.Tests.DurianGeneratorProxy.remove_OnIterateThroughFilter
 - Durian.Tests.DurianGeneratorProxy.add_OnGetEnabledModules
 - Durian.Tests.DurianGeneratorProxy.remove_OnGetEnabledModules
 - Durian.Tests.GeneratorDriverExtensions
 - Durian.Tests.GeneratorDriverExtensions.RunTest(Microsoft.CodeAnalysis.CSharp.CSharpGeneratorDriver generatorDriver, Durian.Tests.GeneratorTestResultProvider resultProvider, System.String source, System.Reflection.Assembly[] assemblies)
 - Durian.Tests.GeneratorDriverExtensions.RunTest(Microsoft.CodeAnalysis.CSharp.CSharpGeneratorDriver generatorDriver, Durian.Tests.GeneratorTestResultProvider resultProvider, System.Collections.Generic.IEnumerable'1[System.String] sources, System.Reflection.Assembly[] assemblies)
 - Durian.Tests.GeneratorDriverExtensions.RunTest(Microsoft.CodeAnalysis.CSharp.CSharpGeneratorDriver generatorDriver, Durian.Tests.GeneratorTestResultProvider resultProvider, Microsoft.CodeAnalysis.CSharp.CSharpCompilation compilation)
 - Durian.Tests.GeneratorDriverExtensions.RunTest'1(Microsoft.CodeAnalysis.CSharp.CSharpGeneratorDriver generatorDriver, Durian.Tests.GeneratorTestResultProvider'1[TResult] resultProvider, System.String source, System.Reflection.Assembly[] assemblies)
 - Durian.Tests.GeneratorDriverExtensions.RunTest'1(Microsoft.CodeAnalysis.CSharp.CSharpGeneratorDriver generatorDriver, Durian.Tests.GeneratorTestResultProvider'1[TResult] resultProvider, System.Collections.Generic.IEnumerable'1[System.String] sources, System.Reflection.Assembly[] assemblies)
 - Durian.Tests.GeneratorDriverExtensions.RunTest'1(Microsoft.CodeAnalysis.CSharp.CSharpGeneratorDriver generatorDriver, Durian.Tests.GeneratorTestResultProvider'1[TResult] resultProvider, Microsoft.CodeAnalysis.CSharp.CSharpCompilation compilation)
 - Durian.Tests.GeneratorDriverRunResultBuilder
 - Durian.Tests.GeneratorDriverRunResultBuilder.AddResult(Microsoft.CodeAnalysis.GeneratorRunResult& result)
 - Durian.Tests.GeneratorDriverRunResultBuilder.AddResult(Microsoft.CodeAnalysis.ISourceGenerator generator, System.Collections.Generic.IEnumerable'1[Microsoft.CodeAnalysis.GeneratedSourceResult] generatedSources, System.Collections.Generic.IEnumerable'1[Microsoft.CodeAnalysis.Diagnostic] diagnostics, System.Exception exception)
 - Durian.Tests.GeneratorDriverRunResultBuilder.BeginResult()
 - Durian.Tests.GeneratorDriverRunResultBuilder.AddResults(System.Collections.Generic.IEnumerable'1[Microsoft.CodeAnalysis.GeneratorRunResult] generatedSources)
 - Durian.Tests.GeneratorDriverRunResultBuilder.WithResults(System.Collections.Generic.IEnumerable'1[Microsoft.CodeAnalysis.GeneratorRunResult] results)
 - Durian.Tests.GeneratorDriverRunResultBuilder.Build()
 - Durian.Tests.GeneratorDriverRunResultBuilder.Reset()
 - Durian.Tests.GeneratorDriverRunResultBuilder()
 - Durian.Tests.GeneratorDriverRunResultBuilder(System.Collections.Generic.IEnumerable'1[Microsoft.CodeAnalysis.GeneratorRunResult] results)
 - Durian.Tests.GeneratorResultFactory
 - Durian.Tests.GeneratorResultFactory.CreateSourceResult(System.String source)
 - Durian.Tests.GeneratorResultFactory.CreateSourceResult(System.String source, System.String hintName)
 - Durian.Tests.GeneratorResultFactory.CreateSourceResult(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree syntaxTree)
 - Durian.Tests.GeneratorResultFactory.CreateSourceResult(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree syntaxTree, System.String hintName)
 - Durian.Tests.GeneratorResultFactory.CreateSourceResult(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree syntaxTree, Microsoft.CodeAnalysis.Text.SourceText sourceText, System.String hintName)
 - Durian.Tests.GeneratorResultFactory.CreateGeneratorResult(Microsoft.CodeAnalysis.ISourceGenerator generator, System.Collections.Generic.IEnumerable'1[Microsoft.CodeAnalysis.GeneratedSourceResult] generatedSources, System.Collections.Generic.IEnumerable'1[Microsoft.CodeAnalysis.Diagnostic] diagnostics, System.Exception exception)
 - Durian.Tests.GeneratorResultFactory.CreateDriverResult(System.Collections.Generic.IEnumerable'1[Microsoft.CodeAnalysis.GeneratorRunResult] results)
 - Durian.Tests.GeneratorRunResultBuilder
 - Durian.Tests.GeneratorRunResultBuilder.WithGenerator(Microsoft.CodeAnalysis.ISourceGenerator generator)
 - Durian.Tests.GeneratorRunResultBuilder.WithException(System.Exception exception)
 - Durian.Tests.GeneratorRunResultBuilder.AddDiagnostic(Microsoft.CodeAnalysis.Diagnostic diagnostic)
 - Durian.Tests.GeneratorRunResultBuilder.AddDiagnostics(System.Collections.Generic.IEnumerable'1[Microsoft.CodeAnalysis.Diagnostic] diagnostics)
 - Durian.Tests.GeneratorRunResultBuilder.WithDiagnostics(System.Collections.Generic.IEnumerable'1[Microsoft.CodeAnalysis.Diagnostic] diagnostics)
 - Durian.Tests.GeneratorRunResultBuilder.AddSource(Microsoft.CodeAnalysis.GeneratedSourceResult& source)
 - Durian.Tests.GeneratorRunResultBuilder.AddSource(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree syntaxTree)
 - Durian.Tests.GeneratorRunResultBuilder.AddSource(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree syntaxTree, System.String hintName)
 - Durian.Tests.GeneratorRunResultBuilder.AddSource(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree syntaxTree, Microsoft.CodeAnalysis.Text.SourceText sourceText, System.String hintName)
 - Durian.Tests.GeneratorRunResultBuilder.AddSource(System.String source)
 - Durian.Tests.GeneratorRunResultBuilder.AddSource(System.String source, System.String hintName)
 - Durian.Tests.GeneratorRunResultBuilder.AddSources(System.Collections.Generic.IEnumerable'1[Microsoft.CodeAnalysis.GeneratedSourceResult] sources)
 - Durian.Tests.GeneratorRunResultBuilder.WithSources(System.Collections.Generic.IEnumerable'1[Microsoft.CodeAnalysis.GeneratedSourceResult] sources)
 - Durian.Tests.GeneratorRunResultBuilder.Build()
 - Durian.Tests.GeneratorRunResultBuilder.Reset()
 - Durian.Tests.GeneratorRunResultBuilder()
 - Durian.Tests.GeneratorRunResultBuilder(Microsoft.CodeAnalysis.ISourceGenerator generator)
 - Durian.Tests.GeneratorRunResultBuilder(System.Collections.Generic.IEnumerable'1[Microsoft.CodeAnalysis.GeneratedSourceResult] sources)
 - Durian.Tests.GeneratorRunResultBuilder(Microsoft.CodeAnalysis.ISourceGenerator generator, System.Collections.Generic.IEnumerable'1[Microsoft.CodeAnalysis.GeneratedSourceResult] sources)
 - Durian.Tests.GeneratorTest
 - Durian.Tests.GeneratorTest.CreateGenerator()
 - Durian.Tests.GeneratorTest.RunGenerator(System.String input)
 - Durian.Tests.GeneratorTest.RunGenerator(System.String input, Int32 index)
 - Durian.Tests.GeneratorTest.RunGenerator(System.String input, Microsoft.CodeAnalysis.ISourceGenerator sourceGenerator)
 - Durian.Tests.GeneratorTest.RunGenerator(System.String input, Microsoft.CodeAnalysis.ISourceGenerator sourceGenerator, Int32 index)
 - Durian.Tests.GeneratorTest()
 - Durian.Tests.GeneratorTest(Boolean enableDiagnostics)
 - Durian.Tests.GeneratorTest.get_Generator
 - Durian.Tests.GeneratorTestResultExtensions
 - Durian.Tests.GeneratorTestResultExtensions.HasFailedAndContainsDiagnosticIDs'1(T result, System.String[] ids)
 - Durian.Tests.GeneratorTestResultExtensions.HasFailedAndDoesNotContainDiagnosticIDs'1(T result, System.String[] ids)
 - Durian.Tests.GeneratorTestResultExtensions.HasSucceededAndContainsDiagnosticIDs'1(T result, System.String[] ids)
 - Durian.Tests.GeneratorTestResultExtensions.HasSucceededAndDoesNotContainDiagnosticIDs'1(T result, System.String[] ids)
 - Durian.Tests.GeneratorTestResultExtensions.ContainsDiagnosticIDs'1(T result, System.String[] ids)
 - Durian.Tests.GeneratorTestResultExtensions.ContainsDiagnosticMessages'1(T result, System.String[] diagnosticMessages)
 - Durian.Tests.GeneratorTestResultExtensions.ContainsDiagnosticMessages'1(T result, Boolean ignoreLocationAndName, System.String[] diagnosticMessages)
 - Durian.Tests.LoggableGeneratorTest`1
 - Durian.Tests.LoggableGeneratorTest'1.CreateGenerator(Durian.Generator.Logging.GeneratorLoggingConfiguration configuration, System.String testName)
 - Durian.Tests.LoggableGeneratorTest'1.RunGenerator(System.String input, System.String testName)
 - Durian.Tests.LoggableGeneratorTest'1.RunGenerator(System.String input, Int32 index, System.String testName)
 - Durian.Tests.LoggableGeneratorTest'1()
 - Durian.Tests.LoggableGeneratorTest'1(Boolean enableDiagnostics)
 - Durian.Tests.MultiOutputGeneratorTestResult
 - Durian.Tests.MultiOutputGeneratorTestResult.Compare(Int32 index, System.String expected)
 - Durian.Tests.MultiOutputGeneratorTestResult.Compare(Int32 index, Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree syntaxTree)
 - Durian.Tests.MultiOutputGeneratorTestResult.Create(Microsoft.CodeAnalysis.GeneratorDriver generatorDriver, Microsoft.CodeAnalysis.CSharp.CSharpCompilation inputCompilation, Microsoft.CodeAnalysis.CSharp.CSharpCompilation outputCompilation)
 - Durian.Tests.MultiOutputGeneratorTestResult(Microsoft.CodeAnalysis.GeneratorDriver generatorDriver, Microsoft.CodeAnalysis.CSharp.CSharpCompilation inputCompilation, Microsoft.CodeAnalysis.CSharp.CSharpCompilation outputCompilation)
 - Durian.Tests.MultiOutputGeneratorTestResult.get_InputCompilation
 - Durian.Tests.MultiOutputGeneratorTestResult.get_OutputCompilation
 - Durian.Tests.MultiOutputGeneratorTestResult.get_Exception
 - Durian.Tests.MultiOutputGeneratorTestResult.get_Generator
 - Durian.Tests.MultiOutputGeneratorTestResult.get_Diagnostics
 - Durian.Tests.MultiOutputGeneratorTestResult.get_GeneratedSources
 - Durian.Tests.MultiOutputGeneratorTestResult.get_Length
 - Durian.Tests.MultiOutputGeneratorTestResult.get_IsGenerated
 - Durian.Tests.RoslynUtilities
 - Durian.Tests.RoslynUtilities.GetSyntaxTrees(System.Collections.Generic.IEnumerable'1[System.String] sources)
 - Durian.Tests.RoslynUtilities.CreateBaseCompilation()
 - Durian.Tests.RoslynUtilities.CreateCompilation(System.String source, System.Type[] types)
 - Durian.Tests.RoslynUtilities.CreateCompilation(System.Collections.Generic.IEnumerable'1[System.String] sources, System.Type[] types)
 - Durian.Tests.RoslynUtilities.CreateCompilation(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree tree, System.Type[] types)
 - Durian.Tests.RoslynUtilities.CreateCompilation(System.Collections.Generic.IEnumerable'1[Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree] trees, System.Type[] types)
 - Durian.Tests.RoslynUtilities.CreateCompilationWithAssemblies(System.String source, System.Reflection.Assembly[] assemblies)
 - Durian.Tests.RoslynUtilities.CreateCompilationWithAssemblies(System.Collections.Generic.IEnumerable'1[System.String] sources, System.Reflection.Assembly[] assemblies)
 - Durian.Tests.RoslynUtilities.CreateCompilationWithAssemblies(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree tree, System.Reflection.Assembly[] assemblies)
 - Durian.Tests.RoslynUtilities.CreateCompilationWithAssemblies(System.Collections.Generic.IEnumerable'1[Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree] trees, System.Reflection.Assembly[] assemblies)
 - Durian.Tests.RoslynUtilities.CreateCompilationWithReferences(System.String source, Microsoft.CodeAnalysis.MetadataReference[] references)
 - Durian.Tests.RoslynUtilities.CreateCompilationWithReferences(System.Collections.Generic.IEnumerable'1[System.String] sources, Microsoft.CodeAnalysis.MetadataReference[] references)
 - Durian.Tests.RoslynUtilities.CreateCompilationWithReferences(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree tree, Microsoft.CodeAnalysis.MetadataReference[] references)
 - Durian.Tests.RoslynUtilities.CreateCompilationWithReferences(System.Collections.Generic.IEnumerable'1[Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree] trees, Microsoft.CodeAnalysis.MetadataReference[] references)
 - Durian.Tests.RoslynUtilities.GetReferences(System.Collections.Generic.IEnumerable'1[System.Type] types, Boolean includeBase)
 - Durian.Tests.RoslynUtilities.GetReferences(System.Collections.Generic.IEnumerable'1[System.Reflection.Assembly] assemblies, Boolean includeBase)
 - Durian.Tests.RoslynUtilities.GetBaseReferences()
 - Durian.Tests.RoslynUtilities.ParseNode'1(System.String source)
 - Durian.Tests.RoslynUtilities.ParseNode'1(System.String source, Int32 index)
 - Durian.Tests.RoslynUtilities.ParseNode'1(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree tree)
 - Durian.Tests.RoslynUtilities.ParseNode'1(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree syntaxTree, Int32 index)
 - Durian.Tests.RoslynUtilities.CreateExecutionContext()
 - Durian.Tests.RoslynUtilities.CreateExecutionContext(Microsoft.CodeAnalysis.Compilation compilation)
 - Durian.Tests.RoslynUtilities.CreateExecutionContext(Microsoft.CodeAnalysis.Compilation compilation, Durian.Tests.GeneratorInitialize onInitialize)
 - Durian.Tests.RoslynUtilities.CreateInitializationContext()
 - Durian.Tests.RoslynUtilities.get_DefaultCompilationName
 - Durian.Tests.SingletonGeneratorTestResult
 - Durian.Tests.SingletonGeneratorTestResult.Compare(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree expected)
 - Durian.Tests.SingletonGeneratorTestResult.Compare(System.String expected)
 - Durian.Tests.SingletonGeneratorTestResult.Create(Microsoft.CodeAnalysis.CSharp.CSharpGeneratorDriver generatorDriver, Microsoft.CodeAnalysis.CSharp.CSharpCompilation inputCompilation, Microsoft.CodeAnalysis.CSharp.CSharpCompilation outputCompilation)
 - Durian.Tests.SingletonGeneratorTestResult.Create(Microsoft.CodeAnalysis.CSharp.CSharpGeneratorDriver generatorDriver, Microsoft.CodeAnalysis.CSharp.CSharpCompilation inputCompilation, Microsoft.CodeAnalysis.CSharp.CSharpCompilation outputCompilation, Int32 sourceIndex)
 - Durian.Tests.SingletonGeneratorTestResult(Microsoft.CodeAnalysis.CSharp.CSharpGeneratorDriver generatorDriver, Microsoft.CodeAnalysis.CSharp.CSharpCompilation inputCompilation, Microsoft.CodeAnalysis.CSharp.CSharpCompilation outputCompilation)
 - Durian.Tests.SingletonGeneratorTestResult(Microsoft.CodeAnalysis.CSharp.CSharpGeneratorDriver generatorDriver, Microsoft.CodeAnalysis.CSharp.CSharpCompilation inputCompilation, Microsoft.CodeAnalysis.CSharp.CSharpCompilation outputCompilation, Int32 sourceIndex)
 - Durian.Tests.SingletonGeneratorTestResult.get_InputCompilation
 - Durian.Tests.SingletonGeneratorTestResult.get_OutputCompilation
 - Durian.Tests.SingletonGeneratorTestResult.get_Exception
 - Durian.Tests.SingletonGeneratorTestResult.get_Generator
 - Durian.Tests.SingletonGeneratorTestResult.get_SyntaxTree
 - Durian.Tests.SingletonGeneratorTestResult.get_SourceText
 - Durian.Tests.SingletonGeneratorTestResult.get_Diagnostics
 - Durian.Tests.SingletonGeneratorTestResult.get_HintName
 - Durian.Tests.SingletonGeneratorTestResult.get_IsGenerated
 - Durian.Tests.SourceGeneratorExtensions
 - Durian.Tests.SourceGeneratorExtensions.RunTest(Microsoft.CodeAnalysis.ISourceGenerator generator, Durian.Tests.GeneratorTestResultProvider resultProvider, System.String source, System.Reflection.Assembly[] assemblies)
 - Durian.Tests.SourceGeneratorExtensions.RunTest(Microsoft.CodeAnalysis.ISourceGenerator generator, Durian.Tests.GeneratorTestResultProvider resultProvider, System.Collections.Generic.IEnumerable'1[System.String] sources, System.Reflection.Assembly[] assemblies)
 - Durian.Tests.SourceGeneratorExtensions.RunTest(Microsoft.CodeAnalysis.ISourceGenerator generator, Durian.Tests.GeneratorTestResultProvider resultProvider, Microsoft.CodeAnalysis.CSharp.CSharpCompilation compilation)
 - Durian.Tests.SourceGeneratorExtensions.RunTest'1(Microsoft.CodeAnalysis.ISourceGenerator generator, Durian.Tests.GeneratorTestResultProvider'1[TResult] resultProvider, System.String source, System.Reflection.Assembly[] assemblies)
 - Durian.Tests.SourceGeneratorExtensions.RunTest'1(Microsoft.CodeAnalysis.ISourceGenerator generator, Durian.Tests.GeneratorTestResultProvider'1[TResult] resultProvider, System.Collections.Generic.IEnumerable'1[System.String] sources, System.Reflection.Assembly[] assemblies)
 - Durian.Tests.SourceGeneratorExtensions.RunTest'1(Microsoft.CodeAnalysis.ISourceGenerator generator, Durian.Tests.GeneratorTestResultProvider'1[TResult] resultProvider, Microsoft.CodeAnalysis.CSharp.CSharpCompilation compilation)
 - Durian.Tests.SourceGeneratorProxy
 - Durian.Tests.SourceGeneratorProxy.Initialize(Microsoft.CodeAnalysis.GeneratorInitializationContext context)
 - Durian.Tests.SourceGeneratorProxy.Execute(Microsoft.CodeAnalysis.GeneratorExecutionContext context)
 - Durian.Tests.SourceGeneratorProxy.Release()
 - Durian.Tests.SourceGeneratorProxy()
 - Durian.Tests.SourceGeneratorProxy.get_ExecutionContext
 - Durian.Tests.SourceGeneratorProxy.get_InitializationContext
 - Durian.Tests.SourceGeneratorProxy.add_OnInitialize
 - Durian.Tests.SourceGeneratorProxy.remove_OnInitialize
 - Durian.Tests.SourceGeneratorProxy.add_OnExecute
 - Durian.Tests.SourceGeneratorProxy.remove_OnExecute
 - Durian.Tests.SyntaxReceiverProxy
 - Durian.Tests.SyntaxReceiverProxy.GetCollectedNodes()
 - Durian.Tests.SyntaxReceiverProxy.IsEmpty()
 - Durian.Tests.SyntaxReceiverProxy.OnVisitSyntaxNode(Microsoft.CodeAnalysis.SyntaxNode syntaxNode)
 - Durian.Tests.SyntaxReceiverProxy()
 - Durian.Tests.TestableCompilationData
 - Durian.Tests.TestableCompilationData.Reset()
 - Durian.Tests.TestableCompilationData.UpdateCompilation(System.String source)
 - Durian.Tests.TestableCompilationData.UpdateCompilation(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree tree)
 - Durian.Tests.TestableCompilationData.UpdateCompilation(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree original, Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree updated)
 - Durian.Tests.TestableCompilationData.UpdateCompilation(System.Collections.Generic.IEnumerable'1[Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree] trees)
 - Durian.Tests.TestableCompilationData.UpdateCompilation(Microsoft.CodeAnalysis.MetadataReference reference)
 - Durian.Tests.TestableCompilationData.UpdateCompilation(Microsoft.CodeAnalysis.MetadataReference original, Microsoft.CodeAnalysis.MetadataReference updated)
 - Durian.Tests.TestableCompilationData.UpdateCompilation(System.Collections.Generic.IEnumerable'1[Microsoft.CodeAnalysis.MetadataReference] references)
 - Durian.Tests.TestableCompilationData.GetNode'1(System.String source, Int32 index)
 - Durian.Tests.TestableCompilationData.GetNode'1(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree syntaxTree, Int32 index)
 - Durian.Tests.TestableCompilationData.GetSymbol'1(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode node)
 - Durian.Tests.TestableCompilationData.GetSymbol'2(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree syntaxTree, Int32 index)
 - Durian.Tests.TestableCompilationData.GetSymbol'2(System.String source, Int32 index)
 - Durian.Tests.TestableCompilationData.GetMemberData'1(System.String source, Int32 index)
 - Durian.Tests.TestableCompilationData.Create(Boolean includeDefaultAssemblies)
 - Durian.Tests.TestableCompilationData.Create(System.Collections.Generic.IEnumerable'1[System.String] sources, System.Collections.Generic.IEnumerable'1[System.Reflection.Assembly] assemblies)
 - Durian.Tests.TestableCompilationData.Create(System.Collections.Generic.IEnumerable'1[System.String] sources)
 - Durian.Tests.TestableCompilationData.Create(System.String source)
 - Durian.Tests.TestableCompilationData.Create(System.String source, System.Collections.Generic.IEnumerable'1[System.Reflection.Assembly] assemblies)
 - Durian.Tests.TestableCompilationData.Create(Microsoft.CodeAnalysis.CSharp.CSharpCompilation compilation)
 - Durian.Tests.TestableCompilationData.get_OriginalCompilation
 - Durian.Tests.TestableCompilationData.set_OriginalCompilation
 - Durian.Tests.TestableCompilationData.get_CurrentCompilation
 - Durian.Tests.TestableCompilationData.get_HasErrors
 - Durian.Tests.TestableCompilationData.set_HasErrors
 - Durian.Tests.TestNameToFile
 - Durian.Tests.TestNameToFile.GetFileName()
 - Durian.Tests.TestNameToFile.Success()
 - Durian.Tests.TestNameToFile.Reset()
 - Durian.Tests.TestNameToFile()
 - Durian.Tests.TestNameToFile(System.String testName)
 - Durian.Tests.TestNameToFile.get_Counter
 - Durian.Tests.TestNameToFile.set_Counter
 - Durian.Tests.TestNameToFile.get_TestName
 - Durian.Tests.TestNameToFile.set_TestName
 - Durian.Tests.AddToCompilationAttribute
 - Durian.Tests.AddToCompilationAttribute.Collect(System.Type sourceType)
 - Durian.Tests.AddToCompilationAttribute.Collect(System.Type sourceType, Microsoft.CodeAnalysis.CSharp.CSharpCompilation compilation)
 - Durian.Tests.AddToCompilationAttribute()
 - Durian.Tests.GeneratorExecute
 - Durian.Tests.GeneratorExecute.Invoke(Microsoft.CodeAnalysis.GeneratorExecutionContext& context)
 - Durian.Tests.GeneratorExecute.BeginInvoke(Microsoft.CodeAnalysis.GeneratorExecutionContext& context, System.AsyncCallback callback, System.Object object)
 - Durian.Tests.GeneratorExecute.EndInvoke(Microsoft.CodeAnalysis.GeneratorExecutionContext& context, System.IAsyncResult result)
 - Durian.Tests.GeneratorExecute(System.Object object, IntPtr method)
 - Durian.Tests.GeneratorFiltrate`1
 - Durian.Tests.GeneratorFiltrate'1.Invoke(Durian.Generator.FilterGroup'1[TFilter] filterGroup, Microsoft.CodeAnalysis.GeneratorExecutionContext& context)
 - Durian.Tests.GeneratorFiltrate'1.BeginInvoke(Durian.Generator.FilterGroup'1[TFilter] filterGroup, Microsoft.CodeAnalysis.GeneratorExecutionContext& context, System.AsyncCallback callback, System.Object object)
 - Durian.Tests.GeneratorFiltrate'1.EndInvoke(Microsoft.CodeAnalysis.GeneratorExecutionContext& context, System.IAsyncResult result)
 - Durian.Tests.GeneratorFiltrate'1(System.Object object, IntPtr method)
 - Durian.Tests.GeneratorInitialize
 - Durian.Tests.GeneratorInitialize.Invoke(Microsoft.CodeAnalysis.GeneratorInitializationContext context)
 - Durian.Tests.GeneratorInitialize.BeginInvoke(Microsoft.CodeAnalysis.GeneratorInitializationContext context, System.AsyncCallback callback, System.Object object)
 - Durian.Tests.GeneratorInitialize.EndInvoke(System.IAsyncResult result)
 - Durian.Tests.GeneratorInitialize(System.Object object, IntPtr method)
 - Durian.Tests.GeneratorTestResultProvider
 - Durian.Tests.GeneratorTestResultProvider.Invoke(Microsoft.CodeAnalysis.CSharp.CSharpGeneratorDriver driver, Microsoft.CodeAnalysis.CSharp.CSharpCompilation input, Microsoft.CodeAnalysis.CSharp.CSharpCompilation output)
 - Durian.Tests.GeneratorTestResultProvider.BeginInvoke(Microsoft.CodeAnalysis.CSharp.CSharpGeneratorDriver driver, Microsoft.CodeAnalysis.CSharp.CSharpCompilation input, Microsoft.CodeAnalysis.CSharp.CSharpCompilation output, System.AsyncCallback callback, System.Object object)
 - Durian.Tests.GeneratorTestResultProvider.EndInvoke(System.IAsyncResult result)
 - Durian.Tests.GeneratorTestResultProvider(System.Object object, IntPtr method)
 - Durian.Tests.GeneratorTestResultProvider`1
 - Durian.Tests.GeneratorTestResultProvider'1.Invoke(Microsoft.CodeAnalysis.CSharp.CSharpGeneratorDriver driver, Microsoft.CodeAnalysis.CSharp.CSharpCompilation input, Microsoft.CodeAnalysis.CSharp.CSharpCompilation output)
 - Durian.Tests.GeneratorTestResultProvider'1.BeginInvoke(Microsoft.CodeAnalysis.CSharp.CSharpGeneratorDriver driver, Microsoft.CodeAnalysis.CSharp.CSharpCompilation input, Microsoft.CodeAnalysis.CSharp.CSharpCompilation output, System.AsyncCallback callback, System.Object object)
 - Durian.Tests.GeneratorTestResultProvider'1.EndInvoke(System.IAsyncResult result)
 - Durian.Tests.GeneratorTestResultProvider'1(System.Object object, IntPtr method)
 - Durian.Tests.IGeneratorTestResult
 - Durian.Tests.IGeneratorTestResult.Compare(Microsoft.CodeAnalysis.GeneratorDriverRunResult result)
 - Durian.Tests.IGeneratorTestResult.get_InputCompilation
 - Durian.Tests.IGeneratorTestResult.get_OutputCompilation
 - Durian.Tests.IGeneratorTestResult.get_Exception
 - Durian.Tests.IGeneratorTestResult.get_Diagnostics
 - Durian.Tests.IGeneratorTestResult.get_Generator
 - Durian.Tests.IGeneratorTestResult.get_IsGenerated
 - Durian.Tests.Fixtures.CompilationDataFixture
 - Durian.Tests.Fixtures.CompilationDataFixture()
 - Durian.Tests.Fixtures.CompilationDataFixture.get_Compilation
 - Durian.Tests.Fixtures.CompilationFixture
 - Durian.Tests.Fixtures.CompilationFixture()
 - Durian.Tests.Fixtures.CompilationFixture.get_Compilation
