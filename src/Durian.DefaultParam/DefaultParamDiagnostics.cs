using System.Linq;
using Microsoft.CodeAnalysis;
using Durian.Generator;

namespace Durian.DefaultParam
{
	public static class DefaultParamDiagnostics
	{
		public static class Descriptors
		{
			public static readonly DiagnosticDescriptor DefaultParamAttributeCannotBeAppliedToMembersWithAttribute =
				DescriptorFactory.AttributeCannotBeAppliedToMembersWithAttribute(DefaultParamAttribute.AttributeName);

			public static readonly DiagnosticDescriptor ParentTypeOfMemberWithDefaultParamAttributeMustBePartial =
				DescriptorFactory.ParentTypeOfMemberWithAttributeMustBePartial(DefaultParamAttribute.AttributeName);

			public static readonly DiagnosticDescriptor TypeParameterWithDefaultParamAttributeMustBeLast =
				DescriptorFactory.TypeParameterWithAttributeMustBeLast(DefaultParamAttribute.AttributeName);

			public static readonly DiagnosticDescriptor DefaultParamMethodCannotBePartialOrExtern =
				DescriptorFactory.MemberWithAttributeCannotHaveModifier(DefaultParamAttribute.AttributeName, "partial or extern");

			public static readonly DiagnosticDescriptor DefaultParamAttributeIsNotValidOnLocalFunctions =
				DescriptorFactory.AttributeCannotBeAppliedToMembersOfType(DefaultParamAttribute.AttributeName, "local function");

			public static readonly DiagnosticDescriptor OverriddenDefaultParamAttributeShouldBeAddedForClarity =
				DescriptorFactory.AttributeOfOverridenMemberShouldBeAddedForClarity(DefaultParamAttribute.AttributeName);

			public static readonly DiagnosticDescriptor DoNotOverrideMethodsGeneratedUsingDefaultParamAttribute =
				DescriptorFactory.DoNotOverrideMembersGeneratedUsingSpecifiedAttribute(DefaultParamAttribute.AttributeName);

			public static readonly DiagnosticDescriptor ValueOfDefaultParamAttributeMustBeTheSameAsValueForOverridenMethod =
				DescriptorFactory.ValueOfAttributeMustBeTheSameAsValueOfTheOverridenMember(DefaultParamAttribute.AttributeName);

			public static readonly DiagnosticDescriptor DoNotAddDefaultParamAttributeOnOverriddenVirtualTypeParameter =
				DescriptorFactory.DoNotAddAttributeOnVirtualTypeParameter(DefaultParamAttribute.AttributeName);
		}

		public static void DefaultParamAttributeCannotBeAppliedToMembersWithAttribute(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName)
		{
			DiagnosticDescriptor d = Descriptors.DefaultParamAttributeCannotBeAppliedToMembersWithAttribute;
			diagnosticReceiver.ReportDiagnostic(d, symbol?.Locations.FirstOrDefault(), symbol, attributeName);
		}

		public static void ParentTypeOfMemberWithDefaultParamAttributeMustBePartial(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol)
		{
			DiagnosticDescriptor d = Descriptors.ParentTypeOfMemberWithDefaultParamAttributeMustBePartial;
			diagnosticReceiver.ReportDiagnostic(d, symbol?.Locations.FirstOrDefault(), symbol);
		}

		public static void TypeParameterWithDefaultParamAttributeMustBeLast(IDiagnosticReceiver diagnosticReceiver, ITypeParameterSymbol? symbol, Location? location)
		{
			DiagnosticDescriptor d = Descriptors.TypeParameterWithDefaultParamAttributeMustBeLast;
			diagnosticReceiver.ReportDiagnostic(d, location, symbol);
		}

		public static void DefaultParamMethodCannotBePartialOrExtern(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol? symbol)
		{
			DiagnosticDescriptor d = Descriptors.DefaultParamMethodCannotBePartialOrExtern;
			diagnosticReceiver.ReportDiagnostic(d, symbol?.Locations.FirstOrDefault(), symbol);
		}

		public static void DefaultParamAttributeIsNotValidOnLocalFunctions(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol? symbol)
		{
			DiagnosticDescriptor d = Descriptors.DefaultParamAttributeIsNotValidOnLocalFunctions;
			diagnosticReceiver.ReportDiagnostic(d, symbol?.Locations.FirstOrDefault(), symbol);
		}

		public static void OverriddenDefaultParamAttributeShouldBeAddedForClarity(IDiagnosticReceiver diagnosticReceiver, ITypeParameterSymbol? symbol)
		{
			DiagnosticDescriptor d = Descriptors.OverriddenDefaultParamAttributeShouldBeAddedForClarity;
			diagnosticReceiver.ReportDiagnostic(d, symbol?.Locations.FirstOrDefault(), symbol);
		}

		public static void DoNotOverrideMethodsGeneratedUsingDefaultParamAttribute(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol? symbol)
		{
			DiagnosticDescriptor d = Descriptors.DoNotOverrideMethodsGeneratedUsingDefaultParamAttribute;
			diagnosticReceiver.ReportDiagnostic(d, symbol?.Locations.FirstOrDefault(), symbol);
		}

		public static void ValueOfDefaultParamAttributeMustBeTheSameAsValueForOverridenMethod(IDiagnosticReceiver diagnosticReceiver, ITypeParameterSymbol? symbol, Location? location)
		{
			DiagnosticDescriptor d = Descriptors.ValueOfDefaultParamAttributeMustBeTheSameAsValueForOverridenMethod;
			diagnosticReceiver.ReportDiagnostic(d, location, symbol);
		}

		public static void DoNotAddDefaultParamAttributeOnOverriddenVirtualTypeParameter(IDiagnosticReceiver diagnosticReceiver, ITypeParameterSymbol? symbol, Location? location)
		{
			DiagnosticDescriptor d = Descriptors.DoNotAddDefaultParamAttributeOnOverriddenVirtualTypeParameter;
			diagnosticReceiver.ReportDiagnostic(d, location, symbol);
		}
	}
}
