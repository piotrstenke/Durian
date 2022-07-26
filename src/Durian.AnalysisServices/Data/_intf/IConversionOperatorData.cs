// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="IMethodSymbol"/> representing a conversion operator.
	/// </summary>
	public interface IConversionOperatorData : IMethodData, ISymbolOrMember<IMethodSymbol, IConversionOperatorData>
	{
		/// <summary>
		/// Target <see cref="ConversionOperatorDeclarationSyntax"/>.
		/// </summary>
		new ConversionOperatorDeclarationSyntax Declaration { get; }

		/// <summary>
		/// Determines whether this conversion operator is explicit.
		/// </summary>
		bool IsExplicit { get; }

		/// <summary>
		/// Determines whether this conversion operator is implicit.
		/// </summary>
		bool IsImplicit { get; }

		/// <summary>
		/// Parameter to convert.
		/// </summary>
		ISymbolOrMember<IParameterSymbol, IParameterData>? Parameter { get; }

		/// <summary>
		/// Creates a shallow copy of the current data.
		/// </summary>
		new IConversionOperatorData Clone();
	}
}