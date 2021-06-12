## Release 1.0.0

### New Rules
Rule ID | Category | Severity | Notes
--------|----------|----------|-----------------------------------------
DUR0101 | Durian.DefaultParam | Error | Containing type of a member with the DefaultParamAttribute must be partial. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0101.md)]
DUR0102 | Durian.DefaultParam | Error | Method with the DefaultParamAttribute cannot be partial or extern. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0102.md)]
DUR0103 | Durian.DefaultParam | Error | DefaultParamAttribute is not valid on this type of method. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0103.md)]
DUR0104 | Durian.DefaultParam | Error | DefaultParamAttribute cannot be applied to members with the GeneratedCodeAttribute or DurianGeneratedAttribute. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0104.md)]
DUR0105 | Durian.DefaultParam | Error | DefaultParamAttribute must be placed on the right-most type parameter or right to the left-most DefaultParam type parameter. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0105.md)]
DUR0106 | Durian.DefaultParam | Error | Value of DefaultParamAttribute does not satisfy the type constraint. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0106.md)]
DUR0107 | Durian.DefaultParam | Error | Do not override methods generated using the DefaultParamAttribute. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0107.md)]
DUR0108 | Durian.DefaultParam | Error | Value of DefaultParamAttribute of overriding method must match the base method. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0108.md)]
DUR0109 | Durian.DefaultParam | Error | Do not add the DefaultParamAttribute on overridden type parameters that are not DefaultParam. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0109.md)]
DUR0110 | Durian.DefaultParam | Warning | DefaultParamAttribute of overridden type parameter should be added for clarity. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0110.md)]
DUR0111 | Durian.DefaultParam | Error | DefaultParamConfigurationAttribute is not valid on members without the DefaultParamAttribute. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0111.md)]
DUR0112 | Durian.DefaultParam | Warning | TypeConvention property should not be used on members other than types. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0112.md)]
DUR0113 | Durian.DefaultParam | Warning | MethodConvention property should not be used on members other than methods. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0113.md)]
DUR0114 | Durian.DefaultParam | Error | Method with generated signature already exists. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0114.md)]
DUR0115 | Durian.DefaultParam | Error | DefaultParamConfigurationAttribute is not valid on this type of method. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0115.md)]
DUR0116 | Durian.DefaultParam | Error | Member with generated name already exists. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0116.md)]
DUR0117 | Durian.DefaultParam | Error | DPTypeConvention.Inherit cannot be used on a struct or a sealed type. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0117.md)]
DUR0118 | Durian.DefaultParam | Warning | DPTypeConvention.Copy or DPTypeConvention.Default should be applied for clarity. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0118.md)]
DUR0119 | Durian.DefaultParam | Error | DefaultParam value cannot be less accessible than the target member. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0119.md)]
DUR0120 | Durian.DefaultParam | Error | Type is invalid DefaultParam value when there is a type parameter constrained to this type parameter. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0120.md)]
DUR0121 | Durian.DefaultParam | Error | Type is invalid DefaultParam value. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0121.md)]
DUR0122 | Durian.DefaultParam | Error | DefaultParamAttribute cannot be used on a partial type. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0122.md)]
DUR0123 | Durian.DefaultParam | Error | TypeConvention.Inherit cannot be used on a type without accessible constructor. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0123.md)]
DUR0124 | Durian.DefaultParam | Warning | ApplyNewModifierWhenPossible should not be used when target is not a child type. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0124.md)]
DUR0125 | Durian.DefaultParam | Warning | DefaultParamScopedConfigurationAttribute should not be used on types with no DefaultParam members. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0125.md)]
DUR0126 | Durian.DefaultParam | Error | Members with the DefaultParamAttribute cannot be nested within other DefaultParam members. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0126.md)]

## Release 1.1.0

### New Rules
Rule ID | Category | Severity | Notes
--------|----------|----------|-----------------------------------------
DUR0127 | Durian.DefaultParam | Warning | Target namespace is not a valid identifier. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0127.md)]

## Release 1.2.0

### New Rules
Rule ID | Category | Severity | Notes
--------|----------|----------|-----------------------------------------
DUR0128 | Durian.DefaultParam | Warning | Do not specify target namespace for a nested member. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0128.md)]
DUR0129 | Durian.DefaultParam | Error | Target namespace already contains member with the specified name. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0129.md)]