// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// <see cref="ICompilationData"/> that collects <see cref="ISymbol"/>s..
	/// </summary>
	public interface ICompilationDataWithSymbols : ICompilationData
	{
		/// <summary>
		/// <see cref="INamedTypeSymbol"/> that represents the <see cref="Durian.Generator.DurianGeneratedAttribute"/>..
		/// </summary>
		public INamedTypeSymbol DurianGeneratedAttribute { get; }

		/// <summary>
		/// Returns a <see cref="INamedTypeSymbol"/> that represents the <see cref="Generator.EnableModuleAttribute"/>..
		/// </summary>
		public INamedTypeSymbol EnableModuleAttribute { get; }

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> that represents the <see cref="System.CodeDom.Compiler.GeneratedCodeAttribute"/>.
		/// </summary>
		public INamedTypeSymbol GeneratedCodeAttribute { get; }

		/// <summary>
		/// Resets all collected <see cref="ISymbol"/>s.
		/// </summary>
		public void Reset();
	}
}
