using System;
using System.Collections.Generic;
using System.Threading;
using Durian.Generator.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// <see cref="TypeData"/> that contains additional information needed by the <see cref="DefaultParamGenerator"/>.
	/// </summary>
	public class DefaultParamTypeData : TypeData, IDefaultParamTarget
	{
		private readonly TypeParameterContainer _typeParameters;

		/// <summary>
		/// Parent <see cref="DefaultParamCompilationData"/> of this <see cref="DefaultParamTypeData"/>.
		/// </summary>
		public new DefaultParamCompilationData ParentCompilation => (DefaultParamCompilationData)base.ParentCompilation;

		/// <inheritdoc/>
		public ref readonly TypeParameterContainer TypeParameters => ref _typeParameters;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamTypeData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="TypeDeclarationSyntax"/> this <see cref="DefaultParamTypeData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="DefaultParamCompilationData"/> of this <see cref="DefaultParamTypeData"/>.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains type parameters of this member.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public DefaultParamTypeData(TypeDeclarationSyntax declaration, DefaultParamCompilationData compilation, in TypeParameterContainer typeParameters) : base(declaration, compilation)
		{
			_typeParameters = typeParameters;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamTypeData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="TypeDeclarationSyntax"/> this <see cref="DefaultParamTypeData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="DefaultParamCompilationData"/> of this <see cref="DefaultParamTypeData"/>.</param>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> this <see cref="DefaultParamTypeData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="partialDeclarations">A collection of <see cref="TypeDeclarationSyntax"/> that represent the partial declarations of the target <paramref name="symbol"/>.</param>
		/// <param name="modifiers">A collection of all modifiers applied to the <paramref name="symbol"/>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="INamespaceSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains type parameters of this member.</param>
		public DefaultParamTypeData(
			TypeDeclarationSyntax declaration,
			DefaultParamCompilationData compilation,
			INamedTypeSymbol symbol,
			SemanticModel semanticModel,
			IEnumerable<TypeDeclarationSyntax>? partialDeclarations,
			IEnumerable<SyntaxToken>? modifiers,
			IEnumerable<ITypeData>? containingTypes,
			IEnumerable<INamespaceSymbol>? containingNamespaces,
			IEnumerable<AttributeData>? attributes,
			in TypeParameterContainer typeParameters
		) : base(declaration, compilation, symbol, semanticModel, partialDeclarations, modifiers, containingTypes, containingNamespaces, attributes)
		{
			_typeParameters = typeParameters;
		}

		/// <inheritdoc/>
		public IEnumerable<string> GetUsedNamespaces(CancellationToken cancellationToken = default)
		{
			return DefaultParamUtilities.GetUsedNamespaces(this, in _typeParameters, cancellationToken);
		}

		IEnumerable<string> IDefaultParamTarget.GetUsedNamespaces()
		{
			return GetUsedNamespaces();
		}

		/// <summary>
		/// Returns a new instance of <see cref="TypeDeclarationBuilder"/> with <see cref="TypeDeclarationBuilder.OriginalDeclaration"/> set to this member's <see cref="TypeData.Declaration"/>.
		/// </summary>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public TypeDeclarationBuilder GetDeclarationBuilder(CancellationToken cancellationToken = default)
		{
			return new TypeDeclarationBuilder(this, cancellationToken);
		}

		IDefaultParamDeclarationBuilder IDefaultParamTarget.GetDeclarationBuilder(CancellationToken cancellationToken)
		{
			return GetDeclarationBuilder(cancellationToken);
		}
	}
}
