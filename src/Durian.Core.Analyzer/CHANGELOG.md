## Release 1.0.0

### Added

 - Durian.Generator.Core.CoreProjectAnalyzer
 - Durian.Generator.Core.CoreProjectAnalyzer.Initialize(Microsoft.CodeAnalysis.Diagnostics.AnalysisContext context)
 - Durian.Generator.Core.CoreProjectAnalyzer()
 - Durian.Generator.Core.CoreProjectAnalyzer.get_SupportedDiagnostics
 - Durian.Generator.Core.CustomTypesInGeneratorNamespaceAnalyzer
 - Durian.Generator.Core.CustomTypesInGeneratorNamespaceAnalyzer.Initialize(Microsoft.CodeAnalysis.Diagnostics.AnalysisContext context)
 - Durian.Generator.Core.CustomTypesInGeneratorNamespaceAnalyzer()
 - Durian.Generator.Core.CustomTypesInGeneratorNamespaceAnalyzer.get_SupportedDiagnostics
 - Durian.Generator.Core.DurianDiagnostics
 - Durian.Generator.Core.DurianDiagnostics.get_DocsPath
 - Durian.Generator.Core.DurianDiagnostics.DUR0001_ProjectMustReferenceDurianCore
 - Durian.Generator.Core.DurianDiagnostics.DUR0002_ModuleOfTypeIsNotImported
 - Durian.Generator.Core.DurianDiagnostics.DUR0003_DoNotUseTypeFromDurianGeneratorNamespace
 - Durian.Generator.Core.DurianDiagnostics.DUR0004_DurianModulesAreValidOnlyInCSharp
 - Durian.Generator.Core.DurianDiagnostics.DUR0005_DoNotAddTypesToGeneratorNamespace
 - Durian.Generator.Core.DurianDiagnostics.DUR0006_ProjectMustUseCSharp9
 - Durian.Generator.Core.EnableCoreGenerator
 - Durian.Generator.Core.EnableCoreGenerator.GetVersion()
 - Durian.Generator.Core.EnableCoreGenerator.GetGeneratorName()
 - Durian.Generator.Core.EnableCoreGenerator.GetEnabledModules()
 - Durian.Generator.Core.EnableCoreGenerator()
 - Durian.Generator.Core.EnableCoreGenerator(Durian.Generator.Logging.LoggableGeneratorConstructionContext& context)
 - Durian.Generator.Core.EnableCoreGenerator(Durian.Generator.Logging.LoggableGeneratorConstructionContext& context, Durian.Generator.Logging.IFileNameProvider fileNameProvider)
 - Durian.Generator.Core.EnableCoreGenerator(Durian.Generator.Logging.GeneratorLoggingConfiguration loggingConfiguration)
 - Durian.Generator.Core.EnableCoreGenerator(Durian.Generator.Logging.GeneratorLoggingConfiguration loggingConfiguration, Durian.Generator.Logging.IFileNameProvider fileNameProvider)
 - Durian.Generator.Core.EnableCoreGenerator.get_Version
 - Durian.Generator.Core.EnableCoreGenerator.get_GeneratorName
 - Durian.Generator.Core.EnableCoreGenerator.NumStaticTrees
 - Durian.Generator.Core.IsCSharpCompilationAnalyzer
 - Durian.Generator.Core.IsCSharpCompilationAnalyzer.Initialize(Microsoft.CodeAnalysis.Diagnostics.AnalysisContext context)
 - Durian.Generator.Core.IsCSharpCompilationAnalyzer()
 - Durian.Generator.Core.IsCSharpCompilationAnalyzer.get_SupportedDiagnostics
 - Durian.Generator.Core.TypeImportAnalyzer
 - Durian.Generator.Core.TypeImportAnalyzer.CreateCompilation(Microsoft.CodeAnalysis.CSharp.CSharpCompilation compilation)
 - Durian.Generator.Core.TypeImportAnalyzer.Register(Microsoft.CodeAnalysis.Diagnostics.CompilationStartAnalysisContext context, Durian.Generator.CompilationWithImportedTypes compilation)
 - Durian.Generator.Core.TypeImportAnalyzer()
 - Durian.Generator.Core.TypeImportAnalyzer.get_SupportedDiagnostics

