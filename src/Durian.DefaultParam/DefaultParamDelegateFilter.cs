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
using static Durian.Generator.DefaultParam.DefaultParamDelegateAnalyzer;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// Filtrates and validates <see cref="DelegateDeclarationSyntax"/>es collected by a <see cref="DefaultParamSyntaxReceiver"/>.
	/// </summary>
	public partial class DefaultParamDelegateFilter : IDefaultParamFilter
	{
		/// <inheritdoc/>
		public DefaultParamGenerator Generator { get; }

		/// <inheritdoc/>
		public IFileNameProvider FileNameProvider { get; }

		/// <summary>
		/// <see cref="FilterMode"/> of this <see cref="DefaultParamDelegateFilter"/>.
		/// </summary>
		public FilterMode Mode => Generator.LoggingConfiguration.CurrentFilterMode;

		/// <inheritdoc/>
		public bool IncludeGeneratedSymbols => true;

		IDurianSourceGenerator IGeneratorSyntaxFilter.Generator => Generator;

		/// <inheritdoc cref="DefaultParamDelegateFilter(DefaultParamGenerator, IFileNameProvider)"/>
		public DefaultParamDelegateFilter(DefaultParamGenerator generator) : this(generator, new SymbolNameToFile())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamSyntaxReceiver"/> class.
		/// </summary>
		/// <param name="generator"><see cref="DefaultParamGenerator"/> that created this filter.</param>
		/// <param name="fileNameProvider"><see cref="IFileNameProvider"/> that is used to create a hint name for the generated source.</param>
		public DefaultParamDelegateFilter(DefaultParamGenerator generator, IFileNameProvider fileNameProvider)
		{
			Generator = generator;
			FileNameProvider = fileNameProvider;
		}

		/// <summary>
		/// Returns an array of <see cref="DelegateDeclarationSyntax"/>s collected by the <see cref="Generator"/>'s <see cref="DefaultParamSyntaxReceiver"/> that can be filtrated by this filter.
		/// </summary>
		public DelegateDeclarationSyntax[] GetCandidateDelegates()
		{
			return Generator.SyntaxReceiver?.CandidateDelegates?.ToArray() ?? Array.Empty<DelegateDeclarationSyntax>();
		}

		/// <summary>
		/// Enumerates through all <see cref="DelegateDeclarationSyntax"/>es returned by the <see cref="GetCandidateDelegates"/> and returns an array of <see cref="DefaultParamDelegateData"/>s created from the valid ones.
		/// </summary>
		public DefaultParamDelegateData[] GetValidDelegates()
		{
			if (Generator.SyntaxReceiver is null || Generator.TargetCompilation is null || Generator.SyntaxReceiver.CandidateDelegates.Count == 0)
			{
				return Array.Empty<DefaultParamDelegateData>();
			}

			return DefaultParamUtilities.IterateFilter<DefaultParamDelegateData>(this);
		}

		/// <summary>
		/// Specifies, if the <see cref="SemanticModel"/>, <see cref="INamedTypeSymbol"/> and <see cref="TypeParameterContainer"/> can be created from the given <paramref name="declaration"/>.
		/// If so, returns them.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="declaration"><see cref="DelegateDeclarationSyntax"/> to validate.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> created from the <paramref name="declaration"/>.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="declaration"/>'s type parameters.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static bool GetValidationData(
			DefaultParamCompilationData compilation,
			DelegateDeclarationSyntax declaration,
			[NotNullWhen(true)] out SemanticModel? semanticModel,
			[NotNullWhen(true)] out INamedTypeSymbol? symbol,
			out TypeParameterContainer typeParameters,
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

		/// <summary>
		/// Enumerates through all the <paramref name="collectedDelegates"/> and returns an array of <see cref="DefaultParamDelegateData"/>s created from the valid ones.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="collectedDelegates">A collection of <see cref="DelegateDeclarationSyntax"/>es to validate.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static DefaultParamDelegateData[] GetValidDelegates(
			DefaultParamCompilationData compilation,
			IEnumerable<DelegateDeclarationSyntax> collectedDelegates,
			CancellationToken cancellationToken = default
		)
		{
			if (compilation is null || collectedDelegates is null || compilation is null)
			{
				return Array.Empty<DefaultParamDelegateData>();
			}

			DelegateDeclarationSyntax[] collected = collectedDelegates.ToArray();

			if (collected.Length == 0)
			{
				return Array.Empty<DefaultParamDelegateData>();
			}

			return GetValidDelegates_Internal(compilation, collected, cancellationToken);
		}

		/// <summary>
		/// Enumerates through all the <see cref="DelegateDeclarationSyntax"/>es collected by the <paramref name="syntaxReceiver"/> and returns an array of <see cref="DefaultParamDelegateData"/>s created from the valid ones.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="syntaxReceiver"><see cref="DefaultParamSyntaxReceiver"/> that collected the <see cref="DelegateDeclarationSyntax"/>es.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static DefaultParamDelegateData[] GetValidDelegates(
			DefaultParamCompilationData compilation,
			DefaultParamSyntaxReceiver syntaxReceiver,
			CancellationToken cancellationToken = default
		)
		{
			if (compilation is null || syntaxReceiver is null || syntaxReceiver.CandidateDelegates.Count == 0)
			{
				return Array.Empty<DefaultParamDelegateData>();
			}

			return GetValidDelegates_Internal(compilation, syntaxReceiver.CandidateDelegates.ToArray(), cancellationToken);
		}

		/// <summary>
		/// Validates the specified <paramref name="declaration"/> and returns a new instance of <see cref="DefaultParamDelegateData"/> if the validation was a success.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="declaration"><see cref="DefaultParamDelegateData"/> to validate.</param>
		/// <param name="data">Newly-created instance of <see cref="DefaultParamDelegateData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static bool ValidateAndCreate(
			DefaultParamCompilationData compilation,
			DelegateDeclarationSyntax declaration,
			[NotNullWhen(true)] out DefaultParamDelegateData? data,
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
		/// Validates the specified <paramref name="declaration"/> and returns a new instance of <see cref="DefaultParamDelegateData"/> if the validation was a success.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="declaration"><see cref="DefaultParamDelegateData"/> to validate.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> created from the <paramref name="declaration"/>.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="declaration"/>'s type parameters.</param>
		/// <param name="data">Newly-created instance of <see cref="DefaultParamDelegateData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static bool ValidateAndCreate(
			DefaultParamCompilationData compilation,
			DelegateDeclarationSyntax declaration,
			SemanticModel semanticModel,
			INamedTypeSymbol symbol,
			in TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out DefaultParamDelegateData? data,
			CancellationToken cancellationToken = default
		)
		{
			if (AnalyzeAgainstProhibitedAttributes(symbol, compilation, out AttributeData[]? attributes) &&
				AnalyzeContainingTypes(symbol, compilation, out ITypeData[]? containingTypes) &&
				AnalyzeTypeParameters(symbol, in typeParameters))
			{
				INamedTypeSymbol[] symbols = DefaultParamUtilities.TypeDatasToTypeSymbols(containingTypes);

				if (AnalyzeCollidingMembers(symbol, in typeParameters, compilation, attributes, symbols, out HashSet<int>? newModifiers, cancellationToken))
				{
					string targetNamespace = GetTargetNamespace(symbol, attributes, symbols, compilation);

					data = new DefaultParamDelegateData(
						declaration,
						compilation,
						symbol,
						semanticModel,
						containingTypes,
						null,
						attributes,
						typeParameters,
						newModifiers,
						targetNamespace
					);

					return true;
				}
			}

			data = null;
			return false;
		}

		private static TypeParameterContainer GetParameters(
			DelegateDeclarationSyntax declaration,
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

		private static DefaultParamDelegateData[] GetValidDelegates_Internal(
			DefaultParamCompilationData compilation,
			DelegateDeclarationSyntax[] collectedDelegates,
			CancellationToken cancellationToken
		)
		{
			List<DefaultParamDelegateData> list = new(collectedDelegates.Length);

			foreach (DelegateDeclarationSyntax decl in collectedDelegates)
			{
				if (decl is null)
				{
					continue;
				}

				if (ValidateAndCreate(compilation, decl, out DefaultParamDelegateData? data, cancellationToken))
				{
					list.Add(data!);
				}
			}

			return list.ToArray();
		}

		#region -Interface Implementations-

		CSharpSyntaxNode[] IDefaultParamFilter.GetCandidateNodes()
		{
			return GetCandidateDelegates();
		}

		IEnumerator<IMemberData> IGeneratorSyntaxFilter.GetEnumerator()
		{
			return DefaultParamUtilities.GetFilterEnumerator(this);
		}

		IMemberData[] IGeneratorSyntaxFilter.Filtrate()
		{
			return GetValidDelegates();
		}

		IEnumerable<IMemberData> ISyntaxFilter.Filtrate(ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken)
		{
			return GetValidDelegates((DefaultParamCompilationData)compilation, (DefaultParamSyntaxReceiver)syntaxReceiver, cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilter.Filtrate(ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken)
		{
			return GetValidDelegates((DefaultParamCompilationData)compilation, collectedNodes.OfType<DelegateDeclarationSyntax>(), cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken)
		{
			return WithDiagnostics.GetValidDelegates(diagnosticReceiver, (DefaultParamCompilationData)compilation, (DefaultParamSyntaxReceiver)syntaxReceiver, cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken)
		{
			return WithDiagnostics.GetValidDelegates(diagnosticReceiver, (DefaultParamCompilationData)compilation, collectedNodes.OfType<DelegateDeclarationSyntax>(), cancellationToken);
		}

		bool IDefaultParamFilter.ValidateAndCreate(
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			bool isValid = ValidateAndCreate(compilation, (DelegateDeclarationSyntax)node, out DefaultParamDelegateData? d, cancellationToken);
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
			bool isValid = ValidateAndCreate(compilation, (DelegateDeclarationSyntax)node, semanticModel, (INamedTypeSymbol)symbol, in typeParameters, out DefaultParamDelegateData? d, cancellationToken);
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
			bool isValid = WithDiagnostics.ValidateAndCreate(diagnosticReceiver, compilation, (DelegateDeclarationSyntax)node, out DefaultParamDelegateData? d, cancellationToken);
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
			bool isValid = WithDiagnostics.ValidateAndCreate(diagnosticReceiver, compilation, (DelegateDeclarationSyntax)node, semanticModel, (INamedTypeSymbol)symbol, in typeParameters, out DefaultParamDelegateData? d, cancellationToken);
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
			bool isValid = GetValidationData(compilation, (DelegateDeclarationSyntax)node, out semanticModel, out INamedTypeSymbol? s, out typeParameters, cancellationToken);
			symbol = s;
			return isValid;
		}

		#endregion
	}
}
