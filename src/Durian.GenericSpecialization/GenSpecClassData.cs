// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.GenericSpecialization
{
	/// <summary>
	/// <see cref="ClassData"/> that contains additional information needed by the <see cref="GenericSpecializationGenerator"/>.
	/// </summary>
	public sealed class GenSpecClassData : ClassData
	{
		/// <summary>
		/// Name of the generated specialization interface.
		/// </summary>
		public string InterfaceName { get; }

		/// <summary>
		/// Name of the class that is the main implementation of the type.
		/// </summary>
		public string TemplateName { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="GenSpecClassData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="TypeDeclarationSyntax"/> this <see cref="GenSpecClassData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="GenSpecCompilationData"/> of this <see cref="GenSpecClassData"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public GenSpecClassData(ClassDeclarationSyntax declaration, GenSpecCompilationData compilation) : base(declaration, compilation)
		{
			InterfaceName = compilation.Configuration.InterfaceName;
			TemplateName = compilation.Configuration.TemplateName;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GenSpecClassData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="TypeDeclarationSyntax"/> this <see cref="GenSpecClassData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="GenSpecCompilationData"/> of this <see cref="GenSpecClassData"/>.</param>
		/// <param name="templateName">Name of the class that is the main implementation of the type.</param>
		/// <param name="interfaceName">Name of the generated specialization interface.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		/// <exception cref="ArgumentException"><paramref name="templateName"/> is not a valid identifier. -or- <paramref name="interfaceName"/> is not a valid identifier.</exception>
		public GenSpecClassData(ClassDeclarationSyntax declaration, GenSpecCompilationData compilation, string templateName, string interfaceName) : base(declaration, compilation)
		{
			GenSpecConfiguration.ThrowIfIsNotValidIdentifier(templateName);
			GenSpecConfiguration.ThrowIfIsNotValidIdentifier(interfaceName);

			InterfaceName = interfaceName;
			TemplateName = templateName;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GenSpecClassData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="TypeDeclarationSyntax"/> this <see cref="GenSpecClassData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="GenSpecCompilationData"/> of this <see cref="GenSpecClassData"/>.</param>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> this <see cref="GenSpecClassData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="partialDeclarations">A collection of <see cref="TypeDeclarationSyntax"/> that represent the partial declarations of the target <paramref name="symbol"/>.</param>
		/// <param name="modifiers">A collection of all modifiers applied to the <paramref name="symbol"/>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="INamespaceSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		/// <param name="templateName">Name of the class that is the main implementation of the type.</param>
		/// <param name="interfaceName">Name of the generated specialization interface.</param>
		public GenSpecClassData(
			ClassDeclarationSyntax declaration,
			GenSpecCompilationData compilation,
			INamedTypeSymbol symbol,
			SemanticModel semanticModel,
			IEnumerable<ClassDeclarationSyntax>? partialDeclarations,
			IEnumerable<SyntaxToken>? modifiers,
			IEnumerable<ITypeData>? containingTypes,
			IEnumerable<INamespaceSymbol>? containingNamespaces,
			IEnumerable<AttributeData>? attributes,
			string templateName,
			string interfaceName
		) : base(declaration, compilation, symbol, semanticModel, partialDeclarations, modifiers, containingTypes, containingNamespaces, attributes)
		{
			InterfaceName = interfaceName;
			TemplateName = templateName;
		}
	}
}
