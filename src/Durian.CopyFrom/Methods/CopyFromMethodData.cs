// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CopyFrom.Methods
{
	/// <summary>
	/// <see cref="MethodData"/> that contains additional information needed by the <see cref="CopyFromGenerator"/>.
	/// </summary>
	public sealed class CopyFromMethodData : MethodData, ICopyFromMember
	{
		/// <summary>
		/// A collection of patterns applied to the method using <c>Durian.PatternAttribute</c>.
		/// </summary>
		public PatternData[]? Patterns { get; }

		/// <summary>
		/// Target method.
		/// </summary>
		public IMethodSymbol Target { get; }

		/// <summary>
		/// <see cref="IMethodSymbol"/>s generation of this type depends on.
		/// </summary>
		public IMethodSymbol[]? Dependencies { get; }

		ISymbol[]? ICopyFromMember.Dependencies => Dependencies;

		TargetData[] ICopyFromMember.Targets => Array.Empty<TargetData>();

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromMethodData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="MethodDeclarationSyntax"/> this <see cref="CopyFromMethodData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="CopyFromCompilationData"/> of this <see cref="CopyFromMethodData"/>.</param>
		/// <param name="target">Target method.</param>
		/// <param name="dependencies"><see cref="IMethodSymbol"/>s generation of this type depends on.</param>
		/// <param name="patterns">A collection of patterns applied to the method using <c>Durian.PatternAttribute</c>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public CopyFromMethodData(
			MethodDeclarationSyntax declaration,
			CopyFromCompilationData compilation,
			IMethodSymbol target,
			IMethodSymbol[]? dependencies,
			PatternData[]? patterns = default
		) : base(declaration, compilation)
		{
			Target = target;
			Dependencies = dependencies;
			Patterns = patterns;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromMethodData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="MethodDeclarationSyntax"/> this <see cref="CopyFromMethodData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="CopyFromCompilationData"/> of this <see cref="CopyFromMethodData"/>.</param>
		/// <param name="symbol"><see cref="IMethodSymbol"/> this <see cref="CopyFromMethodData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="target">Target method.</param>
		/// <param name="dependencies"><see cref="IMethodSymbol"/>s generation of this type depends on.</param>
		/// <param name="patterns">A collection of patterns applied to the method using <c>Durian.PatternAttribute</c>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="INamespaceSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		public CopyFromMethodData(
			MethodDeclarationSyntax declaration,
			CopyFromCompilationData compilation,
			IMethodSymbol symbol,
			SemanticModel semanticModel,
			IMethodSymbol target,
			IMethodSymbol[]? dependencies,
			PatternData[]? patterns = default,
			IEnumerable<ITypeData>? containingTypes = default,
			IEnumerable<INamespaceSymbol>? containingNamespaces = default,
			IEnumerable<AttributeData>? attributes = default
		) : base(
			declaration,
			compilation,
			symbol,
			semanticModel,
			containingTypes,
			containingNamespaces,
			attributes
		)
		{
			Target = target;
			Dependencies = dependencies;
			Patterns = patterns;
		}
	}
}
