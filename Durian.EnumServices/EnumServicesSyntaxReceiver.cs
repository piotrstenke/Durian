// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.EnumServices
{
	/// <summary>
	/// Collects <see cref="CSharpSyntaxNode"/>s that are potential targets for the <see cref="EnumServicesGenerator"/>.
	/// </summary>
	public class EnumServicesSyntaxReceiver : IDurianSyntaxReceiver
	{
		/// <summary>
		/// <see cref="EnumDeclarationSyntax"/>es that potentially have the <see cref="EnumServicesAttribute"/> applied.
		/// </summary>
		public List<EnumDeclarationSyntax> Enums { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="EnumServicesSyntaxReceiver"/> class.
		/// </summary>
		public EnumServicesSyntaxReceiver()
		{
			Enums = new List<EnumDeclarationSyntax>();
		}

		/// <inheritdoc/>
		public bool IsEmpty()
		{
			return Enums.Count == 0;
		}

		/// <inheritdoc/>
		public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
		{
			if(syntaxNode is EnumDeclarationSyntax e && e.AttributeLists.Any())
			{
				Enums.Add(e);
			}
		}

		IEnumerable<CSharpSyntaxNode> INodeProvider.GetNodes()
		{
			return Enums;
		}
	}
}
