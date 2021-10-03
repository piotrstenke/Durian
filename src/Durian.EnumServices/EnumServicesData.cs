// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Durian.Analysis.EnumServices
{
	/// <summary>
	/// Contains data about a target for the <see cref="EnumServicesGenerator"/>.
	/// </summary>
	public class EnumServicesData : EnumData
	{
		/// <summary>
		/// Configures how the extension methods for the enum are generated.
		/// </summary>
		public EnumServicesConfiguration Configuration { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="EnumServicesData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="EnumDeclarationSyntax"/> this <see cref="EnumServicesData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="EnumServicesCompilationData"/> of this <see cref="EnumServicesData"/>.</param>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> this <see cref="EnumServicesData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="configuration">Configures how the extension methods for the enum are generated.</param>
		/// <param name="modifiers">A collection of all modifiers applied to the <paramref name="symbol"/>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="INamespaceSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		public EnumServicesData(
			EnumDeclarationSyntax declaration,
			EnumServicesCompilationData compilation,
			INamedTypeSymbol symbol,
			SemanticModel semanticModel,
			EnumServicesConfiguration? configuration = null,
			IEnumerable<SyntaxToken>? modifiers = null,
			IEnumerable<ITypeData>? containingTypes = null,
			IEnumerable<INamespaceSymbol>? containingNamespaces = null,
			IEnumerable<AttributeData>? attributes = null
		) : base(
			declaration,
			compilation,
			symbol,
			semanticModel,
			modifiers,
			containingTypes,
			containingNamespaces,
			attributes
		)
		{
			Configuration = configuration ?? compilation.GlobalConfiguration;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EnumServicesData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="MethodDeclarationSyntax"/> this <see cref="EnumServicesData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="EnumServicesCompilationData"/> of this <see cref="EnumServicesData"/>.</param>
		/// <param name="configuration">Configures how the extension methods for the enum are generated.
		/// If set to <see langword="null"/>, global configuration specified in the <paramref name="compilation"/> is used instead.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public EnumServicesData(
			EnumDeclarationSyntax declaration,
			EnumServicesCompilationData compilation,
			EnumServicesConfiguration? configuration = null) : base(declaration, compilation)
		{
			Configuration = configuration ?? compilation.GlobalConfiguration;
		}
	}
}
