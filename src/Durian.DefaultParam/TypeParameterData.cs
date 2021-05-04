using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Durian.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;

namespace Durian.DefaultParam
{
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	public readonly struct TypeParameterData : IEquatable<TypeParameterData>
	{
		public readonly AttributeSyntax? Attribute { get; }
		public readonly TypeParameterSyntax Syntax { get; }
		public readonly Location Location => Syntax.GetLocation();
		public readonly SemanticModel SemanticModel { get; }
		public readonly MemberDeclarationSyntax Parent { get; }
		public readonly INamedTypeSymbol? TargetType { get; }
		public readonly ITypeParameterSymbol Symbol { get; }
		public readonly string Name => Symbol.Name;
		public readonly bool IsDefaultParam => Attribute is not null && Symbol is not null;

		public TypeParameterData(TypeParameterSyntax syntax, ITypeParameterSymbol symbol, SemanticModel semanticModel) : this(syntax, symbol, semanticModel, null, null)
		{
		}

		public TypeParameterData(TypeParameterSyntax syntax, ITypeParameterSymbol symbol, SemanticModel semanticModel, AttributeSyntax? attribute, INamedTypeSymbol? targetType)
		{
			Syntax = syntax;
			Symbol = symbol;
			Attribute = attribute;
			TargetType = targetType;
			Parent = (MemberDeclarationSyntax)syntax.Parent!.Parent!;
			SemanticModel = semanticModel;
		}

		public static TypeParameterData CreateFrom(TypeParameterSyntax typeParameter, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			return CreateFrom(typeParameter, compilation.Compilation.GetSemanticModel(typeParameter.SyntaxTree), compilation, cancellationToken);
		}

		public static TypeParameterData CreateFrom(TypeParameterSyntax typeParameter, SemanticModel semanticModel, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			return CreateFrom(typeParameter, semanticModel, compilation.Attribute!, cancellationToken);
		}

		public static TypeParameterData CreateFrom(TypeParameterSyntax typeParameter, SemanticModel semanticModel, INamedTypeSymbol defaultParamAttribute, CancellationToken cancellationToken = default)
		{
			ITypeParameterSymbol symbol = GetParameterSymbol(typeParameter, semanticModel, cancellationToken);

			(AttributeSyntax? attrSyntax, AttributeData? attrData) = GetParameterAttribute(typeParameter, symbol, semanticModel, defaultParamAttribute, cancellationToken);

			if (attrSyntax is not null)
			{
				return new TypeParameterData(typeParameter, symbol, semanticModel, attrSyntax, GetTargetType(attrData!));
			}

			return new TypeParameterData(typeParameter, symbol, semanticModel);
		}

		public static TypeParameterData CreateFrom(ITypeParameterSymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			if (symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) is not TypeParameterSyntax syntax)
			{
				return default;
			}

			ImmutableArray<AttributeData> attributes = symbol.GetAttributes();
			AttributeData? data = attributes.FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.Attribute));
			AttributeSyntax? attrSyntax = data?.ApplicationSyntaxReference?.GetSyntax(cancellationToken) as AttributeSyntax;
			SemanticModel semanticModel = compilation.Compilation.GetSemanticModel(syntax.SyntaxTree);

			if (attrSyntax is not null)
			{
				return new TypeParameterData(syntax, symbol, semanticModel, attrSyntax, GetTargetType(data!));
			}

			return new TypeParameterData(syntax, symbol, semanticModel);
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
				AttributeData? attrData = symbol.GetAttributeData(attrSyntax);

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

		private static INamedTypeSymbol? GetTargetType(AttributeData data)
		{
			return data.ConstructorArguments.FirstOrDefault().Value as INamedTypeSymbol;
		}

		public override bool Equals(object obj)
		{
			if (obj is TypeParameterData data)
			{
				return Equals(data);
			}

			return false;
		}

		public bool IsEquivalentTo(in TypeParameterData other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			int hashCode = -2050731313;
			hashCode = (hashCode * -1521134295) + EqualityComparer<AttributeSyntax?>.Default.GetHashCode(Attribute);
			hashCode = (hashCode * -1521134295) + EqualityComparer<TypeParameterSyntax>.Default.GetHashCode(Syntax);
			hashCode = (hashCode * -1521134295) + EqualityComparer<MemberDeclarationSyntax>.Default.GetHashCode(Parent);
			hashCode = (hashCode * -1521134295) + EqualityComparer<INamedTypeSymbol?>.Default.GetHashCode(TargetType);
			hashCode = (hashCode * -1521134295) + EqualityComparer<ITypeParameterSymbol>.Default.GetHashCode(Symbol);
			return hashCode;
		}

		private string GetDebuggerDisplay()
		{
			return $"Symbol = \"{Symbol}\", TargetType = \"{TargetType?.ToString() ?? string.Empty}\"";
		}

		bool IEquatable<TypeParameterData>.Equals(TypeParameterData other)
		{
			return IsEquivalentTo(in other);
		}

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

		public static bool operator !=(in TypeParameterData first, in TypeParameterData second)
		{
			return !(first == second);
		}
	}
}
