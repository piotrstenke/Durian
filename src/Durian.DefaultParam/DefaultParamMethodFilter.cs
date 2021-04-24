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
using static Durian.DefaultParam.DefaultParamMethodAnalyzer;

namespace Durian.DefaultParam
{
	public partial class DefaultParamMethodFilter : IDefaultParamFilter
	{
		private readonly DeclarationBuilder _declBuilder;
		private readonly DefaultParamGenerator _generator;
		private readonly LoggableSourceGenerator.DiagnosticReceiver? _loggableReceiver;
		private readonly IDirectDiagnosticReceiver? _diagnosticReceiver;

		public DefaultParamMethodFilter(DefaultParamGenerator generator)
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

		public DeclarationBuilder GetDeclarationBuilder(DefaultParamMethodData target, CancellationToken cancellationToken = default)
		{
			_declBuilder.SetData(target, cancellationToken);

			return _declBuilder;
		}

		public DefaultParamMethodData[] GetValidMethods()
		{
			if (_generator.SyntaxReceiver is null || _generator.TargetCompilation is null || _generator.SyntaxReceiver.CandidateMethods.Count == 0)
			{
				return Array.Empty<DefaultParamMethodData>();
			}

			DefaultParamCompilationData compilation = _generator.TargetCompilation;
			CancellationToken cancellationToken = _generator.CancellationToken;

			if (_loggableReceiver is not null)
			{
				List<DefaultParamMethodData> list = new(_generator.SyntaxReceiver.CandidateMethods.Count);
				IDirectDiagnosticReceiver diagnosticReceiver = _generator.EnableDiagnostics ? _diagnosticReceiver! : _loggableReceiver;

				foreach (MethodDeclarationSyntax method in _generator.SyntaxReceiver.CandidateMethods)
				{
					if (method is null)
					{
						continue;
					}

					if (!GetValidationData(compilation, method, out SemanticModel semanticModel, out TypeParameterContainer typeParameters, out IMethodSymbol symbol, cancellationToken))
					{
						continue;
					}

					_loggableReceiver.SetTargetNode(method, symbol.ToString());

					if (ValidateAndCreateWithDiagnostics(diagnosticReceiver, compilation, method, semanticModel, symbol, ref typeParameters, out DefaultParamMethodData? data, cancellationToken))
					{
						list.Add(data!);
					}

					_loggableReceiver.Push();
				}

				return list.ToArray();
			}
			else if (_diagnosticReceiver is not null && _generator.EnableDiagnostics)
			{
				return GetValidMethodsWithDiagnostics_Internal(_diagnosticReceiver, compilation, _generator.SyntaxReceiver.CandidateMethods.ToArray(), cancellationToken);
			}
			else
			{
				return GetValidMethods_Internal(compilation, _generator.SyntaxReceiver.CandidateMethods.ToArray(), cancellationToken);
			}
		}

		#region -Without Diagnostics-
		public static DefaultParamMethodData[] GetValidMethods(DefaultParamCompilationData compilation, DefaultParamSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken = default)
		{
			if (compilation is null || syntaxReceiver is null || syntaxReceiver.CandidateMethods.Count == 0)
			{
				return Array.Empty<DefaultParamMethodData>();
			}

			return GetValidMethods_Internal(compilation, syntaxReceiver.CandidateMethods.ToArray(), cancellationToken);
		}

		public static DefaultParamMethodData[] GetValidMethods(DefaultParamCompilationData compilation, IEnumerable<MethodDeclarationSyntax> collectedMethods, CancellationToken cancellationToken = default)
		{
			if (compilation is null || collectedMethods is null)
			{
				return Array.Empty<DefaultParamMethodData>();
			}

			MethodDeclarationSyntax[] array = collectedMethods.ToArray();

			if (array.Length == 0)
			{
				return Array.Empty<DefaultParamMethodData>();
			}

			return GetValidMethods_Internal(compilation, array, cancellationToken);
		}

