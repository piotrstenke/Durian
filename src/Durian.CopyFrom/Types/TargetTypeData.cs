using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CopyFrom.Types;

/// <summary>
/// Contains data of a <c>CopyFrom</c> type target.
/// </summary>
public sealed class TargetTypeData : IEquatable<TargetTypeData>, ICloneable
{
	/// <summary>
	/// Specifies which non-standard nodes should also be copied.
	/// </summary>
	public AdditionalNodes AdditionalNodes { get; }

	/// <summary>
	/// Determines whether to automatically replace name of the target type in constructor, destructor and operator declarations.
	/// </summary>
	public bool HandleSpecialMembers { get; }

	/// <summary>
	/// Order in which this target should be applied when comparing to other targets of the member.
	/// </summary>
	public int Order { get; }

	/// <summary>
	/// Partial part of the source type to copy the implementation from.
	/// </summary>
	public TypeDeclarationSyntax? PartialPart { get; }

	/// <summary>
	/// Name of the partial part.
	/// </summary>
	public string? PartialPartName { get; }

	/// <summary>
	/// <see cref="INamedTypeSymbol"/> of the target member.
	/// </summary>
	public INamedTypeSymbol Symbol { get; }

	/// <summary>
	/// Array of usings that should be used when generating syntax tree.
	/// </summary>
	public string[]? Usings { get; }

	/// <inheritdoc cref="TargetTypeData(INamedTypeSymbol, int, AdditionalNodes, string[], TypeDeclarationSyntax?, string?, bool)"/>
	public TargetTypeData(INamedTypeSymbol symbol) : this(symbol, default, default, default)
	{
	}

	/// <inheritdoc cref="TargetTypeData(INamedTypeSymbol, int, AdditionalNodes, string[], TypeDeclarationSyntax?, string?, bool)"/>
	public TargetTypeData(
		INamedTypeSymbol symbol,
		int order,
		AdditionalNodes additionalNodes,
		string[]? usings
	) : this(symbol, order, additionalNodes, usings, default, default, default)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="TargetTypeData"/> struct.
	/// </summary>
	/// <param name="symbol"><see cref="INamedTypeSymbol"/> of the target member.</param>
	/// <param name="order">Order in which this target should be applied when comparing to other targets of the member.</param>
	/// <param name="additionalNodes">Specifies which non-standard nodes should also be copied.</param>
	/// <param name="usings">Array of usings that should be used when generating syntax tree.</param>
	/// <param name="partialPart">Partial part of the source type to copy the implementation from.</param>
	/// <param name="partialPartName">Name of the partial part.</param>
	/// <param name="handleSpecialMembers">Determines whether to automatically replace name of the target type in constructor, destructor and operator declarations.</param>
	public TargetTypeData(
		INamedTypeSymbol symbol,
		int order,
		AdditionalNodes additionalNodes,
		string[]? usings,
		TypeDeclarationSyntax? partialPart,
		string? partialPartName,
		bool handleSpecialMembers
	)
	{
		Symbol = symbol;
		PartialPart = partialPart;
		PartialPartName = partialPartName;
		Order = order;
		HandleSpecialMembers = handleSpecialMembers;
		Usings = usings;
		AdditionalNodes = additionalNodes;
	}

	/// <inheritdoc/>
	public static bool operator !=(TargetTypeData left, TargetTypeData right)
	{
		return !(left == right);
	}

	/// <inheritdoc/>
	public static bool operator ==(TargetTypeData left, TargetTypeData right)
	{
		return left.Equals(right);
	}

	/// <inheritdoc cref="ICloneable.Clone"/>
	public TargetTypeData Clone()
	{
		return WithSymbol(Symbol);
	}

	/// <inheritdoc/>
	public override bool Equals(object? obj)
	{
		return obj is TargetTypeData other && Equals(other);
	}

	/// <inheritdoc/>
	public bool Equals(TargetTypeData other)
	{
		return
			other.Order == Order &&
			other.HandleSpecialMembers == HandleSpecialMembers &&
			other.AdditionalNodes == AdditionalNodes &&
			other.PartialPartName == PartialPartName &&
			other.PartialPart == PartialPart &&
			SymbolEqualityComparer.Default.Equals(other.Symbol, Symbol) &&
			AnalysisUtilities.ArraysAreEqual(other.Usings, Usings);
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		int hashCode = 565389259;
		hashCode = (hashCode * -1521134295) + Order.GetHashCode();
		hashCode = (hashCode * -1521134295) + HandleSpecialMembers.GetHashCode();
		hashCode = (hashCode * -1521134295) + AdditionalNodes.GetHashCode();
		hashCode = (hashCode * -1521134295) + PartialPartName?.GetHashCode() ?? 0;
		hashCode = (hashCode * -1521134295) + PartialPart?.GetHashCode() ?? 0;
		hashCode = (hashCode * -1521134295) + SymbolEqualityComparer.Default.GetHashCode(Symbol);
		hashCode = (hashCode * -1521134295) + AnalysisUtilities.GetArrayHashCode(Usings);
		return hashCode;
	}

	/// <inheritdoc/>
	public override string ToString()
	{
		if (string.IsNullOrWhiteSpace(PartialPartName))
		{
			return Symbol.ToString();
		}

		return $"{Symbol} (\"{PartialPart}\")";
	}

	/// <summary>
	/// Returns a new instance of the <see cref="TargetTypeData"/> class being a copy of the current instance, but with the specified <paramref name="symbol"/> instead.
	/// </summary>
	/// <param name="symbol"><see cref="INamedTypeSymbol"/> of the target member.</param>
	public TargetTypeData WithSymbol(INamedTypeSymbol symbol)
	{
		return new(symbol, Order, AdditionalNodes, Usings, PartialPart, PartialPartName, HandleSpecialMembers);
	}

	object ICloneable.Clone()
	{
		return Clone();
	}
}
