using System.Collections.Generic;
using System.Threading;
using Durian.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.DefaultParam
{
	public class DefaultParamMethodData : MethodData, IDefaultParamTarget
	{
		private readonly TypeParameterContainer _typeParameters;

		public new DefaultParamCompilationData ParentCompilation => (DefaultParamCompilationData)base.ParentCompilation;
		public List<int>? TypeParameterIndicesToApplyNewModifier { get; }

		public DefaultParamMethodData(MethodDeclarationSyntax declaration, DefaultParamCompilationData compilation, in TypeParameterContainer typeParameters) : base(declaration, compilation)
		{
			_typeParameters = typeParameters;
		}

		public DefaultParamMethodData(
			MethodDeclarationSyntax declaration,
			DefaultParamCompilationData compilation,
			IMethodSymbol symbol,
			SemanticModel semanticModel,
			IEnumerable<ITypeData>? containingTypes,
			IEnumerable<INamespaceSymbol>? containingNamespaces,
			IEnumerable<AttributeData>? attributes,
			in TypeParameterContainer typeParameters,
			List<int>? typeParameterIndicesToApplyNewModifier
		) : base(declaration, compilation, symbol, semanticModel, containingTypes, containingNamespaces, attributes)
		{
			_typeParameters = typeParameters;
			TypeParameterIndicesToApplyNewModifier = typeParameterIndicesToApplyNewModifier;
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
