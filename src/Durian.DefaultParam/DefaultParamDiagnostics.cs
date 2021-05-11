using System.Linq;
using Microsoft.CodeAnalysis;

namespace Durian.DefaultParam
{
	/// <summary>
	/// Contains methods that report specific DefaultParam-related <see cref="Diagnostic"/>s.
	/// </summary>
	public static class DefaultParamDiagnostics
	{
		/// <summary>
		/// Contains <see cref="DiagnosticDescriptor"/>s of all the DefaultParam-specific <see cref="Diagnostic"/>s.
		/// </summary>
		public static class Descriptors
		{
			/// <summary>
			/// Provides diagnostic message indicating that the parent type of a member with a <see cref="DefaultParamAttribute"/> must be <see langword="partial"/> (DUR0014).
			/// </summary>
			public static readonly DiagnosticDescriptor ParentTypeOfMemberWithDefaultParamAttributeMustBePartial =
				DescriptorFactory.ParentTypeOfMemberWithAttributeMustBePartial(DefaultParamAttribute.AttributeName);

			/// <summary>
			/// Provides diagnostic message indicating that a method marked with a <see cref="DefaultParamAttribute"/> cannot be declared using either <see langword="extern"/> or <see langword="partial"/> keyword (DUR0011).
			/// </summary>
			public static readonly DiagnosticDescriptor DefaultParamMethodCannotBePartialOrExtern =
				DescriptorFactory.MemberWithAttributeCannotHaveModifier(DefaultParamAttribute.AttributeName, "partial or extern");

			/// <summary>
			/// Provides diagnostic message indicating that the <see cref="DefaultParamAttribute"/> cannot be applied to local functions (DUR0016).
			/// </summary>
			public static readonly DiagnosticDescriptor DefaultParamAttributeIsNotValidOnLocalFunctions =
				DescriptorFactory.AttributeCannotBeAppliedToMembersOfType(DefaultParamAttribute.AttributeName, "local function");

			/// <summary>
			/// Provides diagnostic message indicating that the <see cref="DefaultParamAttribute"/> cannot be applied to members with the <see cref="Generator.DurianGeneratedAttribute"/> or <see cref="System.CodeDom.Compiler.GeneratedCodeAttribute"/> (DUR0017).
			/// </summary>
			public static readonly DiagnosticDescriptor DefaultParamAttributeCannotBeAppliedToMembersWithGeneratedCodeOrDurianGeneratedAtribute =
				DescriptorFactory.AttributeCannotBeAppliedToMembersWithAttribute(DefaultParamAttribute.AttributeName, "DurianGenerated' or 'GeneratedCode");

			/// <summary>
			/// Provides diagnostic message indicating that a type parameter marked with a <see cref="DefaultParamAttribute"/> must be placed last in the declaration (DUR0018).
			/// </summary>
			public static readonly DiagnosticDescriptor TypeParameterWithDefaultParamAttributeMustBeLast =
				DescriptorFactory.TypeParameterWithAttributeMustBeLast(DefaultParamAttribute.AttributeName);

			/// <summary>
			/// Provides diagnostic message indicating that a method with the <see langword="override"/> keyword should add the <see cref="DefaultParamAttribute"/> of the overridden method to preserve clarity (DUR0020).
			/// </summary>
			public static readonly DiagnosticDescriptor OverriddenDefaultParamAttributeShouldBeAddedForClarity =
				DescriptorFactory.AttributeOfOverridenMemberShouldBeAddedForClarity(DefaultParamAttribute.AttributeName);

			/// <summary>
			/// Provides diagnostic message indicating that the user shouldn't override DefaultParam-generated methods (DUR0021).
			/// </summary>
			public static readonly DiagnosticDescriptor DoNotOverrideMethodsGeneratedUsingDefaultParamAttribute =
				DescriptorFactory.DoNotOverrideMembersGeneratedUsingSpecifiedAttribute(DefaultParamAttribute.AttributeName);

			/// <summary>
			/// Provides diagnostic message indicating that value of a <see cref="DefaultParamAttribute"/> must be the same as the value defined on the overridden method (DUR0022).
			/// </summary>
			public static readonly DiagnosticDescriptor ValueOfDefaultParamAttributeMustBeTheSameAsValueForOverridenMethod =
				DescriptorFactory.ValueOfAttributeMustBeTheSameAsValueOfTheOverridenMember(DefaultParamAttribute.AttributeName);

			/// <summary>
			/// Provides diagnostic message indicating that the user shouldn't add new <see cref="DefaultParamAttribute"/>s on a type parameter of an overridden virtual method (DUR0025).
			/// </summary>
			public static readonly DiagnosticDescriptor DoNotAddDefaultParamAttributeOnOverriddenVirtualTypeParameter =
				DescriptorFactory.DoNotAddAttributeOnVirtualTypeParameter(DefaultParamAttribute.AttributeName);

