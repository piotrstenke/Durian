﻿using System;
using System.Threading;
using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Cache;

/// <summary>
/// <see cref="IGeneratorPassContext"/> that provides a <see cref="CachedGeneratorExecutionContext{T}"/> instead of <see cref="GeneratorExecutionContext"/>.
/// </summary>
/// <typeparam name="TData">Type of values this context can store.</typeparam>
/// <typeparam name="TContext">Type of <see cref="IGeneratorPassContext"/> used by the target generator.</typeparam>
public class CachedGeneratorPassContext<TData, TContext> : ICachedGeneratorPassContext<TData>
	where TData : IMemberData
	where TContext : IGeneratorPassContext
{
	internal CachedGeneratorExecutionContext<TData> _cachedContext;

	/// <inheritdoc/>
	public ref readonly CachedGeneratorExecutionContext<TData> OriginalContext => ref _cachedContext;

	/// <summary>
	/// <typeparamref name="TContext"/> that actually contains the context data.
	/// </summary>
	public TContext UnderlayingContext { get; }

	CancellationToken IGeneratorPassContext.CancellationToken => UnderlayingContext.CancellationToken;

	IHintNameProvider IGeneratorPassContext.FileNameProvider => UnderlayingContext.FileNameProvider;

	ILoggableSourceGenerator IGeneratorPassContext.Generator => UnderlayingContext.Generator;
	ref readonly GeneratorExecutionContext IGeneratorPassContext.OriginalContext => ref UnderlayingContext.OriginalContext;

	ParseOptions IGeneratorPassContext.ParseOptions => UnderlayingContext.ParseOptions;

	IGeneratorServiceResolver IGeneratorPassContext.Services => UnderlayingContext.Services;

	GeneratorState IGeneratorPassContext.State => UnderlayingContext.State;

	IDurianSyntaxReceiver IGeneratorPassContext.SyntaxReceiver => UnderlayingContext.SyntaxReceiver;

	ICompilationData IGeneratorPassContext.TargetCompilation => UnderlayingContext.TargetCompilation;

	/// <summary>
	/// Initializes a new instance of the <see cref="CachedGeneratorPassContext{TData, TContext}"/> class.
	/// </summary>
	/// <param name="underlayingContext"><typeparamref name="TContext"/> that actually contains the context data.</param>
	/// <param name="originalContext"><see cref="CachedGeneratorExecutionContext{T}"/> created for the current generator pass.</param>
	public CachedGeneratorPassContext(TContext underlayingContext, in CachedGeneratorExecutionContext<TData> originalContext)
	{
		if (underlayingContext is null)
		{
			throw new ArgumentNullException(nameof(underlayingContext));
		}

		UnderlayingContext = underlayingContext;
		_cachedContext = originalContext;
	}

	IDiagnosticReceiver? IGeneratorPassContext.GetDiagnosticReceiver()
	{
		return UnderlayingContext.GetDiagnosticReceiver();
	}
}
