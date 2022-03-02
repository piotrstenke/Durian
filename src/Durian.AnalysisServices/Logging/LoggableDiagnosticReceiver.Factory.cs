// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Analysis.Logging
{
    public partial class LoggableDiagnosticReceiver
    {
        /// <summary>
        /// Contains factory methods for creating <see cref="INodeDiagnosticReceiver"/>s of specific types.
        /// </summary>
        public static class Factory
        {
            /// <summary>
            /// Creates a new instance of the <see cref="ReadonlyContextual{T}"/> class that accepts only <see cref="AdditionalFileAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            public static ReadonlyContextual<AdditionalFileAnalysisContext> AdditionalFile(ILoggableGenerator generator)
            {
                return new ReadonlyContextual<AdditionalFileAnalysisContext>(generator, (in AdditionalFileAnalysisContext context, Diagnostic diag) => context.ReportDiagnostic(diag));
            }

            /// <summary>
            /// Creates a new instance of the <see cref="ReadonlyContextual{T}"/> class that accepts only <see cref="AdditionalFileAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            /// <param name="diagnosticReceiver">Target <see cref="DiagnosticReceiver.ReadonlyContextual{T}"/>.</param>
            public static ReadonlyContextual<AdditionalFileAnalysisContext> AdditionalFile(ILoggableGenerator generator, DiagnosticReceiver.ReadonlyContextual<AdditionalFileAnalysisContext> diagnosticReceiver)
            {
                return new ReadonlyContextual<AdditionalFileAnalysisContext>(generator, diagnosticReceiver);
            }

            /// <summary>
            /// Creates a new instance of the <see cref="ReadonlyContextual{T}"/> class that accepts only <see cref="AdditionalFileAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            /// <param name="context">Context of this <see cref="ReadonlyContextual{T}"/>.</param>
            public static ReadonlyContextual<AdditionalFileAnalysisContext> AdditionalFile(ILoggableGenerator generator, AdditionalFileAnalysisContext context)
            {
                return new ReadonlyContextual<AdditionalFileAnalysisContext>(generator, (in AdditionalFileAnalysisContext context, Diagnostic diag) => context.ReportDiagnostic(diag), context);
            }

            /// <summary>
            /// Creates a new instance of the <see cref="LoggableDiagnosticReceiver"/> class.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            public static LoggableDiagnosticReceiver Basic(ILoggableGenerator generator)
            {
                return new LoggableDiagnosticReceiver(generator);
            }

            /// <summary>
            /// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="CodeBlockAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            public static Contextual<CodeBlockAnalysisContext> CodeBlock(ILoggableGenerator generator)
            {
                return new Contextual<CodeBlockAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag));
            }

            /// <summary>
            /// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="CodeBlockAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
            /// <param name="diagnosticReceiver">Target <see cref="DiagnosticReceiver.Contextual{T}"/>.</param>
            public static Contextual<CodeBlockAnalysisContext> CodeBlock(LoggableGenerator generator, DiagnosticReceiver.Contextual<CodeBlockAnalysisContext> diagnosticReceiver)
            {
                return new Contextual<CodeBlockAnalysisContext>(generator, diagnosticReceiver);
            }

            /// <summary>
            /// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="CodeBlockAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            /// <param name="context">Context of this <see cref="Contextual{T}"/>.</param>
            public static Contextual<CodeBlockAnalysisContext> CodeBlock(ILoggableGenerator generator, CodeBlockAnalysisContext context)
            {
                return new Contextual<CodeBlockAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag), context);
            }

            /// <summary>
            /// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="CompilationAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            public static Contextual<CompilationAnalysisContext> Compilation(ILoggableGenerator generator)
            {
                return new Contextual<CompilationAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag));
            }

            /// <summary>
            /// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="CompilationAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            /// <param name="diagnosticReceiver">Target <see cref="DiagnosticReceiver.Contextual{T}"/>.</param>
            public static Contextual<CompilationAnalysisContext> Compilation(ILoggableGenerator generator, DiagnosticReceiver.Contextual<CompilationAnalysisContext> diagnosticReceiver)
            {
                return new Contextual<CompilationAnalysisContext>(generator, diagnosticReceiver);
            }

            /// <summary>
            /// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="CompilationAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            /// <param name="context">Context of this <see cref="Contextual{T}"/>.</param>
            public static Contextual<CompilationAnalysisContext> Compilation(ILoggableGenerator generator, CompilationAnalysisContext context)
            {
                return new Contextual<CompilationAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag), context);
            }

            /// <summary>
            /// Returns a new instance of the <see cref="DiagnosticReceiver.Empty"/> class.
            /// </summary>
            public static DiagnosticReceiver.Empty Empty()
            {
                return new DiagnosticReceiver.Empty();
            }

            /// <summary>
            /// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="OperationAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            public static Contextual<OperationAnalysisContext> Operation(ILoggableGenerator generator)
            {
                return new Contextual<OperationAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag));
            }

            /// <summary>
            /// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="OperationAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            /// <param name="diagnosticReceiver">Target <see cref="DiagnosticReceiver.Contextual{T}"/>.</param>
            public static Contextual<OperationAnalysisContext> Operation(ILoggableGenerator generator, DiagnosticReceiver.Contextual<OperationAnalysisContext> diagnosticReceiver)
            {
                return new Contextual<OperationAnalysisContext>(generator, diagnosticReceiver);
            }

            /// <summary>
            /// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="OperationAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            /// <param name="context">Context of this <see cref="DiagnosticReceiver.Contextual{T}"/>.</param>
            public static Contextual<OperationAnalysisContext> Operation(ILoggableGenerator generator, OperationAnalysisContext context)
            {
                return new Contextual<OperationAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag), context);
            }

            /// <summary>
            /// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="OperationBlockAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            public static Contextual<OperationBlockAnalysisContext> OperationBlock(ILoggableGenerator generator)
            {
                return new Contextual<OperationBlockAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag));
            }

            /// <summary>
            /// Creates a new OperationBlockAnalysisContext of the <see cref="Contextual{T}"/> class that accepts only <see cref="SymbolAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            /// <param name="diagnosticReceiver">Target <see cref="DiagnosticReceiver.Contextual{T}"/>.</param>
            public static Contextual<OperationBlockAnalysisContext> OperationBlock(ILoggableGenerator generator, DiagnosticReceiver.Contextual<OperationBlockAnalysisContext> diagnosticReceiver)
            {
                return new Contextual<OperationBlockAnalysisContext>(generator, diagnosticReceiver);
            }

            /// <summary>
            /// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="OperationBlockAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            /// <param name="context">Context of this <see cref="DiagnosticReceiver.Contextual{T}"/>.</param>
            public static Contextual<OperationBlockAnalysisContext> OperationBlock(ILoggableGenerator generator, OperationBlockAnalysisContext context)
            {
                return new Contextual<OperationBlockAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag), context);
            }

            /// <summary>
            /// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="SemanticModelAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            public static Contextual<SemanticModelAnalysisContext> SemanticModel(ILoggableGenerator generator)
            {
                return new Contextual<SemanticModelAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag));
            }

            /// <summary>
            /// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="SemanticModelAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            /// <param name="diagnosticReceiver">Target <see cref="DiagnosticReceiver.Contextual{T}"/>.</param>
            public static Contextual<SemanticModelAnalysisContext> SemanticModel(ILoggableGenerator generator, DiagnosticReceiver.Contextual<SemanticModelAnalysisContext> diagnosticReceiver)
            {
                return new Contextual<SemanticModelAnalysisContext>(generator, diagnosticReceiver);
            }

            /// <summary>
            /// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="SemanticModelAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            /// <param name="context">Context of this <see cref="Contextual{T}"/>.</param>
            public static Contextual<SemanticModelAnalysisContext> SemanticModel(ILoggableGenerator generator, SemanticModelAnalysisContext context)
            {
                return new Contextual<SemanticModelAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag), context);
            }

            /// <summary>
            /// Creates a new instance of the <see cref="ReadonlyContextual{T}"/> class that accepts only <see cref="GeneratorExecutionContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            public static ReadonlyContextual<GeneratorExecutionContext> SourceGenerator(ILoggableGenerator generator)
            {
                return new ReadonlyContextual<GeneratorExecutionContext>(generator, (in GeneratorExecutionContext context, Diagnostic diag) => context.ReportDiagnostic(diag));
            }

            /// <summary>
            /// Creates a new instance of the <see cref="ReadonlyContextual{T}"/> class that accepts only <see cref="GeneratorExecutionContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            /// <param name="diagnosticReceiver">Target <see cref="DiagnosticReceiver.ReadonlyContextual{T}"/>.</param>
            public static ReadonlyContextual<GeneratorExecutionContext> SourceGenerator(ILoggableGenerator generator, DiagnosticReceiver.ReadonlyContextual<GeneratorExecutionContext> diagnosticReceiver)
            {
                return new ReadonlyContextual<GeneratorExecutionContext>(generator, diagnosticReceiver);
            }

            /// <summary>
            /// Creates a new instance of the <see cref="ReadonlyContextual{T}"/> class that accepts only <see cref="GeneratorExecutionContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            /// <param name="context">Context of this <see cref="ReadonlyContextual{T}"/>.</param>
            public static ReadonlyContextual<GeneratorExecutionContext> SourceGenerator(ILoggableGenerator generator, GeneratorExecutionContext context)
            {
                return new ReadonlyContextual<GeneratorExecutionContext>(generator, (in GeneratorExecutionContext context, Diagnostic diag) => context.ReportDiagnostic(diag), context);
            }

            /// <summary>
            /// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="SymbolAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            public static Contextual<SymbolAnalysisContext> Symbol(ILoggableGenerator generator)
            {
                return new Contextual<SymbolAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag));
            }

            /// <summary>
            /// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="SymbolAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            /// <param name="diagnosticReceiver">Target <see cref="DiagnosticReceiver.Contextual{T}"/>.</param>
            public static Contextual<SymbolAnalysisContext> Symbol(ILoggableGenerator generator, DiagnosticReceiver.Contextual<SymbolAnalysisContext> diagnosticReceiver)
            {
                return new Contextual<SymbolAnalysisContext>(generator, diagnosticReceiver);
            }

            /// <summary>
            /// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="SymbolAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            /// <param name="context">Context of this <see cref="Contextual{T}"/>.</param>
            public static Contextual<SymbolAnalysisContext> Symbol(ILoggableGenerator generator, SymbolAnalysisContext context)
            {
                return new Contextual<SymbolAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag), context);
            }

            /// <summary>
            /// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="SyntaxNodeAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            public static Contextual<SyntaxNodeAnalysisContext> SyntaxNode(ILoggableGenerator generator)
            {
                return new Contextual<SyntaxNodeAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag));
            }

            /// <summary>
            /// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="SyntaxNodeAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            /// <param name="diagnosticReceiver">Target <see cref="DiagnosticReceiver.Contextual{T}"/>.</param>
            public static Contextual<SyntaxNodeAnalysisContext> SyntaxNode(ILoggableGenerator generator, DiagnosticReceiver.Contextual<SyntaxNodeAnalysisContext> diagnosticReceiver)
            {
                return new Contextual<SyntaxNodeAnalysisContext>(generator, diagnosticReceiver);
            }

            /// <summary>
            /// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="SyntaxNodeAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            /// <param name="context">Context of this <see cref="Contextual{T}"/>.</param>
            public static Contextual<SyntaxNodeAnalysisContext> SyntaxNode(ILoggableGenerator generator, SyntaxNodeAnalysisContext context)
            {
                return new Contextual<SyntaxNodeAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag), context);
            }

            /// <summary>
            /// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="SyntaxTreeAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            public static Contextual<SyntaxTreeAnalysisContext> SyntaxTree(ILoggableGenerator generator)
            {
                return new Contextual<SyntaxTreeAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag));
            }

            /// <summary>
            /// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="SyntaxTreeAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            /// <param name="diagnosticReceiver">Target <see cref="DiagnosticReceiver.Contextual{T}"/>.</param>
            public static Contextual<SyntaxTreeAnalysisContext> SyntaxTree(ILoggableGenerator generator, DiagnosticReceiver.Contextual<SyntaxTreeAnalysisContext> diagnosticReceiver)
            {
                return new Contextual<SyntaxTreeAnalysisContext>(generator, diagnosticReceiver);
            }

            /// <summary>
            /// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="SyntaxTreeAnalysisContext"/>.
            /// </summary>
            /// <param name="generator">Target <see cref="ILoggableGenerator"/>.</param>
            /// <param name="context">Context of this <see cref="Contextual{T}"/>.</param>
            public static Contextual<SyntaxTreeAnalysisContext> SyntaxTree(ILoggableGenerator generator, SyntaxTreeAnalysisContext context)
            {
                return new Contextual<SyntaxTreeAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag), context);
            }
        }
    }
}