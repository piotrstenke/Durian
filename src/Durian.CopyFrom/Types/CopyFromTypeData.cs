// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace Durian.Analysis.CopyFrom.Types
{
	/// <summary>
	/// <see cref="TypeData{TDeclaration}"/> that contains additional information needed by the <see cref="CopyFromGenerator"/>.
	/// </summary>
	public sealed class CopyFromTypeData : TypeData<TypeDeclarationSyntax>, ICopyFromMember
	{
		/// <summary>
		/// A collection of patterns applied to the type using <c>Durian.PatternAttribute</c>.
		/// </summary>
		public PatternData[]? Patterns { get; }

		/// <summary>
		/// A collection of target types.
		/// </summary>
		public TargetData[] Targets { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromTypeData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="TypeDeclarationSyntax"/> this <see cref="CopyFromTypeData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="CopyFromTypeData"/>.</param>
		/// <param name="targets">A collection of target types.</param>
		/// <param name="patterns">A collection of patterns applied to the type using <c>Durian.PatternAttribute</c>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public CopyFromTypeData(
			TypeDeclarationSyntax declaration,
			ICompilationData compilation,
			TargetData[] targets,
			PatternData[]? patterns = default
		) : base(declaration, compilation)
		{
			Targets = targets;
			Patterns = patterns;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromTypeData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="TypeDeclarationSyntax"/> this <see cref="CopyFromTypeData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="CopyFromTypeData"/>.</param>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> this <see cref="CopyFromTypeData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="targets">A collection of target types.</param>
		/// <param name="patterns">A collection of patterns applied to the type using <c>Durian.PatternAttribute</c>.</param>
		/// <param name="partialDeclarations">A collection of <see cref="TypeDeclarationSyntax"/> that represent the partial declarations of the target <paramref name="symbol"/>.</param>
		/// <param name="modifiers">A collection of all modifiers applied to the <paramref name="symbol"/>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="INamespaceSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		public CopyFromTypeData(
			TypeDeclarationSyntax declaration,
			ICompilationData compilation,
			INamedTypeSymbol symbol,
			SemanticModel semanticModel,
			TargetData[] targets,
			PatternData[]? patterns = default,
			IEnumerable<TypeDeclarationSyntax>? partialDeclarations = null,
			IEnumerable<SyntaxToken>? modifiers = null,
			IEnumerable<ITypeData>? containingTypes = null,
			IEnumerable<INamespaceSymbol>? containingNamespaces = null,
			IEnumerable<AttributeData>? attributes = null
		) : base(
			declaration,
			compilation,
			symbol,
			semanticModel,
			partialDeclarations,
			modifiers,
			containingTypes,
			containingNamespaces,
			attributes
		)
		{
			Targets = targets;
			Patterns = patterns;
		}
	}
}
