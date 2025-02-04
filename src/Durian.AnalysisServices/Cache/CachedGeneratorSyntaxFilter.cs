﻿using System.Collections.Generic;
using Durian.Analysis.Data;
using Durian.Analysis.Filtering;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Cache;

/// <summary>
/// <see cref="ISyntaxFilter"/> that filters <see cref="SyntaxNode"/>s for the specified <see cref="IDurianGenerator"/>.
/// If the value associated with a <see cref="SyntaxNode"/> is present in the <see cref="CachedGeneratorExecutionContext{T}"/>, it is re-used instead of creating a new one.
/// </summary>
public abstract class CachedGeneratorSyntaxFilter : GeneratorSyntaxFilter, ICachedGeneratorSyntaxFilter<IMemberData>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CachedGeneratorSyntaxFilter"/>.
	/// </summary>
	protected CachedGeneratorSyntaxFilter()
	{
	}

	/// <inheritdoc/>
	public abstract IEnumerable<IMemberData> Filter(ICachedGeneratorPassContext<IMemberData> context);

	/// <inheritdoc/>
	public virtual IEnumerator<IMemberData> GetEnumerator(ICachedGeneratorPassContext<IMemberData> context)
	{
		return Filter(context).GetEnumerator();
	}
}