		private static DefaultParamMethodData[] GetValidMethods_Internal(DefaultParamCompilationData compilation, MethodDeclarationSyntax[] collectedMethods, CancellationToken cancellationToken)
		{
			List<DefaultParamMethodData> list = new(collectedMethods.Length);

			foreach (MethodDeclarationSyntax decl in collectedMethods)
			{
				if (decl is null)
				{
					continue;
				}

				if (ValidateAndCreateWithoutDiagnostics(compilation, decl, out DefaultParamMethodData? data, cancellationToken))
				{
					list.Add(data!);
				}
			}

			return list.ToArray();
		}

		private static bool ValidateAndCreateWithoutDiagnostics(DefaultParamCompilationData compilation, MethodDeclarationSyntax declaration, out DefaultParamMethodData? data, CancellationToken cancellationToken)
		{
			if (!GetValidationData(compilation, declaration, out SemanticModel semanticModel, out TypeParameterContainer typeParameters, out IMethodSymbol symbol, cancellationToken))
			{
				data = null;
				return false;
			}

			return ValidateAndCreateWithoutDiagnostics(compilation, declaration, semanticModel, symbol, ref typeParameters, out data, cancellationToken);
		}

		private static bool ValidateAndCreateWithoutDiagnostics(
			DefaultParamCompilationData compilation,
			MethodDeclarationSyntax declaration,
			SemanticModel semanticModel,
			IMethodSymbol symbol,
			ref TypeParameterContainer typeParameters,
			out DefaultParamMethodData? data,
			CancellationToken cancellationToken
		)
		{
			if (ValidateIsPartialOrExtern(symbol, declaration) &&
				ValidateHasGeneratedCodeAttribute(symbol, compilation, out AttributeData[]? attributes) &&
				ValidateContainingTypes(symbol, compilation, out ITypeData[]? containingTypes))
			{
				if ((IsOverride(symbol, out IMethodSymbol? baseMethod) &&
					ValidateOverrideMethod(baseMethod, ref typeParameters, compilation, cancellationToken)) ||
					ValidateTypeParameters(in typeParameters))
				{
					if (ValidateMethodSignature(symbol, typeParameters, compilation, out List<int>? newModifiers))
					{
						data = new(
							declaration,
							compilation,
							symbol,
							semanticModel,
							containingTypes,
							null,
							attributes,
							in typeParameters,
							newModifiers,
							CheckShouldCallInsteadOfCopying(attributes!, compilation)
						);

						return true;
					}
				}
			}

			data = null;
			return false;
		}
		#endregion

		#region -With Diagnostics-

		public static DefaultParamMethodData[] GetValidMethodsWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, DefaultParamCompilationData compilation, DefaultParamSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken = default)
		{
			if (compilation is null || diagnosticReceiver is null || syntaxReceiver is null || syntaxReceiver.CandidateMethods.Count == 0)
			{
				return Array.Empty<DefaultParamMethodData>();
			}

			return GetValidMethodsWithDiagnostics_Internal(diagnosticReceiver, compilation, syntaxReceiver.CandidateMethods.ToArray(), cancellationToken);
		}

