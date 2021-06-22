## Release 1.0.0

### New Rules
Rule ID | Category | Severity | Notes
--------|----------|----------|-----------------------------------------
DUR0201 | Durian.GenericSpecialization | Error | Non-generic types cannot use the AllowSpecializationAttribute. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/GenericSpecialization/DUR0201.md)]
DUR0202 | Durian.GenericSpecialization | Error | Target class must be marked with the AllowSpecializationAttribute. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/GenericSpecialization/DUR0202.md)]
DUR0203 | Durian.GenericSpecialization | Error | Specified name is not a valid identifier. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/GenericSpecialization/DUR0203.md)]
DUR0204 | Durian.GenericSpecialization | Warning | Do not specify the GenericSpecializationConfigurationAttribute on members that do not contain any generic child classes or are not generic themselves. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/GenericSpecialization/DUR0204.md)]
DUR0205 | Durian.GenericSpecialization | Error | Generic specialization must inherit the default implementation class. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/GenericSpecialization/DUR0205.md)]
DUR0206 | Durian.GenericSpecialization | Error | Generic specialization must implement the specialization interface. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/GenericSpecialization/DUR0206.md)]
DUR0207 | Durian.GenericSpecialization | Error | Class marked with the GenericSpecializationAttribute cannot be abstract or static. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/GenericSpecialization/DUR0207.md)]
DUR0208 | Durian.GenericSpecialization | Error | Provide default implementation of the target generic class. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/GenericSpecialization/DUR0208.md)]
DUR0209 | Durian.GenericSpecialization | Error | Class marked with the AllowSpecializationAttribute must be partial. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/GenericSpecialization/DUR0209.md)]
DUR0210 | Durian.GenericSpecialization | Error | Containing type of a member marked with the AllowSpecializationAttribute must be partial. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/GenericSpecialization/DUR0210.md)]
DUR0211 | Durian.GenericSpecialization | Error | Class marked with the AllowSpecializationAttribute cannot be abstract or static. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/GenericSpecialization/DUR0211.md)]
DUR0212 | Durian.GenericSpecialization | Error | Do not provide a specialization for a generic class that is not marked with the AllowSpecializationAttribute. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/GenericSpecialization/DUR0212.md)]
DUR0213 | Durian.GenericSpecialization | Error | Do not provide a specialization for a generic class that is not part of the current assembly. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/GenericSpecialization/DUR0213.md)]
DUR0214 | Durian.GenericSpecialization | Error | Do not provide a specialization for a generic class from the System namespace or any of its child namespaces. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/GenericSpecialization/DUR0214.md)]
DUR0215 | Durian.GenericSpecialization | Error | Type specified in the GenericSpecializationAttribute must be an unbound generic type. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/GenericSpecialization/DUR0215.md)]
DUR0216 | Durian.GenericSpecialization | Warning | Duplicate GenericSpecializationAttribute type. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/GenericSpecialization/DUR0216.md)]
DUR0217 | Durian.GenericSpecialization | Error | Do not declare members in a class marked with the AllowSpecializationAttribute. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/GenericSpecialization/DUR0217.md)]
DUR0218 | Durian.GenericSpecialization | Error | Do not specify base types or implemented interfaces for a class marked with the AllowSpecializationAttribute. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/GenericSpecialization/DUR0218.md)]
DUR0219 | Durian.GenericSpecialization | Error | Do not specify attributes on a class marked with the AllowSpecializationAttribute. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/GenericSpecialization/DUR0219.md)]
DUR0220 | Durian.GenericSpecialization | Warning | Do not force inherit a sealed class. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/GenericSpecialization/DUR0220.md)]
