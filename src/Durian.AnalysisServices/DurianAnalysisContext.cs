// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Durian.Generator
{
	/// <summary>
	/// Adapts the <see cref="AnalysisContext"/> class to be used as an <see cref="IDurianAnalysisContext"/>.
	/// </summary>
	public sealed class DurianAnalysisContext : IDurianAnalysisContext
	{
		private readonly AnalysisContext _context;

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianAnalysisContext"/> class.
		/// </summary>
		/// <param name="context">Input analysis context.</param>
		/// <exception cref="ArgumentNullException"><paramref name="context"/> is <see langword="null"/>.</exception>
		public DurianAnalysisContext(AnalysisContext context)
		{
			if (context is null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			_context = context;
		}

		/// <inheritdoc/>
		public void RegisterAdditionalFileAction(Action<AdditionalFileAnalysisContext> action)
		{
			_context.RegisterAdditionalFileAction(action);
		}

		/// <inheritdoc/>
		public void RegisterCodeBlockAction(Action<CodeBlockAnalysisContext> action)
		{
			_context.RegisterCodeBlockAction(action);
		}

		/// <inheritdoc/>
		public void RegisterCodeBlockStartAction<TLanguageKindEnum>(Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action) where TLanguageKindEnum : struct
		{
			_context.RegisterCodeBlockStartAction(action);
		}

		/// <inheritdoc/>
		public void RegisterCompilationAction(Action<CompilationAnalysisContext> action)
		{
			_context.RegisterCompilationAction(action);
		}

		/// <inheritdoc/>
		public void RegisterOperationAction(Action<OperationAnalysisContext> action, params OperationKind[] operationKinds)
		{
			_context.RegisterOperationAction(action, operationKinds);
		}

		/// <inheritdoc/>
		public void RegisterOperationAction(Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds)
		{
			_context.RegisterOperationAction(action, operationKinds);
		}

		/// <inheritdoc/>
		public void RegisterOperationBlockAction(Action<OperationBlockAnalysisContext> action)
		{
			_context.RegisterOperationBlockAction(action);
		}

		/// <inheritdoc/>
		public void RegisterOperationBlockStartAction(Action<OperationBlockStartAnalysisContext> action)
		{
			_context.RegisterOperationBlockStartAction(action);
		}

		/// <inheritdoc/>
		public void RegisterSemanticModelAction(Action<SemanticModelAnalysisContext> action)
		{
			_context.RegisterSemanticModelAction(action);
		}

		/// <inheritdoc/>
		public void RegisterSymbolAction(Action<SymbolAnalysisContext> action, params SymbolKind[] symbolKinds)
		{
			_context.RegisterSymbolAction(action, symbolKinds);
		}

		/// <inheritdoc/>
		public void RegisterSymbolAction(Action<SymbolAnalysisContext> action, ImmutableArray<SymbolKind> symbolKinds)
		{
			_context.RegisterSymbolAction(action, symbolKinds);
		}

		/// <inheritdoc/>
		public void RegisterSymbolStartAction(Action<SymbolStartAnalysisContext> action, SymbolKind symbolKind)
		{
			_context.RegisterSymbolStartAction(action, symbolKind);
		}

		/// <inheritdoc/>
		public void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, params TLanguageKindEnum[] syntaxKinds) where TLanguageKindEnum : struct
		{
			_context.RegisterSyntaxNodeAction(action, syntaxKinds);
		}

		/// <inheritdoc/>
		public void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<TLanguageKindEnum> syntaxKinds) where TLanguageKindEnum : struct
		{
			_context.RegisterSyntaxNodeAction(action, syntaxKinds);
		}

		/// <inheritdoc/>
		public void RegisterSyntaxTreeAction(Action<SyntaxTreeAnalysisContext> action)
		{
			_context.RegisterSyntaxTreeAction(action);
		}

		/// <inheritdoc/>
		public bool TryGetValue<TValue>(SourceText text, SourceTextValueProvider<TValue> valueProvider, [MaybeNullWhen(false)] out TValue value)
		{
			return _context.TryGetValue(text, valueProvider, out value);
		}
	}
}