		public static DefaultParamMethodData[] GetValidMethodsWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, DefaultParamCompilationData compilation, IEnumerable<MethodDeclarationSyntax> collectedMethods, CancellationToken cancellationToken = default)
		{
			if (compilation is null || diagnosticReceiver is null || collectedMethods is null)
			{
				return Array.Empty<DefaultParamMethodData>();
			}

			MethodDeclarationSyntax[] array = collectedMethods.ToArray();

			if (array.Length == 0)
			{
				return Array.Empty<DefaultParamMethodData>();
			}

			return GetValidMethodsWithDiagnostics_Internal(diagnosticReceiver, compilation, array, cancellationToken);
		}

		private static DefaultParamMethodData[] GetValidMethodsWithDiagnostics_Internal(IDiagnosticReceiver diagnosticReceiver, DefaultParamCompilationData compilation, MethodDeclarationSyntax[] collectedMethods, CancellationToken cancellationToken)
		{
			List<DefaultParamMethodData> list = new(collectedMethods.Length);

			foreach (MethodDeclarationSyntax decl in collectedMethods)
			{
				if (decl is null)
				{
					continue;
				}

				if (ValidateAndCreateWithDiagnostics(diagnosticReceiver, compilation, decl, out DefaultParamMethodData? data, cancellationToken))
				{
					list.Add(data!);
				}
			}

			return list.ToArray();
		}

		private static bool ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			MethodDeclarationSyntax declaration,
			out DefaultParamMethodData? data,
			CancellationToken cancellationToken
		)
		{
			if (!GetValidationData(compilation, declaration, out SemanticModel semanticModel, out TypeParameterContainer typeParameters, out IMethodSymbol symbol, cancellationToken))
			{
				data = null;
				return false;
			}

			return ValidateAndCreateWithDiagnostics(diagnosticReceiver, compilation, declaration, semanticModel, symbol, ref typeParameters, out data, cancellationToken);
		}

		private static bool ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			MethodDeclarationSyntax declaration,
			SemanticModel semanticModel,
			IMethodSymbol symbol,
			ref TypeParameterContainer typeParameters,
			out DefaultParamMethodData? data,
			CancellationToken cancellationToken
		)
		{
			bool isValid = ValidateIsPartialOrExtern(diagnosticReceiver, symbol, declaration);
			isValid &= ValidateHasGeneratedCodeAttribute(diagnosticReceiver, symbol, compilation, out AttributeData[]? attributes);
			isValid &= ValidateContainingTypes(diagnosticReceiver, symbol, compilation, out ITypeData[]? containingTypes);

			bool hasValidTypeParameters;

			if (IsOverride(symbol, out IMethodSymbol? baseMethod))
			{
				isValid &= ValidateOverrideMethod(diagnosticReceiver, symbol, baseMethod, ref typeParameters, compilation, cancellationToken, out hasValidTypeParameters);
			}
			else
			{
				hasValidTypeParameters = ValidateTypeParameters(diagnosticReceiver, in typeParameters);
				isValid &= hasValidTypeParameters;
			}

			if (hasValidTypeParameters)
			{
				isValid &= ValidateMethodSignature(diagnosticReceiver, symbol, typeParameters, compilation, out List<int>? newModifiers);

				data = new(
					declaration,
					compilation,
					symbol,
					semanticModel,
					containingTypes,
					null,
					attributes,
					in typeParameters,
					newModifiers,
					CheckShouldCallInsteadOfCopying(attributes!, compilation)
				);
			}
			else
			{
				data = null;
			}

			return isValid;
		}
		#endregion

		#region -Interface Implementations-

		IDefaultParamDeclarationBuilder IDefaultParamFilter.GetDeclarationBuilder(IDefaultParamTarget target, CancellationToken cancellationToken)
		{
			return GetDeclarationBuilder((DefaultParamMethodData)target, cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilter.Filtrate(ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken)
		{
			return GetValidMethods((DefaultParamCompilationData)compilation, (DefaultParamSyntaxReceiver)syntaxReceiver, cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilter.Filtrate(ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken)
		{
			return GetValidMethods((DefaultParamCompilationData)compilation, collectedNodes.OfType<MethodDeclarationSyntax>(), cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken)
		{
			return GetValidMethodsWithDiagnostics(diagnosticReceiver, (DefaultParamCompilationData)compilation, (DefaultParamSyntaxReceiver)syntaxReceiver, cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken)
		{
			return GetValidMethodsWithDiagnostics(diagnosticReceiver, (DefaultParamCompilationData)compilation, collectedNodes.OfType<MethodDeclarationSyntax>(), cancellationToken);
		}
		#endregion

		private static bool GetValidationData(
			DefaultParamCompilationData compilation,
			MethodDeclarationSyntax declaration,
			out SemanticModel semanticModel,
			out TypeParameterContainer typeParameters,
			out IMethodSymbol symbol,
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

		private static TypeParameterContainer GetParameters(MethodDeclarationSyntax declaration, SemanticModel semanticModel, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
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
