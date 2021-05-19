using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Configuration;
using Durian.Generator.Data;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// Contains various utility methods related to the 'DefaultParam' module.
	/// </summary>
	internal static class DefaultParamUtilities
	{
		/// <summary>
		/// Tries to add the <see langword="new"/> modifier to the <paramref name="modifiers"/>.
		/// </summary>
		/// <param name="newModifierIndexes">Indexes at which the <see langword="new"/> modifier should be applied.</param>
		/// <param name="numTypeParameters">Number of type parameters.</param>
		/// <param name="numNonDefaultParam">Number of non-DefaultParam type parameters.</param>
		/// <param name="modifiers">Reference to a<see cref="SyntaxTokenList"/> to update.</param>
		public static bool TryAddNewModifier(HashSet<int>? newModifierIndexes, int numTypeParameters, int numNonDefaultParam, ref SyntaxTokenList modifiers)
		{
			if (newModifierIndexes is not null && newModifierIndexes.Contains(numTypeParameters - numNonDefaultParam) && !modifiers.Any(m => m.IsKind(SyntaxKind.NewKeyword)))
			{
				modifiers = modifiers.Add(SyntaxFactory.Token(SyntaxKind.NewKeyword).WithTrailingTrivia(SyntaxFactory.Space));
				return true;
			}

			return false;
		}

		/// <summary>
		/// Initializes the <paramref name="member"/> by removing unnecessary attributes, applying needed trivia etc.
		/// </summary>
		/// <param name="member"><see cref="MemberDeclarationSyntax"/> to initialize.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="member"/>.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="typeParameters">Type parameters of the <paramref name="member"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <param name="updatedTypeParameters">Updated type parameters of the <paramref name="member"/>.</param>
		public static MemberDeclarationSyntax InitializeDeclaration(
			MemberDeclarationSyntax member,
			SemanticModel semanticModel,
			DefaultParamCompilationData compilation,
			TypeParameterListSyntax typeParameters,
			CancellationToken cancellationToken,
			out TypeParameterListSyntax updatedTypeParameters
		)
		{
			INamedTypeSymbol mainAttribute = compilation.MainAttribute!;

			SeparatedSyntaxList<TypeParameterSyntax> list = typeParameters.Parameters;

			list = SyntaxFactory.SeparatedList(list.Select(p => p.WithAttributeLists(SyntaxFactory.List(p.AttributeLists.Where(attrList => attrList.Attributes.Any(attr =>
			{
				SymbolInfo info = semanticModel.GetSymbolInfo(attr, cancellationToken);
				return !SymbolEqualityComparer.Default.Equals(info.Symbol?.ContainingType, mainAttribute);
			}
			))))));

			int length = list.Count;

			if (length > 1)
			{
				TypeParameterSyntax[] p = new TypeParameterSyntax[length];
				p[0] = list[0];

				for (int i = 1; i < length; i++)
				{
					p[i] = list[i].WithLeadingTrivia(SyntaxFactory.Space);
				}

				list = SyntaxFactory.SeparatedList(p);
			}

			MemberDeclarationSyntax decl = member.WithAttributeLists(SyntaxFactory.List(GetValidAttributes(member, semanticModel, compilation, cancellationToken)));
			updatedTypeParameters = SyntaxFactory.TypeParameterList(list);

			SyntaxTokenList modifiers = decl.Modifiers;

			if (modifiers.Any())
			{
				decl = decl.WithModifiers(SyntaxFactory.TokenList(modifiers.Where(m => !m.IsKind(SyntaxKind.NewKeyword))));
			}

			return decl;
		}

		/// <summary>
		/// Returns a <see cref="SyntaxList{TNode}"/> of <see cref="TypeParameterConstraintClauseSyntax"/> build from the given <paramref name="constraints"/> and updates trivia of the specified <paramref name="parameters"/> if necessary.
		/// </summary>
		/// <param name="constraints">A collection of <see cref="TypeParameterConstraintClauseSyntax"/> to build the <see cref="SyntaxList{TNode}"/> from.</param>
		/// <param name="numOriginalConstraints">Number of type constraints in the original declaration.</param>
		/// <param name="parameters">Current <see cref="ParameterListSyntax"/>.</param>
		public static SyntaxList<TypeParameterConstraintClauseSyntax> ApplyConstraints(IEnumerable<TypeParameterConstraintClauseSyntax> constraints, int numOriginalConstraints, [AllowNull] ref ParameterListSyntax parameters)
		{
			SyntaxList<TypeParameterConstraintClauseSyntax> clauses = SyntaxFactory.List(constraints);

			if (numOriginalConstraints > 0)
			{
				int count = clauses.Count;

				if (count == 0)
				{
					if (parameters is not null)
					{
						parameters = parameters.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
					}
				}
				else if (count < numOriginalConstraints)
				{
					TypeParameterConstraintClauseSyntax last = clauses.Last();
					TypeParameterConstraintClauseSyntax newLast = last.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

					clauses = clauses.Replace(last, newLast);
				}
			}

			return clauses;
		}

		/// <summary>
		/// Tries to update the <paramref name="current"/> <see cref="TypeParameterListSyntax"/> so it contains only <paramref name="count"/> type parameters.
		/// </summary>
		/// <param name="current">Current <see cref="TypeParameterListSyntax"/>.</param>
		/// <param name="count">Number of type parameters the <paramref name="updated"/> <see cref="TypeParameterListSyntax"/> should have.</param>
		/// <param name="updated">The <paramref name="current"/> <see cref="TypeParameterListSyntax"/> with applied changes.</param>
		public static bool TryUpdateTypeParameters(TypeParameterListSyntax? current, int count, out TypeParameterListSyntax? updated)
		{
			if (current is null)
			{
				updated = null;
				return false;
			}

			SeparatedSyntaxList<TypeParameterSyntax> typeParameters = current.Parameters;

			if (typeParameters.Count < count)
			{
				updated = null;
				return false;
			}

			if (count == 0)
			{
				updated = null;
			}
			else
			{
				updated = SyntaxFactory.TypeParameterList(SyntaxFactory.SeparatedList(current.Parameters.Take(count)));
			}

			return true;
		}

		/// <summary>
		/// Checks, if the collection of <see cref="AttributeData"/> and <paramref name="containingTypes"/> of a <see cref="ISymbol"/> allow to apply the 'new' modifier.
		/// </summary>
		/// <param name="attributes">A collection of <see cref="AttributeData"/> representing attributes of a <see cref="ISymbol"/>.</param>
		/// <param name="containingTypes">Containing types of the target <see cref="ISymbol"/>.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		public static bool AllowsNewModifier(IEnumerable<AttributeData> attributes, INamedTypeSymbol[] containingTypes, DefaultParamCompilationData compilation)
		{
			const string configPropertyName = nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible);
			const string scopedPropertyName = nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible);

			if (TryGetConfigurationPropertyName(attributes, compilation.ConfigurationAttribute!, configPropertyName, out bool value))
			{
				return value;
			}
			else
			{
				int length = containingTypes.Length;

				if (length > 0)
				{
					INamedTypeSymbol scopedAttribute = compilation.ScopedConfigurationAttribute!;

					for (int i = 0; i < length; i++)
					{
						if (TryGetConfigurationPropertyName(containingTypes[i].GetAttributes(), scopedAttribute, scopedPropertyName, out value))
						{
							return value;
						}
					}
				}

				return compilation.Configuration.ApplyNewModifierWhenPossible;
			}
		}

		/// <summary>
		/// Gets an <see cref="int"/> value of an enum property of either <see cref="DefaultParamConfigurationAttribute"/> or <see cref="DefaultParamScopedConfigurationAttribute"/> applied for the target <see cref="ISymbol"/>.
		/// </summary>
		/// <param name="propertyName">Name of the property to get the value of.</param>
		/// <param name="attributes">Attributes of the target <see cref="ISymbol"/>.</param>
		/// <param name="containingTypes">Types that contain the target <see cref="ISymbol"/>.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="defaultValue">Value to be returned when no other valid configuration value is found.</param>
		public static int GetConfigurationEnumValue(string propertyName, IEnumerable<AttributeData> attributes, INamedTypeSymbol[] containingTypes, DefaultParamCompilationData compilation, int defaultValue)
		{
			if (!TryGetConfigurationPropertyName(attributes, compilation.ConfigurationAttribute!, propertyName, out int value))
			{
				int length = containingTypes.Length;

				if (length > 0)
				{
					INamedTypeSymbol scopedAttribute = compilation.ScopedConfigurationAttribute!;

					for (int i = 0; i < length; i++)
					{
						if (TryGetConfigurationPropertyName(containingTypes[i].GetAttributes(), scopedAttribute, propertyName, out value))
						{
							return value;
						}
					}
				}

				return defaultValue;
			}

			return value;
		}

		/// <summary>
		/// Converts an array of <see cref="ITypeData"/>s to an array of <see cref="INamedTypeSymbol"/>s.
		/// </summary>
		/// <param name="types">Array of <see cref="ITypeData"/>s to convert.</param>
		public static INamedTypeSymbol[] TypeDatasToTypeSymbols(ITypeData[] types)
		{
			int length = types.Length;
			INamedTypeSymbol[] symbols = new INamedTypeSymbol[length];

			for (int i = 0; i < length; i++)
			{
				symbols[i] = types[i].Symbol;
			}

			return symbols;
		}

		/// <summary>
		/// Returns an enum value of specified property of the <paramref name="configurationAttribute"/>.
		/// </summary>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s to get the value from.</param>
		/// <param name="configurationAttribute"><see cref="INamedTypeSymbol"/> of the configuration attribute.</param>
		/// <param name="propertyName">Name of property to get the value of.</param>
		/// <param name="value">Returned enum value as an <see cref="int"/>.</param>
		public static bool TryGetConfigurationPropertyName<T>(IEnumerable<AttributeData> attributes, INamedTypeSymbol configurationAttribute, string propertyName, out T? value)
		{
			AttributeData? attr = attributes.FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, configurationAttribute));

			if (attr is null)
			{
				value = default!;
				return false;
			}

			return attr.TryGetNamedArgumentValue(propertyName, out value);
		}

		/// <summary>
		/// Returns a new <see cref="IEnumerator{T}"/> for the specified <paramref name="filter"/>.
		/// </summary>
		/// <param name="filter"><see cref="IDefaultParamFilter"/> to get the <see cref="IEnumerator{T}"/> for.</param>
		public static IEnumerator<IMemberData> GetFilterEnumerator(IDefaultParamFilter filter)
		{
			return filter.Mode switch
			{
				FilterMode.None => new FilterEnumerator(filter),
				FilterMode.Diagnostics => new DiagnosticEnumerator(filter),
				FilterMode.Logs => new LoggableEnumerator(filter),
				FilterMode.Both => new LoggableDiagnosticEnumerator(filter),
				_ => new FilterEnumerator(filter)
			};
		}

		/// <summary>
		/// Iterates through whole <paramref name="filter"/>.
		/// </summary>
		/// <typeparam name="T">Type of <see cref="IDefaultParamTarget"/> the <paramref name="filter"/> returns.</typeparam>
		/// <param name="filter"><see cref="IDefaultParamFilter"/> to iterate through.</param>
		/// <returns>An array of <see cref="IDefaultParamTarget"/>s that were returned by the <paramref name="filter"/>.</returns>
		public static T[] IterateFilter<T>(IDefaultParamFilter filter) where T : IDefaultParamTarget
		{
			IEnumerable<IDefaultParamTarget> collection = filter.Mode switch
			{
				FilterMode.None => IterateFilter(new FilterEnumerator(filter)),
				FilterMode.Diagnostics => IterateFilter(new DiagnosticEnumerator(filter)),
				FilterMode.Logs => IterateFilter(new LoggableEnumerator(filter)),
				FilterMode.Both => IterateFilter(new LoggableDiagnosticEnumerator(filter)),
				_ => IterateFilter(new FilterEnumerator(filter))
			};

			return collection.Cast<T>().ToArray();
		}

		/// <summary>
		/// Get the indent level of the <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to get the indent level of.</param>
		public static int GetIndent(CSharpSyntaxNode? node)
		{
			SyntaxNode? parent = node;
			int indent = 0;

			while ((parent = parent!.Parent) is not null)
			{
				if (parent is CompilationUnitSyntax)
				{
					continue;
				}

				indent++;
			}

			if (indent < 0)
			{
				indent = 0;
			}

			return indent;
		}

		/// <summary>
		/// Returns a collection of all namespaces used by the <paramref name="target"/> <see cref="IDefaultParamTarget"/>.
		/// </summary>
		/// <param name="target"><see cref="IDefaultParamTarget"/> to get the namespaces used by.</param>
		/// <param name="parameters"><see cref="TypeParameterContainer"/> that contains the type parameters of the <paramref name="target"/> member.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static IEnumerable<string> GetUsedNamespaces(IDefaultParamTarget target, in TypeParameterContainer parameters, CancellationToken cancellationToken = default)
		{
			int defaultParamCount = parameters.NumDefaultParam;
			List<string> namespaces = GetUsedNamespacesList(target, defaultParamCount, cancellationToken);

			for (int i = 0; i < defaultParamCount; i++)
			{
				ref readonly TypeParameterData data = ref parameters.GetDefaultParamAtIndex(i);

				if (data.TargetType is null || data.TargetType.IsPredefined())
				{
					continue;
				}

				string n = data.TargetType.JoinNamespaces();

				if (!namespaces.Contains(n))
				{
					namespaces.Add(n);
				}
			}

			return namespaces;
		}

		/// <summary>
		/// Returns <see cref="ParameterGeneration"/>s for the specified <paramref name="typeParameters"/>.
		/// </summary>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> to get the <see cref="ParameterGeneration"/>s for.</param>
		/// <param name="symbolParameters">Parameters of the target method/type/delegate.</param>
		public static ParameterGeneration[][] GetParameterGenerations(in TypeParameterContainer typeParameters, IParameterSymbol[] symbolParameters)
		{
			int numParameters = symbolParameters.Length;
			ParameterGeneration[] defaultParameters = new ParameterGeneration[numParameters];
			bool hasTypeArgumentAsParameter = false;

			for (int i = 0; i < numParameters; i++)
			{
				IParameterSymbol parameter = symbolParameters[i];
				ITypeSymbol type = parameter.Type;

				if (type is ITypeParameterSymbol)
				{
					hasTypeArgumentAsParameter = true;
					defaultParameters[i] = CreateDefaultGenerationForTypeArgumentParameter(parameter, in typeParameters);
				}
				else
				{
					defaultParameters[i] = new ParameterGeneration(type, parameter.RefKind, -1);
				}
			}

			if (!hasTypeArgumentAsParameter)
			{
				int numTypeParameters = typeParameters.Length;
				ParameterGeneration[][] generations = new ParameterGeneration[numTypeParameters][];

				for (int i = 0; i < numTypeParameters; i++)
				{
					generations[i] = defaultParameters;
				}

				return generations;
			}
			else
			{
				return CreateParameterGenerationsForTypeParameters(in typeParameters, defaultParameters, numParameters);
			}
		}

		private static ParameterGeneration CreateDefaultGenerationForTypeArgumentParameter(IParameterSymbol parameter, in TypeParameterContainer typeParameters)
		{
			ITypeSymbol type = parameter.Type;

			for (int i = 0; i < typeParameters.Length; i++)
			{
				ref readonly TypeParameterData data = ref typeParameters[i];

				if (SymbolEqualityComparer.Default.Equals(data.Symbol, type))
				{
					return new ParameterGeneration(type, parameter.RefKind, i);
				}
			}

			throw new InvalidOperationException($"Unknown type parameter used as argument for parameter '{parameter.Name}'");
		}

		private static ParameterGeneration[][] CreateParameterGenerationsForTypeParameters(in TypeParameterContainer typeParameters, ParameterGeneration[] defaultParameters, int numParameters)
		{
			ParameterGeneration[][] generations = new ParameterGeneration[typeParameters.NumDefaultParam][];
			ParameterGeneration[] previousParameters = defaultParameters;

			for (int i = typeParameters.Length - 1, genIndex = 0; i >= typeParameters.FirstDefaultParamIndex; i--, genIndex++)
			{
				ParameterGeneration[] currentParameters = new ParameterGeneration[numParameters];

				for (int j = 0; j < numParameters; j++)
				{
					ref readonly ParameterGeneration parameter = ref previousParameters[j];

					if (parameter.GenericParameterIndex == -1)
					{
						currentParameters[j] = parameter;
						continue;
					}

					ref readonly TypeParameterData data = ref typeParameters[parameter.GenericParameterIndex];

					if (data.IsDefaultParam)
					{
						currentParameters[j] = new ParameterGeneration(data.TargetType!, parameter.RefKind, parameter.GenericParameterIndex);
					}
					else
					{
						currentParameters[j] = parameter;
					}
				}

				previousParameters = currentParameters;
				generations[genIndex] = currentParameters;
			}

			return generations;
		}

		private static IEnumerable<IDefaultParamTarget> IterateFilter<T>(T iter) where T : IEnumerator<IDefaultParamTarget>
		{
			while (iter.MoveNext())
			{
				yield return iter.Current;
			}
		}

		private static List<string> GetUsedNamespacesList(IDefaultParamTarget target, int defaultParamCount, CancellationToken cancellationToken)
		{
			INamespaceSymbol globalNamespace = target.ParentCompilation.Compilation.Assembly.GlobalNamespace;
			string[] namespaces = target.SemanticModel.GetUsedNamespacesWithoutDistinct(target.Declaration, globalNamespace, true, cancellationToken).ToArray();

			int count = 0;
			int length = namespaces.Length;

			for (int i = 0; i < length; i++)
			{
				if (namespaces[i] == "Durian")
				{
					count++;

					if (count > defaultParamCount)
					{
						return namespaces.Distinct().ToList();
					}
				}
			}

			return namespaces.Distinct().Where(n => n != "Durian").ToList();
		}

		private static IEnumerable<AttributeListSyntax> GetValidAttributes(MemberDeclarationSyntax member, SemanticModel semanticModel, DefaultParamCompilationData compilation, CancellationToken cancellationToken)
		{
			foreach (AttributeListSyntax list in member.AttributeLists)
			{
				SeparatedSyntaxList<AttributeSyntax> l = SyntaxFactory.SeparatedList(list.Attributes.Where(attr =>
				{
					ISymbol? symbol = semanticModel.GetSymbolInfo(attr, cancellationToken).Symbol;
					return !SymbolEqualityComparer.Default.Equals(symbol?.ContainingType, compilation.ConfigurationAttribute);
				}));

				if (l.Any())
				{
					yield return SyntaxFactory.AttributeList(l);
				}
			}
		}
	}
}
