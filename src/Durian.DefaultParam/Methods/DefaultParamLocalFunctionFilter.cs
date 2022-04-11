// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Filters;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Durian.Analysis.DefaultParam.Methods
{
	/// <summary>
	/// Filtrates and validates <see cref="LocalFunctionStatementSyntax"/>es collected by a <see cref="DefaultParamSyntaxReceiver"/>.
	/// </summary>
	public class DefaultParamLocalFunctionFilter : DefaultParamFilter<SyntaxValidationContext>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamLocalFunctionFilter"/> class.
		/// </summary>
		public DefaultParamLocalFunctionFilter()
		{
		}

		/// <inheritdoc/>
		public override bool ValidateAndCreate(in SyntaxValidationContext context, out IMemberData? data)
		{
			data = default;
			return false;
		}

		/// <inheritdoc/>
		public override bool ValidateAndCreate(in SyntaxValidationContext context, out IMemberData? data, IDiagnosticReceiver diagnosticReceiver)
		{
			DefaultParamMethodAnalyzer.ReportDiagnosticForInvalidMethodType(context.Symbol, diagnosticReceiver);

			data = default;
			return false;
		}

		/// <inheritdoc/>
		protected override IEnumerable<CSharpSyntaxNode>? GetCandidateNodes(IDurianSyntaxReceiver syntaxReceiver)
		{
			if (syntaxReceiver is not DefaultParamSyntaxReceiver sr)
			{
				return base.GetCandidateNodes(syntaxReceiver);
			}

			if (sr.CandidateLocalFunctions.Count == 0)
			{
				return default;
			}

			return sr.CandidateLocalFunctions;
		}

		/// <inheritdoc/>
		protected override TypeParameterListSyntax? GetTypeParameterList(CSharpSyntaxNode node)
		{
			return (node as LocalFunctionStatementSyntax)?.TypeParameterList;
		}

		/// <inheritdoc/>
		protected override bool TryCreateContext(in SyntaxValidationContext validationContext, in TypeParameterContainer typeParameters, [NotNullWhen(true)] out SyntaxValidationContext context)
		{
			context = validationContext;
			return true;
		}
	}
}
