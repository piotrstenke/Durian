using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Durian.Data;
using Durian.Logging;
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

		public DeclarationBuilder GetDeclarationBuilder(DefaultParamTypeData target, CancellationToken cancellationToken = default)
		{
			_declBuilder.SetData(target, cancellationToken);

			return _declBuilder;
		}

		public DefaultParamTypeData[] GetValidTypes()
		{
			if (_generator.SyntaxReceiver is null || _generator.TargetCompilation is null || _generator.SyntaxReceiver.CandidateTypes.Count == 0)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			DefaultParamCompilationData compilation = _generator.TargetCompilation;
			CancellationToken cancellationToken = _generator.CancellationToken;

			if (_loggableReceiver is not null)
			{
				List<DefaultParamTypeData> list = new(_generator.SyntaxReceiver.CandidateTypes.Count);
				IDirectDiagnosticReceiver diagnosticReceiver = _generator.EnableDiagnostics ? _diagnosticReceiver! : _loggableReceiver;

				foreach (TypeDeclarationSyntax type in _generator.SyntaxReceiver.CandidateTypes)
				{
					if (type is null)
					{
						continue;
					}

					if (!GetValidationData(compilation, type, out SemanticModel semanticModel, out TypeParameterContainer typeParameters, out INamedTypeSymbol symbol, cancellationToken))
					{
						continue;
					}

					_loggableReceiver.SetTargetNode(type, symbol.ToString());

					if (ValidateAndCreateWithDiagnostics(diagnosticReceiver, compilation, type, semanticModel, symbol, ref typeParameters, out DefaultParamTypeData? data))
					{
						list.Add(data!);
					}

					_loggableReceiver.Push();
				}

				return list.ToArray();
			}
			else if (_diagnosticReceiver is not null && _generator.EnableDiagnostics)
			{
				return GetValidTypesWithDiagnostics_Internal(_diagnosticReceiver, compilation, _generator.SyntaxReceiver.CandidateTypes.ToArray(), cancellationToken);
			}
			else
			{
				return GetValidTypes_Internal(compilation, _generator.SyntaxReceiver.CandidateTypes.ToArray(), cancellationToken);
			}
		}

		#region -Without Diagnostics-
		public static DefaultParamTypeData[] GetValidTypes(DefaultParamCompilationData compilation, IEnumerable<TypeDeclarationSyntax> collectedTypes, CancellationToken cancellationToken = default)
		{
			if (compilation is null || collectedTypes is null || compilation is null)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			TypeDeclarationSyntax[] collected = collectedTypes.ToArray();

			if (collected.Length == 0)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			return GetValidTypes_Internal(compilation, collected, cancellationToken);
		}

		public static DefaultParamTypeData[] GetValidTypes(DefaultParamCompilationData compilation, DefaultParamSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken = default)
		{
			if (compilation is null || syntaxReceiver is null || syntaxReceiver.CandidateTypes.Count == 0)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			return GetValidTypes_Internal(compilation, syntaxReceiver.CandidateTypes.ToArray(), cancellationToken);
		}
		private static DefaultParamTypeData[] GetValidTypes_Internal(DefaultParamCompilationData compilation, TypeDeclarationSyntax[] collectedTypes, CancellationToken cancellationToken)
		{
			List<DefaultParamTypeData> list = new(collectedTypes.Length);

			foreach (TypeDeclarationSyntax decl in collectedTypes)
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

		#endregion

		#region -With Diagnostics-
		public static DefaultParamTypeData[] GetValidTypesWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, DefaultParamCompilationData compilation, DefaultParamSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken = default)
		{
			if (compilation is null || diagnosticReceiver is null || syntaxReceiver is null || syntaxReceiver.CandidateTypes.Count == 0)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			return GetValidTypesWithDiagnostics_Internal(diagnosticReceiver, compilation, syntaxReceiver.CandidateTypes.ToArray(), cancellationToken);
		}

		public static DefaultParamTypeData[] GetValidTypesWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			IEnumerable<TypeDeclarationSyntax> collectedTypes,
			CancellationToken cancellationToken = default
		)
		{
			if (compilation is null || diagnosticReceiver is null || collectedTypes is null)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			TypeDeclarationSyntax[] collected = collectedTypes.ToArray();

			if (collected.Length == 0)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			return GetValidTypesWithDiagnostics_Internal(diagnosticReceiver, compilation, collected, cancellationToken);
		}

		private static DefaultParamTypeData[] GetValidTypesWithDiagnostics_Internal(IDiagnosticReceiver diagnosticReceiver, DefaultParamCompilationData compilation, TypeDeclarationSyntax[] collectedTypes, CancellationToken cancellationToken)
		{
			List<DefaultParamTypeData> list = new(collectedTypes.Length);

			foreach (TypeDeclarationSyntax decl in collectedTypes)
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
		#endregion

		#region -Interface Implementations
		IDefaultParamDeclarationBuilder IDefaultParamFilter.GetDeclarationBuilder(IDefaultParamTarget target, CancellationToken cancellationToken)
		{
			return GetDeclarationBuilder((DefaultParamTypeData)target, cancellationToken);
		}

		IEnumerable<IDefaultParamTarget> IDefaultParamFilter.Filtrate()
		{
			return GetValidTypes();
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
			return GetValidTypesWithDiagnostics(diagnosticReceiver, (DefaultParamCompilationData)compilation, (DefaultParamSyntaxReceiver)syntaxReceiver, cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken)
		{
			return GetValidTypesWithDiagnostics(diagnosticReceiver, (DefaultParamCompilationData)compilation, collectedNodes.OfType<TypeDeclarationSyntax>(), cancellationToken);
		}
		#endregion

		private static bool GetValidationData(
			DefaultParamCompilationData compilation,
			TypeDeclarationSyntax declaration,
			out SemanticModel semanticModel,
			out TypeParameterContainer typeParameters,
			out INamedTypeSymbol symbol,
			CancellationToken cancellationToken
		)
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

		private static TypeParameterContainer GetParameters(TypeDeclarationSyntax declaration, SemanticModel semanticModel, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			TypeParameterListSyntax? parameters = declaration.TypeParameterList;

			if (parameters is null)
			{
				return new TypeParameterContainer(null);
			}

			return new TypeParameterContainer(parameters.Parameters.Select(p => TypeParameterData.CreateFrom(p, semanticModel, compilation, cancellationToken)));
		}

		private void ReportForBothReceivers(Diagnostic diagnostic)
		{
			_loggableReceiver!.ReportDiagnostic(diagnostic);
			_generator.DiagnosticReceiver!.ReportDiagnostic(diagnostic);
		}
	}
}
