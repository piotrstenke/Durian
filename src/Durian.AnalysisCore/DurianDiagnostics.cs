using System.Linq;
using Microsoft.CodeAnalysis;

namespace Durian
{
	/// <summary>
	/// Contains methods that report diagnostics defined in the <see cref="Descriptors"/> class.
	/// </summary>
	public static partial class DurianDiagnostics
	{
		/// <inheritdoc cref="MemberNameIsReserved(IDiagnosticReceiver, ISymbol?, Location?)"/>
		public static void MemberNameIsReserved(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.MemberNameIsReserved, null, symbol);
		}

		/// <summary>
		/// Reports diagnostics indicating that the name of the target member is reserved for internal purposes (rule DUR0001).
		/// <para>See: <see cref="Descriptors.MemberNameIsReserved"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that caused the error.</param>
		/// <param name="location">Location of the error.</param>
		public static void MemberNameIsReserved(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.MemberNameIsReserved, location, symbol);
		}

		/// <inheritdoc cref="MemberWithNameAlreadyExists(IDiagnosticReceiver, ISymbol?, string?, Location?)"/>
		public static void MemberWithNameAlreadyExists(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? name)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.MemberWithNameAlreadyExists, symbol?.Locations.FirstOrDefault(), symbol, name);
		}

		/// <summary>
		/// Reports diagnostics indicating that a member with the specified name already exists (rule DUR0002).
		/// <para>See: <see cref="Descriptors.MemberWithNameAlreadyExists"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="symbol">Parent type of the member.</param>
		/// <param name="name">Name that caused the error.</param>
		/// <param name="location">Location of the error.</param>
		public static void MemberWithNameAlreadyExists(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? name, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.MemberWithNameAlreadyExists, location, symbol, name);
		}

		/// <summary>
		/// Reports diagnostics indicating that the member has the same name as the enclosing type (rule DUR0003).
		/// <para>See: <see cref="Descriptors.MemberNameCannotBeTheSameAsParentType"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="name">Name that caused the error.</param>
		/// <param name="location">Location of the error.</param>
		public static void MemberNameCannotBeTheSameAsParentType(IDiagnosticReceiver diagnosticReceiver, string? name, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.MemberNameCannotBeTheSameAsParentType, location, name);
		}

		/// <summary>
		/// Reports diagnostics indicating that the target value is not a valid identifier (rule DUR0004).
		/// <para>See: <see cref="Descriptors.ValueIsNotValidIdentifier"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="value">Value that caused the error.</param>
		/// <param name="location">Location of the error.</param>
		public static void ValueIsNotValidIdentifier(IDiagnosticReceiver diagnosticReceiver, string? value, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.ValueIsNotValidIdentifier, location, value);
		}

		/// <inheritdoc cref="MemberNameCouldNotBeResolved(IDiagnosticReceiver, ISymbol?, string?, Location?)"/>
		public static void MemberNameCouldNotBeResolved(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? value)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.MemberNameCouldNotBeResolved, symbol?.Locations.FirstOrDefault(), symbol, value);
		}

		/// <summary>
		/// Reports diagnostics indicating that the member name could not be resolved (rule DUR0005).
		/// <para>See: <see cref="Descriptors.MemberNameCouldNotBeResolved"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that tried to access member using the name.</param>
		/// <param name="value">Value that caused the error.</param>
		/// <param name="location">Location of the error.</param>
		public static void MemberNameCouldNotBeResolved(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? value, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.MemberNameCouldNotBeResolved, location, symbol, value);
		}

		/// <inheritdoc cref="NameRefersToMultipleMembers(IDiagnosticReceiver, ISymbol?, string?, Location?)"/>
		public static void NameRefersToMultipleMembers(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? value)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.NameRefersToMultipleMembers, symbol?.Locations.FirstOrDefault(), symbol, value);
		}

		/// <summary>
		/// Reports diagnostics indicating that the specified name refers to multiple members (rule DUR0006).
		/// <para>See: <see cref="Descriptors.NameRefersToMultipleMembers"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that tried to access member using the name.</param>
		/// <param name="value">Value that caused the error.</param>
		/// <param name="location">Location of the error.</param>
		public static void NameRefersToMultipleMembers(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? value, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.NameRefersToMultipleMembers, location, symbol, value);
		}

		/// <summary>
		/// Reports diagnostics indicating that members from outside of the current assembly cannot be accessed (rule DUR0007).
		/// <para>See: <see cref="Descriptors.MembersFromOutsideOfAssemblyCannotBeAccessed"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that tried to access the external member.</param>
		/// <param name="location">Location of the error.</param>
		public static void MembersFromOutsideOfAssemblyCannotBeAccessed(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.MembersFromOutsideOfAssemblyCannotBeAccessed, location, symbol);
		}

		/// <summary>
		/// Reports diagnostics indicating that the target member cannot refer to itself (rule DUR0008).
		/// <para>See: <see cref="Descriptors.MemberCannotReferToItself"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that caused the error.</param>
		/// <param name="location">Location of the error.</param>
		public static void MemberCannotReferToItself(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.MemberCannotReferToItself, location, symbol);
		}

		/// <summary>
		/// Reports diagnostics indicating that the target members contains errors, and cannot be referenced (rule DUR0009).
		/// <para>See: <see cref="Descriptors.TargetMemberContainsErrors"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="source"><see cref="ISymbol"/> that references the <paramref name="target"/> member.</param>
		/// <param name="target"><see cref="ISymbol"/> that has errors.</param>
		/// <param name="location">Location of the error.</param>
		public static void TargetMemberContainsErrors(IDiagnosticReceiver diagnosticReceiver, ISymbol? source, ISymbol? target, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.TargetMemberContainsErrors, location, source, target);
		}

		/// <inheritdoc cref="MemberWithAttributeMustHaveModifier(IDiagnosticReceiver, ISymbol?, string?, string?, Location?) "/>
		public static void MemberWithAttributeMustHaveModifier(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, string? modifier)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.MemberWithAttributeMustHaveModifier, symbol?.Locations.FirstOrDefault(), symbol, "Member", attributeName, modifier);
		}

		/// <summary>
		/// Reports diagnostics indicating that a member marked with a specified attributeName must be declared using a specific modifier (rule DUR0010).
		/// <para>See: <see cref="Descriptors.MemberWithAttributeMustHaveModifier"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that caused the error.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="modifier">Modifier the symbol must use.</param>
		/// <param name="location">Location of the error.</param>
		public static void MemberWithAttributeMustHaveModifier(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, string? modifier, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.MemberWithAttributeMustHaveModifier, location, symbol, "Member", attributeName, modifier);
		}

		/// <inheritdoc cref="MemberWithAttributeCannotHaveModifier(IDiagnosticReceiver, ISymbol?, string?, string?, Location?)"/>
		public static void MemberWithAttributeCannotHaveModifier(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, string? modifier)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.MemberWithAttributeCannotHaveModifier, symbol?.Locations.FirstOrDefault(), symbol, "Member", attributeName, modifier);
		}

		/// <summary>
		/// Reports diagnostics indicating that a member marked with a specified attributeName cannot be declared using a specific modifier (rule DUR0011).
		/// <para>See: <see cref="Descriptors.MemberWithAttributeCannotHaveModifier"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that caused the error.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="modifier">Modifier the symbol cannot use.</param>
		/// <param name="location">Location of the error.</param>
		public static void MemberWithAttributeCannotHaveModifier(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, string? modifier, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.MemberWithAttributeCannotHaveModifier, location, symbol, "Member", attributeName, modifier);
		}

		/// <inheritdoc cref="MemberWithAttributeMustHaveImplementation(IDiagnosticReceiver, ISymbol?, string?, Location?)"/>
		public static void MemberWithAttributeMustHaveImplementation(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.MemberWithAttributeMustHaveImplementation, symbol?.Locations.FirstOrDefault(), symbol, "Member", attributeName);
		}

		/// <summary>
		/// Reports diagnostics indicating that a member marked with a specified attributeName must have an implementation (rule DUR0012).
		/// <para>See: <see cref="Descriptors.MemberWithAttributeMustHaveImplementation"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that caused the error.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="location">Location of the error.</param>
		public static void MemberWithAttributeMustHaveImplementation(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.MemberWithAttributeMustHaveImplementation, location, symbol, "Member", attributeName);
		}

		/// <inheritdoc cref="MemberWithAttributeCannotHaveImplementation(IDiagnosticReceiver, ISymbol?, string?, Location?)"/>
		public static void MemberWithAttributeCannotHaveImplementation(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.MemberWithAttributeCannotHaveImplementation, symbol?.Locations.FirstOrDefault(), symbol, "Member", attributeName);
		}

		/// <summary>
		/// Reports diagnostics indicating that a member marked with a specified attributeName cannot have an implementation (rule DUR0013).
		/// <para>See: <see cref="Descriptors.MemberWithAttributeCannotHaveModifier"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that caused the error.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="location">Location of the error.</param>
		public static void MemberWithAttributeCannotHaveImplementation(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.MemberWithAttributeCannotHaveImplementation, location, symbol, "Member", attributeName);
		}

		/// <inheritdoc cref="ParentTypeOfMemberWithAttributeMustBePartial(IDiagnosticReceiver, ISymbol?, string?, Location?)"/>
		public static void ParentTypeOfMemberWithAttributeMustBePartial(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.ParentTypeOfMemberWithAttributeMustBePartial, symbol?.Locations.FirstOrDefault(), symbol, attributeName);
		}

		/// <summary>
		/// Reports diagnostics indicating that the parent type of a member with a specified attributeName must be partial (rule DUR0014).
		/// <para>See: <see cref="Descriptors.ParentTypeOfMemberWithAttributeMustBePartial"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> the attribute is defined on.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="location">Location of the error.</param>
		public static void ParentTypeOfMemberWithAttributeMustBePartial(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.ParentTypeOfMemberWithAttributeMustBePartial, location, symbol, attributeName);
		}

		/// <inheritdoc cref="TargetOfAttributeMustBeOfSpecifiedMemberType(IDiagnosticReceiver, ISymbol?, string?, string?, Location?)"/>
		public static void TargetOfAttributeMustBeOfSpecifiedMemberType(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, string? memberType)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.TargetOfAttributeMustBeOfSpecifiedMemberType, symbol?.Locations.FirstOrDefault(), symbol, attributeName, memberType);
		}

		/// <summary>
		/// Reports diagnostics indicating that the target member is of invalid type (e.g expected a method, but got a property) (rule DUR0015).
		/// <para>See: <see cref="Descriptors.TargetOfAttributeMustBeOfSpecifiedMemberType"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> the value is defined on.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="memberType">Type of Symbol that was expected.</param>
		/// <param name="location">Location of the error.</param>
		public static void TargetOfAttributeMustBeOfSpecifiedMemberType(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, string? memberType, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.TargetOfAttributeMustBeOfSpecifiedMemberType, location, attributeName, symbol, memberType);
		}

		/// <summary>
		/// Reports diagnostics indicating that the attribute cannot be applies to members of specific declaration type (rule DUR0016).
		/// <para>See: <see cref="Descriptors.AttributeCannotBeAppliedToMembersOfType"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> the value is defined on.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="memberType">Type of Symbol that was expected.</param>
		/// <param name="location">Location of the error.</param>
		public static void AttributeCannotBeAppliedToMembersOfType(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, string? memberType, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.AttributeCannotBeAppliedToMembersOfType, location, attributeName, symbol, memberType);
		}

		/// <inheritdoc cref="AttributeCannotBeAppliedToMembersWithAttribute(IDiagnosticReceiver, ISymbol?, string?, string?, Location?)"/>
		public static void AttributeCannotBeAppliedToMembersWithAttribute(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName1, string? attributeName2)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.AttributeCannotBeAppliedToMembersWithAttribute, symbol?.Locations.FirstOrDefault(), symbol, attributeName1, attributeName2);
		}

		/// <summary>
		/// Reports diagnostics indicating that the target attributeName cannot be applied to members with a specified attributeName (rule DUR0017).
		/// <para>See: <see cref="Descriptors.AttributeCannotBeAppliedToMembersWithAttribute"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> the attribute cannot be applied to.</param>
		/// <param name="attributeName1">Name of the attribute that can't be applied to the <paramref name="symbol"/>.</param>
		/// <param name="attributeName2">Name of the attribute is already applied to the <paramref name="symbol"/> and doesn't allow the <paramref name="attributeName1"/> to be applied.</param>
		/// <param name="location">Location of the error.</param>
		public static void AttributeCannotBeAppliedToMembersWithAttribute(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName1, string? attributeName2, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.AttributeCannotBeAppliedToMembersWithAttribute, location, symbol, attributeName1, attributeName2);
		}

		/// <inheritdoc cref="TypeParameterWithAttributeMustBeLast(IDiagnosticReceiver, ITypeParameterSymbol, string?, Location?)"/>
		public static void TypeParameterWithAttributeMustBeLast(IDiagnosticReceiver diagnosticReceiver, ITypeParameterSymbol parameter, string? attributeName)
		{
			TypeParameterWithAttributeMustBeLast(diagnosticReceiver, parameter, attributeName, parameter?.Locations.FirstOrDefault()!);
		}

		/// <summary>
		/// Reports diagnostics indicating that a type parameter marked with a specified attributeName must be placed last in the declaration (rule DUR0018).
		/// <para>See: <see cref="Descriptors.TypeParameterWithAttributeMustBeLast"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="parameter">Type parameter that caused the error.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="location">Location of the error.</param>
		public static void TypeParameterWithAttributeMustBeLast(IDiagnosticReceiver diagnosticReceiver, ITypeParameterSymbol? parameter, string? attributeName, Location? location)
		{
			ISymbol? parent;

			if (parameter is not null)
			{
				parent = parameter.DeclaringType is not null ? parameter.DeclaringType : parameter.DeclaringMethod;
			}
			else
			{
				parent = null;
			}

			diagnosticReceiver.ReportDiagnostic(Descriptors.TypeParameterWithAttributeMustBeLast, location, parent, attributeName);
		}

		/// <inheritdoc cref="TypeIsNotValidTypeParameter(IDiagnosticReceiver, ITypeSymbol?, ITypeParameterSymbol?, Location?)"/>
		public static void TypeIsNotValidTypeParameter(IDiagnosticReceiver diagnosticReceiver, ITypeSymbol? type, ITypeParameterSymbol? parameter)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.TypeIsNotValidTypeParameter, parameter?.Locations.FirstOrDefault(), type, parameter);
		}

		/// <summary>
		/// Reports diagnostics indicating that a type is not a valid type parameter (rule DUR0019).
		/// <para>See: <see cref="Descriptors.TypeIsNotValidTypeParameter"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="type">Type that caused the error.</param>
		/// <param name="parameter">Type parameter that caused the error.</param>
		/// <param name="location">Location of the error.</param>
		public static void TypeIsNotValidTypeParameter(IDiagnosticReceiver diagnosticReceiver, ITypeSymbol? type, ITypeParameterSymbol? parameter, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.TypeIsNotValidTypeParameter, location, type, parameter);
		}

		/// <inheritdoc cref="AttributeOfOverridenMemberShouldBeAddedForClarity(IDiagnosticReceiver, ISymbol?, string?, Location?)"/>
		public static void AttributeOfOverridenMemberShouldBeAddedForClarity(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.AttributeOfOverridenMemberShouldBeAddedForClarity, symbol?.Locations.FirstOrDefault(), symbol, attributeName);
		}

		/// <summary>
		/// Reports diagnostics indicating that the member with the 'override' keyword should define a specified attribute to preserve clarity (rule DUR0020).
		/// <para>See: <see cref="Descriptors.AttributeOfOverridenMemberShouldBeAddedForClarity"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that caused the error.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="location">Location of the error.</param>
		public static void AttributeOfOverridenMemberShouldBeAddedForClarity(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.AttributeOfOverridenMemberShouldBeAddedForClarity, location, symbol, attributeName);
		}

		/// <inheritdoc cref="DoNotOverrideMembersGeneratedUsingSpecifiedAttribute(IDiagnosticReceiver, ISymbol?, string?, Location?)"/>
		public static void DoNotOverrideMembersGeneratedUsingSpecifiedAttribute(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.DoNotOverrideMembersGeneratedUsingSpecifiedAttribute, symbol?.Locations.FirstOrDefault(), symbol, attributeName);
		}

		/// <summary>
		/// Reports diagnostics indicating that the user shouldn't override members generated using a specific attribute (rule DUR0021).
		/// <para>See: <see cref="Descriptors.DoNotOverrideMembersGeneratedUsingSpecifiedAttribute"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that caused the error.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="location">Location of the error.</param>
		public static void DoNotOverrideMembersGeneratedUsingSpecifiedAttribute(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.DoNotOverrideMembersGeneratedUsingSpecifiedAttribute, location, symbol, attributeName);
		}

		/// <inheritdoc cref="ValueOfAttributeMustBeTheSameAsValueOfTheOverridenMember(IDiagnosticReceiver, ISymbol?, string?, Location?)"/>
		public static void ValueOfAttributeMustBeTheSameAsValueOfTheOverridenMember(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.ValueOfAttributeMustBeTheSameAsValueOfTheOverridenMember, symbol?.Locations.FirstOrDefault(), symbol, attributeName);
		}

		/// <summary>
		/// Reports diagnostics indicating that value of a specified attribute must be the same as the value defined on the overridden member (rule DUR0022).
		/// <para>See: <see cref="Descriptors.ValueOfAttributeMustBeTheSameAsValueOfTheOverridenMember"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that caused the error.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="location">Location of the error.</param>
		public static void ValueOfAttributeMustBeTheSameAsValueOfTheOverridenMember(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.ValueOfAttributeMustBeTheSameAsValueOfTheOverridenMember, location, symbol, attributeName);
		}

		/// <summary>
		/// Reports diagnostics indicating that an essential type is missing and a specific package should be re-imported (rule DUR0023).
		/// <para>See: <see cref="Descriptors.EssentialTypeIsMissing"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="typeName">Name of the type that is missing.</param>
		/// <param name="packageName">Package the type should be re-imported.</param>
		public static void EssentialTypeIsMissing(IDiagnosticReceiver diagnosticReceiver, string? typeName, string? packageName)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.EssentialTypeIsMissing, Location.None, typeName, packageName);
		}

		/// <inheritdoc cref="MethodWithSignatureAlreadyExists(IDiagnosticReceiver, ISymbol?, string?, Location?)"/>
		public static void MethodWithSignatureAlreadyExists(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? signature)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.MethodWithSignatureAlreadyExists, symbol?.Locations.FirstOrDefault(), symbol, signature);
		}

		/// <summary>
		/// Reports diagnostics indicating that a method with the specified signature already exists (rule DUR0024).
		/// <para>See: <see cref="Descriptors.MethodWithSignatureAlreadyExists"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> the value is defined on.</param>
		/// <param name="signature">Signature that already exists.</param>
		/// <param name="location">Location of the error.</param>
		public static void MethodWithSignatureAlreadyExists(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? signature, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.MethodWithSignatureAlreadyExists, location, symbol, signature);
		}

		/// <inheritdoc cref="DoNotAddDefaultParamAttributeOnOverriddenVirtualTypeParameter(IDiagnosticReceiver, ITypeParameterSymbol?, string?, Location?)"/>
		public static void DoNotAddDefaultParamAttributeOnOverriddenVirtualTypeParameter(IDiagnosticReceiver diagnosticReceiver, ITypeParameterSymbol? symbol, string? attributeName)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.DoNotAddDefaultParamAttributeOnOverriddenVirtualTypeParameter, symbol?.Locations.FirstOrDefault(), symbol, attributeName);
		}

		/// <summary>
		/// Reports diagnostics indicating that the user shouldn't add new attributes on a type parameter of an overridden virtual method (rule DUR0025).
		/// <para>See: <see cref="Descriptors.DoNotAddDefaultParamAttributeOnOverriddenVirtualTypeParameter"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		/// <param name="symbol"><see cref="ITypeParameterSymbol"/> the attribute is added to.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="location">Location of the error.</param>
		public static void DoNotAddDefaultParamAttributeOnOverriddenVirtualTypeParameter(IDiagnosticReceiver diagnosticReceiver, ITypeParameterSymbol? symbol, string? attributeName, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.DoNotAddDefaultParamAttributeOnOverriddenVirtualTypeParameter, location, symbol, attributeName);
		}

		/// <summary>
		/// Reports diagnostics indicating that the target project must reference the <c>Durian.Core</c> package. (rule DUR0026).
		/// <para>See: <see cref="Descriptors.ProjectMustReferenceDurianCore"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the diagnostics to.</param>
		public static void ProjectMustReferenceDurianCore(IDiagnosticReceiver diagnosticReceiver)
		{
			diagnosticReceiver.ReportDiagnostic(Descriptors.ProjectMustReferenceDurianCore, Location.None);
		}
	}
}
