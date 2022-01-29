// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Analysis.Logging
{
	public partial class LoggableGenerator
	{
		/// <summary>
		/// Contains factory methods for creating <see cref="INodeDiagnosticReceiver"/>s of specific types.
		/// </summary>
		public static class DiagnosticReceiverFactory
		{
			/// <summary>
			/// Creates a new instance of the <see cref="ReadonlyContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="AdditionalFileAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			public static ReadonlyContextualDiagnosticReceiver<AdditionalFileAnalysisContext> AdditionalFile(LoggableGenerator generator)
			{
				return new ReadonlyContextualDiagnosticReceiver<AdditionalFileAnalysisContext>(generator, (in AdditionalFileAnalysisContext context, Diagnostic diag) => context.ReportDiagnostic(diag));
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ReadonlyContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="AdditionalFileAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			/// <param name="diagnosticReceiver">Target <see cref="Analysis.ReadonlyContextualDiagnosticReceiver{T}"/>.</param>
			public static ReadonlyContextualDiagnosticReceiver<AdditionalFileAnalysisContext> AdditionalFile(LoggableGenerator generator, Analysis.ReadonlyContextualDiagnosticReceiver<AdditionalFileAnalysisContext> diagnosticReceiver)
			{
				return new ReadonlyContextualDiagnosticReceiver<AdditionalFileAnalysisContext>(generator, diagnosticReceiver);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ReadonlyContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="AdditionalFileAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			/// <param name="context">Context of this <see cref="ReadonlyContextualDiagnosticReceiver{T}"/>.</param>
			public static ReadonlyContextualDiagnosticReceiver<AdditionalFileAnalysisContext> AdditionalFile(LoggableGenerator generator, AdditionalFileAnalysisContext context)
			{
				return new ReadonlyContextualDiagnosticReceiver<AdditionalFileAnalysisContext>(generator, (in AdditionalFileAnalysisContext context, Diagnostic diag) => context.ReportDiagnostic(diag), context);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="DiagnosticReceiver"/> class.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			public static DiagnosticReceiver Basic(LoggableGenerator generator)
			{
				return new DiagnosticReceiver(generator);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="CodeBlockAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			public static ContextualDiagnosticReceiver<CodeBlockAnalysisContext> CodeBlock(LoggableGenerator generator)
			{
				return new ContextualDiagnosticReceiver<CodeBlockAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag));
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="CodeBlockAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			/// <param name="diagnosticReceiver">Target <see cref="Analysis.ContextualDiagnosticReceiver{T}"/>.</param>
			public static ContextualDiagnosticReceiver<CodeBlockAnalysisContext> CodeBlock(LoggableGenerator generator, Analysis.ContextualDiagnosticReceiver<CodeBlockAnalysisContext> diagnosticReceiver)
			{
				return new ContextualDiagnosticReceiver<CodeBlockAnalysisContext>(generator, diagnosticReceiver);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="CodeBlockAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			/// <param name="context">Context of this <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
			public static ContextualDiagnosticReceiver<CodeBlockAnalysisContext> CodeBlock(LoggableGenerator generator, CodeBlockAnalysisContext context)
			{
				return new ContextualDiagnosticReceiver<CodeBlockAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag), context);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="CompilationAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			public static ContextualDiagnosticReceiver<CompilationAnalysisContext> Compilation(LoggableGenerator generator)
			{
				return new ContextualDiagnosticReceiver<CompilationAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag));
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="CompilationAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			/// <param name="diagnosticReceiver">Target <see cref="Analysis.ContextualDiagnosticReceiver{T}"/>.</param>
			public static ContextualDiagnosticReceiver<CompilationAnalysisContext> Compilation(LoggableGenerator generator, Analysis.ContextualDiagnosticReceiver<CompilationAnalysisContext> diagnosticReceiver)
			{
				return new ContextualDiagnosticReceiver<CompilationAnalysisContext>(generator, diagnosticReceiver);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="CompilationAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			/// <param name="context">Context of this <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
			public static ContextualDiagnosticReceiver<CompilationAnalysisContext> Compilation(LoggableGenerator generator, CompilationAnalysisContext context)
			{
				return new ContextualDiagnosticReceiver<CompilationAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag), context);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="OperationAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			public static ContextualDiagnosticReceiver<OperationAnalysisContext> Operation(LoggableGenerator generator)
			{
				return new ContextualDiagnosticReceiver<OperationAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag));
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="OperationAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			/// <param name="diagnosticReceiver">Target <see cref="Analysis.ContextualDiagnosticReceiver{T}"/>.</param>
			public static ContextualDiagnosticReceiver<OperationAnalysisContext> Operation(LoggableGenerator generator, Analysis.ContextualDiagnosticReceiver<OperationAnalysisContext> diagnosticReceiver)
			{
				return new ContextualDiagnosticReceiver<OperationAnalysisContext>(generator, diagnosticReceiver);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="OperationAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			/// <param name="context">Context of this <see cref="Analysis.ContextualDiagnosticReceiver{T}"/>.</param>
			public static ContextualDiagnosticReceiver<OperationAnalysisContext> Operation(LoggableGenerator generator, OperationAnalysisContext context)
			{
				return new ContextualDiagnosticReceiver<OperationAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag), context);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="OperationBlockAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			public static ContextualDiagnosticReceiver<OperationBlockAnalysisContext> OperationBlock(LoggableGenerator generator)
			{
				return new ContextualDiagnosticReceiver<OperationBlockAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag));
			}

			/// <summary>
			/// Creates a new OperationBlockAnalysisContext of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="SymbolAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			/// <param name="diagnosticReceiver">Target <see cref="Analysis.ContextualDiagnosticReceiver{T}"/>.</param>
			public static ContextualDiagnosticReceiver<OperationBlockAnalysisContext> OperationBlock(LoggableGenerator generator, Analysis.ContextualDiagnosticReceiver<OperationBlockAnalysisContext> diagnosticReceiver)
			{
				return new ContextualDiagnosticReceiver<OperationBlockAnalysisContext>(generator, diagnosticReceiver);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="OperationBlockAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			/// <param name="context">Context of this <see cref="Analysis.ContextualDiagnosticReceiver{T}"/>.</param>
			public static ContextualDiagnosticReceiver<OperationBlockAnalysisContext> OperationBlock(LoggableGenerator generator, OperationBlockAnalysisContext context)
			{
				return new ContextualDiagnosticReceiver<OperationBlockAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag), context);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="SemanticModelAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			public static ContextualDiagnosticReceiver<SemanticModelAnalysisContext> SemanticModel(LoggableGenerator generator)
			{
				return new ContextualDiagnosticReceiver<SemanticModelAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag));
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="SemanticModelAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			/// <param name="diagnosticReceiver">Target <see cref="Analysis.ContextualDiagnosticReceiver{T}"/>.</param>
			public static ContextualDiagnosticReceiver<SemanticModelAnalysisContext> SemanticModel(LoggableGenerator generator, Analysis.ContextualDiagnosticReceiver<SemanticModelAnalysisContext> diagnosticReceiver)
			{
				return new ContextualDiagnosticReceiver<SemanticModelAnalysisContext>(generator, diagnosticReceiver);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="SemanticModelAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			/// <param name="context">Context of this <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
			public static ContextualDiagnosticReceiver<SemanticModelAnalysisContext> SemanticModel(LoggableGenerator generator, SemanticModelAnalysisContext context)
			{
				return new ContextualDiagnosticReceiver<SemanticModelAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag), context);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ReadonlyContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="GeneratorExecutionContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			public static ReadonlyContextualDiagnosticReceiver<GeneratorExecutionContext> SourceGenerator(LoggableGenerator generator)
			{
				return new ReadonlyContextualDiagnosticReceiver<GeneratorExecutionContext>(generator, (in GeneratorExecutionContext context, Diagnostic diag) => context.ReportDiagnostic(diag));
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ReadonlyContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="GeneratorExecutionContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			/// <param name="diagnosticReceiver">Target <see cref="Analysis.ReadonlyContextualDiagnosticReceiver{T}"/>.</param>
			public static ReadonlyContextualDiagnosticReceiver<GeneratorExecutionContext> SourceGenerator(LoggableGenerator generator, Analysis.ReadonlyContextualDiagnosticReceiver<GeneratorExecutionContext> diagnosticReceiver)
			{
				return new ReadonlyContextualDiagnosticReceiver<GeneratorExecutionContext>(generator, diagnosticReceiver);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ReadonlyContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="GeneratorExecutionContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			/// <param name="context">Context of this <see cref="ReadonlyContextualDiagnosticReceiver{T}"/>.</param>
			public static ReadonlyContextualDiagnosticReceiver<GeneratorExecutionContext> SourceGenerator(LoggableGenerator generator, GeneratorExecutionContext context)
			{
				return new ReadonlyContextualDiagnosticReceiver<GeneratorExecutionContext>(generator, (in GeneratorExecutionContext context, Diagnostic diag) => context.ReportDiagnostic(diag), context);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="SymbolAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			public static ContextualDiagnosticReceiver<SymbolAnalysisContext> Symbol(LoggableGenerator generator)
			{
				return new ContextualDiagnosticReceiver<SymbolAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag));
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="SymbolAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			/// <param name="diagnosticReceiver">Target <see cref="Analysis.ContextualDiagnosticReceiver{T}"/>.</param>
			public static ContextualDiagnosticReceiver<SymbolAnalysisContext> Symbol(LoggableGenerator generator, Analysis.ContextualDiagnosticReceiver<SymbolAnalysisContext> diagnosticReceiver)
			{
				return new ContextualDiagnosticReceiver<SymbolAnalysisContext>(generator, diagnosticReceiver);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="SymbolAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			/// <param name="context">Context of this <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
			public static ContextualDiagnosticReceiver<SymbolAnalysisContext> Symbol(LoggableGenerator generator, SymbolAnalysisContext context)
			{
				return new ContextualDiagnosticReceiver<SymbolAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag), context);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="SyntaxNodeAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			public static ContextualDiagnosticReceiver<SyntaxNodeAnalysisContext> SyntaxNode(LoggableGenerator generator)
			{
				return new ContextualDiagnosticReceiver<SyntaxNodeAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag));
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="SyntaxNodeAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			/// <param name="diagnosticReceiver">Target <see cref="Analysis.ContextualDiagnosticReceiver{T}"/>.</param>
			public static ContextualDiagnosticReceiver<SyntaxNodeAnalysisContext> SyntaxNode(LoggableGenerator generator, Analysis.ContextualDiagnosticReceiver<SyntaxNodeAnalysisContext> diagnosticReceiver)
			{
				return new ContextualDiagnosticReceiver<SyntaxNodeAnalysisContext>(generator, diagnosticReceiver);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="SyntaxNodeAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			/// <param name="context">Context of this <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
			public static ContextualDiagnosticReceiver<SyntaxNodeAnalysisContext> SyntaxNode(LoggableGenerator generator, SyntaxNodeAnalysisContext context)
			{
				return new ContextualDiagnosticReceiver<SyntaxNodeAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag), context);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="SyntaxTreeAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			public static ContextualDiagnosticReceiver<SyntaxTreeAnalysisContext> SyntaxTree(LoggableGenerator generator)
			{
				return new ContextualDiagnosticReceiver<SyntaxTreeAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag));
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="SyntaxTreeAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			/// <param name="diagnosticReceiver">Target <see cref="Analysis.ContextualDiagnosticReceiver{T}"/>.</param>
			public static ContextualDiagnosticReceiver<SyntaxTreeAnalysisContext> SyntaxTree(LoggableGenerator generator, Analysis.ContextualDiagnosticReceiver<SyntaxTreeAnalysisContext> diagnosticReceiver)
			{
				return new ContextualDiagnosticReceiver<SyntaxTreeAnalysisContext>(generator, diagnosticReceiver);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="SyntaxTreeAnalysisContext"/>.
			/// </summary>
			/// <param name="generator">Target <see cref="LoggableGenerator"/>.</param>
			/// <param name="context">Context of this <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
			public static ContextualDiagnosticReceiver<SyntaxTreeAnalysisContext> SyntaxTree(LoggableGenerator generator, SyntaxTreeAnalysisContext context)
			{
				return new ContextualDiagnosticReceiver<SyntaxTreeAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag), context);
			}
		}
	}
}