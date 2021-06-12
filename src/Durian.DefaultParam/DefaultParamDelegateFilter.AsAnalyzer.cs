﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Durian.Generator.Cache;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Generator.DefaultParam
{
	public partial class DefaultParamDelegateFilter
	{
		/// <summary>
		/// Analyzer-like class that performs analysis defined by the <see cref="DefaultParamDelegateFilter"/>.
		/// </summary>
		public sealed class AsAnalyzer : DefaultParamFilterAsAnalyzer<DefaultParamDelegateData>, ICachedAnalyzerInfo<DefaultParamDelegateData>
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="AsAnalyzer"/> class.
			/// </summary>
			public AsAnalyzer()
			{
			}

			/// <inheritdoc/>
			protected override bool Analyze(DefaultParamCompilationData compilation, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out DefaultParamDelegateData? data)
			{
				if (context.Node is not TypeParameterListSyntax || context.Node.Parent is not DelegateDeclarationSyntax declaration)
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