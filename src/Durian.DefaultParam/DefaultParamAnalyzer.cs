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
			Validate(diagnosticReceiver, context.Symbol, compilation, context.CancellationToken);
		}

		public virtual bool Validate(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			return DefaultValidate(diagnosticReceiver, symbol, compilation, cancellationToken);
		}

		public static bool DefaultValidate(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			TypeParameterContainer typeParameters = TypeParameterContainer.CreateFrom(symbol, compilation, cancellationToken);

			return DefaultValidate(diagnosticReceiver, symbol, compilation, in typeParameters, cancellationToken);
		}

		public static bool DefaultValidate(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, in TypeParameterContainer typeParameters, CancellationToken cancellationToken = default)
		{
			if (!typeParameters.HasDefaultParams)
			{
				return false;
			}

			bool isValid = ValidateHasGeneratedCodeAttribute(diagnosticReceiver, symbol, compilation);
			isValid &= ValidateContainingTypes(diagnosticReceiver, symbol, cancellationToken);
			isValid &= ValidateTypeParameters(diagnosticReceiver, in typeParameters);

			return isValid;
		}

		public static bool DefaultValidate(ISymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			TypeParameterContainer typeParameters = TypeParameterContainer.CreateFrom(symbol, compilation, cancellationToken);

			return DefaultValidate(symbol, compilation, in typeParameters, cancellationToken);
		}

		public static bool DefaultValidate(ISymbol symbol, DefaultParamCompilationData compilation, in TypeParameterContainer typeParameters, CancellationToken cancellationToken = default)
		{
			if (!typeParameters.HasDefaultParams)
			{
				return false;
			}

			return
				ValidateHasGeneratedCodeAttribute(symbol, compilation) &&
				ValidateContainingTypes(symbol, cancellationToken) &&
				ValidateTypeParameters(in typeParameters);
		}

		public static bool ValidateHasGeneratedCodeAttribute(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compialation)
		{
			return ValidateHasGeneratedCodeAttribute(diagnosticReceiver, symbol, compialation, out _);
		}

		public static bool ValidateHasGeneratedCodeAttribute(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, out AttributeData[]? attributes)
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

		public static bool ValidateHasGeneratedCodeAttribute(ISymbol symbol, DefaultParamCompilationData compilation)
		{
			return ValidateHasGeneratedCodeAttribute(symbol, compilation, out _);
		}

		public static bool ValidateHasGeneratedCodeAttribute(ISymbol symbol, DefaultParamCompilationData compilation, out AttributeData[]? attributes)
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

		public static bool ValidateContainingTypes(ISymbol symbol, CancellationToken cancellationToken = default)
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

		public static bool ValidateContainingTypes(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, CancellationToken cancellationToken = default)
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

		public static bool ValidateContainingTypes(ISymbol symbol, DefaultParamCompilationData compilation, out ITypeData[]? containingTypes)
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

		public static bool ValidateContainingTypes(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, out ITypeData[]? containingTypes)
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

		private static bool HasPartialKeyword(ITypeData data)
		{
			return data.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
		}

		private static bool HasPartialKeyword(INamedTypeSymbol symbol, CancellationToken cancellationToken)
		{
			return symbol.GetModifiers(cancellationToken).Any(m => m.IsKind(SyntaxKind.PartialKeyword));
		}

		public static bool ValidateTypeParameters(in TypeParameterContainer typeParameters)
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

		public static bool ValidateTypeParameters(IDiagnosticReceiver diagnosticReceiver, in TypeParameterContainer typeParameters)
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

		public static bool IsDefaultParamGenerated(ISymbol symbol, DefaultParamCompilationData compilation)
		{
			AttributeData? attr = symbol.GetAttributeData(compilation.GeneratedCodeAttribute);

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
	}
}
