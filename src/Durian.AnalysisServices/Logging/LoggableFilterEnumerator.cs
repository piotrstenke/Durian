// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Durian.Analysis.Data;
using Durian.Analysis.Filtration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.Logging
{
	/// <summary>
	/// Enumerates through a collection of <see cref="IMemberData"/>s of type <typeparamref name="T"/> created by the provided <see cref="ISyntaxValidator{T}"/> with an option to log diagnostics using a <see cref="INodeDiagnosticReceiver"/>.
	/// </summary>
	/// <typeparam name="T">Type of target <see cref="ISyntaxValidationContext"/>.</typeparam>
	[DebuggerDisplay("Current = {Current}")]
	public struct LoggableFilterEnumerator<T> : IFilterEnumerator<T>, IEnumerator<IMemberData> where T : ISyntaxValidationContext
	{
		internal readonly IEnumerator<CSharpSyntaxNode> _nodes;

		/// <inheritdoc/>
		public readonly ICompilationData Compilation { get; }

		/// <inheritdoc/>
		public IMemberData? Current { readonly get; private set; }

		/// <summary>
		/// <see cref="IHintNameProvider"/> that creates hint names for the <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		public readonly IHintNameProvider HintNameProvider { get; }

		/// <summary>
		/// <see cref="INodeDiagnosticReceiver"/> that writes the reported <see cref="Diagnostic"/>s into a log file or buffer.
		/// </summary>
		public readonly INodeDiagnosticReceiver LogReceiver { get; }

		/// <inheritdoc cref="IFilterEnumerator{T}.Validator"/>
		public readonly ISyntaxValidatorWithDiagnostics<T> Validator { get; }

		readonly IMemberData IEnumerator<IMemberData>.Current => Current!;

		readonly object IEnumerator.Current => Current!;
		readonly ISyntaxValidator<T> IFilterEnumerator<T>.Validator => Validator;

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggableFilterEnumerator{T}"/> struct.
		/// </summary>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of the provided <paramref name="nodes"/>.</param>
		/// <param name="nodes">A collection of <see cref="CSharpSyntaxNode"/>s to use to create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="validator"><see cref="ISyntaxValidatorWithDiagnostics{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="logReceiver"><see cref="INodeDiagnosticReceiver"/> that writes the reported <see cref="Diagnostic"/>s into a log file or buffer.</param>
		/// <param name="hintNameProvider"><see cref="IHintNameProvider"/> that creates hint names for the <paramref name="nodes"/>.</param>
		public LoggableFilterEnumerator(
			ICompilationData compilation,
			IEnumerable<CSharpSyntaxNode> nodes,
			ISyntaxValidatorWithDiagnostics<T> validator,
			INodeDiagnosticReceiver logReceiver,
			IHintNameProvider hintNameProvider
		) : this(compilation, nodes.GetEnumerator(), validator, logReceiver, hintNameProvider)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggableFilterEnumerator{T}"/> struct.
		/// </summary>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of <see cref="CSharpSyntaxNode"/>s provided by the <paramref name="nodeProvider"/>.</param>
		/// <param name="nodeProvider"><see cref="INodeProvider"/> that creates an array of <see cref="CSharpSyntaxNode"/>s to be used to create the target <see cref="IMemberData"/>s.</param>
		/// <param name="validator"><see cref="ISyntaxValidatorWithDiagnostics{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="logReceiver"><see cref="INodeDiagnosticReceiver"/> that writes the reported <see cref="Diagnostic"/>s into a log file or buffer.</param>
		/// <param name="hintNameProvider"><see cref="IHintNameProvider"/> that creates hint names for the <see cref="CSharpSyntaxNode"/>s provided by the <paramref name="nodeProvider"/>.</param>
		public LoggableFilterEnumerator(
			ICompilationData compilation,
			INodeProvider nodeProvider,
			ISyntaxValidatorWithDiagnostics<T> validator,
			INodeDiagnosticReceiver logReceiver,
			IHintNameProvider hintNameProvider
		) : this(compilation, nodeProvider.GetNodes().GetEnumerator(), validator, logReceiver, hintNameProvider)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggableFilterEnumerator{T}"/> struct.
		/// </summary>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of the provided <paramref name="nodes"/>.</param>
		/// <param name="nodes">An enumerator that iterates through a collection of <see cref="CSharpSyntaxNode"/>s.</param>
		/// <param name="validator"><see cref="ISyntaxValidatorWithDiagnostics{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.</param>
		/// <param name="logReceiver"><see cref="INodeDiagnosticReceiver"/> that writes the reported <see cref="Diagnostic"/>s into a log file or buffer.</param>
		/// <param name="hintNameProvider"><see cref="IHintNameProvider"/> that creates hint names for the <paramref name="nodes"/>.</param>
		public LoggableFilterEnumerator(
			ICompilationData compilation,
			IEnumerator<CSharpSyntaxNode> nodes,
			ISyntaxValidatorWithDiagnostics<T> validator,
			INodeDiagnosticReceiver logReceiver,
			IHintNameProvider hintNameProvider
		)
		{
			Validator = validator;
			Compilation = compilation;
			LogReceiver = logReceiver;
			HintNameProvider = hintNameProvider;
			_nodes = nodes;
			Current = default;
		}

		/// <inheritdoc/>
		[MemberNotNullWhen(true, nameof(Current))]
		public bool MoveNext(CancellationToken cancellationToken = default)
		{
			while (_nodes.MoveNext())
			{
				CSharpSyntaxNode node = _nodes.Current;

				if (node is null)
				{
					continue;
				}

				if (!Validator.TryGetContext(new PreValidationContext(node, Compilation, cancellationToken), out T? context))
				{
					continue;
				}

				string fileName = HintNameProvider.GetHintName(context.Symbol);
				LogReceiver.SetTargetNode(node, fileName);
				bool isValid = Validator.ValidateAndCreate(context, out IMemberData? data, LogReceiver);

				if (LogReceiver.Count > 0)
				{
					LogReceiver.Push();
					HintNameProvider.Success();
				}

				if (isValid)
				{
					Current = data!;
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
		/// Converts this <see cref="LoggableFilterEnumerator{T}"/> to a new instance of <see cref="FilterEnumerator{T}"/>.
		/// </summary>
		public readonly FilterEnumerator<T> ToBasicEnumerator()
		{
			return new FilterEnumerator<T>(Compilation, _nodes, Validator);
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
}
