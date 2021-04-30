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
		public HashSet<int>? NewModifierIndices { get; }
		public ref readonly TypeParameterContainer TypeParameters => ref _typeParameters;
		public bool CallInsteadOfCopying { get; }

		public DefaultParamMethodData(
			MethodDeclarationSyntax declaration,
			DefaultParamCompilationData compilation,
			IMethodSymbol symbol,
			SemanticModel semanticModel,
			IEnumerable<ITypeData>? containingTypes,
			IEnumerable<INamespaceSymbol>? containingNamespaces,
			IEnumerable<AttributeData>? attributes,
			in TypeParameterContainer typeParameters,
			HashSet<int>? typeParameterIndicesToApplyNewModifier,
			bool callInsteadOfCopying
		) : base(declaration, compilation, symbol, semanticModel, containingTypes, containingNamespaces, attributes)
		{
			_typeParameters = typeParameters;
			CallInsteadOfCopying = callInsteadOfCopying;
			NewModifierIndices = typeParameterIndicesToApplyNewModifier;
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
