using System.Linq;
using Microsoft.CodeAnalysis;

namespace Durian
{
	/// <summary>
	/// Contains methods that report <see cref="Diagnostic"/>s defined in the <see cref="DurianDescriptors"/> class.
	/// </summary>
	public static partial class DurianDiagnostics
	{
		/// <inheritdoc cref="MemberNameIsReserved(IDiagnosticReceiver, ISymbol?, Location?)"/>
		public static void MemberNameIsReserved(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.MemberNameIsReserved, null, symbol);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that the name of the target member is reserved for internal purposes (rule DUR0001).
		/// <para>See: <see cref="DurianDescriptors.MemberNameIsReserved"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that caused the error.</param>
		/// <param name="location">Location of the error.</param>
		public static void MemberNameIsReserved(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.MemberNameIsReserved, location, symbol);
		}

		/// <inheritdoc cref="MemberWithNameAlreadyExists(IDiagnosticReceiver, ISymbol?, string?, Location?)"/>
		public static void MemberWithNameAlreadyExists(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? name)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.MemberWithNameAlreadyExists, symbol?.Locations.FirstOrDefault(), symbol, name);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that a member with the specified name already exists (rule DUR0002).
		/// <para>See: <see cref="DurianDescriptors.MemberWithNameAlreadyExists"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol">Parent type of the member.</param>
		/// <param name="name">Name that caused the error.</param>
		/// <param name="location">Location of the error.</param>
		public static void MemberWithNameAlreadyExists(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? name, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.MemberWithNameAlreadyExists, location, symbol, name);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that the member has the same name as the enclosing type (rule DUR0003).
		/// <para>See: <see cref="DurianDescriptors.MemberNameCannotBeTheSameAsParentType"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="name">Name that caused the error.</param>
		/// <param name="location">Location of the error.</param>
		public static void MemberNameCannotBeTheSameAsParentType(IDiagnosticReceiver diagnosticReceiver, string? name, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.MemberNameCannotBeTheSameAsParentType, location, name);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that the target value is not a valid identifier (rule DUR0004).
		/// <para>See: <see cref="DurianDescriptors.ValueIsNotValidIdentifier"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="value">Value that caused the error.</param>
		/// <param name="location">Location of the error.</param>
		public static void ValueIsNotValidIdentifier(IDiagnosticReceiver diagnosticReceiver, string? value, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.ValueIsNotValidIdentifier, location, value);
		}

		/// <inheritdoc cref="MemberNameCouldNotBeResolved(IDiagnosticReceiver, ISymbol?, string?, Location?)"/>
		public static void MemberNameCouldNotBeResolved(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? value)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.MemberNameCouldNotBeResolved, symbol?.Locations.FirstOrDefault(), symbol, value);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that the member name could not be resolved (rule DUR0005).
		/// <para>See: <see cref="DurianDescriptors.MemberNameCouldNotBeResolved"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that tried to access member using the name.</param>
		/// <param name="value">Value that caused the error.</param>
		/// <param name="location">Location of the error.</param>
		public static void MemberNameCouldNotBeResolved(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? value, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.MemberNameCouldNotBeResolved, location, symbol, value);
		}

		/// <inheritdoc cref="NameRefersToMultipleMembers(IDiagnosticReceiver, ISymbol?, string?, Location?)"/>
		public static void NameRefersToMultipleMembers(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? value)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.NameRefersToMultipleMembers, symbol?.Locations.FirstOrDefault(), symbol, value);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that the specified name refers to multiple members (rule DUR0006).
		/// <para>See: <see cref="DurianDescriptors.NameRefersToMultipleMembers"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that tried to access member using the name.</param>
		/// <param name="value">Value that caused the error.</param>
		/// <param name="location">Location of the error.</param>
		public static void NameRefersToMultipleMembers(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? value, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.NameRefersToMultipleMembers, location, symbol, value);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that members from outside of the current assembly cannot be accessed (rule DUR0007).
		/// <para>See: <see cref="DurianDescriptors.MembersFromOutsideOfAssemblyCannotBeAccessed"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that tried to access the external member.</param>
		/// <param name="location">Location of the error.</param>
		public static void MembersFromOutsideOfAssemblyCannotBeAccessed(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.MembersFromOutsideOfAssemblyCannotBeAccessed, location, symbol);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that the target member cannot refer to itself (rule DUR0008).
		/// <para>See: <see cref="DurianDescriptors.MemberCannotReferToItself"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that caused the error.</param>
		/// <param name="location">Location of the error.</param>
		public static void MemberCannotReferToItself(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.MemberCannotReferToItself, location, symbol);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that the target members contains errors, and cannot be referenced (rule DUR0009).
		/// <para>See: <see cref="DurianDescriptors.TargetMemberContainsErrors"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="source"><see cref="ISymbol"/> that references the <paramref name="target"/> member.</param>
		/// <param name="target"><see cref="ISymbol"/> that has errors.</param>
		/// <param name="location">Location of the error.</param>
		public static void TargetMemberContainsErrors(IDiagnosticReceiver diagnosticReceiver, ISymbol? source, ISymbol? target, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.TargetMemberContainsErrors, location, source, target);
		}

		/// <inheritdoc cref="MemberWithAttributeMustHaveModifier(IDiagnosticReceiver, ISymbol?, string?, string?, Location?) "/>
		public static void MemberWithAttributeMustHaveModifier(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, string? modifier)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.MemberWithAttributeMustHaveModifier, symbol?.Locations.FirstOrDefault(), symbol, "Member", attributeName, modifier);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that a member marked with a specified attributeName must be declared using a specific modifier (rule DUR0010).
		/// <para>See: <see cref="DurianDescriptors.MemberWithAttributeMustHaveModifier"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that caused the error.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="modifier">Modifier the symbol must use.</param>
		/// <param name="location">Location of the error.</param>
		public static void MemberWithAttributeMustHaveModifier(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, string? modifier, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.MemberWithAttributeMustHaveModifier, location, symbol, "Member", attributeName, modifier);
		}

		/// <inheritdoc cref="MemberWithAttributeCannotHaveModifier(IDiagnosticReceiver, ISymbol?, string?, string?, Location?)"/>
		public static void MemberWithAttributeCannotHaveModifier(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, string? modifier)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.MemberWithAttributeCannotHaveModifier, symbol?.Locations.FirstOrDefault(), symbol, "Member", attributeName, modifier);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that a member marked with a specified attributeName cannot be declared using a specific modifier (rule DUR0011).
		/// <para>See: <see cref="DurianDescriptors.MemberWithAttributeCannotHaveModifier"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that caused the error.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="modifier">Modifier the symbol cannot use.</param>
		/// <param name="location">Location of the error.</param>
		public static void MemberWithAttributeCannotHaveModifier(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, string? modifier, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.MemberWithAttributeCannotHaveModifier, location, symbol, "Member", attributeName, modifier);
		}

		/// <inheritdoc cref="MemberWithAttributeMustHaveImplementation(IDiagnosticReceiver, ISymbol?, string?, Location?)"/>
		public static void MemberWithAttributeMustHaveImplementation(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.MemberWithAttributeMustHaveImplementation, symbol?.Locations.FirstOrDefault(), symbol, "Member", attributeName);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that a member marked with a specified attributeName must have an implementation (rule DUR0012).
		/// <para>See: <see cref="DurianDescriptors.MemberWithAttributeMustHaveImplementation"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that caused the error.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="location">Location of the error.</param>
		public static void MemberWithAttributeMustHaveImplementation(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.MemberWithAttributeMustHaveImplementation, location, symbol, "Member", attributeName);
		}

		/// <inheritdoc cref="MemberWithAttributeCannotHaveImplementation(IDiagnosticReceiver, ISymbol?, string?, Location?)"/>
		public static void MemberWithAttributeCannotHaveImplementation(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.MemberWithAttributeCannotHaveImplementation, symbol?.Locations.FirstOrDefault(), symbol, "Member", attributeName);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that a member marked with a specified attributeName cannot have an implementation (rule DUR0013).
		/// <para>See: <see cref="DurianDescriptors.MemberWithAttributeCannotHaveModifier"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that caused the error.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="location">Location of the error.</param>
		public static void MemberWithAttributeCannotHaveImplementation(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.MemberWithAttributeCannotHaveImplementation, location, symbol, "Member", attributeName);
		}

		/// <inheritdoc cref="ParentTypeOfMemberWithAttributeMustBePartial(IDiagnosticReceiver, ISymbol?, string?, Location?)"/>
		public static void ParentTypeOfMemberWithAttributeMustBePartial(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.ParentTypeOfMemberWithAttributeMustBePartial, symbol?.Locations.FirstOrDefault(), symbol, attributeName);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that the parent type of a member with a specified attributeName must be partial (rule DUR0014).
		/// <para>See: <see cref="DurianDescriptors.ParentTypeOfMemberWithAttributeMustBePartial"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> the attribute is defined on.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="location">Location of the error.</param>
		public static void ParentTypeOfMemberWithAttributeMustBePartial(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.ParentTypeOfMemberWithAttributeMustBePartial, location, symbol, attributeName);
		}

		/// <inheritdoc cref="TargetOfAttributeMustBeOfSpecifiedMemberType(IDiagnosticReceiver, ISymbol?, string?, string?, Location?)"/>
		public static void TargetOfAttributeMustBeOfSpecifiedMemberType(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, string? memberType)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.TargetOfAttributeMustBeOfSpecifiedMemberType, symbol?.Locations.FirstOrDefault(), symbol, attributeName, memberType);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that the target member is of invalid type (e.g expected a method, but got a property) (rule DUR0015).
		/// <para>See: <see cref="DurianDescriptors.TargetOfAttributeMustBeOfSpecifiedMemberType"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> the value is defined on.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="memberType">Type of Symbol that was expected.</param>
		/// <param name="location">Location of the error.</param>
		public static void TargetOfAttributeMustBeOfSpecifiedMemberType(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, string? memberType, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.TargetOfAttributeMustBeOfSpecifiedMemberType, location, attributeName, symbol, memberType);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that the attribute cannot be applies to members of specific declaration type (rule DUR0016).
		/// <para>See: <see cref="DurianDescriptors.AttributeCannotBeAppliedToMembersOfType"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> the value is defined on.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="memberType">Type of Symbol that was expected.</param>
		/// <param name="location">Location of the error.</param>
		public static void AttributeCannotBeAppliedToMembersOfType(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, string? memberType, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.AttributeCannotBeAppliedToMembersOfType, location, attributeName, symbol, memberType);
		}

		/// <inheritdoc cref="AttributeCannotBeAppliedToMembersWithAttribute(IDiagnosticReceiver, ISymbol?, string?, string?, Location?)"/>
		public static void AttributeCannotBeAppliedToMembersWithAttribute(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName1, string? attributeName2)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.AttributeCannotBeAppliedToMembersWithAttribute, symbol?.Locations.FirstOrDefault(), symbol, attributeName1, attributeName2);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that the target attributeName cannot be applied to members with a specified attributeName (rule DUR0017).
		/// <para>See: <see cref="DurianDescriptors.AttributeCannotBeAppliedToMembersWithAttribute"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> the attribute cannot be applied to.</param>
		/// <param name="attributeName1">Name of the attribute that can't be applied to the <paramref name="symbol"/>.</param>
		/// <param name="attributeName2">Name of the attribute that is already applied to the <paramref name="symbol"/> and doesn't allow the <paramref name="attributeName1"/> to be applied.</param>
		/// <param name="location">Location of the error.</param>
		public static void AttributeCannotBeAppliedToMembersWithAttribute(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName1, string? attributeName2, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.AttributeCannotBeAppliedToMembersWithAttribute, location, symbol, attributeName1, attributeName2);
		}

		/// <inheritdoc cref="TypeParameterWithAttributeMustBeLast(IDiagnosticReceiver, ITypeParameterSymbol, string?, Location?)"/>
		public static void TypeParameterWithAttributeMustBeLast(IDiagnosticReceiver diagnosticReceiver, ITypeParameterSymbol parameter, string? attributeName)
		{
			TypeParameterWithAttributeMustBeLast(diagnosticReceiver, parameter, attributeName, parameter?.Locations.FirstOrDefault()!);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that a type parameter marked with a specified attributeName must be placed last in the declaration (rule DUR0018).
		/// <para>See: <see cref="DurianDescriptors.TypeParameterWithAttributeMustBeLast"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="parameter"><see cref="ITypeParameterSymbol"/> that caused the error.</param>
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

			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.TypeParameterWithAttributeMustBeLast, location, parent, attributeName);
		}

		/// <inheritdoc cref="TypeIsNotValidTypeParameter(IDiagnosticReceiver, ITypeSymbol?, ITypeParameterSymbol?, Location?)"/>
		public static void TypeIsNotValidTypeParameter(IDiagnosticReceiver diagnosticReceiver, ITypeSymbol? type, ITypeParameterSymbol? parameter)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.TypeIsNotValidTypeParameter, parameter?.Locations.FirstOrDefault(), type, parameter);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that a type is not a valid type parameter (rule DUR0019).
		/// <para>See: <see cref="DurianDescriptors.TypeIsNotValidTypeParameter"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="type">Type that caused the error.</param>
		/// <param name="parameter">Type parameter that caused the error.</param>
		/// <param name="location">Location of the error.</param>
		public static void TypeIsNotValidTypeParameter(IDiagnosticReceiver diagnosticReceiver, ITypeSymbol? type, ITypeParameterSymbol? parameter, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.TypeIsNotValidTypeParameter, location, type, parameter);
		}

		/// <inheritdoc cref="AttributeOfOverridenMemberShouldBeAddedForClarity(IDiagnosticReceiver, ISymbol?, string?, Location?)"/>
		public static void AttributeOfOverridenMemberShouldBeAddedForClarity(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.AttributeOfOverridenMemberShouldBeAddedForClarity, symbol?.Locations.FirstOrDefault(), symbol, attributeName);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that the overriding member should define a specified attribute of the base member to preserve clarity (rule DUR0020).
		/// <para>See: <see cref="DurianDescriptors.AttributeOfOverridenMemberShouldBeAddedForClarity"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that caused the error.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="location">Location of the error.</param>
		public static void AttributeOfOverridenMemberShouldBeAddedForClarity(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.AttributeOfOverridenMemberShouldBeAddedForClarity, location, symbol, attributeName);
		}

		/// <inheritdoc cref="DoNotOverrideMembersGeneratedUsingSpecifiedAttribute(IDiagnosticReceiver, ISymbol?, string?, Location?)"/>
		public static void DoNotOverrideMembersGeneratedUsingSpecifiedAttribute(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.DoNotOverrideMembersGeneratedUsingSpecifiedAttribute, symbol?.Locations.FirstOrDefault(), symbol, attributeName);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that the user shouldn't override members generated using a specific attribute (rule DUR0021).
		/// <para>See: <see cref="DurianDescriptors.DoNotOverrideMembersGeneratedUsingSpecifiedAttribute"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that caused the error.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="location">Location of the error.</param>
		public static void DoNotOverrideMembersGeneratedUsingSpecifiedAttribute(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.DoNotOverrideMembersGeneratedUsingSpecifiedAttribute, location, symbol, attributeName);
		}

		/// <inheritdoc cref="ValueOfAttributeMustBeTheSameAsValueOfTheOverridenMember(IDiagnosticReceiver, ISymbol?, string?, Location?)"/>
		public static void ValueOfAttributeMustBeTheSameAsValueOfTheOverridenMember(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.ValueOfAttributeMustBeTheSameAsValueOfTheOverridenMember, symbol?.Locations.FirstOrDefault(), symbol, attributeName);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that value of a specified attribute must be the same as the value defined on the overridden member (rule DUR0022).
		/// <para>See: <see cref="DurianDescriptors.ValueOfAttributeMustBeTheSameAsValueOfTheOverridenMember"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that caused the error.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="location">Location of the error.</param>
		public static void ValueOfAttributeMustBeTheSameAsValueOfTheOverridenMember(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.ValueOfAttributeMustBeTheSameAsValueOfTheOverridenMember, location, symbol, attributeName);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that an essential type is missing and a specific package should be re-imported (rule DUR0023).
		/// <para>See: <see cref="DurianDescriptors.EssentialTypeIsMissing"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="typeName">Name of the type that is missing.</param>
		/// <param name="packageName">Package the type should be re-imported.</param>
		public static void EssentialTypeIsMissing(IDiagnosticReceiver diagnosticReceiver, string? typeName, string? packageName)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.EssentialTypeIsMissing, Location.None, typeName, packageName);
		}

		/// <inheritdoc cref="MethodWithSignatureAlreadyExists(IDiagnosticReceiver, ISymbol?, string?, Location?)"/>
		public static void MethodWithSignatureAlreadyExists(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? signature)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.MethodWithSignatureAlreadyExists, symbol?.Locations.FirstOrDefault(), symbol, signature);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that a method with the specified signature already exists (rule DUR0024).
		/// <para>See: <see cref="DurianDescriptors.MethodWithSignatureAlreadyExists"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> the value is defined on.</param>
		/// <param name="signature">Signature that already exists.</param>
		/// <param name="location">Location of the error.</param>
		public static void MethodWithSignatureAlreadyExists(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? signature, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.MethodWithSignatureAlreadyExists, location, symbol, signature);
		}

		/// <inheritdoc cref="DoNotAddDefaultParamAttributeOnOverriddenVirtualTypeParameter(IDiagnosticReceiver, ITypeParameterSymbol?, string?, Location?)"/>
		public static void DoNotAddDefaultParamAttributeOnOverriddenVirtualTypeParameter(IDiagnosticReceiver diagnosticReceiver, ITypeParameterSymbol? symbol, string? attributeName)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.DoNotAddAttributeOnVirtualTypeParameter, symbol?.Locations.FirstOrDefault(), symbol, attributeName);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that the user shouldn't add new attributes on a type parameter of an overridden virtual method (rule DUR0025).
		/// <para>See: <see cref="DurianDescriptors.DoNotAddAttributeOnVirtualTypeParameter"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ITypeParameterSymbol"/> the attribute is added to.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="location">Location of the error.</param>
		public static void DoNotAddDefaultParamAttributeOnOverriddenVirtualTypeParameter(IDiagnosticReceiver diagnosticReceiver, ITypeParameterSymbol? symbol, string? attributeName, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.DoNotAddAttributeOnVirtualTypeParameter, location, symbol, attributeName);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that the target project must reference the <c>Durian.Core</c> package (rule DUR0026).
		/// <para>See: <see cref="DurianDescriptors.ProjectMustReferenceDurianCore"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		public static void ProjectMustReferenceDurianCore(IDiagnosticReceiver diagnosticReceiver)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.ProjectMustReferenceDurianCore, Location.None);
		}

		/// <inheritdoc cref="AttributeCannotBeAppliedToMembersWithoutAttribute(IDiagnosticReceiver, ISymbol?, string?, string?, Location?)"/>
		public static void AttributeCannotBeAppliedToMembersWithoutAttribute(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName1, string? attributeName2)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.AttributeCannotBeAppliedToMembersWithAttribute, symbol?.Locations.FirstOrDefault(), symbol, attributeName1, attributeName2);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that the target attribute cannot be applied to members without a specified attribute (rule DUR0027).
		/// <para>See: <see cref="DurianDescriptors.AttributeCannotBeAppliedToMembersWithoutAttribute"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> the attribute cannot be applied to.</param>
		/// <param name="attributeName1">Name of the attribute that can't be applied to the <paramref name="symbol"/>.</param>
		/// <param name="attributeName2">Name of the attribute that the <paramref name="attributeName1"/> requires to be applied to the <paramref name="symbol"/>.</param>
		/// <param name="location">Location of the error.</param>
		public static void AttributeCannotBeAppliedToMembersWithoutAttribute(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName1, string? attributeName2, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.AttributeCannotBeAppliedToMembersWithAttribute, location, symbol, attributeName1, attributeName2);
		}

		/// <inheritdoc cref="AttributePropertyShouldNotBeUsedOnMembersOfType(IDiagnosticReceiver, ISymbol?, string?, string?, string?, Location?)"/>
		public static void AttributePropertyShouldNotBeUsedOnMembersOfType(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, string? propertyName, string? memberType)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.ProjectMustReferenceDurianCore, symbol?.Locations.FirstOrDefault(), symbol, propertyName, attributeName, memberType);
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s indicating that the specified attribute property shouldn't be used on members of the specified type (rule DUR0028).
		/// <para>See: <see cref="DurianDescriptors.AttributePropertyShouldNotBeUsedOnMembersOfType"/></para>
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to register the <see cref="Diagnostic"/>s to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that caused the error.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="memberType">Type of member the property should be used with.</param>
		/// <param name="location">Location of the error.</param>
		public static void AttributePropertyShouldNotBeUsedOnMembersOfType(IDiagnosticReceiver diagnosticReceiver, ISymbol? symbol, string? attributeName, string? propertyName, string? memberType, Location? location)
		{
			diagnosticReceiver.ReportDiagnostic(DurianDescriptors.ProjectMustReferenceDurianCore, location, symbol, propertyName, attributeName, memberType);
		}
	}
}
