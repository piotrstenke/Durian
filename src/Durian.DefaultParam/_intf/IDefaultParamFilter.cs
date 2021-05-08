using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Durian.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.DefaultParam
{
	/// <summary>
	/// Filtrates and validates nodes collected by a <see cref="DefaultParamSyntaxReceiver"/>.
	/// </summary>
	public interface IDefaultParamFilter : IGeneratorSyntaxFilterWithDiagnostics
	{
		/// <summary>
		/// <see cref="DefaultParamGenerator"/> that created this filter.
		/// </summary>
		new DefaultParamGenerator Generator { get; }

		/// <summary>
		/// <see cref="IFileNameProvider"/> that is used to create a hint name for the generated source.
		/// </summary>
		IFileNameProvider FileNameProvider { get; }

		/// <summary>
		/// Returns an array of <see cref="CSharpSyntaxNode"/>s collected by the <see cref="Generator"/>'s <see cref="DefaultParamSyntaxReceiver"/> that can be filtrated by this filter.
		/// </summary>
		CSharpSyntaxNode[] GetCandidateNodes();

		/// <inheritdoc cref="ValidateAndCreate(DefaultParamCompilationData, CSharpSyntaxNode, SemanticModel, ISymbol, ref TypeParameterContainer, out IDefaultParamTarget?, CancellationToken)"/>
		bool ValidateAndCreate(
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken = default
		);

		/// <summary>
		/// Validates the specified <paramref name="node"/> and returns a new instance of <see cref="IDefaultParamTarget"/> if the validation was a success.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to validate.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="node"/>.</param>
		/// <param name="symbol"><see cref="ISymbol"/> created from the <paramref name="node"/>.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="node"/>'s type parameters.</param>
		/// <param name="data">Newly-created instance of <see cref="IDefaultParamTarget"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		bool ValidateAndCreate(
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			SemanticModel semanticModel,
			ISymbol symbol,
			ref TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken = default
		);

		/// <inheritdoc cref="ValidateAndCreateWithDiagnostics(IDiagnosticReceiver, DefaultParamCompilationData, CSharpSyntaxNode, SemanticModel, ISymbol, ref TypeParameterContainer, out IDefaultParamTarget?, CancellationToken)"/>
		bool ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken = default
		);

		/// <summary>
		/// Validates the specified <paramref name="node"/> and returns a new instance of <see cref="IDefaultParamTarget"/> if the validation was a success. If the <paramref name="node"/> is not valid, reports appropriate <see cref="Diagnostic"/>s using the <paramref name="diagnosticReceiver"/>.
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to validate.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="node"/>.</param>
		/// <param name="symbol"><see cref="ISymbol"/> created from the <paramref name="node"/>.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="node"/>'s type parameters.</param>
		/// <param name="data">Newly-created instance of <see cref="IDefaultParamTarget"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		bool ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			SemanticModel semanticModel,
			ISymbol symbol,
			ref TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken = default
		);

		/// <summary>
		/// Specifies, if the <see cref="SemanticModel"/>, <see cref="ISymbol"/> and <see cref="TypeParameterContainer"/> can be created from the given <paramref name="node"/>.
		/// If so, returns them.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to validate.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="node"/>.</param>
		/// <param name="symbol"><see cref="ISymbol"/> created from the <paramref name="node"/>.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="node"/>'s type parameters.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		bool GetValidationData(
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			[NotNullWhen(true)] out SemanticModel? semanticModel,
			[NotNullWhen(true)] out ISymbol? symbol,
			out TypeParameterContainer typeParameters,
			CancellationToken cancellationToken = default
		);
	}
}
