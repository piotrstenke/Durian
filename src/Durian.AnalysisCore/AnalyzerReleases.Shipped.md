## Release 1.0

### New Rules
Rule ID | Category | Severity | Notes
--------|----------|----------|-----------------
DUR0001 |  Durian  |  Error   |  [Core]: Member name is reserved for internal purposes
DUR0002 |  Durian  |  Error   |  [Core]: Member with name already exists
DUR0003 |  Durian  |  Error   |  [Core]: Member names cannot be the same as their enclosing type
DUR0004 |  Durian  |  Error   |  [Core]: Value is not a valid identifier
DUR0005 |  Durian  |  Error   |  [Core]: Member name couldn't be resolved
DUR0006 |  Durian  |  Error   |  [Core]: Name refers to multiple members
DUR0007 |  Durian  |  Error   |  [Core]: Members from outside of the current assembly cannot be accessed
DUR0008 |  Durian  |  Error   |  [Core]: Member cannot refer to itself
DUR0009 |  Durian  |  Error   |  [Core]: Target member contains errors, and cannot be referenced
DUR0010 |  Durian  |  Error   |  [Core]: 'memberType' marked with the 'attributeName' attribute must be 'modifier'
DUR0011 |  Durian  |  Error   |  [Core]: 'memberType' marked with the 'attributeName' attribute cannot be 'modifier'
DUR0012 |  Durian  |  Error   |  [Core]: 'memberType' marked with the 'attributeName' attribute must have an implementation
DUR0013 |  Durian  |  Error   |  [Core]: 'memberType' marked with the 'attributeName' attribute cannot have an implementation
DUR0014 |  Durian  |  Error   |  [Core]: Types that contain members marked with the 'attributeName' attribute must be partial
DUR0015 |  Durian  |  Error   |  [Core]: Target of the 'attributeName' attribute must be a 'memberType'
DUR0016 |  Durian  |  Error   |  [Core]: 'attributeName' attribute cannot be applied to a 'memberType'
DUR0017 |  Durian  |  Error   |  [Core]: Attribute 'attributeName1' cannot be applied to members with the 'attributeName2' attribute
DUR0018 |  Durian  |  Error   |  [Core]: Type parameter marked with the 'attributeName' attribute must be placed last
DUR0019 |  Durian  |  Error   |  [Core]: Specified type is not a valid type parameter
DUR0020 |  Durian  |  Warning |  [Core]: Attribute 'attributeName' of overridden member should be added for clarity
DUR0021 |  Durian  |  Error   |  [Core]: Do not override members generated using the 'attributeName' attribute
DUR0022 |  Durian  |  Error   |  [Core]: Value of attribute 'attributeName' must be the same as the value defined on the overridden member
DUR0023 |  Durian  |  Error   |  [Core]: Essential type 'typeName' is missing. Try reimporting the 'packageName' package
DUR0024 |  Durian  |  Error   |  [Core]: Method with the specified signature already exists
DUR0025 |  Durian  |  Error   |  [Core]: Do not add the 'attributeName' attribute on type parameters of overridden virtual methods
DUR0026 |  Durian  |  Error   |  [Core]: Projects with any Durian analyzer must reference the Durian.Core package
DUR0027 |  Durian  |  Error   |  [Core]: Attribute 'attributeName1' cannot be applied to members without the 'attributeName2' attribute
DUR0028 |  Durian  |  Warning   |  [Core]: Property 'propertyName' of attribute 'attributeName' shouldn't be used on members of type 'memberType'