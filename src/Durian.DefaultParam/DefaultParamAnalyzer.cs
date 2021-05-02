using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Durian.Data;
using Durian.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.DefaultParam
{
	public abstract class DefaultParamAnalyzer : DurianAnalyzer<DefaultParamCompilationData>
	{
		public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.CreateRange(GetBaseDiagnostics().Concat(GetAnalyzerSpecificDiagnostics()));

		public abstract SymbolKind SupportedSymbolKind { get; }

		protected abstract IEnumerable<DiagnosticDescriptor> GetAnalyzerSpecificDiagnostics();

		private static IEnumerable<DiagnosticDescriptor> GetBaseDiagnostics()
		{
			return new[]
			{
				DefaultParamDiagnostics.Descriptors.DefaultParamAttributeCannotBeAppliedToMembersWithGeneratedCodeAttribute,
				DefaultParamDiagnostics.Descriptors.ParentTypeOfMemberWithDefaultParamAttributeMustBePartial,
				DefaultParamDiagnostics.Descriptors.TypeParameterWithDefaultParamAttributeMustBeLast,
				Descriptors.TypeIsNotValidTypeParameter,
			};
		}

		protected sealed override DefaultParamCompilationData CreateCompilation(CSharpCompilation compilation)
		{
			return new DefaultParamCompilationData(compilation);
		}

		public sealed override void Initialize(AnalysisContext context)
		{
			base.Initialize(context);
		}

		protected sealed override void Register(CompilationStartAnalysisContext context, DefaultParamCompilationData compilation)
		{
			context.RegisterSymbolAction(c => AnalyzeSymbol(c, compilation), SupportedSymbolKind);
		}

		private void AnalyzeSymbol(SymbolAnalysisContext context, DefaultParamCompilationData compilation)
		{
			ContextualDiagnosticReceiver<SymbolAnalysisContext> diagnosticReceiver = DiagnosticReceiverFactory.Symbol(context);
			Analyze(diagnosticReceiver, context.Symbol, compilation, context.CancellationToken);
		}

		public virtual void Analyze(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			DefaultAnalyzeWithDiagnostics(diagnosticReceiver, symbol, compilation, cancellationToken);
		}

		#region -Without Diagnostics-
		public static bool DefaultAnalyze(ISymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			if (!TryGetTypeParameters(symbol, compilation, cancellationToken, out TypeParameterContainer typeParameters))
			{
				return false;
			}

			return DefaultAnalyze(symbol, compilation, in typeParameters, cancellationToken);
		}

		public static bool DefaultAnalyze(ISymbol symbol, DefaultParamCompilationData compilation, in TypeParameterContainer typeParameters, CancellationToken cancellationToken = default)
		{
			if (!typeParameters.HasDefaultParams)
			{
				return false;
			}

			return
				AnalyzeAgainstGeneratedCodeAttribute(symbol, compilation) &&
				AnalyzeContainingTypes(symbol, cancellationToken) &&
				AnalyzeTypeParameters(in typeParameters);
		}

		public static bool AnalyzeAgainstGeneratedCodeAttribute(ISymbol symbol, DefaultParamCompilationData compilation)
		{
			return AnalyzeAgainstGeneratedCodeAttribute(symbol, compilation, out _);
		}

		public static bool AnalyzeAgainstGeneratedCodeAttribute(ISymbol symbol, DefaultParamCompilationData compilation, out AttributeData[]? attributes)
		{
			AttributeData[] attrs = symbol.GetAttributes().ToArray();

			foreach (AttributeData attr in attrs)
			{
				if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.GeneratedCodeAttribute))
				{
					attributes = null;
					return false;
				}
			}

			attributes = attrs;
			return true;
		}

		public static bool AnalyzeContainingTypes(ISymbol symbol, CancellationToken cancellationToken = default)
		{
			INamedTypeSymbol[] types = symbol.GetContainingTypeSymbols().ToArray();

			if (types.Length > 0)
			{
				foreach (INamedTypeSymbol parent in types)
				{
					if (!HasPartialKeyword(parent, cancellationToken))
					{
						return false;
					}
				}
			}

			return true;
		}

		public static bool AnalyzeContainingTypes(ISymbol symbol, DefaultParamCompilationData compilation, out ITypeData[]? containingTypes)
		{
			ITypeData[] types = symbol.GetContainingTypes(compilation).ToArray();

			if (types.Length > 0)
			{
				foreach (ITypeData parent in types)
				{
					if (!HasPartialKeyword(parent))
					{
						containingTypes = null;
						return false;
					}
				}
			}

			containingTypes = types;
			return true;
		}

		public static bool AnalyzeTypeParameters(in TypeParameterContainer typeParameters)
		{
			if (!typeParameters.HasDefaultParams)
			{
				return false;
			}

			int length = typeParameters.Length;

			for (int i = typeParameters.FirstDefaultParamIndex; i < length; i++)
			{
				ref readonly TypeParameterData data = ref typeParameters[i];

				if (data.IsDefaultParam)
				{
					if (data.TargetType is null)
					{
						return false;
					}
					else if (!data.TargetType.IsValidForTypeParameter(data.Symbol))
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}

			return true;
		}
		#endregion

		#region -With Diagnostics-
		public static bool DefaultAnalyzeWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			if (!TryGetTypeParameters(symbol, compilation, cancellationToken, out TypeParameterContainer typeParameters))
			{
				return false;
			}

			return DefaultAnalyzeWithDiagnostics(diagnosticReceiver, symbol, compilation, in typeParameters, cancellationToken);
		}

		public static bool DefaultAnalyzeWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, in TypeParameterContainer typeParameters, CancellationToken cancellationToken = default)
		{
			if (!typeParameters.HasDefaultParams)
			{
				return false;
			}

			bool isValid = AnalyzeAgainstGeneratedCodeAttributeWithDiagnostics(diagnosticReceiver, symbol, compilation);
			isValid &= AnalyzeContainingTypesWithDiagnostics(diagnosticReceiver, symbol, cancellationToken);
			isValid &= AnalyzeTypeParametersWithDiagnostics(diagnosticReceiver, in typeParameters);

			return isValid;
		}

		public static bool AnalyzeAgainstGeneratedCodeAttributeWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compialation)
		{
			return AnalyzeAgainstGeneratedCodeAttributeWithDiagnostics(diagnosticReceiver, symbol, compialation, out _);
		}

		public static bool AnalyzeAgainstGeneratedCodeAttributeWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, out AttributeData[]? attributes)
		{
			AttributeData[] attrs = symbol.GetAttributes().ToArray();

			foreach (AttributeData attr in attrs)
			{
				if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.GeneratedCodeAttribute))
				{
					DefaultParamDiagnostics.DefaultParamAttributeCannotBeAppliedToMembersWithGeneratedCodeAttribute(diagnosticReceiver, symbol);
					attributes = null;
					return false;
				}
			}

			attributes = attrs;
			return true;
		}

		public static bool AnalyzeContainingTypesWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, CancellationToken cancellationToken = default)
		{
			INamedTypeSymbol[] types = symbol.GetContainingTypeSymbols().ToArray();
			bool isValid = true;

			if (types.Length > 0)
			{
				foreach (INamedTypeSymbol parent in types)
				{
					if (!HasPartialKeyword(parent, cancellationToken))
					{
						DefaultParamDiagnostics.ParentTypeOfMemberWithDefaultParamAttributeMustBePartial(diagnosticReceiver, parent);
						isValid = false;
					}
				}
			}

			return isValid;
		}

		public static bool AnalyzeContainingTypesWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, out ITypeData[]? containingTypes)
		{
			ITypeData[] types = symbol.GetContainingTypes(compilation).ToArray();
			bool isValid = true;

			if (types.Length > 0)
			{
				foreach (ITypeData parent in types)
				{
					if (!parent.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
					{
						DefaultParamDiagnostics.ParentTypeOfMemberWithDefaultParamAttributeMustBePartial(diagnosticReceiver, parent.Symbol);
						isValid = false;
					}
				}
			}

			containingTypes = isValid ? types : null;

			return isValid;
		}

		public static bool AnalyzeTypeParametersWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, in TypeParameterContainer typeParameters)
		{
			if (!typeParameters.HasDefaultParams)
			{
				return false;
			}

			int length = typeParameters.Length;
			bool isValid = true;
			int lastDefaultParam = typeParameters.FirstDefaultParamIndex;

			for (int i = lastDefaultParam; i < length; i++)
			{
				ref readonly TypeParameterData data = ref typeParameters[i];

				if (data.IsDefaultParam)
				{
					if (data.TargetType is null)
					{
						isValid = false;
					}
					else if (!data.TargetType.IsValidForTypeParameter(data.Symbol))
					{
						DurianDiagnostics.TypeIsNotValidTypeParameter(diagnosticReceiver, data.TargetType, data.Symbol, data.Location);
						isValid = false;
					}

					lastDefaultParam = i;
				}
				else if (lastDefaultParam != -1)
				{
					ref readonly TypeParameterData errorData = ref typeParameters[lastDefaultParam];
					DefaultParamDiagnostics.TypeParameterWithDefaultParamAttributeMustBeLast(diagnosticReceiver, errorData.Symbol, errorData.Location);
					isValid = false;
					lastDefaultParam = -1;
				}
			}

			return isValid;
		}
		#endregion

		public static bool IsDefaultParamGenerated(ISymbol symbol, DefaultParamCompilationData compilation)
		{
			AttributeData? attr = symbol.GetAttributeData(compilation.GeneratedCodeAttribute!);

			if (attr is null)
			{
				return false;
			}

			if (attr.ConstructorArguments.FirstOrDefault().Value is not string tool)
			{
				return false;
			}

			return tool == DefaultParamGenerator.GeneratorName;
		}

		private static bool HasPartialKeyword(ITypeData data)
		{
			return data.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
		}

		private static bool HasPartialKeyword(INamedTypeSymbol symbol, CancellationToken cancellationToken)
		{
			return symbol.GetModifiers(cancellationToken).Any(m => m.IsKind(SyntaxKind.PartialKeyword));
		}

		private static bool TryGetTypeParameters(ISymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken, out TypeParameterContainer typeParameters)
		{
			if (symbol is IMethodSymbol m)
			{
				typeParameters = TypeParameterContainer.CreateFrom(m, compilation, cancellationToken);
				return true;
			}
			else if (symbol is INamedTypeSymbol t)
			{
				typeParameters = TypeParameterContainer.CreateFrom(t, compilation, cancellationToken);
				return true;
			}

			typeParameters = default;
			return false;
		}
	}
}
