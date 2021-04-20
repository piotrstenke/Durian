using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Durian.Data;
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

		public DefaultParamMethodFilter()
		{
			_declBuilder = new();
		}

		public static DefaultParamMethodData[] GetValidMethods(DefaultParamCompilationData compilation, DefaultParamSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken = default)
		{
			if (syntaxReceiver is null)
			{
				return Array.Empty<DefaultParamMethodData>();
			}

			return GetValidMethods(compilation, syntaxReceiver.CandidateMethods, cancellationToken);
		}

		public static DefaultParamMethodData[] GetValidMethods(DefaultParamCompilationData compilation, IEnumerable<MethodDeclarationSyntax> collectedMethods, CancellationToken cancellationToken = default)
		{
			if (collectedMethods is null)
			{
				return Array.Empty<DefaultParamMethodData>();
			}

			MethodDeclarationSyntax[] collected = collectedMethods.ToArray();
			List<DefaultParamMethodData> list = new(collected.Length);

			foreach (MethodDeclarationSyntax decl in collected)
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

		public static DefaultParamMethodData[] GetValidMethods(IDiagnosticReceiver diagnosticReceiver, DefaultParamCompilationData compilation, DefaultParamSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken = default)
		{
			if (syntaxReceiver is null)
			{
				return Array.Empty<DefaultParamMethodData>();
			}

			return GetValidMethods(diagnosticReceiver, compilation, syntaxReceiver.CandidateMethods, cancellationToken);
		}

		public static DefaultParamMethodData[] GetValidMethods(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			IEnumerable<MethodDeclarationSyntax> collectedMethods,
			CancellationToken cancellationToken = default
		)
		{
			if (collectedMethods is null)
			{
				return Array.Empty<DefaultParamMethodData>();
			}

			MethodDeclarationSyntax[] collected = collectedMethods.ToArray();
			List<DefaultParamMethodData> list = new(collected.Length);

			foreach (MethodDeclarationSyntax decl in collected)
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

		public DeclarationBuilder GetDeclarationBuilder(DefaultParamMethodData target, CancellationToken cancellationToken = default)
		{
			_declBuilder.SetData(target, cancellationToken);

			return _declBuilder;
		}

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
			return GetValidMethods((DefaultParamCompilationData)compilation, collectedNodes.Cast<MethodDeclarationSyntax>(), cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken)
		{
			return GetValidMethods(diagnosticReceiver, (DefaultParamCompilationData)compilation, (DefaultParamSyntaxReceiver)syntaxReceiver, cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken)
		{
			return GetValidMethods(diagnosticReceiver, (DefaultParamCompilationData)compilation, collectedNodes.Cast<MethodDeclarationSyntax>(), cancellationToken);
		}

		private static bool ValidateAndCreateWithoutDiagnostics(
			DefaultParamCompilationData compilation,
			MethodDeclarationSyntax declaration,
			out DefaultParamMethodData? data,
			CancellationToken cancellationToken = default
		)
		{
			SemanticModel semanticModel = compilation.Compilation.GetSemanticModel(declaration.SyntaxTree);
			TypeParameterContainer typeParameters = GetParameters(declaration, semanticModel, compilation, cancellationToken);

			if (!typeParameters.HasDefaultParams)
			{
				data = null;
				return false;
			}

			IMethodSymbol? symbol = semanticModel.GetDeclaredSymbol(declaration, cancellationToken);

			if (symbol is null)
			{
				data = null;
				return false;
			}

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

		private static bool ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			MethodDeclarationSyntax declaration,
			out DefaultParamMethodData? data,
			CancellationToken cancellationToken = default
		)
		{
			SemanticModel semanticModel = compilation.Compilation.GetSemanticModel(declaration.SyntaxTree);
			TypeParameterContainer typeParameters = GetParameters(declaration, semanticModel, compilation, cancellationToken);

			if (!typeParameters.HasDefaultParams)
			{
				data = null;
				return false;
			}

			IMethodSymbol? symbol = semanticModel.GetDeclaredSymbol(declaration, cancellationToken);

			if (symbol is null)
			{
				data = null;
				return false;
			}

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

		private static TypeParameterContainer GetParameters(MethodDeclarationSyntax declaration, SemanticModel semanticModel, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
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
