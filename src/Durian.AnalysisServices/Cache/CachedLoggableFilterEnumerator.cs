using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Durian.Analysis.Data;
using Durian.Analysis.Filtration;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Cache;

/// <summary>
/// Enumerates through a collection of <see cref="IMemberData"/>s of type <typeparamref name="TContext"/> created by the provided <see cref="ISyntaxValidator{T}"/> or retrieved from a <see cref="CachedData{T}"/> with an option to log diagnostics using a <see cref="INodeDiagnosticReceiver"/>.
/// </summary>
/// <typeparam name="TData">Type of cached data.</typeparam>
/// <typeparam name="TContext">Type of target <see cref="ISyntaxValidationContext"/>.</typeparam>
[DebuggerDisplay("Current = {Current}")]
public struct CachedLoggableFilterEnumerator<TData, TContext> : IFilterEnumerator<TContext>, IEnumerator<TData>
	where TData : class, IMemberData
	where TContext : ISyntaxValidationContext
{
	internal readonly CachedData<TData> _cache;

	private readonly IEnumerator<SyntaxNode> _nodes;

	/// <inheritdoc/>
	public readonly ICompilationData Compilation { get; }

	/// <summary>
	/// <typeparamref name="TData"/> at the current position in the enumerator.
	/// </summary>
	public TData? Current { readonly get; private set; }

	/// <summary>
	/// <see cref="IHintNameProvider"/> that creates hint names for the <see cref="SyntaxNode"/>s.
	/// </summary>
	public readonly IHintNameProvider HintNameProvider { get; }

	/// <summary>
	/// <see cref="INodeDiagnosticReceiver"/> that writes the reported <see cref="Diagnostic"/>s into a log file or buffer.
	/// </summary>
	public readonly INodeDiagnosticReceiver LogReceiver { get; }

	/// <inheritdoc cref="IFilterEnumerator{T}.Validator"/>
	public readonly ISyntaxValidatorWithDiagnostics<TContext> Validator { get; }

	readonly TData IEnumerator<TData>.Current => Current!;
	readonly IMemberData IFilterEnumerator<TContext>.Current => Current!;
	readonly object IEnumerator.Current => Current!;
	readonly ISyntaxValidator<TContext> IFilterEnumerator<TContext>.Validator => Validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="CachedLoggableFilterEnumerator{TData, TContext}"/> struct.
	/// </summary>
	/// <param name="compilation">Parent <see cref="ICompilationData"/> of the provided <paramref name="nodes"/>.</param>
	/// <param name="nodes">A collection of <see cref="SyntaxNode"/>s to use to create the <see cref="IMemberData"/>s to enumerate through.</param>
	/// <param name="validator"><see cref="ISyntaxValidatorWithDiagnostics{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
	/// <param name="logReceiver"><see cref="INodeDiagnosticReceiver"/> that writes the reported <see cref="Diagnostic"/>s into a log file or buffer.</param>
	/// <param name="hintNameProvider"><see cref="IHintNameProvider"/> that creates hint names for the <paramref name="nodes"/>.</param>
	/// <param name="cache">Container of cached <see cref="IMemberData"/>s.</param>
	public CachedLoggableFilterEnumerator(
		ICompilationData compilation,
		IEnumerable<SyntaxNode> nodes,
		ISyntaxValidatorWithDiagnostics<TContext> validator,
		INodeDiagnosticReceiver logReceiver,
		IHintNameProvider hintNameProvider,
		in CachedData<TData> cache
	) : this(compilation, nodes.GetEnumerator(), validator, logReceiver, hintNameProvider, in cache)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CachedLoggableFilterEnumerator{TData, TContext}"/> struct.
	/// </summary>
	/// <param name="compilation">Parent <see cref="ICompilationData"/> of <see cref="SyntaxNode"/>s provided by the <paramref name="provider"/>.</param>
	/// <param name="provider"><see cref="INodeProvider"/> that creates an array of <see cref="SyntaxNode"/>s to be used to create the target <see cref="IMemberData"/>s.</param>
	/// <param name="validator"><see cref="ISyntaxValidatorWithDiagnostics{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
	/// <param name="logReceiver"><see cref="INodeDiagnosticReceiver"/> that writes the reported <see cref="Diagnostic"/>s into a log file or buffer.</param>
	/// <param name="hintNameProvider"><see cref="IHintNameProvider"/> that creates hint names for the <see cref="SyntaxNode"/>s provided by the <paramref name="provider"/>.</param>
	/// <param name="cache">Container of cached <see cref="IMemberData"/>s.</param>
	public CachedLoggableFilterEnumerator(
		ICompilationData compilation,
		INodeProvider provider,
		ISyntaxValidatorWithDiagnostics<TContext> validator,
		INodeDiagnosticReceiver logReceiver,
		IHintNameProvider hintNameProvider,
		in CachedData<TData> cache
	) : this(compilation, provider.GetNodes().GetEnumerator(), validator, logReceiver, hintNameProvider, in cache)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CachedLoggableFilterEnumerator{TData, TContext}"/> struct.
	/// </summary>
	/// <param name="compilation">Parent <see cref="ICompilationData"/> of the provided <paramref name="nodes"/>.</param>
	/// <param name="nodes">An enumerator that iterates through a collection of <see cref="SyntaxNode"/>s.</param>
	/// <param name="validator"><see cref="ISyntaxValidatorWithDiagnostics{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
	/// <param name="logReceiver"><see cref="INodeDiagnosticReceiver"/> that writes the reported <see cref="Diagnostic"/>s into a log file or buffer.</param>
	/// <param name="hintNameProvider"><see cref="IHintNameProvider"/> that creates hint names for the <paramref name="nodes"/>.</param>
	/// <param name="cache">Container of cached <see cref="IMemberData"/>s.</param>
	public CachedLoggableFilterEnumerator(
		ICompilationData compilation,
		IEnumerator<SyntaxNode> nodes,
		ISyntaxValidatorWithDiagnostics<TContext> validator,
		INodeDiagnosticReceiver logReceiver,
		IHintNameProvider hintNameProvider,
		in CachedData<TData> cache
	)
	{
		Validator = validator;
		Compilation = compilation;
		LogReceiver = logReceiver;
		HintNameProvider = hintNameProvider;
		_nodes = nodes;
		_cache = cache;
		Current = default;
	}

#pragma warning disable RCS1242 // Do not pass non-read-only struct by read-only reference.

	/// <inheritdoc/>
	public static explicit operator LoggableFilterEnumerator<TContext>(in CachedLoggableFilterEnumerator<TData, TContext> a)
	{
		return new LoggableFilterEnumerator<TContext>(a.Compilation, a._nodes, a.Validator, a.LogReceiver, a.HintNameProvider);
	}

	/// <inheritdoc/>
	public static implicit operator CachedLoggableFilterEnumerator<TData, TContext>(in LoggableFilterEnumerator<TContext> a)
	{
		return new CachedLoggableFilterEnumerator<TData, TContext>(a.Compilation, a._nodes, a.Validator, a.LogReceiver, a.HintNameProvider, CachedData<TData>.Empty);
	}

#pragma warning restore RCS1242 // Do not pass non-read-only struct by read-only reference.

	/// <inheritdoc/>
	[MemberNotNullWhen(true, nameof(Current))]
	public bool MoveNext(CancellationToken cancellationToken = default)
	{
		while (_nodes.MoveNext())
		{
			SyntaxNode node = _nodes.Current;

			if (node is null)
			{
				continue;
			}

			if (_cache.TryGetCachedValue(node.GetLocation().GetLineSpan(), out TData? data))
			{
				Current = data!;
				return true;
			}

			if (!Validator.TryGetContext(new PreValidationContext(node, Compilation, cancellationToken), out TContext? context))
			{
				continue;
			}

			string fileName = HintNameProvider.GetHintName(context.Symbol);
			LogReceiver.SetTargetNode(node, fileName);
			bool isValid = Validator.ValidateAndCreate(context, out IMemberData? member, LogReceiver) && member is TData;

			if (LogReceiver.Count > 0)
			{
				LogReceiver.Push();
				HintNameProvider.Success();
			}

			if (isValid)
			{
				Current = (member as TData)!;
				return true;
			}
		}

		Current = default;
		return false;
	}

	/// <inheritdoc cref="FilterEnumerator{T}.Reset"/>
	public void Reset()
	{
		_nodes.Reset();
		Current = default;
	}

	/// <summary>
	/// Converts this <see cref="CachedLoggableFilterEnumerator{TData, TContext}"/> to a new instance of <see cref="CachedFilterEnumerator{TData, TContext}"/>.
	/// </summary>
	public readonly CachedFilterEnumerator<TData, TContext> ToBasicCachedEnumerator()
	{
		return new CachedFilterEnumerator<TData, TContext>(Compilation, _nodes, Validator, in _cache);
	}

	/// <summary>
	/// Converts this <see cref="CachedLoggableFilterEnumerator{TData, TContext}"/> to a new instance of <see cref="FilterEnumerator{T}"/>.
	/// </summary>
	public readonly FilterEnumerator<TContext> ToBasicEnumerator()
	{
		return new FilterEnumerator<TContext>(Compilation, _nodes, Validator);
	}

	readonly void IDisposable.Dispose()
	{
		// Do nothing.
	}

	bool IEnumerator.MoveNext()
	{
		return MoveNext();
	}
}
