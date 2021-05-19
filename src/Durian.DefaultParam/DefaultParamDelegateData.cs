using System;
using System.Collections.Generic;
using System.Threading;
using Durian.Generator.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// <see cref="DelegateData"/> that contains additional information needed by the <see cref="DefaultParamGenerator"/>.
	/// </summary>
	public class DefaultParamDelegateData : DelegateData, IDefaultParamTarget
	{
		private readonly TypeParameterContainer _typeParameters;

		/// <summary>
		/// Parent <see cref="DefaultParamCompilationData"/> of this <see cref="DefaultParamDelegateData"/>.
		/// </summary>
		public new DefaultParamCompilationData ParentCompilation => (DefaultParamCompilationData)base.ParentCompilation;

		/// <inheritdoc/>
		public ref readonly TypeParameterContainer TypeParameters => ref _typeParameters;

		/// <inheritdoc cref="DefaultParamMethodData.NewModifierIndexes"/>
		public HashSet<int>? NewModifierIndexes { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamDelegateData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="DelegateDeclarationSyntax"/> this <see cref="DefaultParamDelegateData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="DefaultParamCompilationData"/> of this <see cref="DefaultParamDelegateData"/>.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains type parameters of this member.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public DefaultParamDelegateData(DelegateDeclarationSyntax declaration, DefaultParamCompilationData compilation, in TypeParameterContainer typeParameters) : base(declaration, compilation)
		{
			_typeParameters = typeParameters;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamDelegateData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="DelegateDeclarationSyntax"/> this <see cref="DefaultParamDelegateData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="DefaultParamCompilationData"/> of this <see cref="DefaultParamDelegateData"/>.</param>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> this <see cref="DefaultParamDelegateData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="INamespaceSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains type parameters of this member.</param>
		/// <param name="newModifierIndexes">A <see cref="HashSet{T}"/> of indexes of type parameters with 'DefaultParam' attribute for whom the <see langword="new"/> modifier should be applied.</param>
		public DefaultParamDelegateData(
			DelegateDeclarationSyntax declaration,
			DefaultParamCompilationData compilation,
			INamedTypeSymbol symbol,
			SemanticModel semanticModel,
			IEnumerable<ITypeData>? containingTypes,
			IEnumerable<INamespaceSymbol>? containingNamespaces,
			IEnumerable<AttributeData>? attributes,
			in TypeParameterContainer typeParameters,
			HashSet<int>? newModifierIndexes
		) : base(declaration, compilation, symbol, semanticModel, containingTypes, containingNamespaces, attributes)
		{
			_typeParameters = typeParameters;
			NewModifierIndexes = newModifierIndexes;
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
		/// Returns a new instance of <see cref="DelegateDeclarationBuilder"/> with <see cref="DelegateDeclarationBuilder.OriginalDeclaration"/> set to this member's <see cref="DelegateData.Declaration"/>.
		/// </summary>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public DelegateDeclarationBuilder GetDeclarationBuilder(CancellationToken cancellationToken = default)
		{
			return new DelegateDeclarationBuilder(this, cancellationToken);
		}

		IDefaultParamDeclarationBuilder IDefaultParamTarget.GetDeclarationBuilder(CancellationToken cancellationToken)
		{
			return GetDeclarationBuilder(cancellationToken);
		}
	}
}
