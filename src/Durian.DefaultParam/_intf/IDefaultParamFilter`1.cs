// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Durian.Generator.Cache;
using Durian.Generator.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// Filtrates and validates nodes collected by a <see cref="DefaultParamSyntaxReceiver"/>.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="IDefaultParamTarget"/> this <see cref="IDefaultParamFilter{T}"/> can handle.</typeparam>
	public interface IDefaultParamFilter<T> : ICachedGeneratorSyntaxFilterWithDiagnostics<T>, INodeValidatorWithDiagnostics<T>, INodeProvider where T : IDefaultParamTarget
	{
		/// <summary>
		/// <see cref="DefaultParamGenerator"/> that created this filter.
		/// </summary>
		new DefaultParamGenerator Generator { get; }

		/// <summary>
		/// <see cref="IHintNameProvider"/> that is used to create a hint name for the generated source.
		/// </summary>
		IHintNameProvider HintNameProvider { get; }

		/// <summary>
		/// Specifies, if the <see cref="SemanticModel"/>, <see cref="ISymbol"/> and <see cref="TypeParameterContainer"/> can be created from the given <paramref name="node"/>.
		/// If so, returns them.
		/// </summary>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to validate.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="node"/>.</param>
		/// <param name="symbol"><see cref="ISymbol"/> created from the <paramref name="node"/>.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="node"/>'s type parameters.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		bool GetValidationData(
			CSharpSyntaxNode node,
			DefaultParamCompilationData compilation,
			[NotNullWhen(true)] out SemanticModel? semanticModel,
			[NotNullWhen(true)] out ISymbol? symbol,
			out TypeParameterContainer typeParameters,
			CancellationToken cancellationToken = default
		);

		/// <inheritdoc cref="ValidateAndCreate(CSharpSyntaxNode, DefaultParamCompilationData, SemanticModel, ISymbol, in TypeParameterContainer, out T?, CancellationToken)"/>
		bool ValidateAndCreate(
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			[NotNullWhen(true)] out T? data,
			CancellationToken cancellationToken = default
		);

		/// <summary>
		/// Validates the specified <paramref name="node"/> and returns a new instance of <see cref="IDefaultParamTarget"/> if the validation was a success.
		/// </summary>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to validate.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="node"/>.</param>
		/// <param name="symbol"><see cref="ISymbol"/> created from the <paramref name="node"/>.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="node"/>'s type parameters.</param>
		/// <param name="data">Newly-created instance of <see cref="IDefaultParamTarget"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		bool ValidateAndCreate(
			CSharpSyntaxNode node,
			DefaultParamCompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			in TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out T? data,
			CancellationToken cancellationToken = default
		);

		/// <inheritdoc cref="ValidateAndCreateWithDiagnostics(IDiagnosticReceiver, CSharpSyntaxNode, DefaultParamCompilationData, SemanticModel, ISymbol, in TypeParameterContainer, out T?, CancellationToken)"/>
		bool ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			CSharpSyntaxNode node,
			DefaultParamCompilationData compilation,
			[NotNullWhen(true)] out T? data,
			CancellationToken cancellationToken = default
		);

		/// <summary>
		/// Validates the specified <paramref name="node"/> and returns a new instance of <see cref="IDefaultParamTarget"/> if the validation was a success. If the <paramref name="node"/> is not valid, reports appropriate <see cref="Diagnostic"/>s using the <paramref name="diagnosticReceiver"/>.
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to validate.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="node"/>.</param>
		/// <param name="symbol"><see cref="ISymbol"/> created from the <paramref name="node"/>.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="node"/>'s type parameters.</param>
		/// <param name="data">Newly-created instance of <see cref="IDefaultParamTarget"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		bool ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			CSharpSyntaxNode node,
			DefaultParamCompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			in TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out T? data,
			CancellationToken cancellationToken = default
		);
	}
}
