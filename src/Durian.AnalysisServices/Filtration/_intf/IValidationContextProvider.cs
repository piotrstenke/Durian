// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Filtration
{
	/// <summary>
	/// Provides a method for retrieving data required for syntax validation, like <see cref="SemanticModel"/> or <see cref="ISymbol"/>.
	/// </summary>
	/// <typeparam name="T">Type of target <see cref="ISyntaxValidationContext"/>.</typeparam>
	public interface IValidationContextProvider<T> where T : ISyntaxValidationContext
	{
		/// <summary>
		/// Checks whether a <see cref="SemanticModel"/> and a <see cref="ISymbol"/> can be created from the a given <see cref="SyntaxNode"/>.
		/// </summary>
		/// <param name="context"><see cref="PreValidationContext"/> that contains all data necessary to retrieve the required data.</param>
		/// <param name="data">Returned data.</param>
		bool TryGetContext(in PreValidationContext context, [NotNullWhen(true)] out T? data);
	}
}
