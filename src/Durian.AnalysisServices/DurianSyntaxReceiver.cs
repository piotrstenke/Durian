// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	/// <inheritdoc cref="IDurianSyntaxReceiver"/>
	public abstract class DurianSyntaxReceiver : IDurianSyntaxReceiver
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DurianSyntaxReceiver"/> class.
		/// </summary>
		protected DurianSyntaxReceiver()
		{
		}

		/// <inheritdoc/>
		public abstract bool IsEmpty();

		/// <inheritdoc/>
		public abstract IEnumerable<SyntaxNode> GetNodes();

		/// <inheritdoc cref="ISyntaxReceiver.OnVisitSyntaxNode(SyntaxNode)"/>
		public abstract bool OnVisitSyntaxNode(SyntaxNode syntaxNode);

		void ISyntaxReceiver.OnVisitSyntaxNode(SyntaxNode syntaxNode)
		{
			OnVisitSyntaxNode(syntaxNode);
		}
	}
}
