using Microsoft.CodeAnalysis;

namespace Durian
{
	/// <summary>
	/// Contains <see cref="DiagnosticDescriptor"/>s of the most common errors.
	/// </summary>
	public static class Descriptors
	{
		/// <summary>
		/// Provides diagnostic message indicating that the name of the target member is reserved for internal purposes.
		/// </summary>
		public static readonly DiagnosticDescriptor MemberNameIsReserved = new(
			id: "DUR0001",
			title: "Member name is reserved for internal purposes",
			messageFormat: "Member name '{0}' is reserved for internal purposes",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that a member with the specified name already exists.
		/// </summary>
		public static readonly DiagnosticDescriptor MemberWithNameAlreadyExists = new(
			id: "DUR0002",
			title: "Member with name already exists",
			messageFormat: "'{0}': Member with name '{1}' already exists",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the member has the same name as the enclosing type.
		/// </summary>
		public static readonly DiagnosticDescriptor MemberNameCannotBeTheSameAsParentType = new(
			id: "DUR0003",
			title: "Member names cannot be the same as their enclosing type",
			messageFormat: "'{0}': Member names cannot be the same as their enclosing type",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the target value is not a valid identifier.
		/// </summary>
		public static readonly DiagnosticDescriptor ValueIsNotValidIdentifier = new(
			id: "DUR0004",
			title: "Value is not a valid identifier",
			messageFormat: "Value '{0}' is no a valid identifier",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the member name could not be resolved.
		/// </summary>
		public static readonly DiagnosticDescriptor MemberNameCouldNotBeResolved = new(
			id: "DUR0005",
			title: "Member name couldn't be resolved",
			messageFormat: "'{0}': Member name '{}' couldn't be resolved.",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the specified name refers to multiple members.
		/// </summary>
		public static readonly DiagnosticDescriptor NameRefersToMultipleMembers = new(
			id: "DUR0006",
			title: "Name refers to multiple members",
			messageFormat: "'{0}': Name '{1}' refers to multiple members",
			description: "To be more specific in the search, include parameters, generic parameters etc.",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that members from outside of the current assembly cannot be accessed.
		/// </summary>
		public static readonly DiagnosticDescriptor MembersFromOutsideOfAssemblyCannotBeAccessed = new(
			id: "DUR0007",
			title: "Members from outside of the current assembly cannot be accessed",
			messageFormat: "'{0}': Members from outside of the current assembly cannot be accessed",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the target member cannot refer to itself.
		/// </summary>
		public static readonly DiagnosticDescriptor MemberCannotReferToItself = new(
			id: "DUR0008",
			title: "Member cannot refer to itself",
			messageFormat: "'{0}': Member cannot refer to itself",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the target members contains errors, and cannot be referenced.
		/// </summary>
		public static readonly DiagnosticDescriptor TargetMemberContainsErrors = new(
			id: "DUR0009",
			title: "Target member contains errors, and cannot be referenced",
			messageFormat: "'{0}': Member '{2}' contains errors, and cannot be referenced",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that a member marked with a specified attribute must be declared using a specific modifier.
		/// </summary>
		public static readonly DiagnosticDescriptor MemberWithAttributeMustHaveModifier = new(
			id: "DUR0010",
			title: "'memberType' marked with the 'attributeName' attribute must be 'modifier'",
			messageFormat: "'{0}': {1} marked with the '{2}' attribute must be {3}",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that a member marked with a specified attribute cannot be declared using a specific modifier.
		/// </summary>
		public static readonly DiagnosticDescriptor MemberWithAttributeCannotHaveModifier = new(
			id: "DUR0011",
			title: "'memberType' marked with the 'attributeName' attribute cannot be 'modifier'",
			messageFormat: "'{0}': {1} marked with the '{2}' attribute cannot be {3}",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that a method marked with a specified attribute must have an implementation.
		/// </summary>
		public static readonly DiagnosticDescriptor MemberWithAttributeMustHaveImplementation = new(
			id: "DUR0012",
			title: "'memberType' marked with the 'attributeName' attribute must have an implementation",
			messageFormat: "'{0}': {1} marked with the '{2}' attribute must have an implementation",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that a method marked with a specified attribute cannot have an implementation.
		/// </summary>
		public static readonly DiagnosticDescriptor MemberWithAttributeCannotHaveImplementation = new(
			id: "DUR0013",
			title: "'memberType' marked with the 'attributeName' attribute cannot have an implementation",
			messageFormat: "'{0}': {1} marked with the '{1}' attribute cannot have an implementation",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the parent type of a member with a specified attribute must be partial.
		/// </summary>
		public static readonly DiagnosticDescriptor ParentTypeOfMemberWithAttributeMustBePartial = new(
			id: "DUR0014",
			title: "Types that contain members marked with the 'attributeName' attribute must be partial",
			messageFormat: "'{0}': Types that contain members marked with the '{1}' attribute must be partial",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the target member is of invalid type (e.g expected a method, but got a property)
		/// </summary>
		public static readonly DiagnosticDescriptor TargetOfAttributeMustBeOfSpecifiedMemberType = new(
			id: "DUR0015",
			title: "Target of the 'attributeName' attribute must be a 'memberType'",
			messageFormat: "'{0}': Target of the '{1}' attribute must be a {2}",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the attribute cannot be applies to members of specific declaration type.
		/// </summary>
		public static readonly DiagnosticDescriptor AttributeCannotBeAppliedToMembersOfType = new(
			id: "DUR0016",
			title: "'attributeName' attribute cannot be applied to a 'memberType'",
			messageFormat: "'{0}': '{1}' attribute cannot be applied to a {2}",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the target attribute cannot be applied to members with a specified attribute.
		/// </summary>
		public static readonly DiagnosticDescriptor AttributeCannotBeAppliedToMembersWithAttribute = new(
			id: "DUR0017",
			title: "Attribute 'attributeName1' cannot be applied to members with the 'attributeName2' attribute",
			messageFormat: "'{0}': Attribute '{1}' cannot be applied to members with the '{2}' attribute'",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that a type parameter marked with a specified attribute must be placed last in the declaration.
		/// </summary>
		public static readonly DiagnosticDescriptor TypeParameterWithAttributeMustBeLast = new(
			id: "DUR0018",
			title: "Type parameter marked with the 'attributeName' attribute must be placed last",
			messageFormat: "'{0}': Type parameter marked with the '{1}' attribute must be placed last",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the specified type is not a valid type parameter.
		/// </summary>
		public static readonly DiagnosticDescriptor TypeIsNotValidTypeParameter = new(
			id: "DUR0019",
			title: "Specified type is not a valid type parameter",
			messageFormat: "'Type '{0}' is not valid for the '{1}' type parameter",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the member with the 'override' keyword should define a specified attribute to preserve clarity.
		/// </summary>
		public static readonly DiagnosticDescriptor AttributeOfOverridenMemberShouldBeAddedForClarity = new(
			id: "DUR0020",
			title: "Attribute 'attributeName' of overridden member should be added for clarity",
			messageFormat: "'{0}': Attribute '{1}' of overridden member should be added for clarity",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the user shouldn't override members generated using a specific attribute.
		/// </summary>
		public static readonly DiagnosticDescriptor DoNotOverrideMembersGeneratedUsingSpecifiedAttribute = new(
			id: "DUR0021",
			title: "Do not override members generated using the 'attributeName' attribute",
			messageFormat: "'{0}': Do not override members generated using the '{1}' attribute",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that value of a specified attribute must be the same as the value defined on the overridden member.
		/// </summary>
		public static readonly DiagnosticDescriptor ValueOfAttributeMustBeTheSameAsValueOfTheOverridenMember = new(
			id: "DUR0022",
			title: "Value of attribute 'attributeName' must be the same as the value defined on the overridden member",
			messageFormat: "'{0}': Value of attribute '{1}' must be the same as the value defined on the overridden member",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that an essential type is missing and a specific package should be re-imported.
		/// </summary>
		public static readonly DiagnosticDescriptor EssentialTypeIsMissing = new(
			id: "DUR0023",
			title: "Essential type 'typeName' is missing. Try reimporting the 'packageName' package.",
			messageFormat: "Essential type '{0}' is missing. Try reimporting the '{1}' package",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that a method with the specified signature already exists.
		/// </summary>
		public static readonly DiagnosticDescriptor MethodWithSignatureAlreadyExists = new(
			id: "DUR0024",
			title: "Method with the specified signature already exists",
			messageFormat: "'{0}': Method with signature '{1}' already exists",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the user shouldn't add new attributes on a type parameter of an overridden virtual method.
		/// </summary>
		public static readonly DiagnosticDescriptor DoNotAddDefaultParamAttributeOnOverriddenVirtualTypeParameter = new(
			id: "DUR0025",
			title: "Do not add the 'attributeName' attribute on type parameters of overridden virtual methods",
			messageFormat: "'{0}': Do not add the '{1}' attribute on type parameters of overridden virtual methods",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the target project must reference the <c>Durian.Core</c> package.
		/// </summary>
		public static readonly DiagnosticDescriptor ProjectMustReferenceDurianCore = new(
			id: "DUR0026",
			title: "Projects with any Durian analyzer must reference the Durian.Core package",
			messageFormat: "Projects with any Durian analyzer must reference the Durian.Core package",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);
	}
}
