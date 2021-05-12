## Release 1.0.0

### New Rules
Rule ID | Category | Severity | Notes
--------|----------|----------|-----------------
DUR0101 |  Durian.DefaultParam  |   Error   | [https://github.com/piotrstenke/Durian/docs/DefaultParam/DUR0101.md] Containing type of a member with the DefaultParam attribute must be partial
DUR0102 |  Durian.DefaultParam  |   Error   | [https://github.com/piotrstenke/Durian/docs/DefaultParam/DUR0102.md] Method with the DefaultParam attribute cannot be partial or extern
DUR0103 |  Durian.DefaultParam  |   Error   | [https://github.com/piotrstenke/Durian/docs/DefaultParam/DUR0103.md] DefaultParamAttribute is not valid on local functions or lambdas
DUR0104 |  Durian.DefaultParam  |   Error   | [https://github.com/piotrstenke/Durian/docs/DefaultParam/DUR0104.md] DefaultParamAttribute cannot be applied to members with the GeneratedCodeAttribute or DurianGeneratedAttribute
DUR0105 |  Durian.DefaultParam  |   Error   | [https://github.com/piotrstenke/Durian/docs/DefaultParam/DUR0105.md] DefaultParamAttribute must be placed on the right-most type parameter
DUR0106 |  Durian.DefaultParam  |   Error   | [https://github.com/piotrstenke/Durian/docs/DefaultParam/DUR0106.md] Value of DefaultParamAttribute does not satisfy the type constraint
DUR0107 |  Durian.DefaultParam  |   Error   | [https://github.com/piotrstenke/Durian/docs/DefaultParam/DUR0107.md] Do not override methods generated using DefaultParamAttribute
DUR0108 |  Durian.DefaultParam  |   Error   | [https://github.com/piotrstenke/Durian/docs/DefaultParam/DUR0108.md] Value of DefaultParamAttribute of overriding method must match the base method
DUR0109 |  Durian.DefaultParam  |   Error   | [https://github.com/piotrstenke/Durian/docs/DefaultParam/DUR0109.md] Do not add the DefaultParamAttribute on overridden type parameters
DUR0110 |  Durian.DefaultParam  |  Warning  | [https://github.com/piotrstenke/Durian/docs/DefaultParam/DUR0100.md] DefaultParamAttribute of overridden type parameter should be added for clarity
DUR0111 |  Durian.DefaultParam  |   Error   | [https://github.com/piotrstenke/Durian/docs/DefaultParam/DUR0111.md] DefaultParamConfigurationAttribute is not valid on members without the DefaultParamAttribute
DUR0112 |  Durian.DefaultParam  |  Warning  | [https://github.com/piotrstenke/Durian/docs/DefaultParam/DUR0112.md] TypeConvention property should not be used on members other than types
DUR0113 |  Durian.DefaultParam  |  Warning  | [https://github.com/piotrstenke/Durian/docs/DefaultParam/DUR0113.md] MethodConvention property should not be used on members other than methods
DUR0114 |  Durian.DefaultParam  |  Warning  | [https://github.com/piotrstenke/Durian/docs/DefaultParam/DUR0114.md] ApplyNewModifierWhenPossible property should not be used on members directly