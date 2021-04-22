using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Durian.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Durian.DefaultParam.DefaultParamAnalyzer;

namespace Durian.DefaultParam
{
	public partial class DefaultParamTypeFilter : IDefaultParamFilter
	{
		private readonly DeclarationBuilder _declBuilder;
		private readonly DefaultParamGenerator _generator;
		private readonly LoggableSourceGenerator.DiagnosticReceiver? _loggableReceiver;
		private readonly IDirectDiagnosticReceiver? _diagnosticReceiver;

		public DefaultParamTypeFilter(DefaultParamGenerator generator)
		{
			_declBuilder = new();
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

		public DefaultParamTypeData[] GetValidTypes()
		{
			if (_generator.SyntaxReceiver is null || _generator.TargetCompilation is null)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			DefaultParamCompilationData compilation = _generator.TargetCompilation;
			CancellationToken cancellationToken = _generator.CancellationToken;

			if (_loggableReceiver is not null)
			{
				List<DefaultParamTypeData> list = new(_generator.SyntaxReceiver.CandidateTypes.Count);
				IDirectDiagnosticReceiver diagnosticReceiver = _generator.EnableDiagnostics ? _diagnosticReceiver! : _loggableReceiver;

				foreach (TypeDeclarationSyntax del in _generator.SyntaxReceiver.CandidateTypes)
				{
					if (del is null)
					{
						continue;
					}

					if (!GetValidationData(compilation, del, out SemanticModel semanticModel, out TypeParameterContainer typeParameters, out INamedTypeSymbol symbol, cancellationToken))
					{
						continue;
					}

					_loggableReceiver.SetTargetNode(del, del.Identifier.ToString());

					if (ValidateAndCreateWithDiagnostics(diagnosticReceiver, compilation, del, semanticModel, symbol, ref typeParameters, out DefaultParamTypeData? data))
					{
						list.Add(data!);
					}
				}

				return list.ToArray();
			}
			else if (_diagnosticReceiver is not null && _generator.EnableDiagnostics)
			{
				return GetValidTypes(_diagnosticReceiver, compilation, _generator.SyntaxReceiver, cancellationToken);
			}
			else
			{
				return GetValidTypes(compilation, _generator.SyntaxReceiver, cancellationToken);
			}
		}

		public static DefaultParamTypeData[] GetValidTypes(DefaultParamCompilationData compilation, IEnumerable<TypeDeclarationSyntax> collectedTypes, CancellationToken cancellationToken = default)
		{
			if (collectedTypes is null || compilation is null)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			TypeDeclarationSyntax[] collected = collectedTypes.ToArray();
			List<DefaultParamTypeData> list = new(collected.Length);

			foreach (TypeDeclarationSyntax decl in collected)
			{
				if (decl is null)
				{
					continue;
				}

				if (ValidateAndCreateWithoutDiagnostics(compilation, decl, out DefaultParamTypeData? data, cancellationToken))
				{
					list.Add(data!);
				}
			}

			return list.ToArray();
		}

		public static DefaultParamTypeData[] GetValidTypes(DefaultParamCompilationData compilation, DefaultParamSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken = default)
		{
			if (syntaxReceiver is null)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			return GetValidTypes(compilation, syntaxReceiver.CandidateTypes, cancellationToken);
		}

		public static DefaultParamTypeData[] GetValidTypes(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			DefaultParamSyntaxReceiver syntaxReceiver,
			CancellationToken cancellationToken = default
		)
		{
			if (syntaxReceiver is null)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			return GetValidTypes(diagnosticReceiver, compilation, syntaxReceiver.CandidateTypes, cancellationToken);
		}

		public static DefaultParamTypeData[] GetValidTypes(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			IEnumerable<TypeDeclarationSyntax> collectedTypes,
			CancellationToken cancellationToken = default
		)
		{
			if (collectedTypes is null || compilation is null)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			TypeDeclarationSyntax[] collected = collectedTypes.ToArray();
			List<DefaultParamTypeData> list = new(collected.Length);

			foreach (TypeDeclarationSyntax decl in collected)
			{
				if (decl is null)
				{
					continue;
				}

				if (ValidateAndCreateWithDiagnostics(diagnosticReceiver, compilation, decl, out DefaultParamTypeData? data, cancellationToken))
				{
					list.Add(data!);
				}
			}

			return list.ToArray();
		}

		public DeclarationBuilder GetDeclarationBuilder(DefaultParamTypeData target, CancellationToken cancellationToken = default)
		{
			_declBuilder.SetData(target, cancellationToken);

			return _declBuilder;
		}

		IDefaultParamDeclarationBuilder IDefaultParamFilter.GetDeclarationBuilder(IDefaultParamTarget target, CancellationToken cancellationToken)
		{
			return GetDeclarationBuilder((DefaultParamTypeData)target, cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilter.Filtrate(ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken)
		{
			return GetValidTypes((DefaultParamCompilationData)compilation, (DefaultParamSyntaxReceiver)syntaxReceiver, cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilter.Filtrate(ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken)
		{
			return GetValidTypes((DefaultParamCompilationData)compilation, collectedNodes.OfType<TypeDeclarationSyntax>(), cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken)
		{
			return GetValidTypes(diagnosticReceiver, (DefaultParamCompilationData)compilation, (DefaultParamSyntaxReceiver)syntaxReceiver, cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken)
		{
			return GetValidTypes(diagnosticReceiver, (DefaultParamCompilationData)compilation, collectedNodes.OfType<TypeDeclarationSyntax>(), cancellationToken);
		}

		private static bool GetValidationData(DefaultParamCompilationData compilation, TypeDeclarationSyntax declaration, out SemanticModel semanticModel, out TypeParameterContainer typeParameters, out INamedTypeSymbol symbol, CancellationToken cancellationToken)
		{
			semanticModel = compilation.Compilation.GetSemanticModel(declaration.SyntaxTree);
			typeParameters = GetParameters(declaration, semanticModel, compilation, cancellationToken);

			if (!typeParameters.HasDefaultParams)
			{
				symbol = null!;
				return false;
			}

			symbol = semanticModel.GetDeclaredSymbol(declaration, cancellationToken)!;

			return symbol is not null;
		}

		private static bool ValidateAndCreateWithoutDiagnostics(
			DefaultParamCompilationData compilation,
			TypeDeclarationSyntax declaration,
			out DefaultParamTypeData? data,
			CancellationToken cancellationToken
		)
		{
			if (!GetValidationData(compilation, declaration, out SemanticModel semanticModel, out TypeParameterContainer typeParameters, out INamedTypeSymbol symbol, cancellationToken))
			{
				data = null;
				return false;
			}

			return ValidateAndCreateWithoutDiagnostics(compilation, declaration, semanticModel, symbol, ref typeParameters, out data);
		}

		private static bool ValidateAndCreateWithoutDiagnostics(
			DefaultParamCompilationData compilation,
			TypeDeclarationSyntax declaration,
			SemanticModel semanticModel,
			INamedTypeSymbol symbol,
			ref TypeParameterContainer typeParameters,
			out DefaultParamTypeData? data
		)
		{
			if (ValidateHasGeneratedCodeAttribute(symbol, compilation, out AttributeData[]? attributes) &&
				ValidateContainingTypes(symbol, compilation, out ITypeData[]? containingTypes) &&
				ValidateTypeParameters(in typeParameters))
			{
				data = new DefaultParamTypeData(
						declaration,
						compilation,
						symbol,
						semanticModel,
						null,
						null,
						containingTypes,
						null,
						attributes,
						typeParameters
					);

				return true;
			}

			data = null;
			return false;
		}

		private static bool ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			TypeDeclarationSyntax declaration,
			out DefaultParamTypeData? data,
			CancellationToken cancellationToken
		)
		{
			if (!GetValidationData(compilation, declaration, out SemanticModel semanticModel, out TypeParameterContainer typeParameters, out INamedTypeSymbol symbol, cancellationToken))
			{
				data = null;
				return false;
			}

			return ValidateAndCreateWithDiagnostics(diagnosticReceiver, compilation, declaration, semanticModel, symbol, ref typeParameters, out data);
		}

		private static bool ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			TypeDeclarationSyntax declaration,
			SemanticModel semanticModel,
			INamedTypeSymbol symbol,
			ref TypeParameterContainer typeParameters,
			out DefaultParamTypeData? data
		)
		{
			bool isValid = ValidateHasGeneratedCodeAttribute(diagnosticReceiver, symbol, compilation, out AttributeData[]? attributes);
			isValid &= ValidateContainingTypes(diagnosticReceiver, symbol, compilation, out ITypeData[]? containingTypes);
			isValid &= ValidateTypeParameters(diagnosticReceiver, in typeParameters);

			if (isValid)
			{
				data = new DefaultParamTypeData(
					declaration,
					compilation,
					symbol,
					semanticModel,
					null,
					null,
					containingTypes,
					null,
					attributes,
					typeParameters
				);

				return true;
			}

			data = null;
			return false;
		}

		private static TypeParameterContainer GetParameters(TypeDeclarationSyntax declaration, SemanticModel semanticModel, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			TypeParameterListSyntax? parameters = declaration.TypeParameterList;

			if (parameters is null)
			{
				return new TypeParameterContainer(null);
			}

			return new TypeParameterContainer(parameters.Parameters.Select(p => TypeParameterData.CreateFrom(p, semanticModel, compilation, cancellationToken)));
		}
	}
}
