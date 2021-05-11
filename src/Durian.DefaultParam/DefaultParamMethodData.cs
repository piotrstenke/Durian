using System;
using System.Collections.Generic;
using System.Threading;
using Durian.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.DefaultParam
{
	/// <summary>
	/// <see cref="MethodData"/> that contains additional information needed by the <see cref="DefaultParamGenerator"/>.
	/// </summary>
	public class DefaultParamMethodData : MethodData, IDefaultParamTarget
	{
		private readonly TypeParameterContainer _typeParameters;

		/// <summary>
		/// Parent <see cref="DefaultParamCompilationData"/> of this <see cref="DefaultParamMethodData"/>.
		/// </summary>
		public new DefaultParamCompilationData ParentCompilation => (DefaultParamCompilationData)base.ParentCompilation;

		/// <summary>
		/// A <see cref="HashSet{T}"/> of indexes of type parameters with the <see cref="DefaultParamAttribute"/> applied for whom the <see langword="new"/> modifier should be applied.
		/// </summary>
		public HashSet<int>? NewModifierIndexes { get; }

		/// <inheritdoc/>
		public ref readonly TypeParameterContainer TypeParameters => ref _typeParameters;

		/// <summary>
		/// Determines whether the generated method should call this method instead of copying its contents.
		/// </summary>
		public bool CallInsteadOfCopying { get; }

		/// <summary>
		/// Determines whether to remove the <see langword="override"/> keyword.
		/// </summary>
		public bool RemoveOverrideKeyword { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamMethodData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="MethodDeclarationSyntax"/> this <see cref="DefaultParamMethodData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="DefaultParamCompilationData"/> of this <see cref="DefaultParamMethodData"/>.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains type parameters of this member.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public DefaultParamMethodData(MethodDeclarationSyntax declaration, DefaultParamCompilationData compilation, in TypeParameterContainer typeParameters) : base(declaration, compilation)
		{
			_typeParameters = typeParameters;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamMethodData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="MethodDeclarationSyntax"/> this <see cref="DefaultParamMethodData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="DefaultParamCompilationData"/> of this <see cref="DefaultParamMethodData"/>.</param>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> this <see cref="DefaultParamMethodData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="INamespaceSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains type parameters of this member.</param>
		/// <param name="newModifierIndexes">A <see cref="HashSet{T}"/> of indexes of type parameters with 'DefaultParam' attribute for whom the <see langword="new"/> modifier should be applied.</param>
		/// <param name="callInsteadOfCopying">Determines whether the generated method should call this method instead of copying its contents.</param>
		public DefaultParamMethodData(
			MethodDeclarationSyntax declaration,
			DefaultParamCompilationData compilation,
			IMethodSymbol symbol,
			SemanticModel semanticModel,
			IEnumerable<ITypeData>? containingTypes,
			IEnumerable<INamespaceSymbol>? containingNamespaces,
			IEnumerable<AttributeData>? attributes,
			in TypeParameterContainer typeParameters,
			HashSet<int>? newModifierIndexes,
			bool callInsteadOfCopying
		) : base(declaration, compilation, symbol, semanticModel, containingTypes, containingNamespaces, attributes)
		{
			_typeParameters = typeParameters;
			CallInsteadOfCopying = callInsteadOfCopying;
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
		/// Returns a new instance of <see cref="MethodDeclarationBuilder"/> with <see cref="MethodDeclarationBuilder.OriginalDeclaration"/> set to this member's <see cref="MethodData.Declaration"/>.
		/// </summary>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public MethodDeclarationBuilder GetDeclarationBuilder(CancellationToken cancellationToken = default)
		{
			return new MethodDeclarationBuilder(this, cancellationToken);
		}

		IDefaultParamDeclarationBuilder IDefaultParamTarget.GetDeclarationBuilder(CancellationToken cancellationToken)
		{
			return GetDeclarationBuilder(cancellationToken);
		}
	}
}
