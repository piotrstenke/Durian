// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Analysis
{
	/// <summary>
	/// Contains factory methods for creating <see cref="IDiagnosticReceiver"/>s of specific type.
	/// </summary>
	public static class DiagnosticReceiverFactory
	{
		/// <summary>
		/// Creates a new instance of the <see cref="ReadonlyContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="AdditionalFileAnalysisContext"/>.
		/// </summary>
		public static ReadonlyContextualDiagnosticReceiver<AdditionalFileAnalysisContext> AdditionalFile()
		{
			return new ReadonlyContextualDiagnosticReceiver<AdditionalFileAnalysisContext>((in AdditionalFileAnalysisContext context, Diagnostic diag) => context.ReportDiagnostic(diag));
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ReadonlyContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="AdditionalFileAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="ReadonlyContextualDiagnosticReceiver{T}"/>.</param>
		public static ReadonlyContextualDiagnosticReceiver<AdditionalFileAnalysisContext> AdditionalFile(AdditionalFileAnalysisContext context)
		{
			return new ReadonlyContextualDiagnosticReceiver<AdditionalFileAnalysisContext>((in AdditionalFileAnalysisContext context, Diagnostic diag) => context.ReportDiagnostic(diag), context);
		}

		/// <summary>
		/// Returns a new instance of the <see cref="DiagnosticBag"/> class.
		/// </summary>
		public static DiagnosticBag Bag()
		{
			return new DiagnosticBag();
		}

		/// <summary>
		/// Returns a new instance of the <see cref="DiagnosticBag"/> class.
		/// </summary>
		/// <param name="capacity">Capacity of the <see cref="DiagnosticBag"/>.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than <c>0</c>.</exception>
		public static DiagnosticBag Bag(int capacity)
		{
			return new DiagnosticBag(capacity);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="CodeBlockAnalysisContext"/>.
		/// </summary>
		public static ContextualDiagnosticReceiver<CodeBlockAnalysisContext> CodeBlock()
		{
			return new ContextualDiagnosticReceiver<CodeBlockAnalysisContext>((context, diag) => context.ReportDiagnostic(diag));
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="CodeBlockAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
		public static ContextualDiagnosticReceiver<CodeBlockAnalysisContext> CodeBlock(CodeBlockAnalysisContext context)
		{
			return new ContextualDiagnosticReceiver<CodeBlockAnalysisContext>((context, diag) => context.ReportDiagnostic(diag), context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="CompilationAnalysisContext"/>.
		/// </summary>
		public static ContextualDiagnosticReceiver<CompilationAnalysisContext> Compilation()
		{
			return new ContextualDiagnosticReceiver<CompilationAnalysisContext>((context, diag) => context.ReportDiagnostic(diag));
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="CompilationAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
		public static ContextualDiagnosticReceiver<CompilationAnalysisContext> Compilation(CompilationAnalysisContext context)
		{
			return new ContextualDiagnosticReceiver<CompilationAnalysisContext>((context, diag) => context.ReportDiagnostic(diag), context);
		}

		/// <summary>
		/// Returns a new instance of the <see cref="DiagnosticReceiver"/> class.
		/// </summary>
		/// <param name="reportAction">Action that is performed when <see cref="DiagnosticReceiver.ReportDiagnostic(Diagnostic)"/>is called.</param>
		/// <exception cref="ArgumentNullException"><paramref name="reportAction"/> is <see langword="null"/>.</exception>
		public static DiagnosticReceiver Direct(DirectReportAction reportAction)
		{
			return new DiagnosticReceiver(reportAction);
		}

		/// <summary>
		/// Returns a new instance of the <see cref="EmptyDiagnosticReceiver"/> class.
		/// </summary>
		public static EmptyDiagnosticReceiver Empty()
		{
			return new EmptyDiagnosticReceiver();
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="OperationAnalysisContext"/>.
		/// </summary>
		public static ContextualDiagnosticReceiver<OperationAnalysisContext> Operation()
		{
			return new ContextualDiagnosticReceiver<OperationAnalysisContext>((context, diag) => context.ReportDiagnostic(diag));
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="OperationAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
		public static ContextualDiagnosticReceiver<OperationAnalysisContext> Operation(OperationAnalysisContext context)
		{
			return new ContextualDiagnosticReceiver<OperationAnalysisContext>((context, diag) => context.ReportDiagnostic(diag), context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="OperationBlockAnalysisContext"/>.
		/// </summary>
		public static ContextualDiagnosticReceiver<OperationBlockAnalysisContext> OperationBlock()
		{
			return new ContextualDiagnosticReceiver<OperationBlockAnalysisContext>((context, diag) => context.ReportDiagnostic(diag));
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="OperationBlockAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
		public static ContextualDiagnosticReceiver<OperationBlockAnalysisContext> OperationBlock(OperationBlockAnalysisContext context)
		{
			return new ContextualDiagnosticReceiver<OperationBlockAnalysisContext>((context, diag) => context.ReportDiagnostic(diag), context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="SemanticModelAnalysisContext"/>.
		/// </summary>
		public static ContextualDiagnosticReceiver<SemanticModelAnalysisContext> SemanticModel()
		{
			return new ContextualDiagnosticReceiver<SemanticModelAnalysisContext>((context, diag) => context.ReportDiagnostic(diag));
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="SemanticModelAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
		public static ContextualDiagnosticReceiver<SemanticModelAnalysisContext> SemanticModel(SemanticModelAnalysisContext context)
		{
			return new ContextualDiagnosticReceiver<SemanticModelAnalysisContext>((context, diag) => context.ReportDiagnostic(diag), context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ReadonlyContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="GeneratorExecutionContext"/>.
		/// </summary>
		public static ReadonlyContextualDiagnosticReceiver<GeneratorExecutionContext> SourceGenerator()
		{
			return new ReadonlyContextualDiagnosticReceiver<GeneratorExecutionContext>((in GeneratorExecutionContext context, Diagnostic diag) => context.ReportDiagnostic(diag));
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ReadonlyContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="GeneratorExecutionContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="ReadonlyContextualDiagnosticReceiver{T}"/>.</param>
		public static ReadonlyContextualDiagnosticReceiver<GeneratorExecutionContext> SourceGenerator(GeneratorExecutionContext context)
		{
			return new ReadonlyContextualDiagnosticReceiver<GeneratorExecutionContext>((in GeneratorExecutionContext context, Diagnostic diag) => context.ReportDiagnostic(diag), context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="SymbolAnalysisContext"/>.
		/// </summary>
		public static ContextualDiagnosticReceiver<SymbolAnalysisContext> Symbol()
		{
			return new ContextualDiagnosticReceiver<SymbolAnalysisContext>((context, diag) => context.ReportDiagnostic(diag));
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="SymbolAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
		public static ContextualDiagnosticReceiver<SymbolAnalysisContext> Symbol(SymbolAnalysisContext context)
		{
			return new ContextualDiagnosticReceiver<SymbolAnalysisContext>((context, diag) => context.ReportDiagnostic(diag), context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="SyntaxNodeAnalysisContext"/>.
		/// </summary>
		public static ContextualDiagnosticReceiver<SyntaxNodeAnalysisContext> SyntaxNode()
		{
			return new ContextualDiagnosticReceiver<SyntaxNodeAnalysisContext>((context, diag) => context.ReportDiagnostic(diag));
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="SyntaxNodeAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
		public static ContextualDiagnosticReceiver<SyntaxNodeAnalysisContext> SyntaxNode(SyntaxNodeAnalysisContext context)
		{
			return new ContextualDiagnosticReceiver<SyntaxNodeAnalysisContext>((context, diag) => context.ReportDiagnostic(diag), context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="SyntaxTreeAnalysisContext"/>.
		/// </summary>
		public static ContextualDiagnosticReceiver<SyntaxTreeAnalysisContext> SyntaxTree()
		{
			return new ContextualDiagnosticReceiver<SyntaxTreeAnalysisContext>((context, diag) => context.ReportDiagnostic(diag));
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ContextualDiagnosticReceiver{T}"/> class that accepts only <see cref="SyntaxTreeAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
		public static ContextualDiagnosticReceiver<SyntaxTreeAnalysisContext> SyntaxTree(SyntaxTreeAnalysisContext context)
		{
			return new ContextualDiagnosticReceiver<SyntaxTreeAnalysisContext>((context, diag) => context.ReportDiagnostic(diag), context);
		}
	}
}