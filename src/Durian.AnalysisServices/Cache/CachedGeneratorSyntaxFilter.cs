// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Durian.Analysis.Filters;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Durian.Analysis.Cache
{
	/// <summary>
	/// <see cref="ISyntaxFilter"/> that filtrates <see cref="CSharpSyntaxNode"/>s for the specified <see cref="IDurianGenerator"/>.
	/// If the value associated with a <see cref="CSharpSyntaxNode"/> is present in the <see cref="CachedGeneratorExecutionContext{T}"/>, it is re-used instead of creating a new one.
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
		public abstract IEnumerable<IMemberData> Filtrate(ICachedGeneratorPassContext<IMemberData> context);

		/// <inheritdoc/>
		public virtual IEnumerator<IMemberData> GetEnumerator(ICachedGeneratorPassContext<IMemberData> context)
		{
			return Filtrate(context).GetEnumerator();
		}
	}
}
