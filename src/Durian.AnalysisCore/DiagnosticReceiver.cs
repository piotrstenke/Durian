using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian
{
	/// <summary>
	/// Contains factory methods for creating <see cref="DiagnosticReceiver{T}"/> for specific types.
	/// </summary>
	public static partial class DiagnosticReceiver
	{
		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="SymbolAnalysisContext"/>.
		/// </summary>
		public static DiagnosticReceiver<SymbolAnalysisContext> ForSymbol()
		{
			return new DiagnosticReceiver<SymbolAnalysisContext>(DurianDiagnostics.ReportDiagnostic);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="SymbolAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="DiagnosticReceiver{T}"/>.</param>
		public static DiagnosticReceiver<SymbolAnalysisContext> ForSymbol(SymbolAnalysisContext context)
		{
			return new DiagnosticReceiver<SymbolAnalysisContext>(DurianDiagnostics.ReportDiagnostic, context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="SyntaxNodeAnalysisContext"/>.
		/// </summary>
		public static DiagnosticReceiver<SyntaxNodeAnalysisContext> ForSyntaxNode()
		{
			return new DiagnosticReceiver<SyntaxNodeAnalysisContext>(DurianDiagnostics.ReportDiagnostic);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="SyntaxNodeAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="DiagnosticReceiver{T}"/>.</param>
		public static DiagnosticReceiver<SyntaxNodeAnalysisContext> ForSyntaxNode(SyntaxNodeAnalysisContext context)
		{
			return new DiagnosticReceiver<SyntaxNodeAnalysisContext>(DurianDiagnostics.ReportDiagnostic, context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="SyntaxTreeAnalysisContext"/>.
		/// </summary>
		public static DiagnosticReceiver<SyntaxTreeAnalysisContext> ForSyntaxTree()
		{
			return new DiagnosticReceiver<SyntaxTreeAnalysisContext>(DurianDiagnostics.ReportDiagnostic);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="SyntaxTreeAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="DiagnosticReceiver{T}"/>.</param>
		public static DiagnosticReceiver<SyntaxTreeAnalysisContext> ForSyntaxTree(SyntaxTreeAnalysisContext context)
		{
			return new DiagnosticReceiver<SyntaxTreeAnalysisContext>(DurianDiagnostics.ReportDiagnostic, context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="CompilationAnalysisContext"/>.
		/// </summary>
		public static DiagnosticReceiver<CompilationAnalysisContext> ForCompilation()
		{
			return new DiagnosticReceiver<CompilationAnalysisContext>(DurianDiagnostics.ReportDiagnostic);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="CompilationAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="DiagnosticReceiver{T}"/>.</param>
		public static DiagnosticReceiver<CompilationAnalysisContext> ForCompilation(CompilationAnalysisContext context)
		{
			return new DiagnosticReceiver<CompilationAnalysisContext>(DurianDiagnostics.ReportDiagnostic, context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="AdditionalFileAnalysisContext"/>.
		/// </summary>
		public static DiagnosticReceiver<AdditionalFileAnalysisContext>.Readonly ForAdditionalFile()
		{
			return new DiagnosticReceiver<AdditionalFileAnalysisContext>.Readonly(DurianDiagnostics.ReportDiagnostic);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="AdditionalFileAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="DiagnosticReceiver{T}"/>.</param>
		public static DiagnosticReceiver<AdditionalFileAnalysisContext>.Readonly ForCompilation(AdditionalFileAnalysisContext context)
		{
			return new DiagnosticReceiver<AdditionalFileAnalysisContext>.Readonly(DurianDiagnostics.ReportDiagnostic, context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="CodeBlockAnalysisContext"/>.
		/// </summary>
		public static DiagnosticReceiver<CodeBlockAnalysisContext> ForCodeBlock()
		{
			return new DiagnosticReceiver<CodeBlockAnalysisContext>(DurianDiagnostics.ReportDiagnostic);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="CodeBlockAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="DiagnosticReceiver{T}"/>.</param>
		public static DiagnosticReceiver<CodeBlockAnalysisContext> ForCodeBlock(CodeBlockAnalysisContext context)
		{
			return new DiagnosticReceiver<CodeBlockAnalysisContext>(DurianDiagnostics.ReportDiagnostic, context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="GeneratorExecutionContext"/>.
		/// </summary>
		public static DiagnosticReceiver<GeneratorExecutionContext>.Readonly ForSourceGenerator()
		{
			return new DiagnosticReceiver<GeneratorExecutionContext>.Readonly(DurianDiagnostics.ReportDiagnostic);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="GeneratorExecutionContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="DiagnosticReceiver{T}"/>.</param>
		public static DiagnosticReceiver<GeneratorExecutionContext>.Readonly ForSourceGenerator(GeneratorExecutionContext context)
		{
			return new DiagnosticReceiver<GeneratorExecutionContext>.Readonly(DurianDiagnostics.ReportDiagnostic, context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="OperationAnalysisContext"/>.
		/// </summary>
		public static DiagnosticReceiver<OperationAnalysisContext> ForOperation()
		{
			return new DiagnosticReceiver<OperationAnalysisContext>(DurianDiagnostics.ReportDiagnostic);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="OperationAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="DiagnosticReceiver{T}"/>.</param>
		public static DiagnosticReceiver<OperationAnalysisContext> ForOperation(OperationAnalysisContext context)
		{
			return new DiagnosticReceiver<OperationAnalysisContext>(DurianDiagnostics.ReportDiagnostic, context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="OperationBlockAnalysisContext"/>.
		/// </summary>
		public static DiagnosticReceiver<OperationBlockAnalysisContext> ForOperationBlock()
		{
			return new DiagnosticReceiver<OperationBlockAnalysisContext>(DurianDiagnostics.ReportDiagnostic);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="OperationBlockAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="DiagnosticReceiver{T}"/>.</param>
		public static DiagnosticReceiver<OperationBlockAnalysisContext> ForOperationBlock(OperationBlockAnalysisContext context)
		{
			return new DiagnosticReceiver<OperationBlockAnalysisContext>(DurianDiagnostics.ReportDiagnostic, context);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="SemanticModelAnalysisContext"/>.
		/// </summary>
		public static DiagnosticReceiver<SemanticModelAnalysisContext> ForSemanticModel()
		{
			return new DiagnosticReceiver<SemanticModelAnalysisContext>(DurianDiagnostics.ReportDiagnostic);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="DiagnosticReceiver{T}"/> class that accepts only <see cref="SemanticModelAnalysisContext"/>.
		/// </summary>
		/// <param name="context">Context of this <see cref="DiagnosticReceiver{T}"/>.</param>
		public static DiagnosticReceiver<SemanticModelAnalysisContext> ForSemanticModel(SemanticModelAnalysisContext context)
		{
			return new DiagnosticReceiver<SemanticModelAnalysisContext>(DurianDiagnostics.ReportDiagnostic, context);
		}
	}
}
