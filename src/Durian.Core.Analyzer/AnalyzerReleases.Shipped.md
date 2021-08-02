## Release 1.0.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-----------------------------------------
DUR0001 | Durian | Error | Projects with any Durian analyzer must reference the Durian.Core package. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/Core/DUR0001.md)]
DUR0002 | Durian | Error | Type cannot be accessed, because its module is not imported. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/Core/DUR0002.md)]
DUR0003 | Durian | Error | Do not use types from the Durian.Generator namespace. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/Core/DUR0003.md)]
DUR0004 | Durian | Error | Durian modules can be used only in C#. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/Core/DUR0004.md)]
DUR0005 | Durian | Error | Do not add custom types to the Durian.Generator namespace. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/Core/DUR0005.md)]
DUR0006 | Durian | Error | Target project must use C# 9 or newer.

## Release 1.1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-----------------------------------------
DUR0007 | Durian | Error | Do not reference Durian analyzer package if the main Durian package is already included. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/Core/DUR0007.md)]

## Release 2.0.0

### Removed Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-----------------------------------------
DUR0006 | Durian | Error | Target project must use C# 9 or newer.

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-----------------------------------------
DUR0008 | Durian | Warning | Separate analyzer packages detected, reference the main Durian package instead for better performance. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/Core/DUR0008.md)]
