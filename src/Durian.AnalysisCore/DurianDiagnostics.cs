using Microsoft.CodeAnalysis;

namespace Durian
{
	/// <summary>
	/// Contains <see cref="DiagnosticDescriptor"/>s of the most common Durian errors.
	/// </summary>
	public static class DurianDiagnostics
	{
		/// <summary>
		/// Provides diagnostic message indicating that the name of the target member is reserved for internal purposes.
		/// </summary>
		public static readonly DiagnosticDescriptor MemberNameIsReserved = new(
			id: "DUR0001",
			title: "Member name is reserved for internal purposes",
			messageFormat: "'{0}': Member name '{1}' is reserved for internal purposes",
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
		/// Provides diagnostic message indicating that the member name could not be resolved.
		/// </summary>
		public static readonly DiagnosticDescriptor MemberNameCouldNotBeResolved = new(
			id: "DUR0004",
			title: "Member name could not be resolved",
			messageFormat: "'{0}': Member name '{1}' could not be resolved",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the specified name refers to multiple members.
		/// </summary>
		public static readonly DiagnosticDescriptor NameRefersToMultipleMembers = new(
			id: "DUR0005",
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
			id: "DUR0006",
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
			id: "DUR0007",
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
			id: "DUR0008",
			title: "Target member contains errors, and cannot be referenced",
			messageFormat: "'{0}': Member '{2}' contains errors, and cannot be referenced",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that a method with the specified signature already exists.
		/// </summary>
		public static readonly DiagnosticDescriptor MethodWithSignatureAlreadyExists = new(
			id: "DUR0009",
			title: "Method with the specified signature already exists",
			messageFormat: "'{0}': Method with signature '{1}' already exists",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the target project must reference the <c>Durian.Core</c> package.
		/// </summary>
		public static readonly DiagnosticDescriptor ProjectMustReferenceDurianCore = new(
			id: "DUR0010",
			title: "Projects with any Durian analyzer must reference the Durian.Core package",
			messageFormat: "Projects with any Durian analyzer must reference the Durian.Core package",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);
	}
}
