<div align="left">
	<a href="https://www.nuget.org/packages/Durian.DefaultParam">
		<img src="https://img.shields.io/nuget/v/Durian.DefaultParam?color=seagreen&style=flat-square" alt="Version"/>
	</a>
	<a href="https://www.nuget.org/packages/Durian.DefaultParam">
		<img src="https://img.shields.io/nuget/dt/Durian.DefaultParam?color=blue&style=flat-square" alt="Downloads"/>
	</a> <br />
</div>

<div align="center">
		<img src="../../img/icons/Durian-256.png" alt="Durian logo"/>
</div>

##

**DefaultParam, based on a similar feature in C++, allows the user to specify a default type for a generic parameter.**

## Table of Contents

1. [Structure](#structure)
2. [Setup](#setup)
3. [Basics](#basics)
	1. [Parameter constraints](#parameter-constraints)
	2. [Accessibility](#accessibility)
	3. [Invalid values](#invalid-values)
	4. [Member limitations](#member-limitations)
4. [Configuration](#configuration)
   1. [Local configuration](#local-configuration)
   2. [Scoped configuration](#scoped-configuration)
   3. [Configuration priority](#configuration-priority)
   4. [Method convention](#method-convention)
   5. [Type convention](#type-convention)
   6. [Target namespace](#target-namespace)
   7. ['new' modifier](#new-modifier)
5. [Inheritance](#inheritance) 

## Structure

Packages that are part of the *DefaultParam* module:

 - [*Durian.DefaultParam*](https://www.nuget.org/packages/Durian.DefaultParam/)

*DefaultParam* provides 5 types: 

 - [Durian.DefaultParamAttribute](../Durian.DefaultParam/DefaultParamAttributeProvider.cs)
 - [Durian.Configuration.DefaultParamConfigurationAttribute](../Durian.DefaultParam/DefaultParamConfigurationAttributeProvider.cs)
 - [Durian.Configuration.DefaultParamScopedConfigurationAttribute](../Durian.DefaultParam/DefaultParamScopedConfigurationAttributeProvider.cs)
 - [Durian.Configuration.DPTypeConvention](../Durian.DefaultParam/DPTypeConventionProvider.cs)
 - [Durian.Configuration.DPMethodConvention](../Durian.DefaultParam/DPMethodConventionProvider.cs)

## Setup

To start using *DefaultParam*, reference either the [*Durian.DefaultParam*](https://www.nuget.org/packages/Durian.DefaultParam/) or [*Durian*](https://www.nuget.org/packages/Durian/) package. 

**Note**: 
Like with other Durian modules, the target project must reference the [Durian.Core](../Durian.Core/README.md) package as well.

## Basics

Main type in this module is the [Durian.DefaultParamAttribute](../Durian.DefaultParam/DefaultParamAttributeProvider.cs). When attribute is placed on a type parameter, a new member is generated - a direct copy of the original, but with the target type parameter replaced with the specified default type.

```csharp
using Durian;

public class Test<[DefaultParam(typeof(string))]T>
{
	public T Value { get; }

	public Test(T value)
	{
		Value = value;
	}
}

// Generated

public class Test : Test<string>
{
	public Test(string value) : base(value)
	{
	}
}

```

[Durian.DefaultParamAttribute](../Durian.DefaultParam/DefaultParamAttributeProvider.cs) can only be placed on the last type parameter or next to another [Durian.DefaultParamAttribute](../Durian.DefaultParam/DefaultParamAttributeProvider.cs).

If multiple type parameters are marked with the [Durian.DefaultParamAttribute](../Durian.DefaultParam/DefaultParamAttributeProvider.cs), multiple members are generated accordingly.

```csharp
using Durian;

public class Test<[DefaultParam(typeof(int))]T, [DefaultParam(typeof(string))]U>
{
	public U UValue { get; }
	public T TValue { get; }

	public Test(T t_value, U u_value)
	{
		TValue = t_value;
		UValue = u_value;
	}
}

// Generated

public class Test<T> : Test<T, string>
{
	public Test(T t_value, string u_value) : base(t_value, u_value)
	{
	}
}

public class Test : Test<int, string>
{
	public Test(int t_value, string u_value) : base(t_value, u_value)
	{
	}
}

```

### Parameter constraints

Default type defined using the [Durian.DefaultParamAttribute](../Durian.DefaultParam/DefaultParamAttributeProvider.cs) must fulfill all constraints of the target type parameter.

```csharp
using Durian;

// 'string' fulfills the constraints of 'T', so it is a valid value.
public class Test<[DefaultParam(typeof(string))]T> where T : class
{
}

// 'string' does not fulfill the constrains of 'T', so its not a valid value.
public class Other<[DefaultParam(typeof(string))]T> where T : struct
{
}

```

### Accessibility

*DefaultParam* respects all language restrictions of accessibility. As long as the target type parameter is never used in a context more accessible than the value of [Durian.DefaultParamAttribute](../Durian.DefaultParam/DefaultParamAttributeProvider.cs), it is not an error.

```csharp
using Durian;

internal class Dummy
{
}

// Dummy is valid, because it is the same accessibility as Test<T>
internal class Test<[DefaultParam(typeof(Dummy))]T>
{
}

// Dummy is not valid, because internal is less accessible than public.
public class Other<[DefaultParam(typeof(Dummy))]T>
{
}

```

### Invalid values

Not all types are valid for the [Durian.DefaultParamAttribute](../Durian.DefaultParam/DefaultParamAttributeProvider.cs). This includes: 

 - void or [System.Void](https://docs.microsoft.com/en-us/dotnet/api/system.void);
 - pointers or function pointers;
 - ref structs, like [System.Span\<T\>](https://docs.microsoft.com/en-us/dotnet/api/system.span-1);
 - static classes;
 - dynamic (language restriction);
 - nullable reference type (language restriction);
 - unbound generic types.

Additionally, the following types can't be used when there is a type parameter constrained to the target type parameter:

- [System.Object](https://docs.microsoft.com/en-us/dotnet/api/system.object);
-  [System.ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype);
 - [System.Array](https://docs.microsoft.com/en-us/dotnet/api/system.array);
 - any array type, either jagged or multidimensional;
 - any delegate type.

### Member limitations

Most generic-compatible members are supported, but there are some notable rules and exceptions:

- target member cannot be placed within another member with the [Durian.DefaultParamAttribute](../Durian.DefaultParam/DefaultParamAttributeProvider.cs);
- all containing types of the target member must be *partial*;
- if the member is a type, it cannot be *partial*;
- if the *DPTypeConvention.Inherit* (see: [Configuration](#configuration)) is applied, the member cannot be a *struct* or a *sealed* or *static* class and must define at least one accessible constructor;
- if the member is a method, it cannot be *partial* or *extern*;
- local methods, methods located in an interface and explicit implementations are not supported.

## Configuration

*DefaultParam* allows to configure how new members are generated through the [Durian.Configuration.DefaultParamConfigurationAttribute](../Durian.DefaultParam/DefaultParamConfigurationAttributeProvider.cs) and [Durian.Configuration.DefaultParamScopedConfigurationAttribute](../Durian.DefaultParam/DefaultParamScopedConfigurationATtributeProvider.cs) types.

### Local configuration

Local configuration is determined through the [Durian.Configuration.DefaultParamConfigurationAttribute](../Durian.DefaultParam/DefaultParamConfigurationATtributeProvider.cs). It is applied only to the member it is defined on.

```csharp
using Durian;
using Durian.Configuration;

// The configuration is applied only to  'Test<T, U>'. 'Delegate<T, U>' and Dummy.Method<T> are not included.
[DefaultParamConfiguration(TypeConvention = DPTypeConvention.Inherit)]
public class Test<T, [DefaultParam(typeof(string))]U>
{
	public void Method(U value)
	{
		U second = value;
	}
}

public delegate void Delegate<T, [DefaultParam(typeof(string))]U>();

public partial class Dummy
{
	public void Method<[DefaultParam(typeof(string))]T>()
	{
	}
}

```

**Note**: This attribute shouldn't be placed on members without the [Durian.DefaultParamAttribute](../Durian.DefaultParam/DefaultParamAttributeProvider.cs), as in such case it is meaningless.

### Scoped configuration

Scoped configuration is determined through the [Durian.Configuration.DefaultParamScopedConfigurationAttribute](../Durian.DefaultParam/DefaultParamScopedConfigurationAttributeProvider.cs). It is applied to all members in the current scope.

```csharp
using Durian;
using Durian.Configuration;

// The configuration is applied to all DefaultParam members in the current assembly.
[assembly: DefaultParamScopedConfiguration(TypeConvention = DPTypeConvention.Inherit)]

// The configuration is applied to all DefaultParam members inside this type.
[DefaultParamScopedConfiguration(TypeConvention = DPTypeConvention.Default)]
public partial class Test
{
	public void Method<[DefaultParam(typeof(string))]T>(T value)
	{
		U second = value;
	}
}

public class Another<T, [DefaultParam(typeof(string))]U>
{
}

```

**Note**: This attribute shouldn't be placed on a type with no members with the [Durian.DefaultParamAttribute](../Durian.DefaultParam/DefaultParamAttributeProvider.cs), as in such case it is meaningless.

### Configuration priority

The [Durian.Configuration.DefaultParamScopedConfigurationAttribute](../Durian.DefaultParam/DefaultParamScopedConfigurationAttributeProvider.cs) acts only as the default configuration for the scope - it cannot override values defined by a [Durian.Configuration.DefaultParamConfigurationAttribute](../Durian.DefaultParam/DefaultParamConfigurationAttributeProvider.cs). 

When applying the configuration, the generator will pick the inner most one it can find.

```csharp
using Durian;
using Durian.Configuration;

// Global configuration has the lowest priority and will be picked only if there is no other configuration available.
[assembly: DefaultParamScopedConfiguration(MethodConvention = DPMethodConvention.Call)]

// Configuration of a containing type will be picked only if the target member has no configuration applied on itself.
[DefaultParamScopedConfiguration(MethodConvention = DPMethodConvention.Default)]
public partial class Test
{
	public void Method<[DefaultParam(typeof(string))]T>(T value)
	{
	}
}

[DefaultParamScopedConfiguration(MethodConvention = DPMethodConvention.Default)]
public partial class Other
{
	// This configuration is applied directly on the member, so it has the biggest priority.
	[DefaultParamConfiguration(MethodConvention = DPMethodConvention.Call)]
	public void Method<[DefaultParam(typeof(string))]T>(T value)
	{
	}
}

```

If the configuration with the highest priority does not specify a value for a property, value of configuration with lower priority is used instead.

```csharp
using Durian;
using Durian.Configuration;

[assembly: DefaultParamScopedConfiguration(MethodConvention = DPMethodConvention.Call)]

// Method<T> does not specify its own configuration, so the scoped one is used instead.
// However, it does not specify a value for the MethodConvention property, 
// so the value from the global configuration is picked.
[DefaultParamScopedConfiguration]
public partial class Test
{
	public void Method<[DefaultParam(typeof(string))]T>(T value)
	{
	}
}

[DefaultParamScopedConfiguration(MethodConvention = DPMethodConvention.Default)]
public partial class Other
{
	// This configuration does not specify a MethodConvention,
	// so value of the scoped configuration of the containing type is used instead.
	[DefaultParamConfiguration]
	public void Method<[DefaultParam(typeof(string))]T>(T value)
	{
	}
}

```

### Method convention

Both [Durian.Configuration.DefaultParamConfigurationAttribute](../Durian.DefaultParam/DefaultParamConfigurationAttributeProvider.cs) and [Durian.Configuration.DefaultParamScopedConfigurationAttribute](../Durian.DefaultParam/DefaultParamScopedConfigurationAttributeProvider.cs) define a *MethodConvention* property of type [Durian.Configuration.DPMethodConvention](../Durian.DefaultParam/DPMethodConventionProvider.cs). Thanks to this property, the user can specify how the target methods should be generated.

[Durian.Configuration.DPMethodConvention](../Durian.DefaultParam/DPMethodConventionProvider.cs) is an enum with three constants:

 - *Default*
 - *Call*
 - *Copy*

*Default* and *Call* have the same value, and are used when no other method convention is specified.

When the *Call* convention is applied, the target method is called by the generated method with the specified default type as generic argument.

```csharp
using Durian;
using Durian.Configuration;

public partial class Other
{
	[DefaultParamConfiguration(MethodConvention = DPMethodConvention.Call)]
	public T Method<[DefaultParam(typeof(string))]T>(T value)
	{
		T other = value;
		return default(T);
	}
}

// Generated

public partial class Other
{
	public string Method(string value)
	{
		return Method<string>(value);
	}
}

```

When the *Copy* convention is applied, the contents of the target method is copied and all the references to the DefaultParam type parameter are replaced with the specified default type.

```csharp
using Durian;
using Durian.Configuration;

public partial class Other
{
	[DefaultParamConfiguration(MethodConvention = DPMethodConvention.Copy)]
	public T Method<[DefaultParam(typeof(string))]T>(T value)
	{
		T other = value;
		return default(T);
	}
}

// Generated

public partial class Other
{
	public string Method(string value)
	{
		string other = value;
		return default(string);
	}
}

```

**Note**: *DPMethodConvention.Call* cannot be applied to *abstract* methods.

### Type convention

Both [Durian.Configuration.DefaultParamConfigurationAttribute](../Durian.DefaultParam/DefaultParamConfigurationAttributeProvider.cs) and [Durian.Configuration.DefaultParamScopedConfigurationAttribute](../Durian.DefaultParam/DefaultParamScopedConfigurationAttributeProvider.cs) define a *TypeConvention* property of type [Durian.Configuration.DPTypeConvention](../Durian.DefaultParam/DPTypeConventionProvider.cs). Thanks to this property, the user can specify how the target types should be generated.

[Durian.Configuration.DPTypeConvention](../Durian.DefaultParam/DPTypeConventionProvider.cs) is an enum with three constants:

 - *Default*
 - *Inherit*
 - *Copy*

*Default* and *Inherit* have the same value, and are used when no other type convention is specified.

When the *Inherit* convention is applied, the target type is inherited by the generated type with the specified default type as generic argument.

```csharp
using Durian;
using Durian.Configuration;

[DefaultParamConfiguration(TypeConvention = DPTypeConvention.Inherit)]
public class Test<[DefaultParam(typeof(string))]T>
{
	private readonly T _value;

	protected T Value => _value;

	public Other(T value)
	{
		_value = value;
	}
}

// Generated

public class Test : Test<int>
{
	public Other(string value) : base(value)
	{
	}
}

```

When the *Copy* convention is applied, the contents of the target type is copied and all the references to the DefaultParam type parameter are replaced with the specified default type.

```csharp
using Durian;
using Durian.Configuration;

[DefaultParamConfiguration(TypeConvention = DPTypeConvention.Copy)]
public class Test<[DefaultParam(typeof(string))]T>
{
	private readonly T _value;

	protected T Value => _value;

	public Other(T value)
	{
		_value = value;
	}
}

// Generated

public class Test
{
	private readonly string _value;

	protected string Value => _value;

	public Other(string value)
	{
		_value = value;
	}
}

```

**Note**: If the *DPTypeConvetion.Inherit* is applied and the target type has at least one declared constructor, all accessible constructors will be included in the generated type.

However, not all types support the *Inherit* convention. This includes:

 - static classes
 - sealed classes
 - structs
 - types with no accessible (non-private) constructor

### 'new' modifier

Both [Durian.Configuration.DefaultParamConfigurationAttribute](../Durian.DefaultParam/DefaultParamConfigurationAttributeProvider.cs) and [Durian.Configuration.DefaultParamScopedConfigurationAttribute](../Durian.DefaultParam/DefaultParamScopedConfigurationAttributeProvider.cs) define a *bool* *ApplyNewModifierWhenPossible* property. Thanks to this property, the user can specify whether to apply the *new* modifier whenever it is placeable. By default, this property is set to *true*.

If the *ApplyNewModifierWhenPossible* property is *true*, upon detecting a member with a colliding name, the *new* modifier will be applied to the generated member.

```csharp
using Durian;
using Durian.Configuration;

public class Parent
{
	public void Method()
	{
	}
}

public partial class Other : Parent
{
	[DefaultParamConfiguration(ApplyNewModifierWhenPossible = true)]
	public void Method<[DefaultParam(typeof(string))]T>()
	{
	}
}

// Generated

public partial class Other
{
	public new void Method()
	{
	}
}

```

**Note**: The *new* modifier cannot be applied if the colliding member is located inside the same scope.

**Note**: This behavior is never *true* for *virtual* or *abstract* methods (see: [Inheritance](#inheritance)).

If the *ApplyNewModifierWhenPossible* property is *false*, upon detecting a member with a colliding name, an error will occur.

```csharp
using Durian;
using Durian.Configuration;

public class Parent
{
	public void Method()
	{
	}
}

public partial class Other : Parent
{
	// DUR0114 - Method with generated signature already exist
	[DefaultParamConfiguration(ApplyNewModifierWhenPossible = false)]
	public void Method<[DefaultParam(typeof(string))]T>()
	{
	}
}

```

### Target namespace

Both [Durian.Configuration.DefaultParamConfigurationAttribute](../Durian.DefaultParam/DefaultParamConfigurationAttributeProvider.cs) and [Durian.Configuration.DefaultParamScopedConfigurationAttribute](../Durian.DefaultParam/DefaultParamScopedConfigurationAttributeProvider.cs) define a *string* *TargetNamespace* property. Thanks to this property, the user can specify a namespace where the generated member should be placed.

In some cases, it is desirable to place the generated members in a separate namespace to increase overall readability.

```csharp
using Durian;
using Durian.Configuration;

namespace Durian
{
	[DefaultParamConfiguration(TargetNamespace = "Durian.Extensions")]
	public class Other<[DefaultParam(typeof(string))]T>
	{
	}
}

// Generated

namespace Durian.Extensions
{
	public class Other : Other<string>
	{
	}
}

```

This property also accepts two special values: *null* and *global*.

Setting *TargetNamespace* to *null* is equivalent to not setting this property at all - the namespace of the original member is used.

 ```csharp
using Durian;
using Durian.Configuration;

namespace Durian
{
	[DefaultParamConfiguration(TargetNamespace = "Durian.Extensions")]
	public class Other<[DefaultParam(typeof(string))]T>
	{
	}
}

// Generated

namespace Durian
{
	public class Other : Other<string>
	{
	}
}

```

The *global* value specifies, that the generated member should be placed in the global namespace.

 ```csharp
using Durian;
using Durian.Configuration;

namespace Durian
{
	[DefaultParamConfiguration(TargetNamespace = "Durian.Extensions")]
	public class Other<[DefaultParam(typeof(string))]T>
	{
	}
}

// Generated

public class Other : Other<string>
{
}

```

*NOTE*: The *TargetNamespace* property accepts values that are valid namespace identifiers other than *Durian.Generator* (see: [DUR0005](../../docs/Core/DUR0005.md)).


## Inheritance

*DefaultParam* offers support for *virtual* and *abstract* methods, excluding those declared as part of an interface. However, overriding methods must meet some requirements in order to be valid:

- Overriding methods cannot add a [Durian.DefaultParamAttribute](../Durian.DefaultParam/DefaultParamAttributeProvider.cs) on a type parameter that previously didn't have one.
- Value of [Durian.DefaultParamAttribute](../Durian.DefaultParam/DefaultParamAttributeProvider.cs) must be exactly the same as for the base method.
- Methods that were generated by the *DefaultParam* generator cannot be overridden directly; original method should be overridden instead.

**Note**: It is possible for an overriding method to not have a [Durian.DefaultParamAttribute](../Durian.DefaultParam/DefaultParamAttributeProvider.cs) on a type parameter that in the base method had one. Such situations should be discouraged, however, as it leads to unnecessary confusion. For this reason an appropriate warning will be provided (see: [DUR0110](../../docs/DefaultParam/DUR0110.md)).

```csharp
using Durian;

public partial class Parent
{
	public virtual void Method<[DefaultParam(typeof(string))]T>()
	{
		T t = default(T);
	}
}

public partial class Other : Parent
{
	public override void Method<[DefaultParam(typeof(string))]T>()
	{
	}
}

// Generated

public partial class Parent
{
	public virtual void Method()
	{
		string t = default(string);
	}
}

public partial class Other
{
	public override void Method()
	{
	}
}

```

##

*\(Written by Piotr Stenke\)*