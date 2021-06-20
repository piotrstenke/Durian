// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Collections.Immutable;

#if !MAIN_PACKAGE

using Microsoft.CodeAnalysis.Diagnostics;

#endif

namespace Durian.Analysis.GenericSpecialization
{
	/// <summary>
	/// Analyzes classes marked by the <see cref="AllowSpecializationAttribute"/>.
	/// </summary>
#if !MAIN_PACKAGE

	[DiagnosticAnalyzer(LanguageNames.CSharp)]
#endif
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.

	public sealed class GenSpecAnalyzer : DurianAnalyzer<GenSpecCompilationData>
#pragma warning restore RS1001 // Missing diagnostic analyzer attribute.
	{
		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create<DiagnosticDescriptor>();

		/// <summary>
		/// Initializes a new instance of the <see cref="GenSpecAnalyzer"/> class.
		/// </summary>
		public GenSpecAnalyzer()
		{
		}

		/// <inheritdoc/>
		public override void Register(IDurianAnalysisContext context, GenSpecCompilationData compilation)
		{
			//context.RegisterSymbolAction(context => AnalyzeSymbol(context, compilation), SymbolKind.NamedType);
		}

		/// <inheritdoc/>
		protected override GenSpecCompilationData CreateCompilation(CSharpCompilation compilation)
		{
			return new GenSpecCompilationData(compilation);
		}

		//private void AnalyzeSymbol(SymbolAnalysisContext context, GenSpecCompilationData compilation)
		//{
		//	ContextualDiagnosticReceiver<SymbolAnalysisContext> diagnosticReceiver = DiagnosticReceiverFactory.Symbol(context);

		//	//Analyze(diagnosticReceiver, context.Symbol, compilation, context.CancellationToken);
		//}
	}

	/// <summary>
	/// Collects <see cref="CSharpSyntaxNode"/>s that are potential targets for the <see cref="GenericSpecializationGenerator"/>.
	/// </summary>
	public sealed class GenSpecSyntaxReceiver : IDurianSyntaxReceiver
	{
		/// <summary>
		/// <see cref="ClassDeclarationSyntax"/>es that potentially have the <see cref="AllowSpecializationAttribute"/> applied.
		/// </summary>
		public List<ClassDeclarationSyntax> CandidateClasses { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="GenSpecSyntaxReceiver"/> class.
		/// </summary>
		public GenSpecSyntaxReceiver()
		{
			CandidateClasses = new(64);
		}

		/// <inheritdoc/>
		public bool IsEmpty()
		{
			return CandidateClasses.Count == 0;
		}

		/// <inheritdoc/>
		public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
		{
			if (syntaxNode is ClassDeclarationSyntax decl && decl.TypeParameterList is not null && decl.TypeParameterList.Parameters.Any())
			{
				CandidateClasses.Add(decl);
			}
		}

		IEnumerable<CSharpSyntaxNode> INodeProvider.GetNodes()
		{
			return CandidateClasses;
		}
	}
}
