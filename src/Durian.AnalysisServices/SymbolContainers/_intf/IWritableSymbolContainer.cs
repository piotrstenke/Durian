// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Text;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// <see cref="ISymbolContainer"/> that enumerates the <see cref="ISymbol"/> and writes their formatted contents into a <see cref="StringBuilder"/>.
	/// </summary>
	public interface IWritableSymbolContainer : ISymbolContainer
	{
		/// <summary>
		/// Writes a string representation of the container into the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		void Build(StringBuilder builder);

		/// <summary>
		/// Combines all <see cref="string"/> returned by the <see cref="ISymbolContainer.GetNames"/> method.
		/// </summary>
		string ToString();
	}
}
