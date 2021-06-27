// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Durian.Analysis.Cache;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Analysis.DefaultParam
{
	public partial class DefaultParamMethodFilter
	{
		/// <summary>
		/// Analyzer-like class that performs analysis defined by the <see cref="DefaultParamMethodFilter"/>.
		/// </summary>
		public sealed class AsAnalyzer : DefaultParamFilterAsAnalyzer<DefaultParamMethodData>, ICachedAnalyzerInfo<DefaultParamMethodData>
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
				return DefaultParamMethodAnalyzer.GetSupportedDiagnostics();
			}

			/// <inheritdoc/>
			protected override bool Analyze(
				DefaultParamCompilationData compilation,
				SyntaxNodeAnalysisContext context,
				[NotNullWhen(true)] out DefaultParamMethodData? data
			)
			{
				if (context.Node is not TypeParameterListSyntax || context.Node.Parent is not MethodDeclarationSyntax declaration)
				{
					data = null;
					return false;
				}

				IDiagnosticReceiver diagnosticReceiver = DiagnosticReceiverFactory.SyntaxNode(context);

				return WithDiagnostics.ValidateAndCreate(diagnosticReceiver, compilation, declaration, out data, context.CancellationToken);
			}
		}
	}
}
