using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian
{
	/// <summary>
	/// Contains factory methods for creating <see cref="DiagnosticReceiver{T}"/> for specific types.
	/// </summary>
	public static class DiagnosticReceiverFactory
	{
		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="SymbolAnalysisContext"/>.
		/// </summary>
		public static DiagnosticReceiver<SymbolAnalysisContext> Symbol()
		{
			return new DiagnosticReceiver<SymbolAnalysisContext>(DurianDiagnostics.ReportDiagnostic);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="SymbolAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="DiagnosticReceiver{T}"/>.</param>
		public static DiagnosticReceiver<SymbolAnalysisContext> Symbol(SymbolAnalysisContext context)
		{
			return new DiagnosticReceiver<SymbolAnalysisContext>(DurianDiagnostics.ReportDiagnostic, context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="SyntaxNodeAnalysisContext"/>.
		/// </summary>
		public static DiagnosticReceiver<SyntaxNodeAnalysisContext> SyntaxNode()
		{
			return new DiagnosticReceiver<SyntaxNodeAnalysisContext>(DurianDiagnostics.ReportDiagnostic);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="SyntaxNodeAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="DiagnosticReceiver{T}"/>.</param>
		public static DiagnosticReceiver<SyntaxNodeAnalysisContext> SyntaxNode(SyntaxNodeAnalysisContext context)
		{
			return new DiagnosticReceiver<SyntaxNodeAnalysisContext>(DurianDiagnostics.ReportDiagnostic, context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="SyntaxTreeAnalysisContext"/>.
		/// </summary>
		public static DiagnosticReceiver<SyntaxTreeAnalysisContext> SyntaxTree()
		{
			return new DiagnosticReceiver<SyntaxTreeAnalysisContext>(DurianDiagnostics.ReportDiagnostic);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="SyntaxTreeAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="DiagnosticReceiver{T}"/>.</param>
		public static DiagnosticReceiver<SyntaxTreeAnalysisContext> SyntaxTree(SyntaxTreeAnalysisContext context)
		{
			return new DiagnosticReceiver<SyntaxTreeAnalysisContext>(DurianDiagnostics.ReportDiagnostic, context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="CompilationAnalysisContext"/>.
		/// </summary>
		public static DiagnosticReceiver<CompilationAnalysisContext> Compilation()
		{
			return new DiagnosticReceiver<CompilationAnalysisContext>(DurianDiagnostics.ReportDiagnostic);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="CompilationAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="DiagnosticReceiver{T}"/>.</param>
		public static DiagnosticReceiver<CompilationAnalysisContext> Compilation(CompilationAnalysisContext context)
		{
			return new DiagnosticReceiver<CompilationAnalysisContext>(DurianDiagnostics.ReportDiagnostic, context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="AdditionalFileAnalysisContext"/>.
		/// </summary>
		public static ReadonlyDiagnosticReceiver<AdditionalFileAnalysisContext> AdditionalFile()
		{
			return new ReadonlyDiagnosticReceiver<AdditionalFileAnalysisContext>(DurianDiagnostics.ReportDiagnostic);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="AdditionalFileAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="DiagnosticReceiver{T}"/>.</param>
		public static ReadonlyDiagnosticReceiver<AdditionalFileAnalysisContext> AdditionalFile(AdditionalFileAnalysisContext context)
		{
			return new ReadonlyDiagnosticReceiver<AdditionalFileAnalysisContext>(DurianDiagnostics.ReportDiagnostic, context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="CodeBlockAnalysisContext"/>.
		/// </summary>
		public static DiagnosticReceiver<CodeBlockAnalysisContext> CodeBlock()
		{
			return new DiagnosticReceiver<CodeBlockAnalysisContext>(DurianDiagnostics.ReportDiagnostic);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="CodeBlockAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="DiagnosticReceiver{T}"/>.</param>
		public static DiagnosticReceiver<CodeBlockAnalysisContext> CodeBlock(CodeBlockAnalysisContext context)
		{
			return new DiagnosticReceiver<CodeBlockAnalysisContext>(DurianDiagnostics.ReportDiagnostic, context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="GeneratorExecutionContext"/>.
		/// </summary>
		public static ReadonlyDiagnosticReceiver<GeneratorExecutionContext> SourceGenerator()
		{
			return new ReadonlyDiagnosticReceiver<GeneratorExecutionContext>(DurianDiagnostics.ReportDiagnostic);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="GeneratorExecutionContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="DiagnosticReceiver{T}"/>.</param>
		public static ReadonlyDiagnosticReceiver<GeneratorExecutionContext> SourceGenerator(GeneratorExecutionContext context)
		{
			return new ReadonlyDiagnosticReceiver<GeneratorExecutionContext>(DurianDiagnostics.ReportDiagnostic, context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="OperationAnalysisContext"/>.
		/// </summary>
		public static DiagnosticReceiver<OperationAnalysisContext> Operation()
		{
			return new DiagnosticReceiver<OperationAnalysisContext>(DurianDiagnostics.ReportDiagnostic);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="OperationAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="DiagnosticReceiver{T}"/>.</param>
		public static DiagnosticReceiver<OperationAnalysisContext> Operation(OperationAnalysisContext context)
		{
			return new DiagnosticReceiver<OperationAnalysisContext>(DurianDiagnostics.ReportDiagnostic, context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="OperationBlockAnalysisContext"/>.
		/// </summary>
		public static DiagnosticReceiver<OperationBlockAnalysisContext> OperationBlock()
		{
			return new DiagnosticReceiver<OperationBlockAnalysisContext>(DurianDiagnostics.ReportDiagnostic);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="OperationBlockAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="DiagnosticReceiver{T}"/>.</param>
		public static DiagnosticReceiver<OperationBlockAnalysisContext> OperationBlock(OperationBlockAnalysisContext context)
		{
			return new DiagnosticReceiver<OperationBlockAnalysisContext>(DurianDiagnostics.ReportDiagnostic, context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="SemanticModelAnalysisContext"/>.
		/// </summary>
		public static DiagnosticReceiver<SemanticModelAnalysisContext> SemanticModel()
		{
			return new DiagnosticReceiver<SemanticModelAnalysisContext>(DurianDiagnostics.ReportDiagnostic);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="SemanticModelAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="DiagnosticReceiver{T}"/>.</param>
		public static DiagnosticReceiver<SemanticModelAnalysisContext> SemanticModel(SemanticModelAnalysisContext context)
		{
			return new DiagnosticReceiver<SemanticModelAnalysisContext>(DurianDiagnostics.ReportDiagnostic, context);
		}

		/// <summary>
		/// Returns a new instance of the <see cref="EmptyDiagnosticReceiver"/> class.
		/// </summary>
		public static EmptyDiagnosticReceiver Empty()
		{
			return new EmptyDiagnosticReceiver();
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
		/// Returns a new instance of the <see cref="DiagnosticBag"/> class.
		/// </summary>
		/// <param name="reportAction">Action that is performed when <see cref="ContextlessDiagnosticReceiver.ReportDiagnostic(DiagnosticDescriptor, Location, object[])"/> is called.</param>
		/// <exception cref="ArgumentNullException"><paramref name="reportAction"/> is <c>null</c>.</exception>
		public static ContextlessDiagnosticReceiver ReportAction(ReportAction reportAction)
		{
			return new ContextlessDiagnosticReceiver(reportAction);
		}
	}
}
