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
		protected sealed override DefaultParamCompilationData CreateCompilation(CSharpCompilation compilation)
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
				AnalyzeAgaintsProhibitedAttributes(symbol, compilation) &&
				AnalyzeContainingTypes(symbol, cancellationToken) &&
				AnalyzeTypeParameters(in typeParameters);
		}

		/// <summary>
		/// Analyzes, if the <paramref name="symbol"/> has <see cref="DurianGeneratedAttribute"/> or <see cref="GeneratedCodeAttribute"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid (does not have the prohibited attributes), otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeAgaintsProhibitedAttributes(ISymbol symbol, DefaultParamCompilationData compilation)
		{
			return AnalyzeAgaintsProhibitedAttributes(symbol, compilation, out _);
		}

		/// <summary>
		/// Analyzes, if the <paramref name="symbol"/> has <see cref="DurianGeneratedAttribute"/> or <see cref="GeneratedCodeAttribute"/>. If the <paramref name="symbol"/> is valid, returns an array of <paramref name="attributes"/> of that <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="attributes">An array of <see cref="AttributeData"/>s of the <paramref name="symbol"/>. Returned if the method itself returns <see langword="true"/>.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid (does not have the prohibited attributes), otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeAgaintsProhibitedAttributes(ISymbol symbol, DefaultParamCompilationData compilation, [NotNullWhen(true)] out AttributeData[]? attributes)
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
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
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
				}
			}

			containingTypes = types;
			return true;
		}

		/// <summary>
		/// Checks, if the specified <paramref name="typeParameters"/> are valid.
		/// </summary>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> to analyze.</param>
		/// <returns><see langword="true"/> if the type parameters contained within the <see cref="TypeParameterContainer"/> are valid, otherwise <see langword="false"/>.</returns>
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

			IEnumerable<ISymbol> symbols = containingType.GetAllMembers(name);

			if (numNonDefaultParam < numTypeParameters)
			{
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

					return false;
				});
			}

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

				return members.Where(s =>
				{
					if (s is IMethodSymbol m)
					{
						foreach (AttributeData attr in m.GetAttributes())
						{
							if (SymbolEqualityComparer.Default.Equals(generatedFromAttribute, attr.AttributeClass) && attr.TryGetConstructorArgumentValue(0, out string? value))
							{
								return value != fullName && value != baseFullName;
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
				DefaultParamDiagnostics.DUR0120_MemberWithNameAlreadyExists
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
	}
}
