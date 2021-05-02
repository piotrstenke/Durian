using System.Collections.Generic;
using System.Threading;
using Durian.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.DefaultParam
{
	public class DefaultParamDelegateData : DelegateData, IDefaultParamTarget
	{
		private readonly TypeParameterContainer _typeParameters;

		public new DefaultParamCompilationData ParentCompilation => (DefaultParamCompilationData)base.ParentCompilation;
		public ref readonly TypeParameterContainer TypeParameters => ref _typeParameters;

		public DefaultParamDelegateData(DelegateDeclarationSyntax declaration, DefaultParamCompilationData compilation, in TypeParameterContainer typeParameters) : base(declaration, compilation)
		{
			_typeParameters = typeParameters;
		}

		public DefaultParamDelegateData(
			DelegateDeclarationSyntax declaration,
			DefaultParamCompilationData compilation,
			INamedTypeSymbol symbol,
			SemanticModel semanticModel,
			IEnumerable<ITypeData>? containingTypes,
			IEnumerable<INamespaceSymbol>? containingNamespaces,
			IEnumerable<AttributeData>? attributes,
			in TypeParameterContainer typeParameters
		) : base(declaration, compilation, symbol, semanticModel, containingTypes, containingNamespaces, attributes)
		{
			_typeParameters = typeParameters;
		}

		public IEnumerable<string> GetUsedNamespaces(CancellationToken cancellationToken = default)
		{
			return DefaultParamUtilities.GetUsedNamespaces(this, in _typeParameters, cancellationToken);
		}

		IEnumerable<string> IDefaultParamTarget.GetUsedNamespaces()
		{
			return GetUsedNamespaces();
		}

		public DelegateDeclarationBuilder GetDeclarationBuilder(CancellationToken cancellationToken)
		{
			return new DelegateDeclarationBuilder(this, cancellationToken);
		}

		IDefaultParamDeclarationBuilder IDefaultParamTarget.GetDeclarationBuilder(CancellationToken cancellationToken)
		{
			return GetDeclarationBuilder(cancellationToken);
		}
	}
}
