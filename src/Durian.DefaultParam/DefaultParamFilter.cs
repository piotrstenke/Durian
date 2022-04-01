// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Cache;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Durian.Analysis.Filters;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// <c>DefaultParam</c>-specific <see cref="SyntaxValidator"/>.
	/// </summary>
	public abstract class DefaultParamFilter : CachedSyntaxValidator, IDefaultParamFilter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamFilter"/> class.
		/// </summary>
		protected DefaultParamFilter()
		{
		}

		/// <inheritdoc/>
		[Obsolete("TypeParameterContainer is required for proper analysis. Use GetValidationData with a TypeParameterContainer parameter instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		public sealed override bool GetValidationData(
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
			CSharpSyntaxNode node,
			ICompilationData compilation,
			[NotNullWhen(true)] out SemanticModel? semanticModel,
			[NotNullWhen(true)] out ISymbol? symbol,
			CancellationToken cancellationToken = default
		)
		{
			return GetValidationData(node, GetDPCompilation(compilation), out semanticModel, out symbol, out _, cancellationToken);
		}

		/// <inheritdoc/>
		public bool GetValidationData(
			CSharpSyntaxNode node,
			DefaultParamCompilationData compilation,
			[NotNullWhen(true)] out SemanticModel? semanticModel,
			[NotNullWhen(true)] out ISymbol? symbol,
			out TypeParameterContainer typeParameters,
			CancellationToken cancellationToken = default
		)
		{
			SemanticModel s = compilation.Compilation.GetSemanticModel(node.SyntaxTree);
			TypeParameterListSyntax? parameterList = GetTypeParameterList(node);
			typeParameters = GetTypeParameters(parameterList, s, compilation, cancellationToken);

			if (TypeParametersAreValid(in typeParameters, node))
			{
				symbol = s.GetDeclaredSymbol(node, cancellationToken);

				if (symbol is not null)
				{
					semanticModel = s;
					return true;
				}
			}

			symbol = default;
			semanticModel = default;
			return false;
		}

		/// <inheritdoc/>
		public abstract bool ValidateAndCreate(
			CSharpSyntaxNode node,
			DefaultParamCompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			in TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken = default
		);

		public bool ValidateAndCreate(CSharpSyntaxNode node, DefaultParamCompilationData compilation, [NotNullWhen(true)] out IDefaultParamTarget? data, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public bool ValidateAndCreate(CSharpSyntaxNode node, DefaultParamCompilationData compilation, [NotNullWhen(true)] out IDefaultParamTarget? data, IDiagnosticReceiver diagnosticReceiver, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public abstract bool ValidateAndCreate(
			CSharpSyntaxNode node,
			DefaultParamCompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			in TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken = default
		);

		/// <inheritdoc/>
		[Obsolete("Use ValidateAndCreate with IDefaultParamTarget as return instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		public sealed override bool ValidateAndCreate(
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
			CSharpSyntaxNode node,
			ICompilationData compilation,
			[NotNullWhen(true)] out IMemberData? data,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken = default
		)
		{
			DefaultParamCompilationData c = GetDPCompilation(compilation);

			if (!GetValidationData(node, c, out SemanticModel? semanticModel, out ISymbol? symbol, out TypeParameterContainer typeParameters, cancellationToken))
			{
				data = default;
				return false;
			}

			bool isValid = ValidateAndCreate(node, c, semanticModel, symbol, in typeParameters, out IDefaultParamTarget? target, diagnosticReceiver, cancellationToken);
			data = target;
			return isValid;
		}

		/// <inheritdoc/>
		[Obsolete("Use ValidateAndCreate with IDefaultParamTarget as return instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		public sealed override bool ValidateAndCreate(
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
			CSharpSyntaxNode node,
			ICompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			[NotNullWhen(true)] out IMemberData? data,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken = default
		)
		{
			DefaultParamCompilationData c = GetDPCompilation(compilation);
			TypeParameterContainer typeParameters = GetTypeParameters(GetTypeParameterList(node), semanticModel, c, cancellationToken);

			bool isValid = ValidateAndCreate(node, c, semanticModel, symbol, in typeParameters, out IDefaultParamTarget? target, diagnosticReceiver, cancellationToken);
			data = target;
			return isValid;
		}

		/// <inheritdoc/>
		[Obsolete("Use ValidateAndCreate with IDefaultParamTarget as return instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		public sealed override bool ValidateAndCreate(
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
			CSharpSyntaxNode node,
			ICompilationData compilation,
			[NotNullWhen(true)] out IMemberData? data,
			CancellationToken cancellationToken = default
		)
		{
			DefaultParamCompilationData c = GetDPCompilation(compilation);

			if (!GetValidationData(node, c, out SemanticModel? semanticModel, out ISymbol? symbol, out TypeParameterContainer typeParameters, cancellationToken))
			{
				data = default;
				return false;
			}

			bool isValid = ValidateAndCreate(node, c, semanticModel, symbol, in typeParameters, out IDefaultParamTarget? target, cancellationToken);
			data = target;
			return isValid;
		}

		/// <inheritdoc/>
		[Obsolete("Use ValidateAndCreate with IDefaultParamTarget as return instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		public sealed override bool ValidateAndCreate(
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
			CSharpSyntaxNode node,
			ICompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			[NotNullWhen(true)] out IMemberData? data,
			CancellationToken cancellationToken = default
		)
		{
			DefaultParamCompilationData c = GetDPCompilation(compilation);
			TypeParameterContainer typeParameters = GetTypeParameters(GetTypeParameterList(node), semanticModel, c, cancellationToken);

			bool isValid = ValidateAndCreate(node, c, semanticModel, symbol, in typeParameters, out IDefaultParamTarget? target, cancellationToken);
			data = target;
			return isValid;
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

		bool ISyntaxValidatorWithDiagnostics<IDefaultParamTarget>.ValidateAndCreate(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken
		)
		{
			bool isValid = ValidateAndCreate(node, GetDPCompilation(compilation), out IMemberData? target, diagnosticReceiver, cancellationToken);
			data = target as IDefaultParamTarget;
			return isValid;
		}

		bool ISyntaxValidatorWithDiagnostics<IDefaultParamTarget>.ValidateAndCreate(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken
		)
		{
			bool isValid = ValidateAndCreate(node, GetDPCompilation(compilation), semanticModel, symbol, out IMemberData? target, diagnosticReceiver, cancellationToken);
			data = target as IDefaultParamTarget;
			return isValid;
		}

		bool ISyntaxValidator<IDefaultParamTarget>.ValidateAndCreate(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			bool isValid = ValidateAndCreate(node, GetDPCompilation(compilation), out IMemberData? target, cancellationToken);
			data = target as IDefaultParamTarget;
			return isValid;
		}

		bool ISyntaxValidator<IDefaultParamTarget>.ValidateAndCreate(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			bool isValid = ValidateAndCreate(node, GetDPCompilation(compilation), semanticModel, symbol, out IMemberData? target, cancellationToken);
			data = target as IDefaultParamTarget;
			return isValid;
		}

		IEnumerable<IMemberData> ICachedGeneratorSyntaxFilter<IDefaultParamTarget>.Filtrate(ICachedGeneratorPassContext<IDefaultParamTarget> context)
		{
			if(context is ICachedGeneratorPassContext<IMemberData> cache)
			{
				return Filtrate(cache);
			}

			if (GetCandidateNodes(context.SyntaxReceiver) is not IEnumerable<CSharpSyntaxNode> list)
			{
				return Array.Empty<IMemberData>();
			}

			return Yield();

			IEnumerable<IMemberData> Yield()
			{
				foreach (CSharpSyntaxNode node in list)
				{
					if (node is null)
					{
						continue;
					}

					if(context.OriginalContext.TryGetCachedValue(node, out IDefaultParamTarget? target))
					{
						yield return target;
					}

					if(ValidateAndCreate(node, context.TargetCompilation, out IMemberData? data, context.CancellationToken))
					{
						yield return (IDefaultParamTarget)data;
					}
				}
			}
		}

		IEnumerator<IMemberData> ICachedGeneratorSyntaxFilter<IDefaultParamTarget>.GetEnumerator(ICachedGeneratorPassContext<IDefaultParamTarget> context)
		{
			throw new NotImplementedException();
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

		private static DefaultParamCompilationData GetDPCompilation(ICompilationData compilation)
		{
			if(compilation is DefaultParamCompilationData c)
			{
				return c;
			}

			return new DefaultParamCompilationData(compilation.Compilation);
		}
	}
}
