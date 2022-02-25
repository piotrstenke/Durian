// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CopyFrom
{
	/// <summary>
	/// Filtrates and validates <see cref="TypeDeclarationSyntax"/>es collected by a <see cref="CopyFromSyntaxReceiver"/>.
	/// </summary>
	public sealed class CopyFromTypeFilter : IGeneratorSyntaxFilterWithDiagnostics
	{
		/// <summary>
		/// <see cref="CopyFromGenerator"/> that creates this filter.
		/// </summary>
		public CopyFromGenerator Generator { get; }

		/// <inheritdoc/>
		public IHintNameProvider HintNameProvider { get; }

		/// <inheritdoc/>
		public bool IncludeGeneratedSymbols => true;

		/// <summary>
		/// <see cref="FilterMode"/> of this <see cref="CopyFromTypeFilter"/>.
		/// </summary>
		public FilterMode Mode => Generator.LoggingConfiguration.CurrentFilterMode;

		IDurianGenerator IGeneratorSyntaxFilter.Generator => Generator;

		/// <inheritdoc cref="CopyFromTypeFilter(CopyFromGenerator, IHintNameProvider)"/>
		public CopyFromTypeFilter(CopyFromGenerator generator) : this(generator, new SymbolNameToFile())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromTypeFilter"/> class.
		/// </summary>
		/// <param name="generator"><see cref="CopyFromGenerator"/> that created this filter.</param>
		/// <param name="hintNameProvider"><see cref="IHintNameProvider"/> that is used to create a hint name for the generated source.</param>
		public CopyFromTypeFilter(CopyFromGenerator generator, IHintNameProvider hintNameProvider)
		{
			Generator = generator;
			HintNameProvider = hintNameProvider;
		}

		public IEnumerable<IMemberData> Filtrate(in GeneratorExecutionContext context)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerator<IMemberData> GetEnumerator()
		{
			throw new System.NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<IMemberData> Filtrate(ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, IDiagnosticReceiver diagnosticReceiver, CancellationToken cancellationToken = default)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<IMemberData> Filtrate(ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, IDiagnosticReceiver diagnosticReceiver, CancellationToken cancellationToken = default)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<IMemberData> Filtrate(ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken = default)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<IMemberData> Filtrate(ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken = default)
		{
			throw new System.NotImplementedException();
		}
	}
}