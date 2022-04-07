// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Cache;
using Durian.Analysis.Data;
using Durian.Analysis.Filters;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Durian.Analysis.CopyFrom
{
	/// <summary>
	/// <c>CopyFrom</c>-specific <see cref="SyntaxValidator{TContext}"/>.
	/// </summary>
	public abstract class CopyFromFilter : CachedSyntaxValidator, ICopyFromFilter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromFilter"/> class.
		/// </summary>>
		protected CopyFromFilter()
		{
		}

		public IEnumerable<ICopyFromMember> Filtrate(ICachedGeneratorPassContext<ICopyFromMember> context)
		{
			base.Filtrate(context)
		}

		public IEnumerator<ICopyFromMember> GetEnumerator(ICachedGeneratorPassContext<ICopyFromMember> context)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		protected abstract IEnumerable<CSharpSyntaxNode>? GetCandidateNodes(CopyFromSyntaxReceiver syntaxReceiver);

		/// <inheritdoc/>
		protected sealed override IEnumerable<CSharpSyntaxNode>? GetCandidateNodes(IDurianSyntaxReceiver syntaxReceiver)
		{
			if(syntaxReceiver is not CopyFromSyntaxReceiver s)
			{
				throw new ArgumentException($"Syntax receiver must be of type '{nameof(CopyFromSyntaxReceiver)}'", nameof(syntaxReceiver));
			}

			return GetCandidateNodes(s);
		}
	}
}
