using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Durian.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.DefaultParam
{
	public interface IDefaultParamFilter : IGeneratorSyntaxFilterWithDiagnostics
	{
		new DefaultParamGenerator Generator { get; }
		IFileNameProvider FileNameProvider { get; }

		CSharpSyntaxNode[] GetCandidateNodes();

		bool ValidateAndCreate(
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken = default
		);

		bool ValidateAndCreate(
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			SemanticModel semanticModel,
			ISymbol symbol,
			ref TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken = default
		);

		bool ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken = default
		);

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

		bool GetValidationData(
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			[NotNullWhen(true)] out SemanticModel? semanticModel,
			out TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out ISymbol? symbol,
			CancellationToken cancellationToken = default
		);
	}
}
