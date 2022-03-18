// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;

namespace Durian.Analysis
{
	public sealed partial class DiagnosticReceiver
	{
		/// <summary>
		/// Contains factory methods for creating <see cref="IDiagnosticReceiver"/>s of specific type.
		/// </summary>
		public static class Factory
		{
			/// <summary>
			/// Creates a new instance of the <see cref="ReadonlyContextual{T}"/> class that accepts only <see cref="AdditionalFileAnalysisContext"/>.
			/// </summary>
			public static ReadonlyContextual<AdditionalFileAnalysisContext> AdditionalFile()
			{
				return new ReadonlyContextual<AdditionalFileAnalysisContext>((in AdditionalFileAnalysisContext context, Diagnostic diag) => context.ReportDiagnostic(diag));
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ReadonlyContextual{T}"/> class that accepts only <see cref="AdditionalFileAnalysisContext"/>.
			/// </summary>
			/// <param name="context">Context of this <see cref="ReadonlyContextual{T}"/>.</param>
			public static ReadonlyContextual<AdditionalFileAnalysisContext> AdditionalFile(AdditionalFileAnalysisContext context)
			{
				return new ReadonlyContextual<AdditionalFileAnalysisContext>((in AdditionalFileAnalysisContext context, Diagnostic diag) => context.ReportDiagnostic(diag), context);
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
			/// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="CodeBlockAnalysisContext"/>.
			/// </summary>
			public static Contextual<CodeBlockAnalysisContext> CodeBlock()
			{
				return new Contextual<CodeBlockAnalysisContext>((context, diag) => context.ReportDiagnostic(diag));
			}

			/// <summary>
			/// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="CodeBlockAnalysisContext"/>.
			/// </summary>
			/// <param name="context">Context of this <see cref="Contextual{T}"/>.</param>
			public static Contextual<CodeBlockAnalysisContext> CodeBlock(CodeBlockAnalysisContext context)
			{
				return new Contextual<CodeBlockAnalysisContext>((context, diag) => context.ReportDiagnostic(diag), context);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="CompilationAnalysisContext"/>.
			/// </summary>
			public static Contextual<CompilationAnalysisContext> Compilation()
			{
				return new Contextual<CompilationAnalysisContext>((context, diag) => context.ReportDiagnostic(diag));
			}

			/// <summary>
			/// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="CompilationAnalysisContext"/>.
			/// </summary>
			/// <param name="context">Context of this <see cref="Contextual{T}"/>.</param>
			public static Contextual<CompilationAnalysisContext> Compilation(CompilationAnalysisContext context)
			{
				return new Contextual<CompilationAnalysisContext>((context, diag) => context.ReportDiagnostic(diag), context);
			}

			/// <summary>
			/// Returns a new instance of the <see cref="DiagnosticReceiver"/> class.
			/// </summary>
			/// <param name="reportAction">Action that is performed when <see cref="ReportDiagnostic(Diagnostic)"/>is called.</param>
			/// <exception cref="ArgumentNullException"><paramref name="reportAction"/> is <see langword="null"/>.</exception>
			public static DiagnosticReceiver Direct(ReportAction.Direct reportAction)
			{
				return new DiagnosticReceiver(reportAction);
			}

			/// <summary>
			/// Returns a new instance of the <see cref="Empty"/> class.
			/// </summary>
			public static Empty Empty()
			{
				return new Empty();
			}

			/// <summary>
			/// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="OperationAnalysisContext"/>.
			/// </summary>
			public static Contextual<OperationAnalysisContext> Operation()
			{
				return new Contextual<OperationAnalysisContext>((context, diag) => context.ReportDiagnostic(diag));
			}

			/// <summary>
			/// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="OperationAnalysisContext"/>.
			/// </summary>
			/// <param name="context">Context of this <see cref="Contextual{T}"/>.</param>
			public static Contextual<OperationAnalysisContext> Operation(OperationAnalysisContext context)
			{
				return new Contextual<OperationAnalysisContext>((context, diag) => context.ReportDiagnostic(diag), context);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="OperationBlockAnalysisContext"/>.
			/// </summary>
			public static Contextual<OperationBlockAnalysisContext> OperationBlock()
			{
				return new Contextual<OperationBlockAnalysisContext>((context, diag) => context.ReportDiagnostic(diag));
			}

			/// <summary>
			/// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="OperationBlockAnalysisContext"/>.
			/// </summary>
			/// <param name="context">Context of this <see cref="Contextual{T}"/>.</param>
			public static Contextual<OperationBlockAnalysisContext> OperationBlock(OperationBlockAnalysisContext context)
			{
				return new Contextual<OperationBlockAnalysisContext>((context, diag) => context.ReportDiagnostic(diag), context);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="SemanticModelAnalysisContext"/>.
			/// </summary>
			public static Contextual<SemanticModelAnalysisContext> SemanticModel()
			{
				return new Contextual<SemanticModelAnalysisContext>((context, diag) => context.ReportDiagnostic(diag));
			}

			/// <summary>
			/// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="SemanticModelAnalysisContext"/>.
			/// </summary>
			/// <param name="context">Context of this <see cref="Contextual{T}"/>.</param>
			public static Contextual<SemanticModelAnalysisContext> SemanticModel(SemanticModelAnalysisContext context)
			{
				return new Contextual<SemanticModelAnalysisContext>((context, diag) => context.ReportDiagnostic(diag), context);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ReadonlyContextual{T}"/> class that accepts only <see cref="GeneratorExecutionContext"/>.
			/// </summary>
			public static ReadonlyContextual<GeneratorExecutionContext> SourceGenerator()
			{
				return new ReadonlyContextual<GeneratorExecutionContext>((in GeneratorExecutionContext context, Diagnostic diag) => context.ReportDiagnostic(diag));
			}

			/// <summary>
			/// Creates a new instance of the <see cref="ReadonlyContextual{T}"/> class that accepts only <see cref="GeneratorExecutionContext"/>.
			/// </summary>
			/// <param name="context">Context of this <see cref="ReadonlyContextual{T}"/>.</param>
			public static ReadonlyContextual<GeneratorExecutionContext> SourceGenerator(GeneratorExecutionContext context)
			{
				return new ReadonlyContextual<GeneratorExecutionContext>((in GeneratorExecutionContext context, Diagnostic diag) => context.ReportDiagnostic(diag), context);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="SymbolAnalysisContext"/>.
			/// </summary>
			public static Contextual<SymbolAnalysisContext> Symbol()
			{
				return new Contextual<SymbolAnalysisContext>((context, diag) => context.ReportDiagnostic(diag));
			}

			/// <summary>
			/// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="SymbolAnalysisContext"/>.
			/// </summary>
			/// <param name="context">Context of this <see cref="Contextual{T}"/>.</param>
			public static Contextual<SymbolAnalysisContext> Symbol(SymbolAnalysisContext context)
			{
				return new Contextual<SymbolAnalysisContext>((context, diag) => context.ReportDiagnostic(diag), context);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="SyntaxNodeAnalysisContext"/>.
			/// </summary>
			public static Contextual<SyntaxNodeAnalysisContext> SyntaxNode()
			{
				return new Contextual<SyntaxNodeAnalysisContext>((context, diag) => context.ReportDiagnostic(diag));
			}

			/// <summary>
			/// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="SyntaxNodeAnalysisContext"/>.
			/// </summary>
			/// <param name="context">Context of this <see cref="Contextual{T}"/>.</param>
			public static Contextual<SyntaxNodeAnalysisContext> SyntaxNode(SyntaxNodeAnalysisContext context)
			{
				return new Contextual<SyntaxNodeAnalysisContext>((context, diag) => context.ReportDiagnostic(diag), context);
			}

			/// <summary>
			/// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="SyntaxTreeAnalysisContext"/>.
			/// </summary>
			public static Contextual<SyntaxTreeAnalysisContext> SyntaxTree()
			{
				return new Contextual<SyntaxTreeAnalysisContext>((context, diag) => context.ReportDiagnostic(diag));
			}

			/// <summary>
			/// Creates a new instance of the <see cref="Contextual{T}"/> class that accepts only <see cref="SyntaxTreeAnalysisContext"/>.
			/// </summary>
			/// <param name="context">Context of this <see cref="Contextual{T}"/>.</param>
			public static Contextual<SyntaxTreeAnalysisContext> SyntaxTree(SyntaxTreeAnalysisContext context)
			{
				return new Contextual<SyntaxTreeAnalysisContext>((context, diag) => context.ReportDiagnostic(diag), context);
			}
		}
	}
}
