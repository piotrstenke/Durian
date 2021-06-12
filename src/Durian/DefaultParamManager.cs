// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Durian.Generator.Cache;
using Durian.Generator.DefaultParam;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Generator.DefaultParam.DefaultParamDiagnostics;

namespace Durian.Generator.Manager
{
	/// <summary>
	/// Manages the analyzers and source generators from the <c>Durian.DefaultParam</c> package.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class DefaultParamManager : DurianManagerWithGenerators<IDefaultParamTarget>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamManager"/> class.
		/// </summary>
		public DefaultParamManager()
		{
		}

		/// <inheritdoc/>
		protected override IDurianSyntaxReceiver CreateSyntaxReceiver()
		{
			return new DefaultParamSyntaxReceiver();
		}

		/// <inheritdoc/>
		protected override IAnalyzerInfo[] GetAnalyzers()
		{
			return new IAnalyzerInfo[]
			{
				new DefaultParamConfigurationAnalyzer(),
				new DefaultParamScopedConfigurationAnalyzer(),
				new DefaultParamLocalFunctionAnalyzer()
			};
		}

		/// <inheritdoc/>
		protected override ICachedAnalyzerInfo<IDefaultParamTarget>[] GetCachedAnalyzers()
		{
			return new ICachedAnalyzerInfo<IDefaultParamTarget>[]
			{
				new DefaultParamMethodFilter.AsAnalyzer(),
				new DefaultParamDelegateFilter.AsAnalyzer(),
				new DefaultParamTypeFilter.AsAnalyzer(),
			};
		}

		/// <inheritdoc/>
		protected override IEnumerable<DiagnosticDescriptor> GetManagerSpecificDiagnostics()
		{
			return new DiagnosticDescriptor[]
			{
				DUR0101_ContainingTypeMustBePartial,
				DUR0102_MethodCannotBePartialOrExtern,
				DUR0103_DefaultParamIsNotOnThisTypeOfMethod,
				DUR0104_DefaultParamCannotBeAppliedWhenGenerationAttributesArePresent,
				DUR0105_DefaultParamMustBeLast,
				DUR0106_TargetTypeDoesNotSatisfyConstraint,
				DUR0107_DoNotOverrideGeneratedMethods,
				DUR0108_ValueOfOverriddenMethodMustBeTheSameAsBase,
				DUR0109_DoNotAddDefaultParamAttributeOnOverridenParameters,
				DUR0110_OverriddenDefaultParamAttribuetShouldBeAddedForClarity,
				DUR0111_DefaultParamConfigurationAttributeCannotBeAppliedToMembersWithoutDefaultParamAttribute,
				DUR0112_TypeConvetionShouldNotBeUsedOnMembersOtherThanTypes,
				DUR0113_MethodConventionShouldNotBeUsedOnMembersOtherThanMethods,
				DUR0114_MethodWithSignatureAlreadyExists,
				DUR0115_DefaultParamConfigurationIsNotValidOnThisTypeOfMethod,
				DUR0116_MemberWithNameAlreadyExists,
				DUR0117_InheritTypeConventionCannotBeUsedOnStructOrSealedType,
				DUR0118_ApplyCopyTypeConventionOnStructOrSealedTypeOrTypeWithNoPublicCtor,
				DUR0119_DefaultParamValueCannotBeLessAccessibleThanTargetMember,
				DUR0120_TypeCannotBeUsedWithConstraint,
				DUR0121_TypeIsNotValidDefaultParamValue,
				DUR0122_DoNotUseDefaultParamOnPartialType,
				DUR0123_InheritTypeConventionCannotBeUsedOnTypeWithNoAccessibleConstructor,
				DUR0124_ApplyNewModifierShouldNotBeUsedWhenIsNotChildOfType,
				DUR0125_ScopedConfigurationShouldNotBePlacedOnATypeWithoutDefaultParamMembers,
				DUR0126_DefaultParamMembersCannotBeNested,
				DUR0127_InvalidTargetNamespace,
				DUR0128_DoNotSpecifyTargetNamespaceForNestedMembers,
				DUR0129_TargetNamespaceAlreadyContainsMemberWithName
			};
		}

		/// <inheritdoc/>
		protected override ICachedDurianSourceGenerator<IDefaultParamTarget>[] GetSourceGenerators()
		{
			return new ICachedDurianSourceGenerator<IDefaultParamTarget>[]
			{
				new DefaultParamGenerator()
			};
		}
	}
}
