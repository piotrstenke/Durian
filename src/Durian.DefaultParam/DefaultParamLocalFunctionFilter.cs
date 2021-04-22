using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Durian.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.DefaultParam
{
	public class DefaultParamLocalFunctionFilter : IDefaultParamFilter
	{
		private readonly DefaultParamGenerator _generator;
		private readonly LoggableSourceGenerator.DiagnosticReceiver? _loggableReceiver;
		private readonly IDirectDiagnosticReceiver? _diagnosticReceiver;

		public DefaultParamLocalFunctionFilter(DefaultParamGenerator generator)
		{
			_generator = generator;

			if (generator.LoggingConfiguration.EnableLogging)
			{
				_loggableReceiver = new LoggableSourceGenerator.DiagnosticReceiver(generator);
				_diagnosticReceiver = generator.SupportsDiagnostics ? DiagnosticReceiverFactory.Direct(ReportForBothReceivers) : _loggableReceiver;
			}
			else if (generator.SupportsDiagnostics)
			{
				_diagnosticReceiver = generator.DiagnosticReceiver!;
			}
		}

		private void ReportForBothReceivers(Diagnostic diagnostic)
		{
			_loggableReceiver!.ReportDiagnostic(diagnostic);
			_generator.DiagnosticReceiver!.ReportDiagnostic(diagnostic);
		}

		public void ReportDiagnosticsForLocalFunctions()
		{
			if (_generator.SyntaxReceiver is null || _generator.TargetCompilation is null)
			{
				return;
			}

			if (_loggableReceiver is not null)
			{
				IDirectDiagnosticReceiver diagnosticReceiver = _generator.EnableDiagnostics ? _diagnosticReceiver! : _loggableReceiver;
				ReportDiagnosticsForLocalFunctions(diagnosticReceiver, _generator.TargetCompilation, _generator.SyntaxReceiver, _generator.CancellationToken);
			}
			else if (_diagnosticReceiver is not null && _generator.EnableDiagnostics)
			{
				ReportDiagnosticsForLocalFunctions(_diagnosticReceiver, _generator.TargetCompilation, _generator.SyntaxReceiver, _generator.CancellationToken);
			}
		}

		public static void ReportDiagnosticsForLocalFunctions(IDiagnosticReceiver diagnosticReceiver, DefaultParamCompilationData compilation, IEnumerable<LocalFunctionStatementSyntax> collectedLocalFunctions, CancellationToken cancellationToken = default)
		{
			if (collectedLocalFunctions is null || compilation is null || diagnosticReceiver is null)
			{
				return;
			}

			foreach (LocalFunctionStatementSyntax fn in collectedLocalFunctions)
			{
				if (fn is null)
				{
					continue;
				}

				SemanticModel semanticModel = compilation.Compilation.GetSemanticModel(fn.SyntaxTree);

				if (HasDefaultParamAttrbiute(fn, semanticModel, compilation.AttributeConstructor))
				{
					ISymbol? symbol = semanticModel.GetDeclaredSymbol(fn, cancellationToken);

					if (symbol is not IMethodSymbol s)
					{
						continue;
					}

					DefaultParamMethodAnalyzer.ReportDiagnosticForLocalFunction(diagnosticReceiver, s);
				}
			}
		}

		public static void ReportDiagnosticsForLocalFunctions(IDiagnosticReceiver diagnosticReceiver, DefaultParamCompilationData compilation, DefaultParamSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken = default)
		{
			if (syntaxReceiver is null || syntaxReceiver.CandidateLocalFunctions is null)
			{
				return;
			}

			ReportDiagnosticsForLocalFunctions(diagnosticReceiver, compilation, syntaxReceiver.CandidateLocalFunctions, cancellationToken);
		}

		private static bool HasDefaultParamAttrbiute(LocalFunctionStatementSyntax decl, SemanticModel semanticModel, IMethodSymbol attrCtor)
		{
			if (decl.TypeParameterList is null)
			{
				return false;
			}

			return decl.TypeParameterList.Parameters
				.SelectMany(p => p.AttributeLists)
				.SelectMany(attr => attr.Attributes)
				.Any(attr =>
				{
					SymbolInfo info = semanticModel.GetSymbolInfo(attr);
					return info.Symbol is IMethodSymbol m && SymbolEqualityComparer.Default.Equals(m, attrCtor);
				});
		}

		IEnumerable<IMemberData> ISyntaxFilter.Filtrate(ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken)
		{
			return Array.Empty<IMemberData>();
		}

		IEnumerable<IMemberData> ISyntaxFilter.Filtrate(ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken)
		{
			return Array.Empty<IMemberData>();
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken)
		{
			ReportDiagnosticsForLocalFunctions(diagnosticReceiver, (DefaultParamCompilationData)compilation, (DefaultParamSyntaxReceiver)syntaxReceiver, cancellationToken);
			return Array.Empty<IMemberData>();
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken)
		{
			ReportDiagnosticsForLocalFunctions(diagnosticReceiver, (DefaultParamCompilationData)compilation, collectedNodes.OfType<LocalFunctionStatementSyntax>(), cancellationToken);
			return Array.Empty<IMemberData>();
		}

#pragma warning disable RCS1079 // Throwing of new NotImplementedException.
		IDefaultParamDeclarationBuilder IDefaultParamFilter.GetDeclarationBuilder(IDefaultParamTarget target, CancellationToken cancellationToken)
		{
			// This method should never be called, so its OK to throw an exception here.
			throw new NotImplementedException();
		}
#pragma warning restore RCS1079 // Throwing of new NotImplementedException.
	}
}
