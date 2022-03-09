﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;

namespace Durian.TestServices
{
	/// <summary>
	/// A simple proxy class that implements the <see cref="IDurianSyntaxReceiver"/> interface.
	/// </summary>
	public sealed class SyntaxReceiverProxy : IDurianSyntaxReceiver
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxReceiverProxy"/> class.
		/// </summary>
		public SyntaxReceiverProxy()
		{
		}

		/// <inheritdoc/>
		public IEnumerable<CSharpSyntaxNode> GetNodes()
		{
			return Array.Empty<CSharpSyntaxNode>();
		}

		/// <inheritdoc/>
		public bool IsEmpty()
		{
			return true;
		}

		/// <inheritdoc/>
		public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
		{
			// Do nothing
		}
	}
}