﻿using System.Collections.Generic;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Filtering;

/// <summary>
/// <see cref="ISyntaxFilter"/> that filters <see cref="SyntaxNode"/>s for the specified <see cref="IDurianGenerator"/>.
/// </summary>
public interface IGeneratorSyntaxFilter : ISyntaxFilter
{
	/// <summary>
	/// Determines whether <see cref="ISymbol"/>s should be generated right after a <see cref="IMemberData"/> is created instead of waiting for all the <see cref="IMemberData"/>s to be returned.
	/// </summary>
	public bool IncludeGeneratedSymbols { get; }

	/// <summary>
	/// Decides, which <see cref="SyntaxNode"/>s collected by a <see cref="IDurianSyntaxReceiver"/> of the <paramref name="context"/> are valid and returns a collection of <see cref="IMemberData"/> based on those <see cref="SyntaxNode"/>s.
	/// </summary>
	/// <param name="context"><see cref="IGeneratorPassContext"/> that is used when filtrating the <see cref="IMemberData"/>s.</param>
	IEnumerable<IMemberData> Filter(IGeneratorPassContext context);

	/// <summary>
	/// Iterates through nodes collected by the <see cref="IGeneratorPassContext.SyntaxReceiver"/>.
	/// </summary>
	/// <param name="context"><see cref="IGeneratorPassContext"/> that is used when filtrating the <see cref="IMemberData"/>s.</param>
	IEnumerator<IMemberData> GetEnumerator(IGeneratorPassContext context);
}
