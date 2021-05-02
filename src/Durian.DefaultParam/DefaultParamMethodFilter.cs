using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
		public DefaultParamGenerator Generator { get; }
		public IFileNameProvider FileNameProvider { get; }
		public FilterMode Mode => Generator.LoggingConfiguration.CurrentFilterMode;
		public bool IncludeGeneratedSymbols { get; } = true;

		IDurianSourceGenerator IGeneratorSyntaxFilter.Generator => Generator;

		public DefaultParamMethodFilter(DefaultParamGenerator generator) : this(generator, new SymbolNameToFile())
		{
		}

		public DefaultParamMethodFilter(DefaultParamGenerator generator, IFileNameProvider fileNameProvider)
		{
			Generator = generator;
			FileNameProvider = fileNameProvider;
		}

		public MethodDeclarationSyntax[] GetCandidateMethods()
		{
			return Generator.SyntaxReceiver?.CandidateMethods?.ToArray() ?? Array.Empty<MethodDeclarationSyntax>();
		}

		public DefaultParamMethodData[] GetValidMethods()
		{
			if (Generator.SyntaxReceiver is null || Generator.TargetCompilation is null || Generator.SyntaxReceiver.CandidateMethods.Count == 0)
			{
				return Array.Empty<DefaultParamMethodData>();
			}

			return DefaultParamUtilities.IterateFilter<DefaultParamMethodData>(this);
		}

		public static DefaultParamMethodData[] GetValidMethods(
			DefaultParamCompilationData compilation,
			DefaultParamSyntaxReceiver syntaxReceiver,
			CancellationToken cancellationToken = default
		)
		{
			if (compilation is null || syntaxReceiver is null || syntaxReceiver.CandidateMethods.Count == 0)
			{
				return Array.Empty<DefaultParamMethodData>();
			}

			return GetValidMethods_Internal(compilation, syntaxReceiver.CandidateMethods.ToArray(), cancellationToken);
		}

		public static DefaultParamMethodData[] GetValidMethods(
			DefaultParamCompilationData compilation,
			IEnumerable<MethodDeclarationSyntax> collectedMethods,
			CancellationToken cancellationToken = default
		)
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

		public static bool ValidateAndCreate(
			DefaultParamCompilationData compilation,
			MethodDeclarationSyntax declaration,
			[NotNullWhen(true)] out DefaultParamMethodData? data,
			CancellationToken cancellationToken = default
		)
		{
			if (!GetValidationData(compilation, declaration, out SemanticModel? semanticModel, out TypeParameterContainer typeParameters, out IMethodSymbol? symbol, cancellationToken))
			{
				data = null;
				return false;
			}

			return ValidateAndCreate(compilation, declaration, semanticModel, symbol, ref typeParameters, out data, cancellationToken);
		}

		public static bool ValidateAndCreate(
			DefaultParamCompilationData compilation,
			MethodDeclarationSyntax declaration,
			SemanticModel semanticModel,
			IMethodSymbol symbol,
			ref TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out DefaultParamMethodData? data,
			CancellationToken cancellationToken = default
		)
		{
			if (AnalyzeAgaintsPartialOrExtern(symbol, declaration) &&
				AnalyzeAgainstGeneratedCodeAttribute(symbol, compilation, out AttributeData[]? attributes) &&
				AnalyzeContainingTypes(symbol, compilation, out ITypeData[]? containingTypes))
			{
				if ((IsOverride(symbol, out IMethodSymbol? baseMethod) &&
					AnalyzeOverrideMethod(baseMethod, ref typeParameters, compilation, cancellationToken)) ||
					AnalyzeTypeParameters(in typeParameters))
				{
					if (AnalyzeMethodSignature(symbol, typeParameters, compilation, out HashSet<int>? newModifiers, cancellationToken))
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

		public static bool GetValidationData(
			DefaultParamCompilationData compilation,
			MethodDeclarationSyntax declaration,
			[NotNullWhen(true)] out SemanticModel? semanticModel,
			out TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out IMethodSymbol? symbol,
			CancellationToken cancellationToken = default
		)
		{
			semanticModel = compilation.Compilation.GetSemanticModel(declaration.SyntaxTree);
			typeParameters = GetParameters(declaration, semanticModel, compilation, cancellationToken);

			if (typeParameters.HasDefaultParams || declaration.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)))
			{
				symbol = semanticModel.GetDeclaredSymbol(declaration, cancellationToken)!;

				return symbol is not null;
			}

			symbol = null!;
			return false;
		}

		private static DefaultParamMethodData[] GetValidMethods_Internal(
			DefaultParamCompilationData compilation,
			MethodDeclarationSyntax[] collectedMethods,
			CancellationToken cancellationToken
		)
		{
			List<DefaultParamMethodData> list = new(collectedMethods.Length);

			foreach (MethodDeclarationSyntax decl in collectedMethods)
			{
				if (decl is null)
				{
					continue;
				}

				if (ValidateAndCreate(compilation, decl, out DefaultParamMethodData? data, cancellationToken))
				{
					list.Add(data!);
				}
			}

			return list.ToArray();
		}

		private static TypeParameterContainer GetParameters(
			MethodDeclarationSyntax declaration,
			SemanticModel semanticModel,
			DefaultParamCompilationData compilation,
			CancellationToken cancellationToken
		)
		{
			TypeParameterListSyntax? typeParameters = declaration.TypeParameterList;

			if (typeParameters is null)
			{
				return new TypeParameterContainer(null);
			}

			return new TypeParameterContainer(typeParameters.Parameters.Select(p => TypeParameterData.CreateFrom(p, semanticModel, compilation, cancellationToken)));
		}

		#region -Interface Implementations-

		CSharpSyntaxNode[] IDefaultParamFilter.GetCandidateNodes()
		{
			return GetCandidateMethods();
		}

		IEnumerator<IMemberData> IGeneratorSyntaxFilter.GetEnumerator()
		{
			return DefaultParamUtilities.GetFilterEnumerator(this);
		}

		IMemberData[] IGeneratorSyntaxFilter.Filtrate()
		{
			return GetValidMethods();
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
			return WithDiagnostics.GetValidMethods(diagnosticReceiver, (DefaultParamCompilationData)compilation, (DefaultParamSyntaxReceiver)syntaxReceiver, cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken)
		{
			return WithDiagnostics.GetValidMethods(diagnosticReceiver, (DefaultParamCompilationData)compilation, collectedNodes.OfType<MethodDeclarationSyntax>(), cancellationToken);
		}

		bool IDefaultParamFilter.ValidateAndCreate(
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			bool isValid = ValidateAndCreate(compilation, (MethodDeclarationSyntax)node, out DefaultParamMethodData? d, cancellationToken);
			data = d;
			return isValid;
		}

		bool IDefaultParamFilter.ValidateAndCreate(
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			SemanticModel semanticModel,
			ISymbol symbol,
			ref TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			bool isValid = ValidateAndCreate(compilation, (MethodDeclarationSyntax)node, semanticModel, (IMethodSymbol)symbol, ref typeParameters, out DefaultParamMethodData? d, cancellationToken);
			data = d;
			return isValid;
		}

		bool IDefaultParamFilter.ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			bool isValid = WithDiagnostics.ValidateAndCreate(diagnosticReceiver, compilation, (MethodDeclarationSyntax)node, out DefaultParamMethodData? d, cancellationToken);
			data = d;
			return isValid;
		}

		bool IDefaultParamFilter.ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			SemanticModel semanticModel,
			ISymbol symbol,
			ref TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			bool isValid = WithDiagnostics.ValidateAndCreate(diagnosticReceiver, compilation, (MethodDeclarationSyntax)node, semanticModel, (IMethodSymbol)symbol, ref typeParameters, out DefaultParamMethodData? d, cancellationToken);
			data = d;
			return isValid;
		}

		bool IDefaultParamFilter.GetValidationData(
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			[NotNullWhen(true)] out SemanticModel? semanticModel,
			out TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out ISymbol? symbol,
			CancellationToken cancellationToken
		)
		{
			bool isValid = GetValidationData(compilation, (MethodDeclarationSyntax)node, out semanticModel, out typeParameters, out IMethodSymbol? s, cancellationToken);
			symbol = s;
			return isValid;
		}

		#endregion
	}
}
