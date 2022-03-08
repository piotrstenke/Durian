// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Durian.Analysis.CopyFrom
{
    /// <summary>
    /// Contains data of a <c>CopyFrom</c> target.
    /// </summary>
    public readonly struct TargetData : IEquatable<TargetData>
    {
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
        /// <see cref="ISymbol"/> of the target member.
        /// </summary>
        public ISymbol Symbol { get; }

        /// <summary>
        /// Array of usings that should be used when generating syntax tree.
        /// </summary>
        public string[]? Usings { get; }

        /// <inheritdoc cref="TargetData(ISymbol, int, TypeDeclarationSyntax?, string?, string[])"/>
        public TargetData(ISymbol symbol) : this(symbol, default, default, default)
        {
        }

        /// <inheritdoc cref="TargetData(ISymbol, int, TypeDeclarationSyntax?, string?, string[])"/>
        public TargetData(ISymbol symbol, int order, string[]? usings = default) : this(symbol, order, default, default, usings)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetData"/> struct.
        /// </summary>
        /// <param name="symbol"><see cref="ISymbol"/> of the target member.</param>
        /// <param name="partialPart">Partial part of the source type to copy the implementation from.</param>
        /// <param name="order">Order in which this target should be applied when comparing to other targets of the member.</param>
        /// <param name="partialPartName">Name of the partial part.</param>
        /// <param name="usings">Array of usings that should be used when generating syntax tree.</param>
        public TargetData(
            ISymbol symbol,
            int order,
            TypeDeclarationSyntax? partialPart,
            string? partialPartName,
            string[]? usings = default
        )
        {
            Symbol = symbol;
            PartialPart = partialPart;
            PartialPartName = partialPartName;
            Order = order;
            Usings = usings;
        }

        /// <inheritdoc/>
        public static bool operator ==(TargetData left, TargetData right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc/>
        public static bool operator !=(TargetData left, TargetData right)
        {
            return !(left == right);
        }

        /// <inheritdoc cref="Deconstruct(out ISymbol, out TypeDeclarationSyntax?, out string?, out int)"/>
        public void Deconstruct(out ISymbol symbol, out TypeDeclarationSyntax? partialPart)
        {
            symbol = Symbol;
            partialPart = PartialPart;
        }

        /// <inheritdoc cref="Deconstruct(out ISymbol, out TypeDeclarationSyntax?, out string?, out int)"/>
        public void Deconstruct(out ISymbol symbol, out TypeDeclarationSyntax? partialPart, out string? partialPartName)
        {
            Deconstruct(out symbol, out partialPart);
            partialPartName = PartialPartName;
        }

        /// <summary>
        /// Deconstructs the current object.
        /// </summary>
        /// <param name="symbol"><see cref="ISymbol"/> of the target member.</param>
        /// <param name="partialPart">Partial part of the source type to copy the implementation from.</param>
        /// <param name="partialPartName">Name of the partial part.</param>
        /// <param name="order">Order in which this target should be applied when comparing to other targets of the member.</param>
        public void Deconstruct(out ISymbol symbol, out TypeDeclarationSyntax? partialPart, out string? partialPartName, out int order)
        {
            Deconstruct(out symbol, out partialPart, out partialPartName);
            order = Order;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is TargetData other && Equals(other);
        }

        /// <inheritdoc/>
        public bool Equals(TargetData other)
        {
            return
                other.Order == Order &&
                other.PartialPartName == PartialPartName &&
                other.PartialPart == PartialPart &&
                SymbolEqualityComparer.Default.Equals(other.Symbol, Symbol);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 565389259;
            hashCode = (hashCode * -1521134295) + Order.GetHashCode();
            hashCode = (hashCode * -1521134295) + PartialPartName?.GetHashCode() ?? 0;
            hashCode = (hashCode * -1521134295) + PartialPart?.GetHashCode() ?? 0;
            hashCode = (hashCode * -1521134295) + SymbolEqualityComparer.Default.GetHashCode(Symbol);
            return hashCode;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Symbol} (\"{PartialPart}\")";
        }
    }
}