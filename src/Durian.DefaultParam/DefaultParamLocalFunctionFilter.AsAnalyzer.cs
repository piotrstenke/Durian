// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Analysis.DefaultParam
{
	public partial class DefaultParamLocalFunctionFilter
	{
		/// <summary>
		/// Analyzer-like class that performs analysis defined by the <see cref="DefaultParamLocalFunctionFilter"/>.
		/// </summary>
		public sealed class AsAnalyzer : DefaultParamFilterAsAnalyzer<IDefaultParamTarget>
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="AsAnalyzer"/> class.
			/// </summary>
			public AsAnalyzer()
			{
			}

			/// <inheritdoc/>
			public override IEnumerable<DiagnosticDescriptor> GetSupportedDiagnostics()
			{
				return DefaultParamLocalFunctionAnalyzer.GetSupportedDiagnostics();
			}

			/// <inheritdoc/>
			protected override bool Analyze(
				DefaultParamCompilationData compilation,
				SyntaxNodeAnalysisContext context,
				[NotNullWhen(true)] out IDefaultParamTarget? data
			)
			{
				data = null;

				if (context.Node is not TypeParameterListSyntax || context.Node.Parent is not LocalFunctionStatementSyntax declaration)
				{
					return false;
				}

				IDiagnosticReceiver diagnosticReceiver = DiagnosticReceiverFactory.SyntaxNode(context);
				ReportDiagnosticsForLocalFunction(diagnosticReceiver, compilation, declaration, context.CancellationToken);
				return false;
			}
		}
	}
}
