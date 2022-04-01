// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics.CodeAnalysis;

namespace Durian.Analysis.Filters
{
	/// <summary>
	/// Provides a method for retrieving data required for syntax validation, like <see cref="SemanticModel"/> or <see cref="ISymbol"/>.
	/// </summary>
	public interface IValidationDataProvider
	{
		/// <summary>
		/// Checks whether a <see cref="SemanticModel"/> and a <see cref="ISymbol"/> can be created from the a given <see cref="CSharpSyntaxNode"/>.
		/// </summary>
		/// <param name="context"><see cref="ValidationDataProviderContext"/> that contains all data needed to retrieve the validation data.</param>
		bool GetValidationData(in ValidationDataProviderContext context);
	}
}
