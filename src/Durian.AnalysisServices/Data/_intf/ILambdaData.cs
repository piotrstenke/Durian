// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="IMethodSymbol"/> representing a lambda/anonymous function.
	/// </summary>
	public interface ILambdaData : IMethodData, ISymbolOrMember<IMethodSymbol, ILambdaData>
	{
		/// <summary>
		/// Variables captured by this function.
		/// </summary>
		ISymbolContainer<ISymbol, IMemberData> CapturedVariables { get; }

		/// <summary>
		/// Target <see cref="AnonymousFunctionExpressionSyntax"/>.
		/// </summary>
		new AnonymousFunctionExpressionSyntax Declaration { get; }

		/// <summary>
		/// Creates a shallow copy of the current data.
		/// </summary>
		new ILambdaData Clone();
	}
}