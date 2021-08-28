// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Durian.Analysis.CodeFixes
{
	/// <summary>
	/// Base class for all Durian code fixes.
	/// </summary>
	public abstract class DurianCodeFixBase : CodeFixProvider
	{
		/// <inheritdoc/>
		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get
			{
				return ImmutableArray.Create(GetSupportedDiagnostics().Select(d => d.Id).ToArray());
			}
		}

		/// <summary>
		/// Id of this code fix.
		/// </summary>
		public virtual string Id => Title;

		/// <summary>
		/// Title of this code fix.
		/// </summary>
		public abstract string Title { get; }

		/// <summary>
		/// Creates a new instance of the <see cref="DurianCodeFixBase"/> class.
		/// </summary>
		protected DurianCodeFixBase()
		{
		}

		/// <inheritdoc/>
		public override FixAllProvider? GetFixAllProvider()
		{
			return WellKnownFixAllProviders.BatchFixer;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return Title;
		}

		/// <summary>
		/// Returns the <see cref="DiagnosticDescriptor"/>s supported by this <see cref="DurianCodeFix{T}"/>.
		/// </summary>
		protected abstract DiagnosticDescriptor[] GetSupportedDiagnostics();
	}
}
