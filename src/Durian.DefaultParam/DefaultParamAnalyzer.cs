using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Generator.Data;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// Base class for all DefaultParam analyzers. Contains <see langword="static"/> methods that perform the most basic DefaultParam-related analysis.
	/// </summary>
	public abstract partial class DefaultParamAnalyzer : DurianAnalyzer<DefaultParamCompilationData>
	{
		/// <inheritdoc/>
		public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.CreateRange(GetBaseDiagnostics().Concat(GetAnalyzerSpecificDiagnostics()));

		/// <summary>
		/// <see cref="SymbolKind"/> this analyzer can handle.
		/// </summary>
		public abstract SymbolKind SupportedSymbolKind { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamAnalyzer"/> class.
		/// </summary>
		protected DefaultParamAnalyzer()
		{
		}

		/// <summary>
		/// Returns a collection of <see cref="DiagnosticDescriptor"/>s that are used by this analyzer specifically.
		/// </summary>
		protected abstract IEnumerable<DiagnosticDescriptor> GetAnalyzerSpecificDiagnostics();

		/// <inheritdoc/>
		protected override DefaultParamCompilationData CreateCompilation(CSharpCompilation compilation)
		{
			return new DefaultParamCompilationData(compilation);
		}

		/// <inheritdoc/>
		public sealed override void Initialize(AnalysisContext context)
		{
			base.Initialize(context);
		}

		/// <inheritdoc/>
		protected override void Register(CompilationStartAnalysisContext context, DefaultParamCompilationData compilation)
		{
			context.RegisterSymbolAction(c => AnalyzeSymbol(c, compilation), SupportedSymbolKind);
		}

		private void AnalyzeSymbol(SymbolAnalysisContext context, DefaultParamCompilationData compilation)
		{
			ContextualDiagnosticReceiver<SymbolAnalysisContext> diagnosticReceiver = DiagnosticReceiverFactory.Symbol(context);
			Analyze(diagnosticReceiver, context.Symbol, compilation, context.CancellationToken);
		}

		/// <summary>
		/// Analyzes the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public virtual void Analyze(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			WithDiagnostics.DefaultAnalyze(diagnosticReceiver, symbol, compilation, cancellationToken);
		}

		/// <summary>
		/// Performs basic analysis of the <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
		public static bool DefaultAnalyze(ISymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			if (!TryGetTypeParameters(symbol, compilation, cancellationToken, out TypeParameterContainer typeParameters))
			{
				return false;
			}

			return DefaultAnalyze(symbol, compilation, in typeParameters, cancellationToken);
		}

		/// <summary>
		/// Performs basic analysis of the <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="symbol"/>'s type parameters.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
		public static bool DefaultAnalyze(ISymbol symbol, DefaultParamCompilationData compilation, in TypeParameterContainer typeParameters, CancellationToken cancellationToken = default)
		{
			if (!typeParameters.HasDefaultParams)
			{
				return false;
			}

			return
				AnalyzeAgainstProhibitedAttributes(symbol, compilation) &&
				AnalyzeContainingTypes(symbol, compilation, cancellationToken) &&
				AnalyzeTypeParameters(symbol, in typeParameters);
		}

		/// <summary>
		/// Analyzes, if the <paramref name="symbol"/> has <see cref="DurianGeneratedAttribute"/> or <see cref="GeneratedCodeAttribute"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid (does not have the prohibited attributes), otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeAgainstProhibitedAttributes(ISymbol symbol, DefaultParamCompilationData compilation)
		{
			return AnalyzeAgainstProhibitedAttributes(symbol, compilation, out _);
		}

		/// <summary>
		/// Analyzes, if the <paramref name="symbol"/> has <see cref="DurianGeneratedAttribute"/> or <see cref="GeneratedCodeAttribute"/>. If the <paramref name="symbol"/> is valid, returns an array of <paramref name="attributes"/> of that <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="attributes">An array of <see cref="AttributeData"/>s of the <paramref name="symbol"/>. Returned if the method itself returns <see langword="true"/>.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid (does not have the prohibited attributes), otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeAgainstProhibitedAttributes(ISymbol symbol, DefaultParamCompilationData compilation, [NotNullWhen(true)] out AttributeData[]? attributes)
		{
			AttributeData[] attrs = symbol.GetAttributes().ToArray();
			bool hasDurianGenerated = false;
			bool hasGeneratedCode = false;

			foreach (AttributeData attr in attrs)
			{
				if (!hasGeneratedCode && SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.GeneratedCodeAttribute))
				{
					hasGeneratedCode = true;
					attributes = null;
					return false;
				}
				else if (!hasDurianGenerated && SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.DurianGeneratedAttribute))
				{
					hasDurianGenerated = true;
					attributes = null;
					return false;
				}

				if (hasGeneratedCode && hasDurianGenerated)
				{
					break;
				}
			}

			attributes = attrs;
			return true;
		}

		/// <summary>
		/// Analyzes, if the <paramref name="symbol"/> and its containing types are see <see langword="partial"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeContainingTypes(ISymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
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

					ImmutableArray<ITypeParameterSymbol> typeParameters = parent.TypeParameters;

					if (typeParameters.Length > 0 && typeParameters.SelectMany(t => t.GetAttributes()).Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.MainAttribute)))
					{
						return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Analyzes, if the <paramref name="symbol"/> and its containing types are see <see langword="partial"/>. If the <paramref name="symbol"/> is valid, returns an array of <see cref="ITypeData"/>s of its containing types.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="containingTypes">An array of this <paramref name="symbol"/>'s containing types' <see cref="ITypeData"/>s. Returned if the method itself returns <see langword="true"/>.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
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

					ImmutableArray<ITypeParameterSymbol> typeParameters = parent.Symbol.TypeParameters;

					if (typeParameters.Length > 0 && typeParameters.SelectMany(t => t.GetAttributes()).Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.MainAttribute)))
					{
						containingTypes = null;
						return false;
					}
				}
			}

			containingTypes = types;
			return true;
		}

		/// <summary>
		/// Checks, if the specified <paramref name="typeParameters"/> are valid.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze the type parameters of.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> to analyze.</param>
		/// <returns><see langword="true"/> if the type parameters contained within the <see cref="TypeParameterContainer"/> are valid, otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeTypeParameters(ISymbol symbol, in TypeParameterContainer typeParameters)
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
					if (!ValidateTargetTypeParameter(symbol, in data, in typeParameters))
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

		/// <summary>
		/// Determines, whether the specified <paramref name="symbol"/> has a <see cref="GeneratedCodeAttribute"/> with the <see cref="DefaultParamGenerator.GeneratorName"/> specified as the <see cref="GeneratedCodeAttribute.Tool"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
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

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> has the <see langword="new"/> modifier.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static bool HasNewModifier(ISymbol symbol, CancellationToken cancellationToken = default)
		{
			if (symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) is MemberDeclarationSyntax m)
			{
				return m.Modifiers.Any(m => m.IsKind(SyntaxKind.NewKeyword));
			}

			return false;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="collidingMember"/> actually collides with the <paramref name="targetParameters"/>.
		/// </summary>
		/// <param name="targetParameters">Array of <see cref="ParameterGeneration"/> representing parameters of a generated method.</param>
		/// <param name="collidingMember"><see cref="CollidingMember"/> to check of actually collides with the <paramref name="targetParameters"/>.</param>
		public static bool HasCollidingParameters(ParameterGeneration[] targetParameters, in CollidingMember collidingMember)
		{
			int numParameters = targetParameters.Length;

			for (int i = 0; i < numParameters; i++)
			{
				ref readonly ParameterGeneration generation = ref targetParameters[i];
				IParameterSymbol parameter = collidingMember.Parameters![i];

				if (IsValidParameterInCollidingMember(collidingMember.TypeParameters!, parameter, in generation))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Returns a collection of <see cref="CollidingMember"/>s representing <see cref="ISymbol"/>s that can potentially collide with members generated from the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the colliding members of.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="numTypeParameters">Number of type parameters of this <paramref name="symbol"/>.</param>
		/// <param name="numNonDefaultParam">Number of type parameters of this <paramref name="symbol"/> that don't have the <see cref="DefaultParamAttribute"/>.</param>
		/// <param name="numParameters">Number of parameters of this <paramref name="symbol"/>. Always use <c>0</c> for members other than methods.</param>
		public static CollidingMember[] GetPotentiallyCollidingMembers(ISymbol symbol, DefaultParamCompilationData compilation, int numTypeParameters, int numNonDefaultParam, int numParameters = 0)
		{
			return GetPotentiallyCollidingMembers_Internal(symbol, compilation, numTypeParameters, numNonDefaultParam, numParameters)
				.Select(s =>
				{
					if (s is IMethodSymbol m)
					{
						return new CollidingMember(m, m.TypeParameters.ToArray(), m.Parameters.ToArray());
					}
					else if (s is INamedTypeSymbol t)
					{
						return new CollidingMember(t, t.TypeParameters.ToArray(), null);
					}

					return new CollidingMember(s, null, null);
				})
				.ToArray();
		}

		private static IEnumerable<ISymbol> GetPotentiallyCollidingMembers_Internal(ISymbol symbol, DefaultParamCompilationData compilation, int numTypeParameters, int numNonDefaultParam, int numParameters)
		{
			INamedTypeSymbol? containingType = symbol.ContainingType;

			if (containingType is null)
			{
				return GetCollidingNotNestedTypes(symbol, compilation, numTypeParameters, numNonDefaultParam);
			}

			string name = symbol.Name;
			string fullName = symbol.ToString();
			INamedTypeSymbol generatedFrom = compilation.DurianGeneratedAttribute!;
			int numDefaultParam = numTypeParameters - numNonDefaultParam;

			IEnumerable<ISymbol> symbols = containingType.GetMembers(name);

			if (containingType.TypeKind == TypeKind.Interface)
			{
				symbols = symbols
					.Concat(containingType.AllInterfaces
						.SelectMany(t => t.GetMembers(name)));
			}
			else
			{
				symbols = symbols
					.Concat(containingType.GetBaseTypes()
						.SelectMany(t => t.GetMembers(name))
						.Where(s => s.DeclaredAccessibility > Accessibility.Private));
			}

			bool includeNonGenericCompatibleMembers = numDefaultParam == numTypeParameters;

			symbols = symbols.Where(s =>
			{
				if (s is IMethodSymbol m)
				{
					ImmutableArray<ITypeParameterSymbol> typeParameters = m.TypeParameters;
					return typeParameters.Length >= numNonDefaultParam && typeParameters.Length < numTypeParameters;
				}
				else if (s is INamedTypeSymbol t)
				{
					ImmutableArray<ITypeParameterSymbol> typeParameters = t.TypeParameters;
					return typeParameters.Length >= numNonDefaultParam && typeParameters.Length < numTypeParameters;
				}

				return includeNonGenericCompatibleMembers;
			});

			if (symbol is IMethodSymbol m)
			{
				return GetCollidingMembersForMethodSymbol(m, fullName, symbols, generatedFrom, numParameters);
			}

			if (symbol is INamedTypeSymbol type)
			{
				return symbols.Where(s =>
				{
					if (s is INamedTypeSymbol t && t.TypeKind == type.TypeKind)
					{
						return !IsGeneratedFrom(s, fullName, generatedFrom);
					}

					return true;
				});
			}

			return symbols.Where(s => !IsGeneratedFrom(s, fullName, generatedFrom));
		}

		private static IEnumerable<ISymbol> GetCollidingMembersForMethodSymbol(IMethodSymbol method, string fullName, IEnumerable<ISymbol> symbols, INamedTypeSymbol generatedFromAttribute, int numParameters)
		{
			IEnumerable<ISymbol> members = symbols.Where(s =>
			{
				if (s is IMethodSymbol m)
				{
					return m.Parameters.Length == numParameters;
				}

				return true;
			});

			if (method.IsOverride && method.OverriddenMethod is IMethodSymbol baseMethod)
			{
				string baseFullName = baseMethod.ToString();
				List<string> methodNames = new() { fullName, baseFullName };
				methodNames.AddRange(baseMethod.GetBaseMethods().Select(m => m.ToString()));

				return members.Where(s =>
				{
					if (s is IMethodSymbol m)
					{
						foreach (AttributeData attr in m.GetAttributes())
						{
							if (SymbolEqualityComparer.Default.Equals(generatedFromAttribute, attr.AttributeClass) && attr.TryGetConstructorArgumentValue(0, out string? value))
							{
								return value is not null && !methodNames.Contains(value);
							}
						}
					}

					return true;
				});
			}

			return members.Where(s =>
			{
				if (s is IMethodSymbol)
				{
					return !IsGeneratedFrom(s, fullName, generatedFromAttribute);
				}

				return true;
			});
		}

		private static INamedTypeSymbol[] GetCollidingNotNestedTypes(ISymbol symbol, DefaultParamCompilationData compilation, int numTypeParameters, int numNonDefaultParam)
		{
			INamedTypeSymbol generatedFromAttribute = compilation.DurianGeneratedAttribute!;
			int numDefaultParam = numTypeParameters - numNonDefaultParam;
			string name = symbol.Name;
			string namespaces = symbol.JoinNamespaces();
			string metadata = string.IsNullOrWhiteSpace(namespaces) ? name : $"{namespaces}.{name}";
			string fullName = symbol.ToString();

			List<INamedTypeSymbol> symbols = new(numDefaultParam);

			if (numTypeParameters == numDefaultParam)
			{
				TryAdd(metadata);
			}

			for (int i = numNonDefaultParam; i < numTypeParameters; i++)
			{
				TryAdd($"{metadata}`{i}");
			}

			return symbols.ToArray();

			void TryAdd(string metadataName)
			{
				INamedTypeSymbol? type = compilation.Compilation.GetTypeByMetadataName(metadataName);

				if (type is not null && !IsGeneratedFrom(type, fullName, generatedFromAttribute))
				{
					symbols.Add(type);
				}
			}
		}

		private static bool IsValidParameterInCollidingMember(ITypeParameterSymbol[] typeParameters, IParameterSymbol parameter, in ParameterGeneration targetGeneration)
		{
			if (parameter.Type is ITypeParameterSymbol)
			{
				if (targetGeneration.GenericParameterIndex > -1)
				{
					int typeParameterIndex = GetIndexOfTypeParameterInCollidingMethod(typeParameters, parameter);

					if (targetGeneration.GenericParameterIndex == typeParameterIndex && !AnalysisUtilities.IsValidRefKindForOverload(parameter.RefKind, targetGeneration.RefKind))
					{
						return false;
					}
				}
			}
			else if (SymbolEqualityComparer.Default.Equals(parameter.Type, targetGeneration.Type) && !AnalysisUtilities.IsValidRefKindForOverload(parameter.RefKind, targetGeneration.RefKind))
			{
				return false;
			}

			return true;
		}

		private static int GetIndexOfTypeParameterInCollidingMethod(ITypeParameterSymbol[] typeParameters, IParameterSymbol parameter)
		{
			int currentTypeParameterCount = typeParameters.Length;

			for (int i = 0; i < currentTypeParameterCount; i++)
			{
				if (SymbolEqualityComparer.Default.Equals(parameter.Type, typeParameters[i]))
				{
					return i;
				}
			}

			throw new InvalidOperationException($"Unknown parameter: {parameter}");
		}

		private protected static HashSet<int>? GetApplyNewOrNull(HashSet<int> applyNew)
		{
			if (applyNew.Count == 0)
			{
				return null;
			}

			return applyNew;
		}

		private static bool IsGeneratedFrom(ISymbol symbol, string fullName, INamedTypeSymbol generatedFromAttribute)
		{
			return symbol.GetAttributes().Any(attr =>
			{
				if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, generatedFromAttribute) && attr.TryGetConstructorArgumentValue(0, out string? value))
				{
					return value == fullName;
				}

				return false;
			});
		}

		private static IEnumerable<DiagnosticDescriptor> GetBaseDiagnostics()
		{
			return new[]
			{
				DefaultParamDiagnostics.DUR0101_ContainingTypeMustBePartial,
				DefaultParamDiagnostics.DUR0104_DefaultParamCannotBeAppliedWhenGenerationAttributesArePresent,
				DefaultParamDiagnostics.DUR0105_DefaultParamMustBeLast,
				DefaultParamDiagnostics.DUR0106_TargetTypeDoesNotSatisfyConstraint,
				DefaultParamDiagnostics.DUR0116_MemberWithNameAlreadyExists,
				DefaultParamDiagnostics.DUR0119_DefaultParamValueCannotBeLessAccessibleThanTargetMember,
				DefaultParamDiagnostics.DUR0120_TypeCannotBeUsedWithConstraint,
				DefaultParamDiagnostics.DUR0121_TypeIsNotValidDefaultParamValue,
				DefaultParamDiagnostics.DUR0126_DefaultParamMembersCannotBeNested
			};
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

		private static bool ValidateTargetTypeParameter(ISymbol symbol, in TypeParameterData currentTypeParameter, in TypeParameterContainer typeParameters)
		{
			ITypeSymbol? targetType = currentTypeParameter.TargetType;
			ITypeParameterSymbol typeParameterSymbol = currentTypeParameter.Symbol;

			if (targetType is null || targetType is IErrorTypeSymbol)
			{
				return false;
			}

			if (targetType.IsStatic ||
				targetType.IsRefLikeType ||
				targetType is IFunctionPointerTypeSymbol ||
				targetType is IPointerTypeSymbol ||
				(targetType is INamedTypeSymbol t && (t.IsUnboundGenericType || t.SpecialType == SpecialType.System_Void))
			)
			{
				return false;
			}

			if (!HasValidParameterAccessibility(symbol, targetType, typeParameterSymbol, in typeParameters))
			{
				return false;
			}

			if (targetType.SpecialType == SpecialType.System_Object ||
				targetType.SpecialType == SpecialType.System_Array ||
				targetType.SpecialType == SpecialType.System_ValueType ||
				targetType is IArrayTypeSymbol ||
				targetType.IsValueType ||
				targetType.IsSealed
			)
			{
				if (HasTypeParameterAsConstraint(typeParameterSymbol, in typeParameters))
				{
					return false;
				}
			}

			return IsValidForConstraint(targetType, currentTypeParameter.Symbol, in typeParameters);
		}

		private static bool HasValidParameterAccessibility(ISymbol symbol, ITypeSymbol targetType, ITypeParameterSymbol currentTypeParameter, in TypeParameterContainer typeParameters)
		{
			if (targetType.GetEffectiveAccessibility() < symbol.GetEffectiveAccessibility())
			{
				if (symbol is IMethodSymbol m)
				{
					if (IsInvalidMethod(m))
					{
						return false;
					}
				}
				else if (symbol is INamedTypeSymbol t)
				{
					if (t.TypeKind == TypeKind.Delegate && IsInvalidMethod(t.DelegateInvokeMethod))
					{
						return false;
					}
				}
				else
				{
					return true;
				}

				return !HasTypeParameterAsConstraint(currentTypeParameter, in typeParameters);
			}

			return true;

			bool IsInvalidMethod(IMethodSymbol? method)
			{
				return method is null || method.ReturnType.IsOrUsesTypeParameter(currentTypeParameter) || method.Parameters.Any(p => p.Type.IsOrUsesTypeParameter(currentTypeParameter));
			}
		}

		private static bool HasTypeParameterAsConstraint(ITypeParameterSymbol currentTypeParameter, in TypeParameterContainer typeParameters)
		{
			int length = typeParameters.Length;

			for (int i = 0; i < length; i++)
			{
				ref readonly TypeParameterData param = ref typeParameters[i];

				foreach (ITypeSymbol constraint in param.Symbol.ConstraintTypes)
				{
					if (SymbolEqualityComparer.Default.Equals(constraint, currentTypeParameter))
					{
						return true;
					}
				}
			}

			return false;
		}

		private static bool IsValidForConstraint(ITypeSymbol type, ITypeParameterSymbol parameter, in TypeParameterContainer typeParameters)
		{
			if (parameter.HasReferenceTypeConstraint)
			{
				if (!type.IsReferenceType)
				{
					return false;
				}
			}
			else if (parameter.HasUnmanagedTypeConstraint)
			{
				if (!type.IsUnmanagedType)
				{
					return false;
				}
			}
			else if (parameter.HasValueTypeConstraint)
			{
				if (!type.IsValueType)
				{
					return false;
				}
			}

			if (parameter.HasConstructorConstraint)
			{
				if (type is INamedTypeSymbol n)
				{
					if (!n.InstanceConstructors.Any(ctor => ctor.Parameters.Length == 0 && ctor.DeclaredAccessibility == Accessibility.Public))
					{
						return false;
					}
				}
				else if (type is not IDynamicTypeSymbol)
				{
					return false;
				}
			}

			foreach (ITypeSymbol constraint in parameter.ConstraintTypes)
			{
				if (constraint is ITypeParameterSymbol p)
				{
					ref readonly TypeParameterData data = ref typeParameters[p.Ordinal];

					if (data.IsDefaultParam)
					{
						if (!type.InheritsOrImplementsFrom(data.TargetType))
						{
							return false;
						}
					}
					else if (!IsValidForConstraint(type, p, in typeParameters))
					{
						return false;
					}
				}
				else if (!type.InheritsOrImplementsFrom(constraint))
				{
					return false;
				}
			}

			return true;
		}
	}
}
