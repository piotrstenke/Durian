// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// Contains data of a single type parameter required by the <see cref="DefaultParamGenerator"/>.
	/// </summary>
	public readonly struct TypeParameterData : IEquatable<TypeParameterData>
	{
		/// <summary>
		/// <see cref="AttributeSyntax"/> where the <c>Durian.DefaultParamAttribute</c> value was defined. -or- <see langword="null"/> if the <c>Durian.DefaultParamAttribute</c> is not defined on the target <see cref="Symbol"/>.
		/// </summary>
		public readonly AttributeSyntax? Attribute { get; }

		/// <summary>
		/// Determines whether the target <see cref="Symbol"/> has a <c>Durian.DefaultParamAttribute</c>.
		/// </summary>
		[MemberNotNullWhen(true, nameof(Attribute))]
		public readonly bool IsDefaultParam => Attribute is not null;

		/// <summary>
		/// Determines whether the target <see cref="Symbol"/> has a valid <c>Durian.DefaultParamAttribute</c>.
		/// </summary>
		[MemberNotNullWhen(true, nameof(TargetType))]
		public readonly bool IsValidDefaultParam => IsDefaultParam && TargetType is not null;

		/// <summary>
		/// <see cref="Location"/> of the <see cref="Syntax"/>.
		/// </summary>
		public readonly Location Location => Attribute?.GetLocation() ?? Syntax.GetLocation();

		/// <summary>
		/// Name of the <see cref="Symbol"/>.
		/// </summary>
		public readonly string Name => Symbol.Name;

		/// <summary>
		/// Parent <see cref="CSharpSyntaxNode"/> of the <see cref="Syntax"/>.
		/// </summary>
		public readonly CSharpSyntaxNode Parent { get; }

		/// <summary>
		/// <see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the <see cref="Syntax"/>.
		/// </summary>
		public readonly SemanticModel SemanticModel { get; }

		/// <summary>
		/// Target <see cref="ITypeParameterSymbol"/>.
		/// </summary>
		public readonly ITypeParameterSymbol Symbol { get; }

		/// <summary>
		/// <see cref="TypeParameterSyntax"/> of the target <see cref="Symbol"/>.
		/// </summary>
		public readonly TypeParameterSyntax Syntax { get; }

		/// <summary>
		/// The <see cref="ITypeSymbol"/> that was specified using the <see cref="Attribute"/>. -or- <see langword="null"/> if <see cref="Attribute"/> is <see langword="null"/> or the type cannot be resolved because of error.
		/// </summary>
		public readonly ITypeSymbol? TargetType { get; }

		/// <inheritdoc cref="TypeParameterData(TypeParameterSyntax, ITypeParameterSymbol, SemanticModel, AttributeSyntax, ITypeSymbol)"/>
		public TypeParameterData(TypeParameterSyntax syntax, ITypeParameterSymbol symbol, SemanticModel semanticModel) : this(syntax, symbol, semanticModel, null, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeParameterData"/> struct.
		/// </summary>
		/// <param name="syntax">Target <see cref="TypeParameterSyntax"/>.</param>
		/// <param name="symbol"><see cref="ITypeParameterSymbol"/> represented by the target <paramref name="syntax"/>.</param>
		/// <param name="semanticModel"><see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the target <paramref name="syntax"/>.</param>
		/// <param name="attribute">Valid <c>Durian.DefaultParamAttribute</c> defined on the target <paramref name="symbol"/>.</param>
		/// <param name="targetType">The <see cref="ITypeSymbol"/> that was specified using the <paramref name="attribute"/>. -or- <see langword="null"/> if <paramref name="attribute"/> is <see langword="null"/> or the type cannot be resolved because of error.</param>
		public TypeParameterData(TypeParameterSyntax syntax, ITypeParameterSymbol symbol, SemanticModel semanticModel, AttributeSyntax? attribute, ITypeSymbol? targetType)
		{
			Syntax = syntax;
			Symbol = symbol;
			Attribute = attribute;
			TargetType = targetType;
			Parent = (CSharpSyntaxNode)syntax.Parent!.Parent!;
			SemanticModel = semanticModel;
		}

		/// <summary>
		/// Creates a new <see cref="TypeParameterData"/> based on the specified <paramref name="typeParameter"/>.
		/// </summary>
		/// <param name="typeParameter"><see cref="TypeParameterSyntax"/> to create the <see cref="TypeParameterData"/> from.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static TypeParameterData CreateFrom(TypeParameterSyntax typeParameter, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			return CreateFrom(typeParameter, compilation.Compilation.GetSemanticModel(typeParameter.SyntaxTree), compilation, cancellationToken);
		}

		/// <summary>
		/// Creates a new <see cref="TypeParameterData"/> based on the specified <paramref name="typeParameter"/>.
		/// </summary>
		/// <param name="typeParameter"><see cref="TypeParameterSyntax"/> to create the <see cref="TypeParameterData"/> from.</param>
		/// <param name="semanticModel"><see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the target <paramref name="typeParameter"/>.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static TypeParameterData CreateFrom(TypeParameterSyntax typeParameter, SemanticModel semanticModel, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			return CreateFrom(typeParameter, semanticModel, compilation.DefaultParamAttribute!, cancellationToken);
		}

		/// <summary>
		/// Creates a new <see cref="TypeParameterData"/> based on the specified <paramref name="typeParameter"/>.
		/// </summary>
		/// <param name="typeParameter"><see cref="TypeParameterSyntax"/> to create the <see cref="TypeParameterData"/> from.</param>
		/// <param name="semanticModel"><see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the target <paramref name="typeParameter"/>.</param>
		/// <param name="defaultParamAttribute"><see cref="INamedTypeSymbol"/> that represents the <c>Durian.DefaultParamAttribute</c>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static TypeParameterData CreateFrom(TypeParameterSyntax typeParameter, SemanticModel semanticModel, INamedTypeSymbol defaultParamAttribute, CancellationToken cancellationToken = default)
		{
			ITypeParameterSymbol symbol = GetParameterSymbol(typeParameter, semanticModel, cancellationToken);

			(AttributeSyntax? attrSyntax, AttributeData? attrData) = GetParameterAttribute(typeParameter, symbol, semanticModel, defaultParamAttribute, cancellationToken);

			if (attrSyntax is not null)
			{
				ITypeSymbol? targetType = GetTargetType(attrData!);
				return new TypeParameterData(typeParameter, symbol, semanticModel, attrSyntax, targetType);
			}

			return new TypeParameterData(typeParameter, symbol, semanticModel);
		}

		/// <summary>
		/// Creates a new <see cref="TypeParameterData"/> based on the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ITypeParameterSymbol"/> to create the <see cref="TypeParameterData"/> from.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static TypeParameterData CreateFrom(ITypeParameterSymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			if (symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) is not TypeParameterSyntax syntax)
			{
				return default;
			}

			ImmutableArray<AttributeData> attributes = symbol.GetAttributes();
			AttributeData? data = attributes.FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.DefaultParamAttribute));
			AttributeSyntax? attrSyntax = data?.ApplicationSyntaxReference?.GetSyntax(cancellationToken) as AttributeSyntax;
			SemanticModel semanticModel = compilation.Compilation.GetSemanticModel(syntax.SyntaxTree);

			if (attrSyntax is not null)
			{
				ITypeSymbol? targetType = GetTargetType(data!);
				return new TypeParameterData(syntax, symbol, semanticModel, attrSyntax, targetType);
			}

			return new TypeParameterData(syntax, symbol, semanticModel);
		}

		/// <inheritdoc/>
		public static bool operator !=(in TypeParameterData first, in TypeParameterData second)
		{
			return !(first == second);
		}

		/// <inheritdoc/>
		public static bool operator ==(in TypeParameterData first, in TypeParameterData second)
		{
			bool areEqual = first.Attribute is null
				? second.Attribute is null
				: second.Attribute is not null && first.Attribute.IsEquivalentTo(second.Attribute);

			if (!areEqual)
			{
				return false;
			}

			return
				areEqual &&
				SymbolEqualityComparer.Default.Equals(first.TargetType, second.TargetType) &&
				SymbolEqualityComparer.Default.Equals(first.Symbol, second.Symbol) &&
				first.Syntax.IsEquivalentTo(second.Syntax) &&
				first.Parent.IsEquivalentTo(second.Parent);
		}

		/// <inheritdoc/>
		public override readonly bool Equals(object obj)
		{
			if (obj is TypeParameterData data)
			{
				return Equals(data);
			}

			return false;
		}

		/// <inheritdoc/>
		public readonly bool Equals(TypeParameterData other)
		{
			return other == this;
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			int hashCode = -2050731313;
			hashCode = (hashCode * -1521134295) + EqualityComparer<AttributeSyntax?>.Default.GetHashCode(Attribute);
			hashCode = (hashCode * -1521134295) + EqualityComparer<TypeParameterSyntax>.Default.GetHashCode(Syntax);
			hashCode = (hashCode * -1521134295) + EqualityComparer<CSharpSyntaxNode>.Default.GetHashCode(Parent);
			hashCode = (hashCode * -1521134295) + EqualityComparer<ITypeSymbol?>.Default.GetHashCode(TargetType);
			hashCode = (hashCode * -1521134295) + EqualityComparer<ITypeParameterSymbol>.Default.GetHashCode(Symbol);
			return hashCode;
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Symbol = \"{Symbol}\", TargetType = \"{TargetType?.ToString() ?? string.Empty}\"";
		}

		private static (AttributeSyntax?, AttributeData?) GetParameterAttribute(
			TypeParameterSyntax syntax,
			ISymbol symbol,
			SemanticModel semanticModel,
			INamedTypeSymbol defaultParamAttribute,
			CancellationToken cancellationToken
		)
		{
			AttributeSyntax? attrSyntax = semanticModel.GetAttribute(syntax, defaultParamAttribute, cancellationToken);

			if (attrSyntax is null)
			{
				return (null, null);
			}
			else
			{
				AttributeData? attrData = symbol.GetAttribute(attrSyntax, cancellationToken);

				if (attrData is null || attrData.ConstructorArguments.Length == 0)
				{
					return (null, null);
				}
				else
				{
					return (attrSyntax, attrData);
				}
			}
		}

		private static ITypeParameterSymbol GetParameterSymbol(TypeParameterSyntax syntax, SemanticModel semanticModel, CancellationToken cancellationToken)
		{
			ITypeParameterSymbol? symbol = semanticModel.GetDeclaredSymbol(syntax, cancellationToken);

			if (symbol is null)
			{
				throw new InvalidOperationException($"Syntax node {nameof(syntax)} is not a descendant node of the target node!");
			}
			else
			{
				return symbol;
			}
		}

		private static ITypeSymbol? GetTargetType(AttributeData data)
		{
			ImmutableArray<TypedConstant> ctor = data.ConstructorArguments;

			if (ctor.Length == 0)
			{
				return null;
			}

			return ctor[0].Value as ITypeSymbol;
		}
	}
}