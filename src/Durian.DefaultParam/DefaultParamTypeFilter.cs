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
using static Durian.Analysis.DefaultParam.DefaultParamTypeAnalyzer;

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// Filtrates and validates <see cref="TypeDeclarationSyntax"/>es collected by a <see cref="DefaultParamSyntaxReceiver"/>.
	/// </summary>
	public sealed partial class DefaultParamTypeFilter : IDefaultParamFilter<DefaultParamTypeData>, IDefaultParamFilter, INodeProvider<TypeDeclarationSyntax>
	{
		/// <inheritdoc/>
		public DefaultParamGenerator Generator { get; }

		/// <inheritdoc/>
		public IHintNameProvider HintNameProvider { get; }

		/// <inheritdoc/>
		public bool IncludeGeneratedSymbols => true;

		/// <summary>
		/// <see cref="FilterMode"/> of this <see cref="DefaultParamTypeFilter"/>.
		/// </summary>
		public FilterMode Mode => Generator.LoggingConfiguration.CurrentFilterMode;

		IDurianSourceGenerator IGeneratorSyntaxFilter.Generator => Generator;

		/// <inheritdoc cref="DefaultParamTypeFilter(DefaultParamGenerator, IHintNameProvider)"/>
		public DefaultParamTypeFilter(DefaultParamGenerator generator) : this(generator, new SymbolNameToFile())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamTypeFilter"/> class.
		/// </summary>
		/// <param name="generator"><see cref="DefaultParamGenerator"/> that created this filter.</param>
		/// <param name="fileNameProvider"><see cref="IHintNameProvider"/> that is used to create a hint name for the generated source.</param>
		public DefaultParamTypeFilter(DefaultParamGenerator generator, IHintNameProvider fileNameProvider)
		{
			Generator = generator;
			HintNameProvider = fileNameProvider;
		}

		/// <summary>
		/// Specifies, if the <see cref="SemanticModel"/>, <see cref="INamedTypeSymbol"/> and <see cref="TypeParameterContainer"/> can be created from the given <paramref name="declaration"/>.
		/// If so, returns them.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="declaration"><see cref="TypeDeclarationSyntax"/> to validate.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> created from the <paramref name="declaration"/>.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="declaration"/>'s type parameters.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static bool GetValidationData(
			DefaultParamCompilationData compilation,
			TypeDeclarationSyntax declaration,
			[NotNullWhen(true)] out SemanticModel? semanticModel,
			[NotNullWhen(true)] out INamedTypeSymbol? symbol,
			out TypeParameterContainer typeParameters,
			CancellationToken cancellationToken = default
		)
		{
			semanticModel = compilation.Compilation.GetSemanticModel(declaration.SyntaxTree);
			typeParameters = GetTypeParameters(declaration, semanticModel, compilation, cancellationToken);

			if (!typeParameters.HasDefaultParams)
			{
				symbol = null!;
				return false;
			}

			symbol = semanticModel.GetDeclaredSymbol(declaration, cancellationToken)!;

			return symbol is not null;
		}

		/// <summary>
		/// Enumerates through all the <paramref name="collectedTypes"/> and returns an array of <see cref="DefaultParamTypeData"/>s created from the valid ones.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="collectedTypes">A collection of <see cref="TypeDeclarationSyntax"/>es to validate.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static DefaultParamTypeData[] GetValidTypes(
			DefaultParamCompilationData compilation,
			IEnumerable<TypeDeclarationSyntax> collectedTypes,
			CancellationToken cancellationToken = default
		)
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

		/// <summary>
		/// Enumerates through all the <paramref name="collectedTypes"/> and returns an array of <see cref="DefaultParamTypeData"/>s created from the valid ones. If the target <see cref="DefaultParamTypeData"/> already exists in the specified <paramref name="cache"/>, includes it instead.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="collectedTypes">A collection of <see cref="TypeDeclarationSyntax"/>es to validate.</param>
		/// <param name="cache">Container of cached <see cref="DefaultParamTypeData"/>s.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static DefaultParamTypeData[] GetValidTypes(
			DefaultParamCompilationData compilation,
			IEnumerable<TypeDeclarationSyntax> collectedTypes,
			in CachedData<DefaultParamTypeData> cache,
			CancellationToken cancellationToken = default
		)
		{
			if (compilation is null || collectedTypes is null)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			TypeDeclarationSyntax[] array = collectedTypes.ToArray();

			if (array.Length == 0)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			return GetValidTypes_Internal(compilation, array, cancellationToken, in cache);
		}

		/// <summary>
		/// Enumerates through all the <see cref="TypeDeclarationSyntax"/>es collected by the <paramref name="syntaxReceiver"/> and returns an array of <see cref="DefaultParamTypeData"/>s created from the valid ones.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="syntaxReceiver"><see cref="DefaultParamSyntaxReceiver"/> that collected the <see cref="TypeDeclarationSyntax"/>es.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static DefaultParamTypeData[] GetValidTypes(
			DefaultParamCompilationData compilation,
			DefaultParamSyntaxReceiver syntaxReceiver,
			CancellationToken cancellationToken = default
		)
		{
			if (compilation is null || syntaxReceiver is null || syntaxReceiver.CandidateTypes.Count == 0)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			return GetValidTypes_Internal(compilation, syntaxReceiver.CandidateTypes.ToArray(), cancellationToken);
		}

		/// <summary>
		/// Enumerates through all the <see cref="TypeDeclarationSyntax"/>es collected by the <paramref name="syntaxReceiver"/> and returns an array of <see cref="DefaultParamTypeData"/>s created from the valid ones. If the target <see cref="DefaultParamTypeData"/> already exists in the specified <paramref name="cache"/>, includes it instead.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="syntaxReceiver"><see cref="DefaultParamSyntaxReceiver"/> that collected the <see cref="TypeDeclarationSyntax"/>es.</param>
		/// <param name="cache">Container of cached <see cref="DefaultParamTypeData"/>s.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static DefaultParamTypeData[] GetValidTypes(
			DefaultParamCompilationData compilation,
			DefaultParamSyntaxReceiver syntaxReceiver,
			in CachedData<DefaultParamTypeData> cache,
			CancellationToken cancellationToken = default
		)
		{
			if (compilation is null || syntaxReceiver is null || syntaxReceiver.CandidateTypes.Count == 0)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			return GetValidTypes_Internal(compilation, syntaxReceiver.CandidateTypes.ToArray(), cancellationToken, in cache);
		}

		/// <summary>
		/// Validates the specified <paramref name="declaration"/> and returns a new instance of <see cref="DefaultParamTypeData"/> if the validation was a success.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="declaration"><see cref="DefaultParamTypeData"/> to validate.</param>
		/// <param name="data">Newly-created instance of <see cref="DefaultParamTypeData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static bool ValidateAndCreate(
			DefaultParamCompilationData compilation,
			TypeDeclarationSyntax declaration,
			[NotNullWhen(true)] out DefaultParamTypeData? data,
			CancellationToken cancellationToken = default
		)
		{
			if (!GetValidationData(compilation, declaration, out SemanticModel? semanticModel, out INamedTypeSymbol? symbol, out TypeParameterContainer typeParameters, cancellationToken))
			{
				data = null;
				return false;
			}

			return ValidateAndCreate(compilation, declaration, semanticModel, symbol, in typeParameters, out data, cancellationToken);
		}

		/// <summary>
		/// Validates the specified <paramref name="declaration"/> and returns a new instance of <see cref="DefaultParamTypeData"/> if the validation was a success. If the target <see cref="DefaultParamTypeData"/> already exists in the specified <paramref name="cache"/>, includes it instead.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="declaration"><see cref="DefaultParamTypeData"/> to validate.</param>
		/// <param name="cache">Container of cached <see cref="DefaultParamTypeData"/>s.</param>
		/// <param name="data">Newly-created instance of <see cref="DefaultParamTypeData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static bool ValidateAndCreate(
			DefaultParamCompilationData compilation,
			TypeDeclarationSyntax declaration,
			in CachedData<DefaultParamTypeData> cache,
			[NotNullWhen(true)] out DefaultParamTypeData? data,
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
		/// Validates the specified <paramref name="declaration"/> and returns a new instance of <see cref="DefaultParamTypeData"/> if the validation was a success.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="declaration"><see cref="DefaultParamTypeData"/> to validate.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> created from the <paramref name="declaration"/>.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="declaration"/>'s type parameters.</param>
		/// <param name="data">Newly-created instance of <see cref="DefaultParamTypeData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static bool ValidateAndCreate(
			DefaultParamCompilationData compilation,
			TypeDeclarationSyntax declaration,
			SemanticModel semanticModel,
			INamedTypeSymbol symbol,
			in TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out DefaultParamTypeData? data,
			CancellationToken cancellationToken = default
		)
		{
			if (AnalyzeAgainstPartial(symbol, cancellationToken) &&
				AnalyzeAgainstProhibitedAttributes(symbol, compilation, out AttributeData[]? attributes) &&
				AnalyzeContainingTypes(symbol, compilation, out ITypeData[]? containingTypes) &&
				AnalyzeTypeParameters(symbol, in typeParameters)
			)
			{
				INamedTypeSymbol[] symbols = DefaultParamUtilities.TypeDatasToTypeSymbols(containingTypes);
				string targetNamespace = GetTargetNamespace(symbol, attributes, symbols, compilation);

				if (AnalyzeCollidingMembers(symbol, in typeParameters, compilation, targetNamespace, attributes, symbols, out HashSet<int>? applyNewModifiers, cancellationToken))
				{
					bool inherit = ShouldInheritInsteadOfCopying(symbol, compilation, attributes, symbols);

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
						typeParameters,
						applyNewModifiers,
						inherit,
						targetNamespace
					);

					return true;
				}
			}

			data = null;
			return false;
		}

		/// <summary>
		/// Validates the specified <paramref name="declaration"/> and returns a new instance of <see cref="DefaultParamTypeData"/> if the validation was a success. If the target <see cref="DefaultParamTypeData"/> already exists in the specified <paramref name="cache"/>, includes it instead.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="declaration"><see cref="DefaultParamTypeData"/> to validate.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> created from the <paramref name="declaration"/>.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="declaration"/>'s type parameters.</param>
		/// <param name="cache">Container of cached <see cref="DefaultParamTypeData"/>s.</param>
		/// <param name="data">Newly-created instance of <see cref="DefaultParamTypeData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static bool ValidateAndCreate(
			DefaultParamCompilationData compilation,
			TypeDeclarationSyntax declaration,
			SemanticModel semanticModel,
			INamedTypeSymbol symbol,
			in TypeParameterContainer typeParameters,
			in CachedData<DefaultParamTypeData> cache,
			[NotNullWhen(true)] out DefaultParamTypeData? data,
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
		/// Returns an array of <see cref="TypeDeclarationSyntax"/>s collected by the <see cref="Generator"/>'s <see cref="DefaultParamSyntaxReceiver"/> that can be filtrated by this filter.
		/// </summary>
		public TypeDeclarationSyntax[] GetCandidateTypes()
		{
			return Generator.SyntaxReceiver?.CandidateTypes?.ToArray() ?? Array.Empty<TypeDeclarationSyntax>();
		}

		/// <summary>
		/// Enumerates through all <see cref="TypeDeclarationSyntax"/>es returned by the <see cref="GetCandidateTypes"/> and returns an array of <see cref="DefaultParamTypeData"/>s created from the valid ones.
		/// </summary>
		public DefaultParamTypeData[] GetValidTypes()
		{
			if (Generator.SyntaxReceiver is null || Generator.TargetCompilation is null || Generator.SyntaxReceiver.CandidateTypes.Count == 0)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			return DefaultParamUtilities.IterateFilter<DefaultParamTypeData>(this);
		}

		/// <summary>
		/// Enumerates through all <see cref="MethodDeclarationSyntax"/>es returned by the <see cref="GetCandidateTypes"/> and returns an array of <see cref="DefaultParamTypeData"/>s created from the valid ones. If the target <see cref="DefaultParamTypeData"/> already exists in the specified <paramref name="cache"/>, includes it instead.
		/// </summary>
		/// <param name="cache">Container of cached <see cref="DefaultParamTypeData"/>s.</param>
		public DefaultParamTypeData[] GetValidTypes(in CachedData<DefaultParamTypeData> cache)
		{
			if (Generator.SyntaxReceiver is null || Generator.TargetCompilation is null || Generator.SyntaxReceiver.CandidateTypes.Count == 0)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			return DefaultParamUtilities.IterateFilter(this, in cache);
		}

		private static TypeParameterContainer GetTypeParameters(
			TypeDeclarationSyntax declaration,
			SemanticModel semanticModel,
			DefaultParamCompilationData compilation,
			CancellationToken cancellationToken
		)
		{
			TypeParameterListSyntax? parameters = declaration.TypeParameterList;

			if (parameters is null)
			{
				return new TypeParameterContainer(null);
			}

			return new TypeParameterContainer(parameters.Parameters.Select(p => TypeParameterData.CreateFrom(p, semanticModel, compilation, cancellationToken)));
		}

		private static DefaultParamTypeData[] GetValidTypes_Internal(
					DefaultParamCompilationData compilation,
			TypeDeclarationSyntax[] collectedTypes,
			CancellationToken cancellationToken
		)
		{
			List<DefaultParamTypeData> list = new(collectedTypes.Length);

			foreach (TypeDeclarationSyntax decl in collectedTypes)
			{
				if (decl is null)
				{
					continue;
				}

				if (ValidateAndCreate(compilation, decl, out DefaultParamTypeData? data, cancellationToken))
				{
					list.Add(data!);
				}
			}

			return list.ToArray();
		}

		private static DefaultParamTypeData[] GetValidTypes_Internal(
			DefaultParamCompilationData compilation,
			TypeDeclarationSyntax[] collectedTypes,
			CancellationToken cancellationToken,
			in CachedData<DefaultParamTypeData> cache
		)
		{
			List<DefaultParamTypeData> list = new(collectedTypes.Length);

			foreach (TypeDeclarationSyntax decl in collectedTypes)
			{
				if (decl is null)
				{
					continue;
				}

				if (cache.TryGetCachedValue(decl.GetLocation().GetLineSpan(), out DefaultParamTypeData? data) ||
					ValidateAndCreate(compilation, decl, out data, cancellationToken))
				{
					list.Add(data!);
				}
			}

			return list.ToArray();
		}

		#region -Interface Implementations-

		IEnumerable<IMemberData> IGeneratorSyntaxFilter.Filtrate(in GeneratorExecutionContext context)
		{
			return GetValidTypes();
		}

		IEnumerable<IMemberData> ICachedGeneratorSyntaxFilter<IDefaultParamTarget>.Filtrate(in CachedGeneratorExecutionContext<IDefaultParamTarget> context)
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
			return WithDiagnostics.GetValidTypes(diagnosticReceiver, (DefaultParamCompilationData)compilation, (DefaultParamSyntaxReceiver)syntaxReceiver, cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken)
		{
			return WithDiagnostics.GetValidTypes(diagnosticReceiver, (DefaultParamCompilationData)compilation, collectedNodes.OfType<TypeDeclarationSyntax>(), cancellationToken);
		}

		IEnumerable<IMemberData> ICachedGeneratorSyntaxFilter<DefaultParamTypeData>.Filtrate(in CachedGeneratorExecutionContext<DefaultParamTypeData> context)
		{
			return GetValidTypes(in context.GetCachedData());
		}

		IEnumerator<IMemberData> IGeneratorSyntaxFilter.GetEnumerator()
		{
			return DefaultParamUtilities.GetFilterEnumerator<DefaultParamTypeData>(this);
		}

		IEnumerator<IMemberData> ICachedGeneratorSyntaxFilter<DefaultParamTypeData>.GetEnumerator(in CachedGeneratorExecutionContext<DefaultParamTypeData> context)
		{
			ref readonly CachedData<DefaultParamTypeData> cache = ref context.GetCachedData();

			return DefaultParamUtilities.GetFilterEnumerator(this, in cache);
		}

		IEnumerator<IMemberData> ICachedGeneratorSyntaxFilter<IDefaultParamTarget>.GetEnumerator(in CachedGeneratorExecutionContext<IDefaultParamTarget> context)
		{
			ref readonly CachedData<IDefaultParamTarget> cache = ref context.GetCachedData();

			return DefaultParamUtilities.GetFilterEnumerator(this, in cache);
		}

		IEnumerable<CSharpSyntaxNode> INodeProvider.GetNodes()
		{
			return GetCandidateTypes();
		}

		IEnumerable<TypeDeclarationSyntax> INodeProvider<TypeDeclarationSyntax>.GetNodes()
		{
			return GetCandidateTypes();
		}

		bool IDefaultParamFilter<IDefaultParamTarget>.GetValidationData(CSharpSyntaxNode node, DefaultParamCompilationData compilation, [NotNullWhen(true)] out SemanticModel? semanticModel, [NotNullWhen(true)] out ISymbol? symbol, out TypeParameterContainer typeParameters, CancellationToken cancellationToken)
		{
			if (node is not TypeDeclarationSyntax type)
			{
				semanticModel = null;
				symbol = null;
				typeParameters = default;
				return false;
			}

			bool isValid = GetValidationData(compilation, type, out semanticModel, out INamedTypeSymbol? s, out typeParameters, cancellationToken);
			symbol = s;
			return isValid;
		}

		bool IDefaultParamFilter<DefaultParamTypeData>.GetValidationData(CSharpSyntaxNode node, DefaultParamCompilationData compilation, [NotNullWhen(true)] out SemanticModel? semanticModel, [NotNullWhen(true)] out ISymbol? symbol, out TypeParameterContainer typeParameters, CancellationToken cancellationToken)
		{
			if (node is not TypeDeclarationSyntax type)
			{
				semanticModel = null;
				symbol = null;
				typeParameters = default;
				return false;
			}

			bool isValid = GetValidationData(compilation, type, out semanticModel, out INamedTypeSymbol? s, out typeParameters, cancellationToken);
			symbol = s;
			return isValid;
		}

		bool INodeValidator<DefaultParamTypeData>.GetValidationData(CSharpSyntaxNode node, ICompilationData compilation, [NotNullWhen(true)] out SemanticModel? semanticModel, [NotNullWhen(true)] out ISymbol? symbol, CancellationToken cancellationToken)
		{
			if (node is not TypeDeclarationSyntax type)
			{
				semanticModel = null;
				symbol = null;
				return false;
			}

			bool isValid = GetValidationData((DefaultParamCompilationData)compilation, type, out semanticModel, out INamedTypeSymbol? s, out _, cancellationToken);

			symbol = s;
			return isValid;
		}

		bool INodeValidator<IDefaultParamTarget>.GetValidationData(CSharpSyntaxNode node, ICompilationData compilation, [NotNullWhen(true)] out SemanticModel? semanticModel, [NotNullWhen(true)] out ISymbol? symbol, CancellationToken cancellationToken)
		{
			if (node is not TypeDeclarationSyntax type)
			{
				semanticModel = null;
				symbol = null;
				return false;
			}

			bool isValid = GetValidationData((DefaultParamCompilationData)compilation, type, out semanticModel, out INamedTypeSymbol? s, out _, cancellationToken);

			symbol = s;
			return isValid;
		}

		bool IDefaultParamFilter<IDefaultParamTarget>.ValidateAndCreate(CSharpSyntaxNode node, DefaultParamCompilationData compilation, SemanticModel semanticModel, ISymbol symbol, in TypeParameterContainer typeParameters, [NotNullWhen(true)] out IDefaultParamTarget? data, CancellationToken cancellationToken)
		{
			if (node is not TypeDeclarationSyntax type || symbol is not INamedTypeSymbol s)
			{
				data = null;
				return false;
			}

			bool isValid = ValidateAndCreate(compilation, type, semanticModel, s, in typeParameters, out DefaultParamTypeData? d, cancellationToken);
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
			if (node is not TypeDeclarationSyntax type)
			{
				data = null;
				return false;
			}

			bool isValid = ValidateAndCreate(compilation, type, out DefaultParamTypeData? d, cancellationToken);
			data = d;
			return isValid;
		}

		bool IDefaultParamFilter<DefaultParamTypeData>.ValidateAndCreate(DefaultParamCompilationData compilation, CSharpSyntaxNode node, [NotNullWhen(true)] out DefaultParamTypeData? data, CancellationToken cancellationToken)
		{
			if (node is not TypeDeclarationSyntax type)
			{
				data = null;
				return false;
			}

			return ValidateAndCreate(compilation, type, out data, cancellationToken);
		}

		bool IDefaultParamFilter<DefaultParamTypeData>.ValidateAndCreate(CSharpSyntaxNode node, DefaultParamCompilationData compilation, SemanticModel semanticModel, ISymbol symbol, in TypeParameterContainer typeParameters, [NotNullWhen(true)] out DefaultParamTypeData? data, CancellationToken cancellationToken)
		{
			if (node is not TypeDeclarationSyntax type || symbol is not INamedTypeSymbol s)
			{
				data = null;
				return false;
			}

			return ValidateAndCreate(compilation, type, semanticModel, s, in typeParameters, out data, cancellationToken);
		}

		bool INodeValidator<DefaultParamTypeData>.ValidateAndCreate(CSharpSyntaxNode node, ICompilationData compilation, [NotNullWhen(true)] out DefaultParamTypeData? data, CancellationToken cancellationToken)
		{
			if (node is not TypeDeclarationSyntax type)
			{
				data = null;
				return false;
			}

			return ValidateAndCreate((DefaultParamCompilationData)compilation, type, out data, cancellationToken);
		}

		bool INodeValidator<DefaultParamTypeData>.ValidateAndCreate(CSharpSyntaxNode node, ICompilationData compilation, SemanticModel semanticModel, ISymbol symbol, [NotNullWhen(true)] out DefaultParamTypeData? data, CancellationToken cancellationToken)
		{
			if (node is not TypeDeclarationSyntax type || symbol is not INamedTypeSymbol s)
			{
				data = null;
				return false;
			}

			DefaultParamCompilationData c = (DefaultParamCompilationData)compilation;
			TypeParameterContainer typeParameters = GetTypeParameters(type, semanticModel, c, cancellationToken);

			if (!typeParameters.HasDefaultParams)
			{
				data = null;
				return false;
			}

			return ValidateAndCreate(c, type, semanticModel, s, in typeParameters, out data, cancellationToken);
		}

		bool INodeValidator<IDefaultParamTarget>.ValidateAndCreate(CSharpSyntaxNode node, ICompilationData compilation, [NotNullWhen(true)] out IDefaultParamTarget? data, CancellationToken cancellationToken)
		{
			if (node is not TypeDeclarationSyntax type)
			{
				data = null;
				return false;
			}

			bool isValid = ValidateAndCreate((DefaultParamCompilationData)compilation, type, out DefaultParamTypeData? d, cancellationToken);
			data = d;
			return isValid;
		}

		bool INodeValidator<IDefaultParamTarget>.ValidateAndCreate(CSharpSyntaxNode node, ICompilationData compilation, SemanticModel semanticModel, ISymbol symbol, [NotNullWhen(true)] out IDefaultParamTarget? data, CancellationToken cancellationToken)
		{
			if (node is not TypeDeclarationSyntax type || symbol is not INamedTypeSymbol s)
			{
				data = null;
				return false;
			}

			DefaultParamCompilationData c = (DefaultParamCompilationData)compilation;
			TypeParameterContainer typeParameters = GetTypeParameters(type, semanticModel, c, cancellationToken);

			if (!typeParameters.HasDefaultParams)
			{
				data = null;
				return false;
			}

			bool isValid = ValidateAndCreate(c, type, semanticModel, s, in typeParameters, out DefaultParamTypeData? d, cancellationToken);
			data = d;
			return isValid;
		}

		bool IDefaultParamFilter<IDefaultParamTarget>.ValidateAndCreateWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, CSharpSyntaxNode node, DefaultParamCompilationData compilation, [NotNullWhen(true)] out IDefaultParamTarget? data, CancellationToken cancellationToken)
		{
			if (node is not TypeDeclarationSyntax type)
			{
				data = null;
				return false;
			}

			bool isValid = WithDiagnostics.ValidateAndCreate(diagnosticReceiver, compilation, type, out DefaultParamTypeData? d, cancellationToken);
			data = d;
			return isValid;
		}

		bool IDefaultParamFilter<IDefaultParamTarget>.ValidateAndCreateWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, CSharpSyntaxNode node, DefaultParamCompilationData compilation, SemanticModel semanticModel, ISymbol symbol, in TypeParameterContainer typeParameters, [NotNullWhen(true)] out IDefaultParamTarget? data, CancellationToken cancellationToken)
		{
			if (node is not TypeDeclarationSyntax type || symbol is not INamedTypeSymbol s)
			{
				data = null;
				return false;
			}

			bool isValid = WithDiagnostics.ValidateAndCreate(diagnosticReceiver, compilation, type, semanticModel, s, in typeParameters, out DefaultParamTypeData? d, cancellationToken);
			data = d;
			return isValid;
		}

		bool IDefaultParamFilter<DefaultParamTypeData>.ValidateAndCreateWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, CSharpSyntaxNode node, DefaultParamCompilationData compilation, [NotNullWhen(true)] out DefaultParamTypeData? data, CancellationToken cancellationToken)
		{
			if (node is not TypeDeclarationSyntax type)
			{
				data = null;
				return false;
			}

			return WithDiagnostics.ValidateAndCreate(diagnosticReceiver, compilation, type, out data, cancellationToken);
		}

		bool IDefaultParamFilter<DefaultParamTypeData>.ValidateAndCreateWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, CSharpSyntaxNode node, DefaultParamCompilationData compilation, SemanticModel semanticModel, ISymbol symbol, in TypeParameterContainer typeParameters, [NotNullWhen(true)] out DefaultParamTypeData? data, CancellationToken cancellationToken)
		{
			if (node is not TypeDeclarationSyntax type || symbol is not INamedTypeSymbol s)
			{
				data = null;
				return false;
			}

			return WithDiagnostics.ValidateAndCreate(diagnosticReceiver, compilation, type, semanticModel, s, in typeParameters, out data, cancellationToken);
		}

		bool INodeValidatorWithDiagnostics<DefaultParamTypeData>.ValidateAndCreateWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, CSharpSyntaxNode node, ICompilationData compilation, [NotNullWhen(true)] out DefaultParamTypeData? data, CancellationToken cancellationToken)
		{
			if (node is not TypeDeclarationSyntax type)
			{
				data = null;
				return false;
			}

			return WithDiagnostics.ValidateAndCreate(diagnosticReceiver, (DefaultParamCompilationData)compilation, type, out data, cancellationToken);
		}

		bool INodeValidatorWithDiagnostics<DefaultParamTypeData>.ValidateAndCreateWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, CSharpSyntaxNode node, ICompilationData compilation, SemanticModel semanticModel, ISymbol symbol, [NotNullWhen(true)] out DefaultParamTypeData? data, CancellationToken cancellationToken)
		{
			if (node is not TypeDeclarationSyntax type || symbol is not INamedTypeSymbol s)
			{
				data = null;
				return false;
			}

			DefaultParamCompilationData c = (DefaultParamCompilationData)compilation;
			TypeParameterContainer typeParameters = GetTypeParameters(type, semanticModel, c, cancellationToken);

			if (!typeParameters.HasDefaultParams)
			{
				data = null;
				return false;
			}

			return WithDiagnostics.ValidateAndCreate(diagnosticReceiver, (DefaultParamCompilationData)compilation, type, semanticModel, s, in typeParameters, out data, cancellationToken);
		}

		bool INodeValidatorWithDiagnostics<IDefaultParamTarget>.ValidateAndCreateWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, CSharpSyntaxNode node, ICompilationData compilation, [NotNullWhen(true)] out IDefaultParamTarget? data, CancellationToken cancellationToken)
		{
			if (node is not TypeDeclarationSyntax type)
			{
				data = null;
				return false;
			}

			bool isValid = WithDiagnostics.ValidateAndCreate(diagnosticReceiver, (DefaultParamCompilationData)compilation, type, out DefaultParamTypeData? d, cancellationToken);
			data = d;
			return isValid;
		}

		bool INodeValidatorWithDiagnostics<IDefaultParamTarget>.ValidateAndCreateWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, CSharpSyntaxNode node, ICompilationData compilation, SemanticModel semanticModel, ISymbol symbol, [NotNullWhen(true)] out IDefaultParamTarget? data, CancellationToken cancellationToken)
		{
			if (node is not TypeDeclarationSyntax type || symbol is not INamedTypeSymbol s)
			{
				data = null;
				return false;
			}

			DefaultParamCompilationData c = (DefaultParamCompilationData)compilation;
			TypeParameterContainer typeParameters = GetTypeParameters(type, semanticModel, c, cancellationToken);

			if (!typeParameters.HasDefaultParams)
			{
				data = null;
				return false;
			}

			bool isValid = WithDiagnostics.ValidateAndCreate(diagnosticReceiver, c, type, semanticModel, s, in typeParameters, out DefaultParamTypeData? d, cancellationToken);
			data = d;
			return isValid;
		}

		#endregion -Interface Implementations-
	}
}
