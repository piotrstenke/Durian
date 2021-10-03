// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Analysis.Cache;
using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Durian.Analysis.DefaultParam.DefaultParamAnalyzer;
using static Durian.Analysis.DefaultParam.DefaultParamMethodAnalyzer;

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// Filtrates and validates <see cref="MethodDeclarationBuilder"/>es collected by a <see cref="DefaultParamSyntaxReceiver"/>.
	/// </summary>
	public partial class DefaultParamMethodFilter : IDefaultParamFilter<DefaultParamMethodData>, IDefaultParamFilter, INodeProvider<MethodDeclarationSyntax>
	{
		/// <inheritdoc/>
		public DefaultParamGenerator Generator { get; }

		/// <inheritdoc/>
		public IHintNameProvider HintNameProvider { get; }

		/// <inheritdoc/>
		public bool IncludeGeneratedSymbols { get; } = true;

		/// <summary>
		/// <see cref="FilterMode"/> of this <see cref="DefaultParamMethodFilter"/>.
		/// </summary>
		public FilterMode Mode => Generator.LoggingConfiguration.CurrentFilterMode;

		IDurianSourceGenerator IGeneratorSyntaxFilter.Generator => Generator;

		/// <inheritdoc cref="DefaultParamMethodFilter(DefaultParamGenerator, IHintNameProvider)"/>
		public DefaultParamMethodFilter(DefaultParamGenerator generator) : this(generator, new SymbolNameToFile())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamMethodFilter"/> class.
		/// </summary>
		/// <param name="generator"><see cref="DefaultParamGenerator"/> that created this filter.</param>
		/// <param name="hintNameProvider"><see cref="IHintNameProvider"/> that is used to create a hint name for the generated source.</param>
		public DefaultParamMethodFilter(DefaultParamGenerator generator, IHintNameProvider hintNameProvider)
		{
			Generator = generator;
			HintNameProvider = hintNameProvider;
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
			typeParameters = GetTypeParameters(declaration, semanticModel, compilation, cancellationToken);

			if (TypeParametersAreValid(in typeParameters, declaration))
			{
				symbol = semanticModel.GetDeclaredSymbol(declaration, cancellationToken)!;

				return symbol is not null;
			}

			symbol = null!;
			return false;
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
		/// Enumerates through all the <see cref="MethodDeclarationSyntax"/>es collected by the <paramref name="syntaxReceiver"/> and returns an array of <see cref="DefaultParamMethodData"/>s created from the valid ones. If the target <see cref="DefaultParamMethodData"/> already exists in the specified <paramref name="cache"/>, includes it instead.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="syntaxReceiver"><see cref="DefaultParamSyntaxReceiver"/> that collected the <see cref="MethodDeclarationSyntax"/>es.</param>
		/// <param name="cache">Container of cached <see cref="DefaultParamMethodData"/>s.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static DefaultParamMethodData[] GetValidMethods(
			DefaultParamCompilationData compilation,
			DefaultParamSyntaxReceiver syntaxReceiver,
			in CachedData<DefaultParamMethodData> cache,
			CancellationToken cancellationToken = default
		)
		{
			if (compilation is null || syntaxReceiver is null || syntaxReceiver.CandidateMethods.Count == 0)
			{
				return Array.Empty<DefaultParamMethodData>();
			}

			return GetValidMethods_Internal(compilation, syntaxReceiver.CandidateMethods.ToArray(), cancellationToken, in cache);
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
		/// Enumerates through all the <paramref name="collectedMethods"/> and returns an array of <see cref="DefaultParamMethodData"/>s created from the valid ones. If the target <see cref="DefaultParamMethodData"/> already exists in the specified <paramref name="cache"/>, includes it instead.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="collectedMethods">A collection of <see cref="MethodDeclarationSyntax"/>es to validate.</param>
		/// <param name="cache">Container of cached <see cref="DefaultParamMethodData"/>s.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static DefaultParamMethodData[] GetValidMethods(
			DefaultParamCompilationData compilation,
			IEnumerable<MethodDeclarationSyntax> collectedMethods,
			in CachedData<DefaultParamMethodData> cache,
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

			return GetValidMethods_Internal(compilation, array, cancellationToken, in cache);
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
		/// Validates the specified <paramref name="declaration"/> and returns a new instance of <see cref="DefaultParamMethodData"/> if the validation was a success. If the target <see cref="DefaultParamMethodData"/> already exists in the specified <paramref name="cache"/>, includes it instead.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="declaration"><see cref="DefaultParamMethodData"/> to validate.</param>
		/// <param name="cache">Container of cached <see cref="DefaultParamMethodData"/>s.</param>
		/// <param name="data">Newly-created instance of <see cref="DefaultParamMethodData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static bool ValidateAndCreate(
			DefaultParamCompilationData compilation,
			MethodDeclarationSyntax declaration,
			in CachedData<DefaultParamMethodData> cache,
			[NotNullWhen(true)] out DefaultParamMethodData? data,
			CancellationToken cancellationToken = default
		)
		{
			if (cache.TryGetCachedValue(declaration.GetLocation().GetLineSpan(), out data))
			{
				return true;
			}

			return ValidateAndCreate(compilation, declaration, out data, cancellationToken);
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
			if (ShouldBeAnalyzed(symbol, compilation, in typeParameters, out TypeParameterContainer combinedTypeParameters, cancellationToken) &&
				AnalyzeAgainstInvalidMethodType(symbol) &&
				AnalyzeAgainstPartialOrExtern(symbol, declaration) &&
				AnalyzeAgainstProhibitedAttributes(symbol, compilation, out AttributeData[]? attributes) &&
				AnalyzeContainingTypes(symbol, compilation, out ITypeData[]? containingTypes)
			)
			{
				TypeParameterContainer combinedParameters = typeParameters;

				if (AnalyzeBaseMethodAndTypeParameters(symbol, ref combinedParameters, in combinedTypeParameters))
				{
					INamedTypeSymbol[] symbols = DefaultParamUtilities.TypeDatasToTypeSymbols(containingTypes);

					if (AnalyzeMethodSignature(symbol, in combinedParameters, compilation, out HashSet<int>? newModifiers, attributes, symbols, cancellationToken))
					{
						bool call = ShouldCallInsteadOfCopying(symbol, compilation, attributes!, symbols);
						string targetNamespace = GetTargetNamespace(symbol, compilation, attributes, symbols);

						data = new(
							declaration,
							compilation,
							symbol,
							semanticModel,
							in combinedParameters,
							call,
							targetNamespace,
							newModifiers,
							containingTypes,
							null,
							attributes
						);

						return true;
					}
				}
			}

			data = null;
			return false;
		}

		/// <summary>
		/// Validates the specified <paramref name="declaration"/> and returns a new instance of <see cref="DefaultParamMethodData"/> if the validation was a success. If the target <see cref="DefaultParamMethodData"/> already exists in the specified <paramref name="cache"/>, includes it instead.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="declaration"><see cref="DefaultParamMethodData"/> to validate.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="symbol"><see cref="IMethodSymbol"/> created from the <paramref name="declaration"/>.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="declaration"/>'s type parameters.</param>
		/// <param name="cache">Container of cached <see cref="DefaultParamMethodData"/>s.</param>
		/// <param name="data">Newly-created instance of <see cref="DefaultParamMethodData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static bool ValidateAndCreate(
			DefaultParamCompilationData compilation,
			MethodDeclarationSyntax declaration,
			SemanticModel semanticModel,
			IMethodSymbol symbol,
			in TypeParameterContainer typeParameters,
			in CachedData<DefaultParamMethodData> cache,
			[NotNullWhen(true)] out DefaultParamMethodData? data,
			CancellationToken cancellationToken = default
		)
		{
			if (cache.TryGetCachedValue(declaration.GetLocation().GetLineSpan(), out data))
			{
				return true;
			}

			return ValidateAndCreate(compilation, declaration, semanticModel, symbol, in typeParameters, out data, cancellationToken);
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
		/// Enumerates through all <see cref="MethodDeclarationSyntax"/>es returned by the <see cref="GetCandidateMethods"/> and returns an array of <see cref="DefaultParamMethodData"/>s created from the valid ones. If the target <see cref="DefaultParamMethodData"/> already exists in the specified <paramref name="cache"/>, includes it instead.
		/// </summary>
		/// <param name="cache">Container of cached <see cref="DefaultParamMethodData"/>s.</param>
		public DefaultParamMethodData[] GetValidMethods(in CachedData<DefaultParamMethodData> cache)
		{
			if (Generator.SyntaxReceiver is null || Generator.TargetCompilation is null || Generator.SyntaxReceiver.CandidateMethods.Count == 0)
			{
				return Array.Empty<DefaultParamMethodData>();
			}

			return DefaultParamUtilities.IterateFilter(this, in cache);
		}

		private static TypeParameterContainer GetTypeParameters(
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

		private static DefaultParamMethodData[] GetValidMethods_Internal(
			DefaultParamCompilationData compilation,
			MethodDeclarationSyntax[] collectedMethods,
			CancellationToken cancellationToken,
			in CachedData<DefaultParamMethodData> cache
		)
		{
			List<DefaultParamMethodData> list = new(collectedMethods.Length);

			foreach (MethodDeclarationSyntax decl in collectedMethods)
			{
				if (decl is null)
				{
					continue;
				}

				if (cache.TryGetCachedValue(decl.GetLocation().GetLineSpan(), out DefaultParamMethodData? data) ||
					ValidateAndCreate(compilation, decl, out data, cancellationToken))
				{
					list.Add(data!);
				}
			}

			return list.ToArray();
		}

		private static bool TypeParametersAreValid(in TypeParameterContainer typeParameters, MethodDeclarationSyntax declaration)
		{
			return typeParameters.HasDefaultParams || declaration.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword));
		}

		#region -Interface Implementations-

		IEnumerable<IMemberData> IGeneratorSyntaxFilter.Filtrate(in GeneratorExecutionContext context)
		{
			return GetValidMethods();
		}

		IEnumerable<IMemberData> ICachedGeneratorSyntaxFilter<IDefaultParamTarget>.Filtrate(in CachedGeneratorExecutionContext<IDefaultParamTarget> context)
		{
			return GetValidMethods();
		}

		IEnumerable<IMemberData> ISyntaxFilter.Filtrate(ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken)
		{
			return GetValidMethods(
				(DefaultParamCompilationData)compilation,
				(DefaultParamSyntaxReceiver)syntaxReceiver,
				cancellationToken
			);
		}

		IEnumerable<IMemberData> ISyntaxFilter.Filtrate(ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken)
		{
			return GetValidMethods(
				(DefaultParamCompilationData)compilation,
				collectedNodes.OfType<MethodDeclarationSyntax>(),
				cancellationToken
			);
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(
			IDiagnosticReceiver diagnosticReceiver,
			ICompilationData compilation,
			IDurianSyntaxReceiver syntaxReceiver,
			CancellationToken cancellationToken
		)
		{
			return WithDiagnostics.GetValidMethods(
				diagnosticReceiver,
				(DefaultParamCompilationData)compilation,
				(DefaultParamSyntaxReceiver)syntaxReceiver,
				cancellationToken
			);
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken)
		{
			return WithDiagnostics.GetValidMethods(
				diagnosticReceiver,
				(DefaultParamCompilationData)compilation,
				collectedNodes.OfType<MethodDeclarationSyntax>(),
				cancellationToken
			);
		}

		IEnumerable<IMemberData> ICachedGeneratorSyntaxFilter<DefaultParamMethodData>.Filtrate(in CachedGeneratorExecutionContext<DefaultParamMethodData> context)
		{
			return GetValidMethods(in context.GetCachedData());
		}

		IEnumerator<IMemberData> IGeneratorSyntaxFilter.GetEnumerator()
		{
			return DefaultParamUtilities.GetFilterEnumerator<DefaultParamMethodData>(this);
		}

		IEnumerator<IMemberData> ICachedGeneratorSyntaxFilter<DefaultParamMethodData>.GetEnumerator(in CachedGeneratorExecutionContext<DefaultParamMethodData> context)
		{
			ref readonly CachedData<DefaultParamMethodData> cache = ref context.GetCachedData();

			return DefaultParamUtilities.GetFilterEnumerator(this, in cache);
		}

		IEnumerator<IMemberData> ICachedGeneratorSyntaxFilter<IDefaultParamTarget>.GetEnumerator(in CachedGeneratorExecutionContext<IDefaultParamTarget> context)
		{
			ref readonly CachedData<IDefaultParamTarget> cache = ref context.GetCachedData();

			return DefaultParamUtilities.GetFilterEnumerator(this, in cache);
		}

		IEnumerable<CSharpSyntaxNode> INodeProvider.GetNodes()
		{
			return GetCandidateMethods();
		}

		IEnumerable<MethodDeclarationSyntax> INodeProvider<MethodDeclarationSyntax>.GetNodes()
		{
			return GetCandidateMethods();
		}

		bool IDefaultParamFilter<IDefaultParamTarget>.GetValidationData(
			CSharpSyntaxNode node,
			DefaultParamCompilationData compilation,
			[NotNullWhen(true)] out SemanticModel? semanticModel,
			[NotNullWhen(true)] out ISymbol? symbol,
			out TypeParameterContainer typeParameters,
			CancellationToken cancellationToken
		)
		{
			if (node is not MethodDeclarationSyntax method)
			{
				semanticModel = null;
				symbol = null;
				typeParameters = default;
				return false;
			}

			bool isValid = GetValidationData(
				compilation,
				method,
				out semanticModel,
				out IMethodSymbol? s,
				out typeParameters,
				cancellationToken
			);

			symbol = s;
			return isValid;
		}

		bool IDefaultParamFilter<DefaultParamMethodData>.GetValidationData(
			CSharpSyntaxNode node,
			DefaultParamCompilationData compilation,
			[NotNullWhen(true)] out SemanticModel? semanticModel,
			[NotNullWhen(true)] out ISymbol? symbol,
			out TypeParameterContainer typeParameters,
			CancellationToken cancellationToken
		)
		{
			if (node is not MethodDeclarationSyntax method)
			{
				semanticModel = null;
				symbol = null;
				typeParameters = default;
				return false;
			}

			bool isValid = GetValidationData(compilation, method, out semanticModel, out IMethodSymbol? s, out typeParameters, cancellationToken);
			symbol = s;
			return isValid;
		}

		bool INodeValidator<DefaultParamMethodData>.GetValidationData(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			[NotNullWhen(true)] out SemanticModel? semanticModel,
			[NotNullWhen(true)] out ISymbol? symbol,
			CancellationToken cancellationToken
		)
		{
			if (node is not MethodDeclarationSyntax method)
			{
				semanticModel = null;
				symbol = null;
				return false;
			}

			bool isValid = GetValidationData(
				(DefaultParamCompilationData)compilation,
				method,
				out semanticModel,
				out IMethodSymbol? s,
				out _,
				cancellationToken
			);

			symbol = s;
			return isValid;
		}

		bool INodeValidator<IDefaultParamTarget>.GetValidationData(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			[NotNullWhen(true)] out SemanticModel? semanticModel,
			[NotNullWhen(true)] out ISymbol? symbol,
			CancellationToken cancellationToken
		)
		{
			if (node is not MethodDeclarationSyntax method)
			{
				semanticModel = null;
				symbol = null;
				return false;
			}

			bool isValid = GetValidationData(
				(DefaultParamCompilationData)compilation,
				method,
				out semanticModel,
				out IMethodSymbol? s,
				out _,
				cancellationToken
			);

			symbol = s;
			return isValid;
		}

		bool IDefaultParamFilter<IDefaultParamTarget>.ValidateAndCreate(
			CSharpSyntaxNode node,
			DefaultParamCompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			in TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			if (node is not MethodDeclarationSyntax method || symbol is not IMethodSymbol s)
			{
				data = null;
				return false;
			}

			bool isValid = ValidateAndCreate(
				compilation,
				method,
				semanticModel,
				s,
				in typeParameters,
				out DefaultParamMethodData? d,
				cancellationToken
			);

			data = d;
			return isValid;
		}

		bool IDefaultParamFilter<IDefaultParamTarget>.ValidateAndCreate(
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			if (node is not MethodDeclarationSyntax method)
			{
				data = null;
				return false;
			}

			bool isValid = ValidateAndCreate(compilation, method, out DefaultParamMethodData? d, cancellationToken);
			data = d;
			return isValid;
		}

		bool IDefaultParamFilter<DefaultParamMethodData>.ValidateAndCreate(
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			[NotNullWhen(true)] out DefaultParamMethodData? data,
			CancellationToken cancellationToken
		)
		{
			if (node is not MethodDeclarationSyntax method)
			{
				data = null;
				return false;
			}

			return ValidateAndCreate(compilation, method, out data, cancellationToken);
		}

		bool IDefaultParamFilter<DefaultParamMethodData>.ValidateAndCreate(
			CSharpSyntaxNode node,
			DefaultParamCompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			in TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out DefaultParamMethodData? data,
			CancellationToken cancellationToken
		)
		{
			if (node is not MethodDeclarationSyntax method || symbol is not IMethodSymbol s)
			{
				data = null;
				return false;
			}

			return ValidateAndCreate(
				compilation,
				method,
				semanticModel,
				s,
				in typeParameters,
				out data,
				cancellationToken
			);
		}

		bool INodeValidator<DefaultParamMethodData>.ValidateAndCreate(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			[NotNullWhen(true)] out DefaultParamMethodData? data,
			CancellationToken cancellationToken
		)
		{
			if (node is not MethodDeclarationSyntax method)
			{
				data = null;
				return false;
			}

			return ValidateAndCreate((DefaultParamCompilationData)compilation, method, out data, cancellationToken);
		}

		bool INodeValidator<DefaultParamMethodData>.ValidateAndCreate(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			[NotNullWhen(true)] out DefaultParamMethodData? data,
			CancellationToken cancellationToken
		)
		{
			if (node is not MethodDeclarationSyntax method || symbol is not IMethodSymbol s)
			{
				data = null;
				return false;
			}

			DefaultParamCompilationData c = (DefaultParamCompilationData)compilation;
			TypeParameterContainer typeParameters = GetTypeParameters(method, semanticModel, c, cancellationToken);

			return ValidateAndCreate(
				c,
				method,
				semanticModel,
				s,
				in typeParameters,
				out data,
				cancellationToken
			);
		}

		bool INodeValidator<IDefaultParamTarget>.ValidateAndCreate(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			if (node is not MethodDeclarationSyntax method)
			{
				data = null;
				return false;
			}

			bool isValid = ValidateAndCreate(
				(DefaultParamCompilationData)compilation,
				method,
				out DefaultParamMethodData? d,
				cancellationToken
			);

			data = d;
			return isValid;
		}

		bool INodeValidator<IDefaultParamTarget>.ValidateAndCreate(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			if (node is not MethodDeclarationSyntax method || symbol is not IMethodSymbol s)
			{
				data = null;
				return false;
			}

			DefaultParamCompilationData c = (DefaultParamCompilationData)compilation;
			TypeParameterContainer typeParameters = GetTypeParameters(method, semanticModel, c, cancellationToken);

			bool isValid = ValidateAndCreate(
				c,
				method,
				semanticModel,
				s,
				in typeParameters,
				out DefaultParamMethodData? d,
				cancellationToken
			);

			data = d;
			return isValid;
		}

		bool IDefaultParamFilter<IDefaultParamTarget>.ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			CSharpSyntaxNode node,
			DefaultParamCompilationData compilation,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			if (node is not MethodDeclarationSyntax method)
			{
				data = null;
				return false;
			}

			bool isValid = WithDiagnostics.ValidateAndCreate(
				diagnosticReceiver,
				compilation,
				method,
				out DefaultParamMethodData? d,
				cancellationToken
			);

			data = d;
			return isValid;
		}

		bool IDefaultParamFilter<IDefaultParamTarget>.ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			CSharpSyntaxNode node,
			DefaultParamCompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			in TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			if (node is not MethodDeclarationSyntax method || symbol is not IMethodSymbol s)
			{
				data = null;
				return false;
			}

			bool isValid = WithDiagnostics.ValidateAndCreate(
				diagnosticReceiver,
				compilation,
				method,
				semanticModel,
				s,
				in typeParameters,
				out DefaultParamMethodData? d,
				cancellationToken
			);

			data = d;
			return isValid;
		}

		bool IDefaultParamFilter<DefaultParamMethodData>.ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			CSharpSyntaxNode node,
			DefaultParamCompilationData compilation,
			[NotNullWhen(true)] out DefaultParamMethodData? data,
			CancellationToken cancellationToken
		)
		{
			if (node is not MethodDeclarationSyntax method)
			{
				data = null;
				return false;
			}

			return WithDiagnostics.ValidateAndCreate(
				diagnosticReceiver,
				compilation,
				method,
				out data,
				cancellationToken
			);
		}

		bool IDefaultParamFilter<DefaultParamMethodData>.ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			CSharpSyntaxNode node,
			DefaultParamCompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			in TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out DefaultParamMethodData? data,
			CancellationToken cancellationToken
		)
		{
			if (node is not MethodDeclarationSyntax method || symbol is not IMethodSymbol s)
			{
				data = null;
				return false;
			}

			return WithDiagnostics.ValidateAndCreate(
				diagnosticReceiver,
				compilation,
				method,
				semanticModel,
				s,
				in typeParameters,
				out data,
				cancellationToken
			);
		}

		bool INodeValidatorWithDiagnostics<DefaultParamMethodData>.ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			CSharpSyntaxNode node,
			ICompilationData compilation,
			[NotNullWhen(true)] out DefaultParamMethodData? data,
			CancellationToken cancellationToken
		)
		{
			if (node is not MethodDeclarationSyntax method)
			{
				data = null;
				return false;
			}

			return WithDiagnostics.ValidateAndCreate(
				diagnosticReceiver,
				(DefaultParamCompilationData)compilation,
				method,
				out data,
				cancellationToken
			);
		}

		bool INodeValidatorWithDiagnostics<DefaultParamMethodData>.ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			CSharpSyntaxNode node,
			ICompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			[NotNullWhen(true)] out DefaultParamMethodData? data,
			CancellationToken cancellationToken
		)
		{
			if (node is not MethodDeclarationSyntax method || symbol is not IMethodSymbol s)
			{
				data = null;
				return false;
			}

			DefaultParamCompilationData c = (DefaultParamCompilationData)compilation;
			TypeParameterContainer typeParameters = GetTypeParameters(method, semanticModel, c, cancellationToken);

			return WithDiagnostics.ValidateAndCreate(
				diagnosticReceiver,
				(DefaultParamCompilationData)compilation,
				method,
				semanticModel,
				s,
				in typeParameters,
				out data,
				cancellationToken
			);
		}

		bool INodeValidatorWithDiagnostics<IDefaultParamTarget>.ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			CSharpSyntaxNode node,
			ICompilationData compilation,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			if (node is not MethodDeclarationSyntax method)
			{
				data = null;
				return false;
			}

			bool isValid = WithDiagnostics.ValidateAndCreate(
				diagnosticReceiver,
				(DefaultParamCompilationData)compilation,
				method,
				out DefaultParamMethodData? d,
				cancellationToken
			);

			data = d;
			return isValid;
		}

		bool INodeValidatorWithDiagnostics<IDefaultParamTarget>.ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			CSharpSyntaxNode node,
			ICompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			if (node is not MethodDeclarationSyntax method || symbol is not IMethodSymbol s)
			{
				data = null;
				return false;
			}

			DefaultParamCompilationData c = (DefaultParamCompilationData)compilation;
			TypeParameterContainer typeParameters = GetTypeParameters(method, semanticModel, c, cancellationToken);

			bool isValid = WithDiagnostics.ValidateAndCreate(
				diagnosticReceiver,
				c,
				method,
				semanticModel,
				s,
				in typeParameters,
				out DefaultParamMethodData? d,
				cancellationToken
			);

			data = d;
			return isValid;
		}

		#endregion -Interface Implementations-
	}
}
