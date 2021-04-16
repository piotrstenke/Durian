using System.Collections.Generic;
using System.Threading;
using Durian.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.DefaultParam
{
	public class DefaultParamTypeData : TypeData, IDefaultParamTarget
	{
		private readonly TypeParameterContainer _typeParameters;

		public new DefaultParamCompilationData ParentCompilation => (DefaultParamCompilationData)base.ParentCompilation;

		public DefaultParamTypeData(TypeDeclarationSyntax declaration, DefaultParamCompilationData compilation, in TypeParameterContainer typeParameters) : base(declaration, compilation)
		{
			_typeParameters = typeParameters;
		}

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

		public string GetHintName()
		{
			return DefaultParamUtilities.GetHintName(Symbol);
		}

		public ref readonly TypeParameterContainer GetTypeParameters()
		{
			return ref _typeParameters;
		}

		public IEnumerable<string> GetUsedNamespaces(CancellationToken cancellationToken = default)
		{
			return DefaultParamUtilities.GetUsedNamespaces(this, in _typeParameters, cancellationToken);
		}

		IEnumerable<string> IDefaultParamTarget.GetUsedNamespaces()
		{
			return GetUsedNamespaces();
		}
	}
}
