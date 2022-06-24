// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using System;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="INamespaceOrTypeData"/>.
	/// </summary>
	public interface INamespaceOrTypeData : INamespaceData, ITypeData
	{
		/// <summary>
		/// <see cref="INamespaceOrTypeSymbol"/> associated with the <see cref="IMemberData.Declaration"/>.
		/// </summary>
		new INamespaceOrTypeSymbol Symbol { get; }

		/// <summary>
		/// Converts the current <see cref="INamespaceOrTypeData"/> to an actual <see cref="ITypeData"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">Current member is not a <see cref="ITypeData"/>.</exception>
		ITypeData ToType();

		/// <summary>
		/// Converts the current <see cref="INamespaceOrTypeData"/> to an actual <see cref="INamespaceData"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">Current member is not a <see cref="INamespaceData"/>.</exception>
		INamespaceData ToNamespace();
	}
}
