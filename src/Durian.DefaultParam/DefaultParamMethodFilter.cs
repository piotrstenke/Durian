using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Generator.Data;
using Durian.Generator.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Durian.Generator.DefaultParam.DefaultParamAnalyzer;
using static Durian.Generator.DefaultParam.DefaultParamMethodAnalyzer;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// Filtrates and validates <see cref="MethodDeclarationBuilder"/>es collected by a <see cref="DefaultParamSyntaxReceiver"/>.
	/// </summary>
	public partial class DefaultParamMethodFilter : IDefaultParamFilter
	{
		/// <inheritdoc/>
		public DefaultParamGenerator Generator { get; }

		/// <inheritdoc/>
		public IFileNameProvider FileNameProvider { get; }

		/// <summary>
		/// <see cref="FilterMode"/> of this <see cref="DefaultParamMethodFilter"/>.
		/// </summary>
		public FilterMode Mode => Generator.LoggingConfiguration.CurrentFilterMode;

		/// <inheritdoc/>
		public bool IncludeGeneratedSymbols { get; } = true;

		IDurianSourceGenerator IGeneratorSyntaxFilter.Generator => Generator;

		/// <inheritdoc cref="DefaultParamMethodFilter(DefaultParamGenerator, IFileNameProvider)"/>
		public DefaultParamMethodFilter(DefaultParamGenerator generator) : this(generator, new SymbolNameToFile())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamMethodFilter"/> class.
		/// </summary>
		/// <param name="generator"><see cref="DefaultParamGenerator"/> that created this filter.</param>
		/// <param name="fileNameProvider"><see cref="IFileNameProvider"/> that is used to create a hint name for the generated source.</param>
		public DefaultParamMethodFilter(DefaultParamGenerator generator, IFileNameProvider fileNameProvider)
		{
			Generator = generator;
			FileNameProvider = fileNameProvider;
		}

		/// <summary>
		/// Returns an array of <see cref="MethodDeclarationSyntax"/>s collected by the <see cref="Generator"/>'s <see cref="DefaultParamSyntaxReceiver"/> that can be filtrated by this filter.
		/// </summary>
		public MethodDeclarationSyntax[] GetCandidateMethods()
		{
			return Generator.SyntaxReceiver?.CandidateMethods?.ToArray() ?? Array.Empty<MethodDeclarationSyntax>();
		}

		/// <summary>
		/// Enumerates through all <see cref="MethodDeclarationSyntax"/>es returned by the <see cref="GetCandidateMethods"/> and returns an array of <see cref="DefaultParamMethodData"/>s created from the valid ones.
		/// </summary>
		public DefaultParamMethodData[] GetValidMethods()
		{
			if (Generator.SyntaxReceiver is null || Generator.TargetCompilation is null || Generator.SyntaxReceiver.CandidateMethods.Count == 0)
			{
				return Array.Empty<DefaultParamMethodData>();
			}

			return DefaultParamUtilities.IterateFilter<DefaultParamMethodData>(this);
		}

		/// <summary>
		/// Enumerates through all the <see cref="MethodDeclarationSyntax"/>es collected by the <paramref name="syntaxReceiver"/> and returns an array of <see cref="DefaultParamMethodData"/>s created from the valid ones.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="syntaxReceiver"><see cref="DefaultParamSyntaxReceiver"/> that collected the <see cref="MethodDeclarationSyntax"/>es.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
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

		/// <summary>
		/// Enumerates through all the <paramref name="collectedMethods"/> and returns an array of <see cref="DefaultParamMethodData"/>s created from the valid ones.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="collectedMethods">A collection of <see cref="MethodDeclarationSyntax"/>es to validate.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
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

		/// <summary>
		/// Validates the specified <paramref name="declaration"/> and returns a new instance of <see cref="DefaultParamMethodData"/> if the validation was a success.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="declaration"><see cref="DefaultParamMethodData"/> to validate.</param>
		/// <param name="data">Newly-created instance of <see cref="DefaultParamMethodData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static bool ValidateAndCreate(
			DefaultParamCompilationData compilation,
			MethodDeclarationSyntax declaration,
			[NotNullWhen(true)] out DefaultParamMethodData? data,
			CancellationToken cancellationToken = default
		)
		{
			if (!GetValidationData(compilation, declaration, out SemanticModel? semanticModel, out IMethodSymbol? symbol, out TypeParameterContainer typeParameters, cancellationToken))
			{
				data = null;
				return false;
			}

			return ValidateAndCreate(compilation, declaration, semanticModel, symbol, in typeParameters, out data, cancellationToken);
		}

		/// <summary>
		/// Validates the specified <paramref name="declaration"/> and returns a new instance of <see cref="DefaultParamMethodData"/> if the validation was a success.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="declaration"><see cref="DefaultParamMethodData"/> to validate.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="symbol"><see cref="IMethodSymbol"/> created from the <paramref name="declaration"/>.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="declaration"/>'s type parameters.</param>
		/// <param name="data">Newly-created instance of <see cref="DefaultParamMethodData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static bool ValidateAndCreate(
			DefaultParamCompilationData compilation,
			MethodDeclarationSyntax declaration,
			SemanticModel semanticModel,
			IMethodSymbol symbol,
			in TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out DefaultParamMethodData? data,
			CancellationToken cancellationToken = default
		)
		{
			if (AnalyzeAgaintsPartialOrExtern(symbol, declaration) &&
				AnalyzeAgaintsProhibitedAttributes(symbol, compilation, out AttributeData[]? attributes) &&
				AnalyzeContainingTypes(symbol, compilation, out ITypeData[]? containingTypes))
			{
				TypeParameterContainer combinedParameters = typeParameters;

				if (AnalyzeBaseMethodAndTypeParameters(symbol, ref combinedParameters, compilation, cancellationToken) &&
					AnalyzeMethodSignature(symbol, in combinedParameters, compilation, out HashSet<int>? newModifiers, cancellationToken))
				{
					data = new(
						declaration,
						compilation,
						symbol,
						semanticModel,
						containingTypes,
						null,
						attributes,
						in combinedParameters,
						newModifiers,
						CheckShouldCallInsteadOfCopying(attributes!, compilation)
					);

					return true;
				}
			}

			data = null;
			return false;
		}

		/// <summary>
		/// Specifies, if the <see cref="SemanticModel"/>, <see cref="IMethodSymbol"/> and <see cref="TypeParameterContainer"/> can be created from the given <paramref name="declaration"/>.
		/// If so, returns them.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="declaration"><see cref="MethodDeclarationSyntax"/> to validate.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="symbol"><see cref="IMethodSymbol"/> created from the <paramref name="declaration"/>.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="declaration"/>'s type parameters.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static bool GetValidationData(
			DefaultParamCompilationData compilation,
			MethodDeclarationSyntax declaration,
			[NotNullWhen(true)] out SemanticModel? semanticModel,
			[NotNullWhen(true)] out IMethodSymbol? symbol,
			out TypeParameterContainer typeParameters,
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
			in TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			bool isValid = ValidateAndCreate(compilation, (MethodDeclarationSyntax)node, semanticModel, (IMethodSymbol)symbol, in typeParameters, out DefaultParamMethodData? d, cancellationToken);
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
			in TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			bool isValid = WithDiagnostics.ValidateAndCreate(diagnosticReceiver, compilation, (MethodDeclarationSyntax)node, semanticModel, (IMethodSymbol)symbol, in typeParameters, out DefaultParamMethodData? d, cancellationToken);
			data = d;
			return isValid;
		}

		bool IDefaultParamFilter.GetValidationData(
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			[NotNullWhen(true)] out SemanticModel? semanticModel,
			[NotNullWhen(true)] out ISymbol? symbol,
			out TypeParameterContainer typeParameters,
			CancellationToken cancellationToken
		)
		{
			bool isValid = GetValidationData(compilation, (MethodDeclarationSyntax)node, out semanticModel, out IMethodSymbol? s, out typeParameters, cancellationToken);
			symbol = s;
			return isValid;
		}

		#endregion
	}
}
