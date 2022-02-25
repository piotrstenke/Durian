// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis
{
	/// <summary>
	/// <see cref="ISyntaxFilter"/> that filtrates <see cref="CSharpSyntaxNode"/>s for the specified <see cref="IDurianGenerator"/>.
	/// </summary>
	public interface IGeneratorSyntaxFilter : ISyntaxFilter, IEnumerable<IMemberData>
	{
		/// <summary>
		/// <see cref="IDurianGenerator"/> this <see cref="IGeneratorSyntaxFilter"/> takes the <see cref="CSharpSyntaxNode"/>s from.
		/// </summary>
		IDurianGenerator Generator { get; }

		/// <summary>
		/// Determines whether <see cref="ISymbol"/>s should be generated right after a <see cref="IMemberData"/> is created instead of waiting for all the <see cref="IMemberData"/>s to be returned.
		/// </summary>
		public bool IncludeGeneratedSymbols { get; }

		/// <summary>
		/// Decides, which <see cref="CSharpSyntaxNode"/>s collected by the <see cref="IDurianGenerator.SyntaxReceiver"/> of the <see cref="Generator"/> are valid and returns a collection of <see cref="IMemberData"/> based on those <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> that is used when filtrating the <see cref="IMemberData"/>s.</param>
		IEnumerable<IMemberData> Filtrate(in GeneratorExecutionContext context);
	}
}