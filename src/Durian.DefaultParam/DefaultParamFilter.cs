// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Analysis.Cache;
using Durian.Analysis.Data;
using Durian.Analysis.Filters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// <c>DefaultParam</c>-specific <see cref="SyntaxValidator"/>.
	/// </summary>
	public abstract class DefaultParamFilter<TContext> : CachedSyntaxValidator<IDefaultParamTarget, TContext> where TContext : ISyntaxValidationContext
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamFilter{TContext}"/> class.
		/// </summary>
		protected DefaultParamFilter()
		{
		}

		/// <inheritdoc/>
		public sealed override bool TryGetContext(in ValidationDataContext validationContext, [NotNullWhen(true)] out TContext? context)
		{
			if (validationContext.TargetCompilation is not DefaultParamCompilationData compilation)
			{
				context = default;
				return false;
			}

			SemanticModel semanticModel = compilation.Compilation.GetSemanticModel(validationContext.Node.SyntaxTree);
			TypeParameterListSyntax? parameterList = GetTypeParameterList(validationContext.Node);
			TypeParameterContainer typeParameters = GetTypeParameters(parameterList, semanticModel, compilation, validationContext.CancellationToken);

			if (TypeParametersAreValid(in typeParameters, validationContext.Node) && semanticModel.GetDeclaredSymbol(validationContext.Node, validationContext.CancellationToken) is ISymbol symbol)
			{
				return TryCreateContext(validationContext.ToSyntaxContext(semanticModel, symbol), typeParameters, out context);
			}

			context = default;
			return false;
		}

		/// <inheritdoc/>
		public abstract override bool ValidateAndCreate(in TContext context, out IMemberData? data, IDiagnosticReceiver diagnosticReceiver);

		/// <summary>
		/// Attempts to create a new <typeparamref name="TContext"/> from the specified <paramref name="validationContext"/>.
		/// </summary>
		/// <param name="validationContext"><see cref="SyntaxValidationContext"/> that contains all data necessary to retrieve the required data.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the node's type parameters.</param>
		/// <param name="context">Created <typeparamref name="TContext"/>.</param>
		protected abstract bool TryCreateContext(
			in SyntaxValidationContext validationContext,
			in TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out TContext? context
		);

		/// <inheritdoc/>
		[Obsolete("Use TryCreateContext with TypeParameterContainer instead.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		protected sealed override bool TryCreateContext(
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
			in SyntaxValidationContext validationContext,
			[NotNullWhen(true)] out TContext? context
		)
		{
			if (validationContext.TargetCompilation is not DefaultParamCompilationData compilation)
			{
				context = default;
				return false;
			}

			TypeParameterListSyntax? parameterList = GetTypeParameterList(validationContext.Node);
			TypeParameterContainer typeParameters = GetTypeParameters(parameterList, validationContext.SemanticModel, compilation, validationContext.CancellationToken);

			if (TypeParametersAreValid(in typeParameters, validationContext.Node))
			{
				return TryCreateContext(in validationContext, out context);
			}

			context = default;
			return false;
		}

		/// <summary>
		/// Returns a <see cref="TypeParameterListSyntax"/> associated with the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to get the <see cref="TypeParameterListSyntax"/> associated with.</param>
		protected abstract TypeParameterListSyntax? GetTypeParameterList(CSharpSyntaxNode node);

		/// <summary>
		/// Determines whether the collected <see cref="TypeParameterContainer"/> is valid for analysis.
		/// </summary>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> to check if is valid for analysis.</param>
		/// <param name="node">Current <see cref="CSharpSyntaxNode"/>.</param>
		protected virtual bool TypeParametersAreValid(in TypeParameterContainer typeParameters, CSharpSyntaxNode node)
		{
			return typeParameters.HasDefaultParams;
		}

		private static TypeParameterContainer GetTypeParameters(
			TypeParameterListSyntax? typeParameters,
			SemanticModel semanticModel,
			DefaultParamCompilationData compilation,
			CancellationToken cancellationToken
		)
		{
			if (typeParameters is null)
			{
				return new TypeParameterContainer(null);
			}

			return new TypeParameterContainer(typeParameters.Parameters.Select(p => TypeParameterData.CreateFrom(p, semanticModel, compilation, cancellationToken)));
		}
	}
}
