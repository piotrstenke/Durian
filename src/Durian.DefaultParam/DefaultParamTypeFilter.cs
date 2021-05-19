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
using static Durian.Generator.DefaultParam.DefaultParamTypeAnalyzer;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// Filtrates and validates <see cref="TypeDeclarationSyntax"/>es collected by a <see cref="DefaultParamSyntaxReceiver"/>.
	/// </summary>
	public partial class DefaultParamTypeFilter : IDefaultParamFilter
	{
		/// <inheritdoc/>
		public DefaultParamGenerator Generator { get; }

		/// <inheritdoc/>
		public IFileNameProvider FileNameProvider { get; }

		/// <summary>
		/// <see cref="FilterMode"/> of this <see cref="DefaultParamTypeFilter"/>.
		/// </summary>
		public FilterMode Mode => Generator.LoggingConfiguration.CurrentFilterMode;

		/// <inheritdoc/>
		public bool IncludeGeneratedSymbols { get; }
		IDurianSourceGenerator IGeneratorSyntaxFilter.Generator => Generator;

		/// <inheritdoc cref="DefaultParamTypeFilter(DefaultParamGenerator, IFileNameProvider)"/>
		public DefaultParamTypeFilter(DefaultParamGenerator generator) : this(generator, new SymbolNameToFile())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamTypeFilter"/> class.
		/// </summary>
		/// <param name="generator"><see cref="DefaultParamGenerator"/> that created this filter.</param>
		/// <param name="fileNameProvider"><see cref="IFileNameProvider"/> that is used to create a hint name for the generated source.</param>
		public DefaultParamTypeFilter(DefaultParamGenerator generator, IFileNameProvider fileNameProvider)
		{
			Generator = generator;
			FileNameProvider = fileNameProvider;
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
			if (AnalyzeAgaintsProhibitedAttributes(symbol, compilation, out AttributeData[]? attributes) &&
				AnalyzeContainingTypes(symbol, compilation, out ITypeData[]? containingTypes) &&
				AnalyzeTypeParameters(in typeParameters))
			{
				INamedTypeSymbol[] symbols = DefaultParamUtilities.TypeDatasToTypeSymbols(containingTypes);

				if (AnalyzeCollidingMembers(symbol, in typeParameters, compilation, attributes, symbols, out HashSet<int>? applyNewModifiers, cancellationToken))
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
						inherit
					);

					return true;
				}
			}

			data = null;
			return false;
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
			typeParameters = GetParameters(declaration, semanticModel, compilation, cancellationToken);

			if (!typeParameters.HasDefaultParams)
			{
				symbol = null!;
				return false;
			}

			symbol = semanticModel.GetDeclaredSymbol(declaration, cancellationToken)!;

			return symbol is not null;
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

		private static TypeParameterContainer GetParameters(
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

		#region -Interface Implementations

		CSharpSyntaxNode[] IDefaultParamFilter.GetCandidateNodes()
		{
			return GetCandidateTypes();
		}

		IEnumerator<IMemberData> IGeneratorSyntaxFilter.GetEnumerator()
		{
			return DefaultParamUtilities.GetFilterEnumerator(this);
		}

		IMemberData[] IGeneratorSyntaxFilter.Filtrate()
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

		bool IDefaultParamFilter.ValidateAndCreate(
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			bool isValid = ValidateAndCreate(compilation, (TypeDeclarationSyntax)node, out DefaultParamTypeData? d, cancellationToken);
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
			bool isValid = ValidateAndCreate(compilation, (TypeDeclarationSyntax)node, semanticModel, (INamedTypeSymbol)symbol, in typeParameters, out DefaultParamTypeData? d, cancellationToken);
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
			bool isValid = WithDiagnostics.ValidateAndCreate(diagnosticReceiver, compilation, (TypeDeclarationSyntax)node, out DefaultParamTypeData? d, cancellationToken);
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
			bool isValid = WithDiagnostics.ValidateAndCreate(diagnosticReceiver, compilation, (TypeDeclarationSyntax)node, semanticModel, (INamedTypeSymbol)symbol, in typeParameters, out DefaultParamTypeData? d, cancellationToken);
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
			bool isValid = GetValidationData(compilation, (TypeDeclarationSyntax)node, out semanticModel, out INamedTypeSymbol? s, out typeParameters, cancellationToken);
			symbol = s;
			return isValid;
		}
		#endregion
	}
}
