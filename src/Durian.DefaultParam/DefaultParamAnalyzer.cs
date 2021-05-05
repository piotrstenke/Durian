using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Data;
using Durian.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.DefaultParam
{
	public abstract partial class DefaultParamAnalyzer : DurianAnalyzer<DefaultParamCompilationData>
	{
		public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.CreateRange(GetBaseDiagnostics().Concat(GetAnalyzerSpecificDiagnostics()));

		public abstract SymbolKind SupportedSymbolKind { get; }

		protected abstract IEnumerable<DiagnosticDescriptor> GetAnalyzerSpecificDiagnostics();

		private static IEnumerable<DiagnosticDescriptor> GetBaseDiagnostics()
		{
			return new[]
			{
				DefaultParamDiagnostics.Descriptors.DefaultParamAttributeCannotBeAppliedToMembersWithAttribute,
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
			WithDiagnostics.DefaultAnalyze(diagnosticReceiver, symbol, compilation, cancellationToken);
		}

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
				AnalyzeAgaintsProhibitedAttributes(symbol, compilation) &&
				AnalyzeContainingTypes(symbol, cancellationToken) &&
				AnalyzeTypeParameters(in typeParameters);
		}

		public static bool AnalyzeAgaintsProhibitedAttributes(ISymbol symbol, DefaultParamCompilationData compilation)
		{
			return AnalyzeAgaintsProhibitedAttributes(symbol, compilation, out _);
		}

		public static bool AnalyzeAgaintsProhibitedAttributes(ISymbol symbol, DefaultParamCompilationData compilation, [NotNullWhen(true)] out AttributeData[]? attributes)
		{
			AttributeData[] attrs = symbol.GetAttributes().ToArray();
			(INamedTypeSymbol type, string name)[] prohibitedAttributes = GetProhibitedAttributes(compilation);

			foreach (AttributeData attr in attrs)
			{
				foreach ((INamedTypeSymbol type, string name) in prohibitedAttributes)
				{
					if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, type))
					{
						attributes = null;
						return false;
					}
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

		public static bool AnalyzeContainingTypes(ISymbol symbol, DefaultParamCompilationData compilation, [NotNullWhen(true)] out ITypeData[]? containingTypes)
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

		private static (INamedTypeSymbol symbol, string name)[] GetProhibitedAttributes(DefaultParamCompilationData compilation)
		{
			return new[]
			{
				(compilation.GeneratedCodeAttribute!, "GeneratedCode"),
				(compilation.DurianGeneratedAttribute!, "DurianGenerated")
			};
		}
	}
}
