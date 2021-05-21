using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Durian.Generator.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Generator.Extensions
{
	/// <summary>
	/// Contains various extension methods for the <see cref="ISymbol"/>-derived interfaces.
	/// </summary>
	public static class SymbolExtensions
	{
		/// <summary>
		/// Returns the effective <see cref="Accessibility"/> of the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the effective <see cref="Accessibility"/> of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public static Accessibility GetEffectiveAccessibility(this ISymbol symbol)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			ISymbol? s = symbol;
			Accessibility lowest = Accessibility.Public;

			while(s is not null)
			{
				Accessibility current = s.DeclaredAccessibility;

				if (current == Accessibility.Private)
				{
					return current;
				}

				if(current != Accessibility.NotApplicable && current < lowest)
				{
					lowest = current;
				}

				s = s.ContainingSymbol;
			}

			return lowest;
		}

		/// <summary>
		/// Checks if the specified <paramref name="type"/> is the <paramref name="typeParameter"/> or if it uses it as its element type (for <see cref="IArrayTypeSymbol"/>) or pointed at type (for <see cref="IPointerTypeSymbol"/>).
		/// </summary>
		/// <param name="type"><see cref="ITypeSymbol"/> to check.</param>
		/// <param name="typeParameter"><see cref="ITypeParameterSymbol"/> to check if is used by the target <paramref name="type"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>. -or- <paramref name="typeParameter"/> is <see langword="null"/>.</exception>
		public static bool IsOrUsesTypeParameter(this ITypeSymbol type, ITypeParameterSymbol typeParameter)
		{
			if(type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if(typeParameter is null)
			{
				throw new ArgumentNullException(nameof(typeParameter));
			}

			if(SymbolEqualityComparer.Default.Equals(type, typeParameter))
			{
				return true;
			}

			ITypeSymbol symbol;

			if (type is IArrayTypeSymbol array)
			{
				symbol = array.GetUnderlayingElementType();
			}
			else if (type is IPointerTypeSymbol pointer)
			{
				symbol = pointer.GetUnderlayingPointedAtType();
			}
			else
			{
				return false;
			}

			if(SymbolEqualityComparer.Default.Equals(symbol, typeParameter))
			{
				return true;
			}

			if (symbol is INamedTypeSymbol t && t.Arity > 0)
			{
				foreach (ITypeSymbol s in t.TypeArguments)
				{
					if (IsOrUsesTypeParameter(s, typeParameter))
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Returns the effective underlaying element type of the <paramref name="array"/> or any of its element array types.
		/// </summary>
		/// <param name="array"><see cref="IArrayTypeSymbol"/> to get the effective underlaying type of.</param>
		/// <returns>The effective underlaying type the <paramref name="array"/> or any of its element array types. -or- <paramref name="array"/> if no such type was found.</returns>
		public static ITypeSymbol GetUnderlayingElementType(this IArrayTypeSymbol array)
		{
			if (array is null)
			{
				throw new ArgumentNullException(nameof(array));
			}

			ITypeSymbol? a = array;

			while (a is IArrayTypeSymbol t)
			{
				a = t.ElementType;
			}

			if (a is null)
			{
				return array;
			}

			return a;
		}

		/// <summary>
		/// Returns the effective underlaying type the <paramref name="pointer"/> or any of its child pointers point to.
		/// </summary>
		/// <param name="pointer"><see cref="IPointerTypeSymbol"/> to get the effective underlaying type of.</param>
		/// <returns>The effective underlaying type the <paramref name="pointer"/> or any of its child pointers point to. -or- <paramref name="pointer"/> if no such type was found.</returns>
		public static ITypeSymbol GetUnderlayingPointedAtType(this IPointerTypeSymbol pointer)
		{
			if(pointer is null)
			{
				throw new ArgumentNullException(nameof(pointer));
			}

			ITypeSymbol? p = pointer;

			while(p is IPointerTypeSymbol t)
			{
				p = t.PointedAtType;
			}

			if(p is null)
			{
				return pointer;
			}

			return p;
		}

		/// <summary>
		/// Determines whether the <paramref name="symbol"/> was generated from the <paramref name="target"/> <see cref="ISymbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check.</param>
		/// <param name="target"><see cref="ISymbol"/> to check if the <paramref name="symbol"/> is generated from.</param>
		/// <param name="compilation"><see cref="CompilationDataWithSymbols"/> to get the needed <see cref="INamedTypeSymbol"/> from.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="symbol"/> is <see langword="null"/>. -or-
		/// <paramref name="target"/> is <see langword="null"/>. -or-
		/// <paramref name="compilation"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InvalidOperationException">Target <paramref name="compilation"/> has errors.</exception>
		public static bool IsGeneratedFrom(this ISymbol symbol, ISymbol target, CompilationDataWithSymbols compilation)
		{
			return IsGeneratedFrom(symbol, target?.ToString()!, compilation);
		}

		/// <summary>
		/// Determines whether the <paramref name="symbol"/> was generated from the <paramref name="target"/> member.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check.</param>
		/// <param name="target"><see cref="string"/> representing a <see cref="ISymbol"/> to check if the <paramref name="symbol"/> was generated from.</param>
		/// <param name="compilation"><see cref="CompilationDataWithSymbols"/> to get the needed <see cref="INamedTypeSymbol"/> from.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="symbol"/> is <see langword="null"/>. -or-
		/// <paramref name="target"/> is <see langword="null"/>. -or-
		/// <paramref name="compilation"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InvalidOperationException">Target <paramref name="compilation"/> has errors.</exception>
		public static bool IsGeneratedFrom(this ISymbol symbol, string target, CompilationDataWithSymbols compilation)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (target is null)
			{
				throw new ArgumentNullException(nameof(target));
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (compilation.HasErrors)
			{
				throw new InvalidOperationException($"Target {nameof(compilation)} has errors!");
			}

			AttributeData? attribute = symbol.GetAttributeData(compilation.DurianGeneratedAttribute);

			if (attribute is null)
			{
				return false;
			}

			return attribute.ConstructorArguments.FirstOrDefault().Value is string value && value == target;
		}

		/// <summary>
		/// Returns all <see cref="IMethodSymbol"/> this <paramref name="method"/> overrides.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the base methods of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
		public static IEnumerable<IMethodSymbol> GetBaseMethods(this IMethodSymbol method)
		{
			if (method is null)
			{
				throw new ArgumentNullException(nameof(method));
			}

			return Yield();

			IEnumerable<IMethodSymbol> Yield()
			{
				IMethodSymbol? m = method;

				while ((m = m!.OverriddenMethod) is not null)
				{
					yield return m;
				}
			}
		}

		/// <summary>
		/// Determines whether the <paramref name="method"/> is partial.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check.</param>
		/// <param name="cancellationToken">Target <see cref="CancellationToken"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
		public static bool IsPartial(this IMethodSymbol method, CancellationToken cancellationToken = default)
		{
			return IsPartial(method, (method?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) as MethodDeclarationSyntax)!);
		}

		/// <summary>
		/// Determines whether the <paramref name="method"/> is partial.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check.</param>
		/// <param name="declaration">Main declaration of this <paramref name="method"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>. -or- <paramref name="declaration"/> is <see langword="null"/>.</exception>
		public static bool IsPartial(this IMethodSymbol method, MethodDeclarationSyntax declaration)
		{
			if (method is null)
			{
				throw new ArgumentNullException(nameof(method));
			}

			if (declaration is null)
			{
				throw new ArgumentNullException(nameof(declaration));
			}

			return
				method.DeclaringSyntaxReferences.Length > 1 ||
				method.PartialImplementationPart is not null ||
				method.PartialDefinitionPart is not null ||
				declaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
		}

		/// <inheritdoc cref="GetAllMembers(ITypeSymbol, string)"/>
		public static IEnumerable<ISymbol> GetAllMembers(this ITypeSymbol type)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			return type.GetMembers().Concat(GetBaseTypes(type).SelectMany(t => t.GetMembers()));
		}

		/// <summary>
		/// Returns all members of the specified <paramref name="type"/> including the members that are declared in base types of this <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="ITypeSymbol"/> to get the members of.</param>
		/// <param name="name">Name of the members to find.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		public static IEnumerable<ISymbol> GetAllMembers(this ITypeSymbol type, string name)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (type.TypeKind == TypeKind.Interface)
			{
				return type.GetMembers(name)
					.Concat(type.AllInterfaces
						.SelectMany(intf => intf.GetMembers(name)));
			}

			return type.GetMembers(name)
				.Concat(GetBaseTypes(type)
					.SelectMany(t => t.GetMembers(name)));
		}

		/// <summary>
		/// Returns all types the specified <paramref name="symbol"/> inherits from.
		/// </summary>
		/// <param name="symbol"><see cref="ITypeSymbol"/> to get the base types of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public static IEnumerable<INamedTypeSymbol> GetBaseTypes(this ITypeSymbol symbol)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			return Yield();

			IEnumerable<INamedTypeSymbol> Yield()
			{
				INamedTypeSymbol? type = symbol.BaseType;

				if (type is not null)
				{
					yield return type;

					while ((type = type!.BaseType) is not null)
					{
						yield return type;
					}
				}
			}
		}

		/// <summary>
		/// Determines whether the <paramref name="first"/> <see cref="IParameterSymbol"/> is equivalent to the <paramref name="second"/> <see cref="IParameterSymbol"/>.
		/// </summary>
		/// <param name="first">First <see cref="IParameterSymbol"/>.</param>
		/// <param name="second">Second <see cref="IParameterSymbol"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="first"/> is <see langword="null"/>. -or <paramref name="second"/> is <see langword="null"/>.</exception>
		public static bool IsEquivalentTo(this IParameterSymbol first, IParameterSymbol second)
		{
			if (first is null)
			{
				throw new ArgumentNullException(nameof(first));
			}

			if (second is null)
			{
				throw new ArgumentNullException(nameof(second));
			}

			if (AnalysisUtilities.IsValidRefKindForOverload(first.RefKind, second.RefKind))
			{
				return false;
			}

			if (!SymbolEqualityComparer.Default.Equals(first.Type, second.Type))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Determines whether the <paramref name="first"/> <see cref="IMethodSymbol"/> has equivalent parameters to the <paramref name="second"/> <see cref="IMethodSymbol"/>.
		/// </summary>
		/// <param name="first">First <see cref="IMethodSymbol"/>.</param>
		/// <param name="second">Second <see cref="IMethodSymbol"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="first"/> is <see langword="null"/>. -or <paramref name="second"/> is <see langword="null"/>.</exception>
		public static bool HasEquivalentParameters(this IMethodSymbol first, IMethodSymbol second)
		{
			if (first is null)
			{
				throw new ArgumentNullException(nameof(first));
			}

			if (second is null)
			{
				throw new ArgumentNullException(nameof(second));
			}

			ImmutableArray<IParameterSymbol> firstParameters = first.Parameters;
			ImmutableArray<IParameterSymbol> secondParameters = second.Parameters;

			if (firstParameters.Length != secondParameters.Length)
			{
				return false;
			}

			for (int i = 0; i < firstParameters.Length; i++)
			{
				if (!IsEquivalentTo(firstParameters[i], secondParameters[i]))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static IMemberData GetMemberData(this ISymbol symbol, ICompilationData compilation)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (symbol is INamedTypeSymbol type)
			{
				if (type.IsRecord)
				{
					return new RecordData(type, compilation);
				}

				return type.TypeKind switch
				{
					TypeKind.Class => new ClassData(type, compilation),
					TypeKind.Struct => new StructData(type, compilation),
					TypeKind.Interface => new InterfaceData(type, compilation),
					TypeKind.Delegate => new DelegateData(type, compilation),
					_ => new TypeData(type, compilation),
				};
			}
			else if (symbol is IMethodSymbol method)
			{
				return new MethodData(method, compilation);
			}
			else if (symbol is IFieldSymbol field)
			{
				return new FieldData(field, compilation);
			}
			else if (symbol is IEventSymbol e)
			{
				return new EventData(e, compilation);
			}
			else if (symbol is IPropertySymbol p)
			{
				return new PropertyData(p, compilation);
			}
			else
			{
				return new MemberData(symbol, compilation);
			}
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing generic identifier of the specified <paramref name="symbol"/> -or- name of the <paramref name="symbol"/> if it is not an <see cref="IMethodSymbol"/> or <see cref="INamedTypeSymbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the generic name of.</param>
		/// <param name="paramsOrArgs">
		/// Determines whether to write the type parameters (e.g. "T") or the type arguments the parameters were substituted by;
		/// <see langword="true"/> for parameters, <see langword="false"/> for arguments.
		/// </param>
		/// <param name="includeParameters">If the <paramref name="symbol"/> is a <see cref="IMethodSymbol"/>, determines whether to include the method's parameters in the returned <see cref="string"/>.</param>
		/// <param name="includeVariance">Determines whether to include variance of the <paramref name="symbol"/>'s type parameters.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public static string GetGenericName(this ISymbol symbol, bool paramsOrArgs, bool includeParameters = false, bool includeVariance = false)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (symbol is INamedTypeSymbol t)
			{
				if (paramsOrArgs)
				{
					return GetGenericName(t.TypeParameters, t.Name, includeVariance);
				}
				else
				{
					return GetGenericName(t.TypeArguments, t.Name);
				}
			}
			else if (symbol is IMethodSymbol m)
			{
				return GetGenericName(m, paramsOrArgs, includeParameters);
			}

			return symbol.Name;
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing generic identifier of the specified <paramref name="method"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the generic name of.</param>
		/// <param name="paramsOrArgs">
		/// Determines whether to write the type parameters (e.g. "T") or the type arguments the parameters were substituted by;
		/// <see langword="true"/> for parameters, <see langword="false"/> for arguments.
		/// </param>
		/// <param name="includeParameters">Determines whether to include the method's parameters in the returned <see cref="string"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
		public static string GetGenericName(this IMethodSymbol method, bool paramsOrArgs, bool includeParameters = false)
		{
			if (method is null)
			{
				throw new ArgumentNullException(nameof(method));
			}

			string name = GetGenericName(paramsOrArgs ? method.TypeParameters : method.TypeArguments, method.Name);

			if (includeParameters)
			{
				name = $"{name}{GetParameterSignature(method)}";
			}

			return name;
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing generic identifier of the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the generic name of.</param>
		/// <param name="paramsOrArgs">
		/// Determines whether to write the type parameters (e.g. "T") or the type arguments the parameters were substituted by;
		/// <see langword="true"/> for parameters, <see langword="false"/> for arguments.
		/// </param>
		/// <param name="includeVariance">Determines whether to include variance of the <paramref name="type"/>'s type parameters.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		public static string GetGenericName(this INamedTypeSymbol type, bool paramsOrArgs, bool includeVariance = false)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (paramsOrArgs)
			{
				return GetGenericName(type.TypeParameters, type.Name, includeVariance);
			}
			else
			{
				return GetGenericName(type.TypeArguments, type.Name);
			}
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing the generic part of an identifier created from the collection of <paramref name="typeParameters"/>.
		/// </summary>
		/// <param name="typeParameters">Type parameters.</param>
		/// <param name="includeVariance">Determines whether to include variance of the <paramref name="typeParameters"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="typeParameters"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Pointers can't be used as generic arguments.</exception>
		public static string GetGenericName(this IEnumerable<ITypeParameterSymbol> typeParameters, bool includeVariance = false)
		{
			return GetGenericName(typeParameters, null, includeVariance);
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing the generic part of an identifier created from the collection of <paramref name="typeArguments"/>.
		/// </summary>
		/// <param name="typeArguments">Type arguments.</param>
		/// <exception cref="ArgumentNullException"><paramref name="typeArguments"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Pointers can't be used as generic arguments.</exception>
		public static string GetGenericName(this IEnumerable<ITypeSymbol> typeArguments)
		{
			if (typeArguments is null)
			{
				throw new ArgumentNullException(nameof(typeArguments));
			}

			if (typeArguments is IEnumerable<ITypeParameterSymbol> enumerable)
			{
				return GetGenericName(enumerable);
			}

			ITypeSymbol[] symbols = typeArguments.ToArray();

			if (symbols.Length == 0)
			{
				return string.Empty;
			}

			StringBuilder sb = new();
			sb.Append('<');

			foreach (ITypeSymbol argument in symbols)
			{
				if (argument is null)
				{
					continue;
				}

				if (argument is IPointerTypeSymbol or IFunctionPointerTypeSymbol)
				{
					throw new InvalidOperationException("Pointers can't be used as generic arguments!");
				}

				AnalysisUtilities.WriteTypeNameOfParameter(argument, sb);

				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);
			sb.Append('>');

			return sb.ToString();
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing generic identifier combined of the specified <paramref name="name"/> and the collection of <paramref name="typeParameters"/>.
		/// </summary>
		/// <param name="typeParameters">Type parameters.</param>
		/// <param name="name">Actual member identifier.</param>
		/// <param name="includeVariance">Determines whether to include variance of the <paramref name="typeParameters"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="typeParameters"/> is <see langword="null"/>.</exception>
		public static string GetGenericName(this IEnumerable<ITypeParameterSymbol> typeParameters, string? name, bool includeVariance = false)
		{
			if (typeParameters is null)
			{
				throw new ArgumentNullException(nameof(typeParameters));
			}

			if (includeVariance)
			{
				return AnalysisUtilities.GetGenericName(typeParameters.Select(p =>
				{
					if (p.Variance == VarianceKind.Out || p.Variance == VarianceKind.In)
					{
						return $"{p.Variance.ToString().ToLower()} {p.Name}";
					}

					return p.Name;
				}),
				name);
			}

			return AnalysisUtilities.GetGenericName(typeParameters.Select(p => p.Name), name);
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing generic identifier combined of the specified <paramref name="name"/> and the collection of <paramref name="typeArguments"/>.
		/// </summary>
		/// <param name="typeArguments">Type arguments.</param>
		/// <param name="name">Actual member identifier.</param>
		/// <exception cref="ArgumentNullException"><paramref name="typeArguments"/> is <see langword="null"/>.</exception>
		public static string GetGenericName(this IEnumerable<ITypeSymbol> typeArguments, string? name)
		{
			if (typeArguments is null)
			{
				throw new ArgumentNullException(nameof(typeArguments));
			}

			return $"{name ?? string.Empty}{GetGenericName(typeArguments)}";
		}

		/// <summary>
		/// Returns a <see cref="string"/> that represents the parameter signature of the <paramref name="method"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the signature of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Function pointers are not supported.</exception>
		public static string GetParameterSignature(this IMethodSymbol method)
		{
			if (method is null)
			{
				throw new ArgumentNullException(nameof(method));
			}

			StringBuilder sb = new();

			sb.Append('(');

			if (method.Parameters.Length > 0)
			{
				foreach (IParameterSymbol parameter in method.Parameters)
				{
					if (parameter.RefKind != RefKind.None)
					{
						switch (parameter.RefKind)
						{
							case RefKind.In:
								sb.Append("in ");
								break;

							case RefKind.Out:
								sb.Append("out ");
								break;

							case RefKind.Ref:
								sb.Append("ref ");
								break;
						}
					}

					AnalysisUtilities.WriteTypeNameOfParameter(parameter.Type, sb);

					sb.Append(", ");
				}

				sb.Remove(sb.Length - 2, 2);
			}

			sb.Append(')');
			return sb.ToString();
		}

		/// <summary>
		/// Returns all <see cref="TypeDeclarationSyntax"/>es of the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the <see cref="TypeDeclarationSyntax"/>es of.</param>
		/// <param name="cancellationToken">Target <see cref="CancellationToken"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		public static IEnumerable<T> GetPartialDeclarations<T>(this INamedTypeSymbol type, CancellationToken cancellationToken = default) where T : TypeDeclarationSyntax
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			return type.DeclaringSyntaxReferences.Select(e => e.GetSyntax(cancellationToken)).OfType<T>();
		}

		/// <summary>
		/// Returns modifiers applied to the target <see cref="INamedTypeSymbol"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the modifiers of.</param>
		/// <param name="cancellationToken">Target <see cref="CancellationToken"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		public static IEnumerable<SyntaxToken> GetModifiers(this INamedTypeSymbol type, CancellationToken cancellationToken = default)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			return GetModifiers(type.DeclaringSyntaxReferences.Select(e => e.GetSyntax(cancellationToken)).Cast<TypeDeclarationSyntax>());
		}

		/// <summary>
		/// Returns modifiers contained withing the given collection of <see cref="TypeDeclarationSyntax"/>es.
		/// </summary>
		/// <param name="decl">Collection of <see cref="TypeDeclarationSyntax"/>es to get the modifiers from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="decl"/> is <see langword="null"/>.</exception>
		public static IEnumerable<SyntaxToken> GetModifiers(this IEnumerable<TypeDeclarationSyntax> decl)
		{
			if (decl is null)
			{
				throw new ArgumentNullException(nameof(decl));
			}

			return Yield();

			IEnumerable<SyntaxToken> Yield()
			{
				List<SyntaxToken> tokens = new();

				foreach (TypeDeclarationSyntax d in decl)
				{
					if (d is null)
					{
						continue;
					}

					foreach (SyntaxToken modifier in d.Modifiers)
					{
						if (!tokens.Exists(m => m.IsKind(modifier.Kind())))
						{
							tokens.Add(modifier);
							yield return modifier;
						}
					}
				}
			}
		}

		/// <summary>
		/// Determines whether the <paramref name="type"/> is a predefined type (any primitive, <see cref="string"/>, <see cref="void"/>, <see cref="object"/>).
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		public static bool IsPredefined(this ITypeSymbol type)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (type.SpecialType == SpecialType.None)
			{
				return false;
			}

			return
				type.SpecialType is SpecialType.System_Void or
				SpecialType.System_String or
				SpecialType.System_Int32 or
				SpecialType.System_Int64 or
				SpecialType.System_Boolean or
				SpecialType.System_Single or
				SpecialType.System_Double or
				SpecialType.System_Decimal or
				SpecialType.System_Char or
				SpecialType.System_Int16 or
				SpecialType.System_Byte or
				SpecialType.System_UInt16 or
				SpecialType.System_UInt32 or
				SpecialType.System_UInt64 or
				SpecialType.System_SByte or
				SpecialType.System_Object;
		}

		/// <summary>
		/// Determines whether the <paramref name="type"/> is a predefined type (any primitive, <see cref="string"/>, <see cref="void"/>, <see cref="object"/>) or a dynamic type.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to get the dynamic symbol from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <returns><see langword="true"/> if the type is predefined or dynamic, otherwise <see langword="false"/>.</returns>
		public static bool IsPredefinedOrDynamic(this ITypeSymbol type, ICompilationData compilation)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			return IsPredefined(type) || SymbolEqualityComparer.Default.Equals(type, compilation.Compilation.DynamicType);
		}

		/// <summary>
		/// Determines whether the <paramref name="type"/> is a predefined type (any primitive, <see cref="string"/>, <see cref="void"/>, <see cref="object"/>) or a dynamic type.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the dynamic symbol from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <returns><see langword="true"/> if the type is predefined or dynamic, otherwise <see langword="false"/>.</returns>
		public static bool IsPredefinedOrDynamic(this ITypeSymbol type, CSharpCompilation compilation)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			return IsPredefined(type) || SymbolEqualityComparer.Default.Equals(type, compilation.DynamicType);
		}

		/// <summary>
		/// Determines whether the target <paramref name="type"/> inherits the <paramref name="baseType"/>.
		/// </summary>
		/// <param name="type">Type to check if inherits the <paramref name="baseType"/>.</param>
		/// <param name="baseType">Base type to check if is inherited by the target <paramref name="type"/>.</param>
		/// <param name="toReturnIfSame">Determines what to return when the <paramref name="type"/> and <paramref name="baseType"/> are the same.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>. -or - <paramref name="baseType"/> is <see langword="null"/>.</exception>
		public static bool InheritsOrImplementsFrom(this ITypeSymbol type, ITypeSymbol baseType, bool toReturnIfSame = true)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (baseType is null)
			{
				throw new ArgumentNullException(nameof(baseType));
			}

			if (SymbolEqualityComparer.Default.Equals(type, baseType))
			{
				return toReturnIfSame;
			}

			if (baseType.TypeKind == TypeKind.Interface)
			{
				if (type.AllInterfaces.IsDefaultOrEmpty)
				{
					return false;
				}

				foreach (INamedTypeSymbol intf in type.AllInterfaces)
				{
					if (SymbolEqualityComparer.Default.Equals(baseType, intf))
					{
						return true;
					}
				}
			}
			else
			{
				INamedTypeSymbol? current = type.BaseType;

				while (current is not null)
				{
					if (SymbolEqualityComparer.Default.Equals(current, baseType))
					{
						return true;
					}

					current = current.BaseType;
				}
			}

			return false;
		}

		/// <summary>
		/// Determines whether the <paramref name="type"/> can be applied to the <paramref name="parameter"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if is valid for the <paramref name="parameter"/>.</param>
		/// <param name="parameter">Target <see cref="ITypeParameterSymbol"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>. -or - <paramref name="parameter"/> is <see langword="null"/>.</exception>
		public static bool IsValidForTypeParameter(this INamedTypeSymbol type, ITypeParameterSymbol parameter)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (parameter is null)
			{
				throw new ArgumentNullException(nameof(parameter));
			}

			return IsValidForTypeParameter_Internal(type, parameter);
		}

		/// <summary>
		/// Determines whether the <paramref name="arrayType"/> can be applied to the <paramref name="parameter"/>.
		/// </summary>
		/// <param name="arrayType"><see cref="IArrayTypeSymbol"/> to check if is valid for the <paramref name="parameter"/>.</param>
		/// <param name="parameter">Target <see cref="ITypeParameterSymbol"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="arrayType"/> is <see langword="null"/>. -or - <paramref name="parameter"/> is <see langword="null"/>.</exception>
		public static bool IsValidForTypeParameter(this IArrayTypeSymbol arrayType, ITypeParameterSymbol parameter)
		{
			if (arrayType is null)
			{
				throw new ArgumentNullException(nameof(arrayType));
			}

			if (parameter is null)
			{
				throw new ArgumentNullException(nameof(parameter));
			}

			return IsValidForTypeParameter_Internal(arrayType, parameter);
		}

		/// <summary>
		/// Determines whether the <paramref name="dynamicType"/> can be applied to the <paramref name="parameter"/>.
		/// </summary>
		/// <param name="dynamicType"><see cref="IDynamicTypeSymbol"/> to check if is valid for the <paramref name="parameter"/>.</param>
		/// <param name="parameter">Target <see cref="ITypeParameterSymbol"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="dynamicType"/> is <see langword="null"/>. -or - <paramref name="parameter"/> is <see langword="null"/>.</exception>
		public static bool IsValidForTypeParameter(this IDynamicTypeSymbol dynamicType, ITypeParameterSymbol parameter)
		{
			if (dynamicType is null)
			{
				throw new ArgumentNullException(nameof(dynamicType));
			}

			if (parameter is null)
			{
				throw new ArgumentNullException(nameof(parameter));
			}

			return IsValidForTypeParameter_Internal(dynamicType, parameter);
		}

		/// <summary>
		/// Determines whether the <paramref name="type"/> can be applied to the <paramref name="parameter"/>.
		/// </summary>
		/// <param name="type"><see cref="ITypeSymbol"/> to check if is valid for the <paramref name="parameter"/>.</param>
		/// <param name="parameter">Target <see cref="ITypeParameterSymbol"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>. -or - <paramref name="parameter"/> is <see langword="null"/>.</exception>
		/// <remarks>Symbols other than <see cref="INamedTypeSymbol"/>, <see cref="IArrayTypeSymbol"/>, <see cref="ITypeParameterSymbol"/> and <see cref="IDynamicTypeSymbol"/> will never be valid.</remarks>
		public static bool IsValidForTypeParameter(this ITypeSymbol type, ITypeParameterSymbol parameter)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (parameter is null)
			{
				throw new ArgumentNullException(nameof(parameter));
			}

			if (type is INamedTypeSymbol or IArrayTypeSymbol or ITypeParameterSymbol or IDynamicTypeSymbol)
			{
				return IsValidForTypeParameter_Internal(type, parameter);
			}

			return false;
		}

		/// <summary>
		/// Returns a <see cref="string"/> that is created by joining the names of the namespaces the provided <paramref name="symbol"/> is contained in.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the containing namespaces of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public static string JoinNamespaces(this ISymbol symbol)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (symbol.ContainingNamespace is null || symbol.ContainingNamespace.IsGlobalNamespace)
			{
				return string.Empty;
			}

			return JoinNamespaces(symbol.GetContainingNamespaces(false));
		}

		/// <summary>
		/// Returns a <see cref="string"/> that is created by joining the names of the provided <paramref name="namespaces"/>.
		/// </summary>
		/// <param name="namespaces">A collection of <see cref="INamespaceSymbol"/>s create the <see cref="string"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="namespaces"/> is <see langword="null"/>.</exception>
		public static string JoinNamespaces(this IEnumerable<INamespaceSymbol> namespaces)
		{
			if (namespaces is null)
			{
				throw new ArgumentNullException(nameof(namespaces));
			}

			StringBuilder sb = new();

			foreach (INamespaceSymbol n in namespaces)
			{
				if (n is null)
				{
					continue;
				}

				sb.Append(n.Name).Append('.');
			}

			return sb.ToString().TrimEnd('.');
		}

		/// <summary>
		/// Returns a <see cref="QualifiedNameSyntax"/> created from the specified <paramref name="namespaces"/>.
		/// </summary>
		/// <param name="namespaces">A collection of <see cref="INamespaceSymbol"/>s to create the <see cref="QualifiedNameSyntax"/> from.</param>
		/// <returns>A <see cref="QualifiedNameSyntax"/> created by combining the <paramref name="namespaces"/>. -or- <see langword="null"/> if there were less then 2 <paramref name="namespaces"/> provided.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="namespaces"/> is <see langword="null"/>.</exception>
		public static QualifiedNameSyntax? JoinIntoQualifiedName(this IEnumerable<INamespaceSymbol> namespaces)
		{
			if (namespaces is null)
			{
				throw new ArgumentNullException(nameof(namespaces));
			}

			return AnalysisUtilities.JoinIntoQualifiedName(namespaces.Select(n => n.Name));
		}

		/// <summary>
		/// Returns a <see cref="string"/> representing the fully qualified name of the <paramref name="symbol"/> that can be used in the XML documentation.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the fully qualified name of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public static string GetXmlFullyQualifiedName(this ISymbol symbol)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			return AnalysisUtilities.ConvertFullyQualifiedNameToXml(symbol.ToString());
		}

		/// <summary>
		/// Determines whether the <paramref name="child"/> is contained withing the <paramref name="parent"/> at any nesting level.
		/// </summary>
		/// <param name="parent">Parent <see cref="ISymbol"/>.</param>
		/// <param name="child">Child <see cref="ISymbol"/>.</param>
		/// <returns>True if the <paramref name="parent"/> contains the <paramref name="child"/> or the <paramref name="parent"/> is equivalent to <paramref name="child"/>, otherwise false.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="parent"/> is <see langword="null"/>. -or- <paramref name="child"/> is <see langword="null"/>.</exception>
		public static bool ContainsSymbol(this ISymbol parent, ISymbol child)
		{
			if (parent is null)
			{
				throw new ArgumentNullException(nameof(parent));
			}

			if (child is null)
			{
				throw new ArgumentNullException(nameof(child));
			}

			ISymbol? current = child;

			while (current is not null)
			{
				if (SymbolEqualityComparer.Default.Equals(current, parent))
				{
					return true;
				}

				current = current.ContainingSymbol;
			}

			return false;
		}

		/// <summary>
		/// Returns all <see cref="ITypeData"/>s that contain the target <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the parent types of.</param>
		/// <param name="compilation">Current <see cref="ICompilationData"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static IEnumerable<ITypeData> GetContainingTypes(this ISymbol symbol, ICompilationData compilation)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			INamedTypeSymbol[] parentSymbols = GetContainingTypeSymbols(symbol).ToArray();
			List<ITypeData> parentList = new(parentSymbols.Length);

			return parentSymbols.Select<INamedTypeSymbol, ITypeData>(parent =>
			{
				if (parent.IsRecord)
				{
					RecordData data = new(parent, compilation) { _containingTypes = parentList.ToArray() };
					parentList.Add(data);
					return data;
				}

				switch (parent.TypeKind)
				{
					case TypeKind.Class:
					{
						ClassData data = new(parent, compilation) { _containingTypes = parentList.ToArray() };
						parentList.Add(data);
						return data;
					}

					case TypeKind.Interface:
					{
						InterfaceData data = new(parent, compilation) { _containingTypes = parentList.ToArray() };
						parentList.Add(data);
						return data;
					}

					case TypeKind.Struct:
					{
						StructData data = new(parent, compilation) { _containingTypes = parentList.ToArray() };
						parentList.Add(data);
						return data;
					}

					default:
					{
						TypeData data = new(parent, compilation) { _containingTypes = parentList.ToArray() };
						parentList.Add(data);
						return data;
					}
				}
			});
		}

		/// <summary>
		/// Returns all <see cref="INamedTypeSymbol"/>s that contain the target <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the parent types of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public static IEnumerable<INamedTypeSymbol> GetContainingTypeSymbols(this ISymbol symbol)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			return GetTypes_WrongOrder().Reverse();

			IEnumerable<INamedTypeSymbol> GetTypes_WrongOrder()
			{
				INamedTypeSymbol parent = symbol.ContainingType;

				if (parent is not null)
				{
					yield return parent;

					while ((parent = parent!.ContainingType) is not null)
					{
						yield return parent;
					}
				}
			}
		}

		/// <summary>
		/// Returns all <see cref="INamespaceOrTypeSymbol"/>s contain the target <paramref name="symbol"/> in namespace-first order.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the parent types and namespaces of.</param>
		/// <param name="includeGlobal">Determines whether to return the global namespace as well</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public static IEnumerable<INamespaceOrTypeSymbol> GetContainingNamespacesAndTypes(this ISymbol symbol, bool includeGlobal = false)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			return GetNamespacesAndTypes();

			IEnumerable<INamespaceOrTypeSymbol> GetNamespacesAndTypes()
			{
				foreach (INamespaceSymbol s in GetContainingNamespaces(symbol, includeGlobal))
				{
					yield return s;
				}

				foreach (INamedTypeSymbol s in GetContainingTypeSymbols(symbol))
				{
					yield return s;
				}
			}
		}

		/// <summary>
		/// Returns root namespace of the <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the root namespaces of.</param>
		/// <param name="includeGlobal">Determines whether to return the global namespace as well.</param>
		/// <returns>The root <see cref="INamespaceSymbol"/> -or- <see langword="null"/> if root <see cref="INamespaceSymbol"/> was not found.</returns>
		public static INamespaceSymbol? GetRootNamespace(this ISymbol symbol, bool includeGlobal = false)
		{
			return GetContainingNamespaces(symbol, includeGlobal).FirstOrDefault();
		}

		/// <summary>
		/// Returns all <see cref="INamespaceSymbol"/>s that contain the target <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the parent namespaces of.</param>
		/// <param name="includeGlobal">Determines whether to return the global namespace as well.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public static IEnumerable<INamespaceSymbol> GetContainingNamespaces(this ISymbol symbol, bool includeGlobal = false)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			IEnumerable<INamespaceSymbol> namespaces = GetNamespaces_WrongOrder().Reverse();

			if (!includeGlobal)
			{
				namespaces = namespaces.Where(n => !n.IsGlobalNamespace);
			}

			return namespaces;

			IEnumerable<INamespaceSymbol> GetNamespaces_WrongOrder()
			{
				INamespaceSymbol parent = symbol.ContainingNamespace;

				if (parent is not null)
				{
					yield return parent;

					while ((parent = parent!.ContainingNamespace) is not null)
					{
						yield return parent;
					}
				}
			}
		}

		/// <summary>
		/// Gets <see cref="AttributeData"/> of the <paramref name="syntax"/> defined on the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol">Target <see cref="ISymbol"/>.</param>
		/// <param name="syntax"><see cref="AttributeSyntax"/> to get the data of.</param>
		/// <returns>The <see cref="AttributeData"/> of the given <see cref="AttributeSyntax"/> or <see langword="null"/> if no such <see cref="AttributeData"/> found.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>. -or- <paramref name="syntax"/> is <see langword="null"/>.</exception>
		public static AttributeData? GetAttributeData(this ISymbol symbol, AttributeSyntax syntax)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (syntax is null)
			{
				throw new ArgumentNullException(nameof(syntax));
			}

			foreach (AttributeData attr in symbol.GetAttributes())
			{
				SyntaxReference? reference = attr.ApplicationSyntaxReference;

				if (reference is null)
				{
					continue;
				}

				if (reference.GetSyntax().IsEquivalentTo(syntax))
				{
					return attr;
				}
			}

			return null;
		}

		/// <summary>
		/// Gets <see cref="AttributeData"/> that corresponds to the <paramref name="attrSymbol"/> and is defined on the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol">Target <see cref="ISymbol"/>.</param>
		/// <param name="attrSymbol">Type of attribute to look for.</param>
		/// <returns>The <see cref="AttributeData"/> that corresponds to the <paramref name="attrSymbol"/> or <see langword="null"/> if no such <see cref="AttributeData"/> found.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>. -or- <paramref name="attrSymbol"/> is <see langword="null"/>.</exception>
		public static AttributeData? GetAttributeData(this ISymbol symbol, INamedTypeSymbol attrSymbol)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (attrSymbol is null)
			{
				throw new ArgumentNullException(nameof(attrSymbol));
			}

			return symbol.GetAttributes()
				.FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attrSymbol));
		}

		/// <summary>
		/// Checks if an attribute of type <paramref name="attrSymbol"/> is defined on the target <paramref name="symbol"/>
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check if contains the specified attribute.</param>
		/// <param name="attrSymbol"><see cref="INamedTypeSymbol"/> of attribute to check for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>. -or- <paramref name="attrSymbol"/> is <see langword="null"/>.</exception>
		public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol attrSymbol)
		{
			return GetAttributeData(symbol, attrSymbol) is not null;
		}

		private static bool IsValidForTypeParameter_Internal(ITypeSymbol type, ITypeParameterSymbol parameter)
		{
			if(type.IsStatic || type.IsRefLikeType || type is IErrorTypeSymbol)
			{
				return false;
			}

			if(type is INamedTypeSymbol s && s.IsUnboundGenericType)
			{
				return false;
			}

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

			foreach (ITypeSymbol t in parameter.ConstraintTypes)
			{
				if(t is ITypeParameterSymbol p)
				{
					if(!IsValidForTypeParameter_Internal(type, p))
					{
						return false;
					}
				}
				else if (!InheritsOrImplementsFrom(type, t))
				{
					return false;
				}
			}

			return true;
		}
	}
}
