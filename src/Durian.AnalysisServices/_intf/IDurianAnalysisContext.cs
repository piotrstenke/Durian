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
	/// Abstracts out the usage of <see cref="AnalysisContext"/> and <see cref="CompilationStartAnalysisContext"/>.
	/// </summary>
	public interface IDurianAnalysisContext
	{
		/// <inheritdoc cref="AnalysisContext.RegisterAdditionalFileAction(Action{AdditionalFileAnalysisContext})"/>
		void RegisterAdditionalFileAction(Action<AdditionalFileAnalysisContext> action);

		/// <inheritdoc cref="AnalysisContext.RegisterCodeBlockAction(Action{CodeBlockAnalysisContext})"/>
		void RegisterCodeBlockAction(Action<CodeBlockAnalysisContext> action);

		/// <inheritdoc cref="AnalysisContext.RegisterCodeBlockStartAction{TLanguageKindEnum}(Action{CodeBlockStartAnalysisContext{TLanguageKindEnum}})"/>
		void RegisterCodeBlockStartAction<TLanguageKindEnum>(Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action) where TLanguageKindEnum : struct;

		/// <inheritdoc cref="AnalysisContext.RegisterCompilationAction(Action{CompilationAnalysisContext})"/>
		void RegisterCompilationAction(Action<CompilationAnalysisContext> action);

		/// <inheritdoc cref="AnalysisContext.RegisterOperationAction(Action{OperationAnalysisContext}, OperationKind[])"/>
		void RegisterOperationAction(Action<OperationAnalysisContext> action, params OperationKind[] operationKinds);

		/// <inheritdoc cref="AnalysisContext.RegisterOperationAction(Action{OperationAnalysisContext}, ImmutableArray{OperationKind})"/>
		void RegisterOperationAction(Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds);

		/// <inheritdoc cref="AnalysisContext.RegisterOperationBlockAction(Action{OperationBlockAnalysisContext})"/>
		void RegisterOperationBlockAction(Action<OperationBlockAnalysisContext> action);

		/// <inheritdoc cref="AnalysisContext.RegisterOperationBlockStartAction(Action{OperationBlockStartAnalysisContext})"/>
		void RegisterOperationBlockStartAction(Action<OperationBlockStartAnalysisContext> action);

		/// <inheritdoc cref="AnalysisContext.RegisterSemanticModelAction(Action{SemanticModelAnalysisContext})"/>
		void RegisterSemanticModelAction(Action<SemanticModelAnalysisContext> action);

		/// <inheritdoc cref="AnalysisContext.RegisterSymbolAction(Action{SymbolAnalysisContext}, SymbolKind[])"/>
		void RegisterSymbolAction(Action<SymbolAnalysisContext> action, params SymbolKind[] symbolKinds);

		/// <inheritdoc cref="AnalysisContext.RegisterSymbolAction(Action{SymbolAnalysisContext}, ImmutableArray{SymbolKind})"/>
		void RegisterSymbolAction(Action<SymbolAnalysisContext> action, ImmutableArray<SymbolKind> symbolKinds);

		/// <inheritdoc cref="AnalysisContext.RegisterSymbolStartAction(Action{SymbolStartAnalysisContext}, SymbolKind)"/>
		void RegisterSymbolStartAction(Action<SymbolStartAnalysisContext> action, SymbolKind symbolKind);

		/// <inheritdoc cref="AnalysisContext.RegisterSyntaxNodeAction{TLanguageKindEnum}(Action{SyntaxNodeAnalysisContext}, TLanguageKindEnum[])"/>
		void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, params TLanguageKindEnum[] syntaxKinds) where TLanguageKindEnum : struct;

		/// <inheritdoc cref="AnalysisContext.RegisterSyntaxNodeAction{TLanguageKindEnum}(Action{SyntaxNodeAnalysisContext}, ImmutableArray{TLanguageKindEnum})"/>
		void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<TLanguageKindEnum> syntaxKinds) where TLanguageKindEnum : struct;

		/// <inheritdoc cref="AnalysisContext.RegisterSyntaxTreeAction(Action{SyntaxTreeAnalysisContext})"/>
		void RegisterSyntaxTreeAction(Action<SyntaxTreeAnalysisContext> action);

		/// <inheritdoc cref="AnalysisContext.TryGetValue{TValue}(SourceText, SourceTextValueProvider{TValue}, out TValue)"/>
		bool TryGetValue<TValue>(SourceText text, SourceTextValueProvider<TValue> valueProvider, [MaybeNullWhen(false)] out TValue value);
	}
}
