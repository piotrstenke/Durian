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
		private readonly LoggableGeneratorDiagnosticReceiver? _loggableReceiver;
		private readonly IDirectDiagnosticReceiver? _diagnosticReceiver;
		private readonly IFileNameProvider _fileNameProvider;

		public DefaultParamGenerator Generator { get; }
		IDurianSourceGenerator IGeneratorSyntaxFilter.Generator => Generator;

		public DefaultParamTypeFilter(DefaultParamGenerator generator) : this(generator, new SymbolNameToFile())
		{
		}

		public DefaultParamTypeFilter(DefaultParamGenerator generator, IFileNameProvider fileNameProvider)
		{
			_declBuilder = new();
			Generator = generator;

			if (generator.LoggingConfiguration.EnableLogging)
			{
				_loggableReceiver = new LoggableGeneratorDiagnosticReceiver(generator);
				_diagnosticReceiver = generator.SupportsDiagnostics ? DiagnosticReceiverFactory.Direct(ReportForBothReceivers) : _loggableReceiver;
			}
			else if (generator.SupportsDiagnostics)
			{
				_diagnosticReceiver = generator.DiagnosticReceiver!;
			}

			_fileNameProvider = fileNameProvider;
		}

		public DeclarationBuilder GetDeclarationBuilder(DefaultParamTypeData target, CancellationToken cancellationToken = default)
		{
			_declBuilder.SetData(target, cancellationToken);

			return _declBuilder;
		}

		public DefaultParamTypeData[] GetValidTypes()
		{
			if (Generator.SyntaxReceiver is null || Generator.TargetCompilation is null || Generator.SyntaxReceiver.CandidateTypes.Count == 0)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			DefaultParamCompilationData compilation = Generator.TargetCompilation;
			CancellationToken cancellationToken = Generator.CancellationToken;

			if (_loggableReceiver is not null)
			{
				List<DefaultParamTypeData> list = new(Generator.SyntaxReceiver.CandidateTypes.Count);
				IDirectDiagnosticReceiver diagnosticReceiver = Generator.EnableDiagnostics ? _diagnosticReceiver! : _loggableReceiver;

				foreach (TypeDeclarationSyntax type in Generator.SyntaxReceiver.CandidateTypes)
				{
					if (type is null)
					{
						continue;
					}

					if (!GetValidationData(compilation, type, out SemanticModel semanticModel, out TypeParameterContainer typeParameters, out INamedTypeSymbol symbol, cancellationToken))
					{
						continue;
					}

					string fileName = _fileNameProvider.GetFileName(symbol);
					_loggableReceiver.SetTargetNode(type, fileName);

					if (ValidateAndCreateWithDiagnostics(diagnosticReceiver, compilation, type, semanticModel, symbol, ref typeParameters, out DefaultParamTypeData? data))
					{
						list.Add(data!);
					}

					if (_loggableReceiver.Count > 0)
					{
						_loggableReceiver.Push();
						_fileNameProvider.Success();
					}
				}

				return list.ToArray();
			}
			else if (_diagnosticReceiver is not null && Generator.EnableDiagnostics)
			{
				return GetValidTypesWithDiagnostics_Internal(_diagnosticReceiver, compilation, Generator.SyntaxReceiver.CandidateTypes.ToArray(), cancellationToken);
			}
			else
			{
				return GetValidTypes_Internal(compilation, Generator.SyntaxReceiver.CandidateTypes.ToArray(), cancellationToken);
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

				if (ValidateAndCreate(compilation, decl, out DefaultParamTypeData? data, cancellationToken))
				{
					list.Add(data!);
				}
			}

			return list.ToArray();
		}

		private static bool ValidateAndCreate(
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

			return ValidateAndCreate(compilation, declaration, semanticModel, symbol, ref typeParameters, out data);
		}

		private static bool ValidateAndCreate(
			DefaultParamCompilationData compilation,
			TypeDeclarationSyntax declaration,
			SemanticModel semanticModel,
			INamedTypeSymbol symbol,
			ref TypeParameterContainer typeParameters,
			out DefaultParamTypeData? data
		)
		{
			if (AnalyzeAgainstGeneratedCodeAttribute(symbol, compilation, out AttributeData[]? attributes) &&
				AnalyzeContainingTypes(symbol, compilation, out ITypeData[]? containingTypes) &&
				AnalyzeTypeParameters(in typeParameters))
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
			bool isValid = AnalyzeAgainstGeneratedCodeAttributeWithDiagnostics(diagnosticReceiver, symbol, compilation, out AttributeData[]? attributes);
			isValid &= AnalyzeContainingTypesWithDiagnostics(diagnosticReceiver, symbol, compilation, out ITypeData[]? containingTypes);
			isValid &= AnalyzeTypeParametersWithDiagnostics(diagnosticReceiver, in typeParameters);

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

		IEnumerable<IMemberData> IGeneratorSyntaxFilter.Filtrate()
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
			Generator.DiagnosticReceiver!.ReportDiagnostic(diagnostic);
		}
	}
}
