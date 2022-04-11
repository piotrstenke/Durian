// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

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

		TargetData[] ICopyFromMember.Targets => Array.Empty<TargetData>();

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromMethodData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="MethodDeclarationSyntax"/> this <see cref="CopyFromMethodData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="CopyFromMethodData"/>.</param>
		/// <param name="target">Target method.</param>
		/// <param name="patterns">A collection of patterns applied to the method using <c>Durian.PatternAttribute</c>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public CopyFromMethodData(MethodDeclarationSyntax declaration, ICompilationData compilation, IMethodSymbol target, PatternData[]? patterns = default) : base(declaration, compilation)
		{
			Target = target;
			Patterns = patterns;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromMethodData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="MethodDeclarationSyntax"/> this <see cref="CopyFromMethodData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="CopyFromMethodData"/>.</param>
		/// <param name="symbol"><see cref="IMethodSymbol"/> this <see cref="CopyFromMethodData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="target">Target method.</param>
		/// <param name="patterns">A collection of patterns applied to the method using <c>Durian.PatternAttribute</c>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="INamespaceSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		public CopyFromMethodData(
			MethodDeclarationSyntax declaration,
			ICompilationData compilation,
			IMethodSymbol symbol,
			SemanticModel semanticModel,
			IMethodSymbol target,
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
			Patterns = patterns;
		}
	}
}
