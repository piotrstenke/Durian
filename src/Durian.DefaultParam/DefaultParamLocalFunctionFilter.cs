// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// Filtrates and validates <see cref="LocalFunctionStatementSyntax"/>es collected by a <see cref="DefaultParamSyntaxReceiver"/>.
	/// </summary>
	public class DefaultParamLocalFunctionFilter : DefaultParamFilter<LocalFunctionStatementSyntax, IMethodSymbol, IDefaultParamTarget>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamLocalFunctionFilter"/> class.
		/// </summary>
		/// <param name="generator"><see cref="DefaultParamGenerator"/> that is the target of this filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		public DefaultParamLocalFunctionFilter(DefaultParamGenerator generator) : base(generator)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamLocalFunctionFilter"/> class.
		/// </summary>
		/// <param name="generator"><see cref="DefaultParamGenerator"/> that is the target of this filter.</param>
		/// <param name="hintNameProvider"><see cref="IHintNameProvider"/> that is used to create a hint name for the generated source.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="hintNameProvider"/> is <see langword="null"/>.</exception>
		public DefaultParamLocalFunctionFilter(DefaultParamGenerator generator, IHintNameProvider hintNameProvider) : base(generator, hintNameProvider)
		{
		}

		/// <inheritdoc/>
		public override bool ValidateAndCreate(
			LocalFunctionStatementSyntax node,
			DefaultParamCompilationData compilation,
			SemanticModel semanticModel,
			IMethodSymbol symbol,
			in TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken = default
		)
		{
			data = default;
			return false;
		}

		/// <inheritdoc/>
		public override bool ValidateAndCreate(
			LocalFunctionStatementSyntax node,
			DefaultParamCompilationData compilation,
			SemanticModel semanticModel,
			IMethodSymbol symbol,
			in TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken = default
		)
		{
			DefaultParamMethodAnalyzer.ReportDiagnosticForInvalidMethodType(symbol, diagnosticReceiver);

			data = default;
			return false;
		}

		/// <inheritdoc/>
		protected override IEnumerable<LocalFunctionStatementSyntax>? GetCandidateNodes(DefaultParamSyntaxReceiver syntaxReceiver)
		{
			if (Mode == FilterMode.None)
			{
				return null;
			}

			return syntaxReceiver.CandidateLocalFunctions;
		}

		/// <inheritdoc/>
		protected override TypeParameterListSyntax? GetTypeParameterList(LocalFunctionStatementSyntax node)
		{
			return node.TypeParameterList;
		}
	}
}