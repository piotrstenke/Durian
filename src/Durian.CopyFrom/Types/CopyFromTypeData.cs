// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CopyFrom.Types
{
	/// <summary>
	/// <see cref="TypeData{TDeclaration}"/> that contains additional information needed by the <see cref="CopyFromGenerator"/>.
	/// </summary>
	public sealed class CopyFromTypeData : TypeData<TypeDeclarationSyntax>, ICopyFromMember
	{
		/// <summary>
		/// <see cref="INamedTypeSymbol"/>s generation of this type depends on.
		/// </summary>
		public INamedTypeSymbol[]? Dependencies { get; }

		/// <inheritdoc cref="MemberData.ParentCompilation"/>
		public new CopyFromCompilationData ParentCompilation => (base.ParentCompilation as CopyFromCompilationData)!;

		/// <summary>
		/// A collection of patterns applied to the type using <c>Durian.PatternAttribute</c>.
		/// </summary>
		public PatternData[]? Patterns { get; }

		/// <summary>
		/// A collection of target types.
		/// </summary>
		public TargetTypeData[] Targets { get; }

		ISymbol[]? ICopyFromMember.Dependencies => Dependencies;

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromTypeData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="TypeDeclarationSyntax"/> this <see cref="CopyFromTypeData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="CopyFromCompilationData"/> of this <see cref="CopyFromTypeData"/>.</param>
		/// <param name="targets">A collection of target types.</param>
		/// <param name="dependencies"><see cref="INamedTypeSymbol"/>s generation of this type depends on.</param>
		/// <param name="patterns">A collection of patterns applied to the type using <c>Durian.PatternAttribute</c>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public CopyFromTypeData(
			TypeDeclarationSyntax declaration,
			CopyFromCompilationData compilation,
			TargetTypeData[] targets,
			INamedTypeSymbol[]? dependencies = default,
			PatternData[]? patterns = default
		) : base(declaration, compilation)
		{
			Targets = targets;
			Dependencies = dependencies;
			Patterns = patterns;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromTypeData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="TypeDeclarationSyntax"/> this <see cref="CopyFromTypeData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="CopyFromCompilationData"/> of this <see cref="CopyFromTypeData"/>.</param>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> this <see cref="CopyFromTypeData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="targets">A collection of target types.</param>
		/// <param name="dependencies"><see cref="INamedTypeSymbol"/>s generation of this type depends on.</param>
		/// <param name="patterns">A collection of patterns applied to the type using <c>Durian.PatternAttribute</c>.</param>
		/// <param name="modifiers">A collection of all modifiers applied to the <paramref name="symbol"/>.</param>
		/// <param name="partialDeclarations">A collection of <see cref="TypeDeclarationSyntax"/> that represent the partial declarations of the target <paramref name="symbol"/>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="INamespaceSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		public CopyFromTypeData(
			TypeDeclarationSyntax declaration,
			CopyFromCompilationData compilation,
			INamedTypeSymbol symbol,
			SemanticModel semanticModel,
			TargetTypeData[] targets,
			INamedTypeSymbol[]? dependencies = default,
			PatternData[]? patterns = default,
			string[]? modifiers = null,
			IEnumerable<TypeDeclarationSyntax>? partialDeclarations = null,
			IEnumerable<ITypeData>? containingTypes = null,
			IEnumerable<INamespaceSymbol>? containingNamespaces = null,
			IEnumerable<AttributeData>? attributes = null
		) : base(
			declaration,
			compilation,
			symbol,
			semanticModel,
			modifiers,
			partialDeclarations,
			containingTypes,
			containingNamespaces,
			attributes
		)
		{
			Targets = targets;
			Dependencies = dependencies;
			Patterns = patterns;
		}
	}
}
