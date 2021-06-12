﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Durian.Generator.Cache;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Generator.DefaultParam
{
	public sealed partial class DefaultParamTypeFilter
	{
		/// <summary>
		/// Analyzer-like class that performs analysis defined by the <see cref="DefaultParamTypeFilter"/>.
		/// </summary>
		public sealed class AsAnalyzer : DefaultParamFilterAsAnalyzer<DefaultParamTypeData>, ICachedAnalyzerInfo<DefaultParamTypeData>
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="AsAnalyzer"/> class.
			/// </summary>
			public AsAnalyzer()
			{
			}

			/// <inheritdoc/>
			protected override bool Analyze(DefaultParamCompilationData compilation, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out DefaultParamTypeData? data)
			{
				if (context.Node is not TypeParameterListSyntax || context.Node.Parent is not TypeDeclarationSyntax declaration)
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