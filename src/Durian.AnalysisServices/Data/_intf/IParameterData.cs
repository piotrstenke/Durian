// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="IParameterSymbol"/>.
	/// </summary>
	public interface IParameterData : IMemberData
	{
		/// <summary>
		/// Target <see cref="BaseParameterSyntax"/>.
		/// </summary>
		new BaseParameterSyntax Declaration { get; }

		/// <summary>
		/// <see cref="IParameterSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		new IParameterSymbol Symbol { get; }
	}
}
