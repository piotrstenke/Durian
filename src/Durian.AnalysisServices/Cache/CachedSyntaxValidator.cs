// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Durian.Analysis.Filters;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Durian.Analysis.Cache
{
	/// <summary>
	/// <see cref="ISyntaxFilter"/> that validates the filtrated nodes.
	/// If the value associated with a <see cref="CSharpSyntaxNode"/> is present in the <see cref="CachedGeneratorExecutionContext{T}"/>, it is re-used instead of creating a new one.
	/// </summary>
	public abstract class CachedSyntaxValidator : SyntaxValidator, ICachedGeneratorSyntaxFilterWithDiagnostics<IMemberData>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CachedSyntaxValidator"/> class.
		/// </summary>
		protected CachedSyntaxValidator()
		{
		}

		/// <inheritdoc/>
		public IEnumerable<IMemberData> Filtrate(ICachedGeneratorPassContext<IMemberData> context)
		{
			if (GetCandidateNodes(context.SyntaxReceiver) is not IEnumerable<CSharpSyntaxNode> list)
			{
				return Array.Empty<IMemberData>();
			}

			return Yield();

			IEnumerable<IMemberData> Yield()
			{
				foreach (CSharpSyntaxNode node in list)
				{
					if (node is null)
					{
						continue;
					}

					if (context.OriginalContext.TryGetCachedValue(node.GetLocation().GetLineSpan(), out IMemberData? data) ||
						ValidateAndCreate(node, context.TargetCompilation, out data, context.CancellationToken))
					{
						yield return data;
					}
				}
			}
		}

		/// <inheritdoc/>
		public virtual IEnumerator<IMemberData> GetEnumerator(ICachedGeneratorPassContext<IMemberData> context)
		{
			if (GetCandidateNodes(context.SyntaxReceiver) is not IEnumerable<CSharpSyntaxNode> list)
			{
				return Enumerable.Empty<IMemberData>().GetEnumerator();
			}

			ref readonly CachedData<IMemberData> cache = ref context.OriginalContext.GetCachedData();

			return context.Generator.GetFilterMode() switch
			{
				FilterMode.Diagnostics => new CachedFilterEnumeratorWithDiagnostics<IMemberData>(list, context.TargetCompilation, this, GetDiagnosticReceiver(context), in cache),
				FilterMode.Logs => new CachedLoggableFilterEnumerator<IMemberData>(list, context.TargetCompilation, this, GetLogReceiver(context, false), context.FileNameProvider, in cache),
				FilterMode.Both => new CachedLoggableFilterEnumerator<IMemberData>(list, context.TargetCompilation, this, GetLogReceiver(context, true), context.FileNameProvider, in cache),
				_ => new CachedFilterEnumerator<IMemberData>(list, context.TargetCompilation, this, in cache)
			};
		}
	}
}
