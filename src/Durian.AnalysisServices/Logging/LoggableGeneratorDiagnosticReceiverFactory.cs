// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Analysis.Logging
{
	/// <summary>
	/// Contains factory methods for creating <see cref="INodeDiagnosticReceiver"/>s of specific types.
	/// </summary>
	public static class LoggableGeneratorDiagnosticReceiverFactory
	{
		/// <summary>
		/// Creates a new instance of the <see cref="ReadonlyContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="AdditionalFileAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		public static ReadonlyContextualLoggableGeneratorDiagnosticReceiver<AdditionalFileAnalysisContext> AdditionalFile(LoggableSourceGenerator generator)
		{
			return new ReadonlyContextualLoggableGeneratorDiagnosticReceiver<AdditionalFileAnalysisContext>(generator, (in AdditionalFileAnalysisContext context, Diagnostic diag) => context.ReportDiagnostic(diag));
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ReadonlyContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="AdditionalFileAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		/// <param name="diagnosticReceiver">Target <see cref="ReadonlyContextualDiagnosticReceiver{T}"/>.</param>
		public static ReadonlyContextualLoggableGeneratorDiagnosticReceiver<AdditionalFileAnalysisContext> AdditionalFile(LoggableSourceGenerator generator, ReadonlyContextualDiagnosticReceiver<AdditionalFileAnalysisContext> diagnosticReceiver)
		{
			return new ReadonlyContextualLoggableGeneratorDiagnosticReceiver<AdditionalFileAnalysisContext>(generator, diagnosticReceiver);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ReadonlyContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="AdditionalFileAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		/// <param name="context">Context of this <see cref="ReadonlyContextualLoggableGeneratorDiagnosticReceiver{T}"/>.</param>
		public static ReadonlyContextualLoggableGeneratorDiagnosticReceiver<AdditionalFileAnalysisContext> AdditionalFile(LoggableSourceGenerator generator, AdditionalFileAnalysisContext context)
		{
			return new ReadonlyContextualLoggableGeneratorDiagnosticReceiver<AdditionalFileAnalysisContext>(generator, (in AdditionalFileAnalysisContext context, Diagnostic diag) => context.ReportDiagnostic(diag), context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="LoggableGeneratorDiagnosticReceiver"/> class.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		public static LoggableGeneratorDiagnosticReceiver Basic(LoggableSourceGenerator generator)
		{
			return new LoggableGeneratorDiagnosticReceiver(generator);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="CodeBlockAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		public static ContextualLoggableGeneratorDiagnosticReceiver<CodeBlockAnalysisContext> CodeBlock(LoggableSourceGenerator generator)
		{
			return new ContextualLoggableGeneratorDiagnosticReceiver<CodeBlockAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag));
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="CodeBlockAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		/// <param name="diagnosticReceiver">Target <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
		public static ContextualLoggableGeneratorDiagnosticReceiver<CodeBlockAnalysisContext> CodeBlock(LoggableSourceGenerator generator, ContextualDiagnosticReceiver<CodeBlockAnalysisContext> diagnosticReceiver)
		{
			return new ContextualLoggableGeneratorDiagnosticReceiver<CodeBlockAnalysisContext>(generator, diagnosticReceiver);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="CodeBlockAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		/// <param name="context">Context of this <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/>.</param>
		public static ContextualLoggableGeneratorDiagnosticReceiver<CodeBlockAnalysisContext> CodeBlock(LoggableSourceGenerator generator, CodeBlockAnalysisContext context)
		{
			return new ContextualLoggableGeneratorDiagnosticReceiver<CodeBlockAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag), context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="CompilationAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		public static ContextualLoggableGeneratorDiagnosticReceiver<CompilationAnalysisContext> Compilation(LoggableSourceGenerator generator)
		{
			return new ContextualLoggableGeneratorDiagnosticReceiver<CompilationAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag));
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="CompilationAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		/// <param name="diagnosticReceiver">Target <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
		public static ContextualLoggableGeneratorDiagnosticReceiver<CompilationAnalysisContext> Compilation(LoggableSourceGenerator generator, ContextualDiagnosticReceiver<CompilationAnalysisContext> diagnosticReceiver)
		{
			return new ContextualLoggableGeneratorDiagnosticReceiver<CompilationAnalysisContext>(generator, diagnosticReceiver);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="CompilationAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		/// <param name="context">Context of this <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/>.</param>
		public static ContextualLoggableGeneratorDiagnosticReceiver<CompilationAnalysisContext> Compilation(LoggableSourceGenerator generator, CompilationAnalysisContext context)
		{
			return new ContextualLoggableGeneratorDiagnosticReceiver<CompilationAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag), context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="OperationAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		public static ContextualLoggableGeneratorDiagnosticReceiver<OperationAnalysisContext> Operation(LoggableSourceGenerator generator)
		{
			return new ContextualLoggableGeneratorDiagnosticReceiver<OperationAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag));
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="OperationAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		/// <param name="diagnosticReceiver">Target <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
		public static ContextualLoggableGeneratorDiagnosticReceiver<OperationAnalysisContext> Operation(LoggableSourceGenerator generator, ContextualDiagnosticReceiver<OperationAnalysisContext> diagnosticReceiver)
		{
			return new ContextualLoggableGeneratorDiagnosticReceiver<OperationAnalysisContext>(generator, diagnosticReceiver);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="OperationAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		/// <param name="context">Context of this <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
		public static ContextualLoggableGeneratorDiagnosticReceiver<OperationAnalysisContext> Operation(LoggableSourceGenerator generator, OperationAnalysisContext context)
		{
			return new ContextualLoggableGeneratorDiagnosticReceiver<OperationAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag), context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="OperationBlockAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		public static ContextualLoggableGeneratorDiagnosticReceiver<OperationBlockAnalysisContext> OperationBlock(LoggableSourceGenerator generator)
		{
			return new ContextualLoggableGeneratorDiagnosticReceiver<OperationBlockAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag));
		}

		/// <summary>
		/// Creates a new OperationBlockAnalysisContext of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="SymbolAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		/// <param name="diagnosticReceiver">Target <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
		public static ContextualLoggableGeneratorDiagnosticReceiver<OperationBlockAnalysisContext> OperationBlock(LoggableSourceGenerator generator, ContextualDiagnosticReceiver<OperationBlockAnalysisContext> diagnosticReceiver)
		{
			return new ContextualLoggableGeneratorDiagnosticReceiver<OperationBlockAnalysisContext>(generator, diagnosticReceiver);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="OperationBlockAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		/// <param name="context">Context of this <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
		public static ContextualLoggableGeneratorDiagnosticReceiver<OperationBlockAnalysisContext> OperationBlock(LoggableSourceGenerator generator, OperationBlockAnalysisContext context)
		{
			return new ContextualLoggableGeneratorDiagnosticReceiver<OperationBlockAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag), context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="SemanticModelAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		public static ContextualLoggableGeneratorDiagnosticReceiver<SemanticModelAnalysisContext> SemanticModel(LoggableSourceGenerator generator)
		{
			return new ContextualLoggableGeneratorDiagnosticReceiver<SemanticModelAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag));
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="SemanticModelAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		/// <param name="diagnosticReceiver">Target <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
		public static ContextualLoggableGeneratorDiagnosticReceiver<SemanticModelAnalysisContext> SemanticModel(LoggableSourceGenerator generator, ContextualDiagnosticReceiver<SemanticModelAnalysisContext> diagnosticReceiver)
		{
			return new ContextualLoggableGeneratorDiagnosticReceiver<SemanticModelAnalysisContext>(generator, diagnosticReceiver);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="SemanticModelAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		/// <param name="context">Context of this <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/>.</param>
		public static ContextualLoggableGeneratorDiagnosticReceiver<SemanticModelAnalysisContext> SemanticModel(LoggableSourceGenerator generator, SemanticModelAnalysisContext context)
		{
			return new ContextualLoggableGeneratorDiagnosticReceiver<SemanticModelAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag), context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ReadonlyContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="GeneratorExecutionContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		public static ReadonlyContextualLoggableGeneratorDiagnosticReceiver<GeneratorExecutionContext> SourceGenerator(LoggableSourceGenerator generator)
		{
			return new ReadonlyContextualLoggableGeneratorDiagnosticReceiver<GeneratorExecutionContext>(generator, (in GeneratorExecutionContext context, Diagnostic diag) => context.ReportDiagnostic(diag));
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ReadonlyContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="GeneratorExecutionContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		/// <param name="diagnosticReceiver">Target <see cref="ReadonlyContextualDiagnosticReceiver{T}"/>.</param>
		public static ReadonlyContextualLoggableGeneratorDiagnosticReceiver<GeneratorExecutionContext> SourceGenerator(LoggableSourceGenerator generator, ReadonlyContextualDiagnosticReceiver<GeneratorExecutionContext> diagnosticReceiver)
		{
			return new ReadonlyContextualLoggableGeneratorDiagnosticReceiver<GeneratorExecutionContext>(generator, diagnosticReceiver);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ReadonlyContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="GeneratorExecutionContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		/// <param name="context">Context of this <see cref="ReadonlyContextualLoggableGeneratorDiagnosticReceiver{T}"/>.</param>
		public static ReadonlyContextualLoggableGeneratorDiagnosticReceiver<GeneratorExecutionContext> SourceGenerator(LoggableSourceGenerator generator, GeneratorExecutionContext context)
		{
			return new ReadonlyContextualLoggableGeneratorDiagnosticReceiver<GeneratorExecutionContext>(generator, (in GeneratorExecutionContext context, Diagnostic diag) => context.ReportDiagnostic(diag), context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="SymbolAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		public static ContextualLoggableGeneratorDiagnosticReceiver<SymbolAnalysisContext> Symbol(LoggableSourceGenerator generator)
		{
			return new ContextualLoggableGeneratorDiagnosticReceiver<SymbolAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag));
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="SymbolAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		/// <param name="diagnosticReceiver">Target <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
		public static ContextualLoggableGeneratorDiagnosticReceiver<SymbolAnalysisContext> Symbol(LoggableSourceGenerator generator, ContextualDiagnosticReceiver<SymbolAnalysisContext> diagnosticReceiver)
		{
			return new ContextualLoggableGeneratorDiagnosticReceiver<SymbolAnalysisContext>(generator, diagnosticReceiver);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="SymbolAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		/// <param name="context">Context of this <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/>.</param>
		public static ContextualLoggableGeneratorDiagnosticReceiver<SymbolAnalysisContext> Symbol(LoggableSourceGenerator generator, SymbolAnalysisContext context)
		{
			return new ContextualLoggableGeneratorDiagnosticReceiver<SymbolAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag), context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="SyntaxNodeAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		public static ContextualLoggableGeneratorDiagnosticReceiver<SyntaxNodeAnalysisContext> SyntaxNode(LoggableSourceGenerator generator)
		{
			return new ContextualLoggableGeneratorDiagnosticReceiver<SyntaxNodeAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag));
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="SyntaxNodeAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		/// <param name="diagnosticReceiver">Target <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
		public static ContextualLoggableGeneratorDiagnosticReceiver<SyntaxNodeAnalysisContext> SyntaxNode(LoggableSourceGenerator generator, ContextualDiagnosticReceiver<SyntaxNodeAnalysisContext> diagnosticReceiver)
		{
			return new ContextualLoggableGeneratorDiagnosticReceiver<SyntaxNodeAnalysisContext>(generator, diagnosticReceiver);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="SyntaxNodeAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		/// <param name="context">Context of this <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/>.</param>
		public static ContextualLoggableGeneratorDiagnosticReceiver<SyntaxNodeAnalysisContext> SyntaxNode(LoggableSourceGenerator generator, SyntaxNodeAnalysisContext context)
		{
			return new ContextualLoggableGeneratorDiagnosticReceiver<SyntaxNodeAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag), context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="SyntaxTreeAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		public static ContextualLoggableGeneratorDiagnosticReceiver<SyntaxTreeAnalysisContext> SyntaxTree(LoggableSourceGenerator generator)
		{
			return new ContextualLoggableGeneratorDiagnosticReceiver<SyntaxTreeAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag));
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="SyntaxTreeAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		/// <param name="diagnosticReceiver">Target <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
		public static ContextualLoggableGeneratorDiagnosticReceiver<SyntaxTreeAnalysisContext> SyntaxTree(LoggableSourceGenerator generator, ContextualDiagnosticReceiver<SyntaxTreeAnalysisContext> diagnosticReceiver)
		{
			return new ContextualLoggableGeneratorDiagnosticReceiver<SyntaxTreeAnalysisContext>(generator, diagnosticReceiver);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class that accepts only <see cref="SyntaxTreeAnalysisContext"/>.
		/// </summary>
		/// <param name="generator">Target <see cref="LoggableSourceGenerator"/>.</param>
		/// <param name="context">Context of this <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/>.</param>
		public static ContextualLoggableGeneratorDiagnosticReceiver<SyntaxTreeAnalysisContext> SyntaxTree(LoggableSourceGenerator generator, SyntaxTreeAnalysisContext context)
		{
			return new ContextualLoggableGeneratorDiagnosticReceiver<SyntaxTreeAnalysisContext>(generator, (context, diag) => context.ReportDiagnostic(diag), context);
		}
	}
}