			/// <summary>
			/// Provides diagnostic message indicating that the <see cref="DefaultParamConfigurationAttribute"/> cannot be applies to members without the <see cref="DefaultParamAttribute"/> (DUR0027).
			/// </summary>
			public static readonly DiagnosticDescriptor DefaultParamConfigurationAttributeCannotBeAppliedToMembersWithoutDefaultParamAttribute =
				DescriptorFactory.AttributeCannotBeAppliedToMembersWithoutAttribute(DefaultParamConfigurationAttribute.AttributeName, DefaultParamAttribute.AttributeName);

			/// <summary>
			/// Provides diagnostic message indicating the specified property of the <see cref="DefaultParamConfigurationAttribute"/> shouldn't be used on members of the specified type. (DUR0028).
			/// </summary>
			public static readonly DiagnosticDescriptor DefaultParamPropertyShouldNotBeUsedOnMembersOfType =
				DescriptorFactory.AttributePropertyShouldNotBeUsedOnMembersOfType(DefaultParamAttribute.AttributeName);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that the target attributeName cannot be applied to members with the <see cref="Generator.DurianGeneratedAttribute"/> or <see cref="System.CodeDom.Compiler.GeneratedCodeAttribute"/> (DUR0017).
		/// <para>See: <see cref="Descriptors.DefaultParamAttributeCannotBeAppliedToMembersWithGeneratedCodeOrDurianGeneratedAtribute"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> the attribute cannot be applied to.</param>
		public static void DefaultParamAttributeCannotBeAppliedToMembersWithGeneratedCodeOrDurianGeneratedAtribute(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol)
		{
			DiagnosticDescriptor d = Descriptors.DefaultParamAttributeCannotBeAppliedToMembersWithGeneratedCodeOrDurianGeneratedAtribute;
			diagnosticReceiver.ReportDiagnostic(d, symbol?.Locations.FirstOrDefault(), symbol);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that the parent type of a member with a <see cref="DefaultParamAttribute"/> must be <see langword="partial"/> (DUR0014).
		/// <para>See: <see cref="Descriptors.ParentTypeOfMemberWithDefaultParamAttributeMustBePartial"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> the attribute is defined on.</param>
		public static void ParentTypeOfMemberWithDefaultParamAttributeMustBePartial(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol)
		{
			DiagnosticDescriptor d = Descriptors.ParentTypeOfMemberWithDefaultParamAttributeMustBePartial;
			diagnosticReceiver.ReportDiagnostic(d, symbol?.Locations.FirstOrDefault(), symbol);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that a type parameter marked with a <see cref="DefaultParamAttribute"/> must be placed last in the declaration (DUR0018).
		/// <para>See: <see cref="Descriptors.TypeParameterWithDefaultParamAttributeMustBeLast"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ITypeParameterSymbol"/> that caused the error.</param>
		/// <param name="location">Location of the error.</param>
		public static void TypeParameterWithDefaultParamAttributeMustBeLast(IDiagnosticReceiver diagnosticReceiver, ITypeParameterSymbol? symbol, Location? location)
		{
			DiagnosticDescriptor d = Descriptors.TypeParameterWithDefaultParamAttributeMustBeLast;
			diagnosticReceiver.ReportDiagnostic(d, location, symbol);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that a method marked with a <see cref="DefaultParamAttribute"/> cannot be declared using either <see langword="extern"/> or <see langword="partial"/> keyword (DUR0011).
		/// <para>See: <see cref="Descriptors.DefaultParamMethodCannotBePartialOrExtern"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="IMethodSymbol"/> that caused the error.</param>
		public static void DefaultParamMethodCannotBePartialOrExtern(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol? symbol)
		{
			DiagnosticDescriptor d = Descriptors.DefaultParamMethodCannotBePartialOrExtern;
			diagnosticReceiver.ReportDiagnostic(d, symbol?.Locations.FirstOrDefault(), symbol);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that the <see cref="DefaultParamAttribute"/> cannot be applied to local functions (DUR0016).
		/// <para>See: <see cref="Descriptors.DefaultParamAttributeIsNotValidOnLocalFunctions"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="symbol"><see cref="IMethodSymbol"/> the value is defined on.</param>
		public static void DefaultParamAttributeIsNotValidOnLocalFunctions(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol? symbol)
		{
			DiagnosticDescriptor d = Descriptors.DefaultParamAttributeIsNotValidOnLocalFunctions;
			diagnosticReceiver.ReportDiagnostic(d, symbol?.Locations.FirstOrDefault(), symbol);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that a method with the <see langword="override"/> keyword should add the <see cref="DefaultParamAttribute"/> of the overridden method to preserve clarity (DUR0020).
		/// <para>See: <see cref="Descriptors.OverriddenDefaultParamAttributeShouldBeAddedForClarity"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ITypeParameterSymbol"/> that caused the error.</param>
		public static void OverriddenDefaultParamAttributeShouldBeAddedForClarity(IDiagnosticReceiver diagnosticReceiver, ITypeParameterSymbol? symbol)
		{
			DiagnosticDescriptor d = Descriptors.OverriddenDefaultParamAttributeShouldBeAddedForClarity;
			diagnosticReceiver.ReportDiagnostic(d, symbol?.Locations.FirstOrDefault(), symbol);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that the user shouldn't override DefaultParam-generated methods (DUR0021).
		/// <para>See: <see cref="Descriptors.DoNotOverrideMethodsGeneratedUsingDefaultParamAttribute"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="IMethodSymbol"/> that caused the error.</param>
		public static void DoNotOverrideMethodsGeneratedUsingDefaultParamAttribute(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol? symbol)
		{
			DiagnosticDescriptor d = Descriptors.DoNotOverrideMethodsGeneratedUsingDefaultParamAttribute;
			diagnosticReceiver.ReportDiagnostic(d, symbol?.Locations.FirstOrDefault(), symbol);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that value of a <see cref="DefaultParamAttribute"/> must be the same as the value defined on the overridden method (DUR0022).
		/// <para>See: <see cref="Descriptors.ValueOfDefaultParamAttributeMustBeTheSameAsValueForOverridenMethod"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ITypeParameterSymbol"/> that caused the error.</param>
		/// <param name="location">Location of the error.</param>
		public static void ValueOfDefaultParamAttributeMustBeTheSameAsValueForOverridenMethod(IDiagnosticReceiver diagnosticReceiver, ITypeParameterSymbol? symbol, Location? location)
		{
			DiagnosticDescriptor d = Descriptors.ValueOfDefaultParamAttributeMustBeTheSameAsValueForOverridenMethod;
			diagnosticReceiver.ReportDiagnostic(d, location, symbol);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that the user shouldn't add new <see cref="DefaultParamAttribute"/>s on a type parameter of an overridden virtual method (DUR0025).
		/// <para>See: <see cref="Descriptors.DoNotAddDefaultParamAttributeOnOverriddenVirtualTypeParameter"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ITypeParameterSymbol"/> the attribute is added to.</param>
		/// <param name="location">Location of the error.</param>
		public static void DoNotAddDefaultParamAttributeOnOverriddenVirtualTypeParameter(IDiagnosticReceiver diagnosticReceiver, ITypeParameterSymbol? symbol, Location? location)
		{
			DiagnosticDescriptor d = Descriptors.DoNotAddDefaultParamAttributeOnOverriddenVirtualTypeParameter;
			diagnosticReceiver.ReportDiagnostic(d, location, symbol);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that the <see cref="DefaultParamConfigurationAttribute"/> cannot be applies to members without the <see cref="DefaultParamAttribute"/> (DUR0027).
		/// <para>See: <see cref="Descriptors.DefaultParamConfigurationAttributeCannotBeAppliedToMembersWithoutDefaultParamAttribute"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that caused to error.</param>
		/// <param name="location">Location of the error.</param>
		public static void DefaultParamConfigurationAttributeCannotBeAppliedToMembersWithoutDefaultParamAttribute(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, Location? location)
		{
			DiagnosticDescriptor d = Descriptors.DefaultParamConfigurationAttributeCannotBeAppliedToMembersWithoutDefaultParamAttribute;
			diagnosticReceiver.ReportDiagnostic(d, location, symbol);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating the type convention property of the <see cref="DefaultParamConfigurationAttribute"/> shouldn't be used on members other than types. (DUR0028).
		/// <para>See: <see cref="Descriptors.DefaultParamPropertyShouldNotBeUsedOnMembersOfType"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that caused the error.</param>
		/// <param name="location">Location of the error.</param>
		public static void DefaultParamTypeConventionPropertyShouldNotBeUsedOnMembersOtherThanTypes(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, Location? location)
		{
			DiagnosticDescriptor d = Descriptors.DefaultParamPropertyShouldNotBeUsedOnMembersOfType;
			diagnosticReceiver.ReportDiagnostic(d, location, symbol, DefaultParamConfigurationAttribute.TypeConventionProperty, "members other than types");
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating the method convention property of the <see cref="DefaultParamConfigurationAttribute"/> shouldn't be used on members other than methods. (DUR0028).
		/// <para>See: <see cref="Descriptors.DefaultParamPropertyShouldNotBeUsedOnMembersOfType"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that caused the error.</param>
		/// <param name="location">Location of the error.</param>
		public static void DefaultParamMethodConventionPropertyShouldNotBeUsedOnMembersOtherThanMethods(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, Location? location)
		{
			DiagnosticDescriptor d = Descriptors.DefaultParamPropertyShouldNotBeUsedOnMembersOfType;
			diagnosticReceiver.ReportDiagnostic(d, location, symbol, DefaultParamConfigurationAttribute.MethodConvetionProperty, "members other than methods");
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating the new modifier property of the <see cref="DefaultParamConfigurationAttribute"/> shouldn't be used on any members. (DUR0028).
		/// <para>See: <see cref="Descriptors.DefaultParamPropertyShouldNotBeUsedOnMembersOfType"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that caused the error.</param>
		/// <param name="location">Location of the error.</param>
		public static void DefaultParamNewModifierPropertyShouldNotBeUsedOnAnyMembers(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, Location? location)
		{
			DiagnosticDescriptor d = Descriptors.DefaultParamPropertyShouldNotBeUsedOnMembersOfType;
			diagnosticReceiver.ReportDiagnostic(d, location, symbol, DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossibleProperty, "members");
		}
	}
}
