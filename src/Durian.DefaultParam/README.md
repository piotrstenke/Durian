<div align="left">
    <a href="https://www.nuget.org/packages/Durian.DefaultParam">
        <img src="https://img.shields.io/nuget/v/Durian.DefaultParam?color=seagreen&style=flat-square" alt="Version"/>
    </a>
    <a href="https://www.nuget.org/packages/Durian.DefaultParam">
        <img src="https://img.shields.io/nuget/dt/Durian.DefaultParam?color=mediumgreen" alt="Downloads"/>
    </a> <br />
</div>

<div align="center">
        <img src="../../img/icons/Durian.DefaultParam-256.png" alt="Durian.DefaultParam logo"/>
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
   6. ['new' modifier](#new-modifier)
5. [Inheritance](#inheritance) 

## Structure

Packages that are part of the *DefaultParam* module:

 - Durian.DefaultParam

*DefaultParam* includes 5 types: 
 - [Durian.DefaultParamAttribute](../Durian.Core/_attr/DefaultParamAttribute.cs)
 - [Durian.Configuration.DefaultParamConfigurationAttribute](../Durian.Core/Configuration/_attr/DefaultParamConfigurationATtribute.cs)
 - [Durian.Configuration.DefaultParamScopedConfigurationAttribute](../Durian.Core/Configuration/_attr/DefaultParamScopedConfigurationAttribute.cs)
 - [Durian.Configuration.DPTypeConvention](../Durian.Core/Configuration/_enum/DPTypeConvention.cs)
 - [Durian.Configuration.DPMethodConvention](../Durian.Core/Configuration/_enum/DPMethodConvention.cs)

## Setup

To start using *DefaultParam*, reference the *Durian.DefaultParam* package. 

**Note**: 
Like with other Durian modules, the target project must reference the *Durian.Core* package as well.

## Basics

Main type in this module is the [Durian.DefaultParamAttribute](../Durian.Core/_attr/DefaultParamAttribute.cs). When attribute is placed on a type parameter, a new member is generated - a direct copy of the original, but with the target type parameter replaced with the specified default type.

```csharp
using Durian;

public class Test<T, [DefaultParam(typeof(string))]U>
{
    public void Method(U value)
    {
        U second = value;
    }
}

// Generated

public class Test<T>
{
    public void Method(string value)
    {
        string second = value;
    }
}

```

[Durian.DefaultParamAttribute](../Durian.Core/_attr/DefaultParamAttribute.cs) can only be placed on the last type parameter or next to another [Durian.DefaultParamAttribute](../Durian.Core/_attr/DefaultParamAttribute.cs).

If multiple type parameters are marked with the [Durian.DefaultParamAttribute](../Durian.Core/_attr/DefaultParamAttribute.cs), mutliple members are generated accordingly.

```csharp
using Durian;

public class Test<[DefaultParam(typeof(int))]T, [DefaultParam(typeof(string))]U>
{
    public T Method(U value)
    {
        U second = value;
           
        return default(T);
    }
}

// Generated

public class Test<T>
{
    public T Method(string value)
    {
        string second = value;

        return default(T);
    }
}

public class Test
{
    public int Method(string value)
    {
        string second = valuel

        return default(int);
    }
}

```

### Parameter constraints

Default type defined using the [Durian.DefaultParamAttribute](../Durian.Core/_attr/DefaultParamAttribute.cs) must fulfil all constraints of the target type parameter.

```csharp
using Durian;

// 'string' fulfils the constraints of 'T', so it is a valid value.
public class Test<[DefaultParam(typeof(string))]T> where T : class
{
}

// 'string' does not fulfil the constrains of 'T', so its not a valid value.
public class Other<[DefaultParam(typeof(string))]T> where T : struct
{
}

```

### Accessibility

*DefaultParam* respects all language restrictions of accessibility. As long as the target type parameter is never used in a context more acessible than the value of [Durian.DefaultParamAttribute](../Durian.Core/_attr/DefaultParamAttribute.cs), it is not an error.

```csharp
using Durian;

internal class Dummy
{
}

// Dummy is valid, because is is the same accessiblity as Test<T>
internal class Test<[DefaultParam(typeof(Dummy))]T>
{
}

// Dummy is not valid, because internal is less accessible than public.
public class Other<[DefaultParam(typeof(Dummy))]T>
{
}

```

### Invalid values

Not all types are valid for the [Durian.DefaultParamAttribute](../Durian.Core/_attr/DefaultParamAttribute.cs). This includes: 

 - void or [System.Void](https://docs.microsoft.com/en-us/dotnet/api/system.void?view=net-5.0);
 - pointers or function pointers;
 - ref structs, like [System.Span\<T\>](https://docs.microsoft.com/en-us/dotnet/api/system.span-1?view=net-5.0);
 - static classes;
 - dynamic (language restriction);
 - nullable reference type (language restriction);
 - unbound generic types.

Additionaly, the following types can't be used when there is a type parameter constrained to the target type parameter:

- [System.Object](https://docs.microsoft.com/en-us/dotnet/api/system.object?view=net-5.0);
-  [System.ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype?view=net-5.0);
 - [System.Array](https://docs.microsoft.com/en-us/dotnet/api/system.array?view=net-5.0);
 - any array type, either jagged or multidimensional.

### Member limitations

Most generic-compatible members are supported, but there are some notable rules and exceptions:

- target member cannot be placed within another member with the [Durian.DefaultParamAttribute](../Durian.Core/_attr/DefaultParamAttribute.cs);
- all containing types of the target member must be *partial*;
- if the member is a type, it cannot be *partial*;
- if the *DPTypeConvention.Inherit* (see: [Configuration](#configuration)) is applied, the member cannot be a *struct* or a *sealed* or *static* class and must define at least one accessible constructor;
- if the member is a method, it cannot be *partial* or *extern*;
- local methods, methods located in an interface and explicit implementations are not supported.

## Configuration

*DefaultParam* allows the user to configure, how new members are generated through the [Durian.Configuration.DefaultParamConfigurationAttribute](../Durian.Core/Configuration/_attr/DefaultParamConfigurationATtribute.cs) and [Durian.Configuration.DefaultParamScopedConfigurationAttribute](../Durian.Core/Configuration/_attr/DefaultParamScopedConfigurationATtribute.cs) types.

### Local configuration

Local configuration is determined through the [Durian.Configuration.DefaultParamConfigurationAttribute](../Durian.Core/Configuration/_attr/DefaultParamConfigurationATtribute.cs). Is is applied only to the member it is defined on.

```csharp
using Durian;
using Durian.Configuration;

// The configuration is applied only to  'Test<T, U>'. 'Delegate<T, U>' and Dummy.Method<T> are not included.
[DefaultParamConfiguration(TypeConvention = DPTypeConvention.Inherit]
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

**Note**: This attribute shouldn't be placed on members without the [Durian.DefaultParamAttribute](../Durian.Core/_attr/DefaultParamAttribute.cs), as in such case it is meaningless.

### Scoped configuration

Scoped configuration is determined through the [Durian.Configuration.DefaultParamScopedConfigurationAttribute](../Durian.Core/Configuration/_attr/DefaultParamScopedConfigurationAttribute.cs). Is is applied to all members in the current scope.

```csharp
using Durian;
using Durian.Configuration;

// The configuration is applied to all DefaultParam members in the current assembly.
[assembly: DefaultParamScopedConfiguration(TypeConvention = DPTypeConvention.Inherit]

// The configuration is applied to all DefaultParam members inside this type.
[DefaultParamScopedConfiguration(TypeConvention = DPTypeConvention.Default]
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

**Note**: This attribute shouldn't be placed on a type with no members with the [Durian.DefaultParamAttribute](../Durian.Core/_attr/DefaultParamAttribute.cs), as in such case it is meaningless.

### Configuration priority

The [Durian.Configuration.DefaultParamScopedConfigurationAttribute](../Durian.Core/Configuration/_attr/DefaultParamScopedConfigurationAttribute.cs) acts only as the default configuration for the scope - it cannot override values defined by a [Durian.Configuration.DefaultParamConfigurationAttribute](../Durian.Core/Configuration/_attr/DefaultParamConfigurationAttribute.cs). 

When applying the configuration, the generator will pick the inner most one it can find.

```csharp
using Durian;
using Durian.Configuration;

// Global configuration has the lowest priority and will be picked only if there is no other configuration available.
[assembly: DefaultParamScopedConfiguration(MethodConvention = DPMethodConvention.Call]

// Configuration of a containing type will be picked only if the target member has no configuration applied on itself.
[DefaultParamScopedConfiguration(MethodConvention = DPMethodConvention.Default]
public partial class Test
{
    public void Method<[DefaultParam(typeof(string))]T>(T value)
    {
    }
}

[DefaultParamScopedConfiguration(MethodConvention = DPMethodConvention.Default]
public partial class Other
{
    // This configuration is applied directly on the member, so it has the biggest prriority.
    [DefaultParamConfiguration(MethodConvention = DPMethodConvention.Call]
    public void Method<[DefaultParam(typeof(string))]T>(T value)
    {
    }
}

```

If the configuration with the highest priority does not specify a value for a property, value of configuration with lower priority is used instead.

```csharp
using Durian;
using Durian.Configuration;

[assembly: DefaultParamScopedConfiguration(MethodConvention = DPMethodConvention.Call]

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

[DefaultParamScopedConfiguration(MethodConvention = DPMethodConvention.Default]
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

Both [Durian.Configuration.DefaultParamConfigurationAttribute](../Durian.Core/Configuration/_attr/DefaultParamConfigurationAttribute.cs) and [Durian.Configuration.DefaultParamScopedConfigurationAttribute](../Durian.Core/Configuration/_attr/DefaultParamScopedConfigurationAttribute.cs) define a *MethodConvention* property of type [Durian.Configuration.DPMethodConvention](../Durian.Core/Configuration/_enum/DPMethodConvention.cs). Thanks to this property, the user can specify how the target methods should be generated.

[Durian.Configuration.DPMethodConvention](../Durian.Core/Configuration/_enum/DPMethodConvention.cs) is an enum with three constants:

 - *Default*
 - *Copy*
 - *Call*

*Default* and *Copy* have the same value, and are used when no other method convention is specified.

When the *Copy* convention is applied, the contents of the target method is copied and all the references to the DefaultParam type parameter are replaced with the specified default type.

```csharp
using Durian;
using Durian.Configuration;

public partial class Other
{
    [DefaultParamConfiguration(MethodConvention = DPMethodConvention.Copy]
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

When the *Call* convention is applied, instead of copying its contents, the target method is called by the generated method with the specified default type as generic argument.

```csharp
using Durian;
using Durian.Configuration;

public partial class Other
{
    [DefaultParamConfiguration(MethodConvention = DPMethodConvention.Call]
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

**Note**: *DPMethodConvention.Call* cannot be applied to *abstract* methods.

### Type convention

Both [Durian.Configuration.DefaultParamConfigurationAttribute](../Durian.Core/Configuration/_attr/DefaultParamConfigurationAttribute.cs) and [Durian.Configuration.DefaultParamScopedConfigurationAttribute](../Durian.Core/Configuration/_attr/DefaultParamScopedConfigurationAttribute.cs) define a *TypeConvention* property of type [Durian.Configuration.DPTypeConvention](../Durian.Core/Configuration/_enum/DPTypeConvention.cs). Thanks to this property, the user can specify how the target types should be generated.

[Durian.Configuration.DPTypeConvention](../Durian.Core/Configuration/_enum/DPTypeConvention.cs) is an enum with three constants:

 - *Default*
 - *Copy*
 - *Inherit*

*Default* and *Copy* have the same value, and are used when no other type convention is specified.

When the *Copy* convention is applied, the contents of the target type is copied and all the references to the DefaultParam type parameter are replaced with the specified default type.

```csharp
using Durian;
using Durian.Configuration;

[DefaultParamConfiguration(TypeConvention = DPTypeConvention.Copy]
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

When the *Inherit* convention is applied, instead of copying its contents, the target type is inherited by the generated type with the specified default type as generic argument.

```csharp
using Durian;
using Durian.Configuration;

[DefaultParamConfiguration(TypeConvention = DPTypeConvention.Inherit]
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

**Note**: If the *DPTypeConvetion.Inherit* is applied and the target type has at least one declared constructor, all accessible constructors will be included in the generated type.

However, not all types support the *Inherit* convention. This includes:

 - static classes
 - sealed classes
 - structs
 - types with no accessible (non-private) constructor

### 'new' modifier

Both [Durian.Configuration.DefaultParamConfigurationAttribute](../Durian.Core/Configuration/_attr/DefaultParamConfigurationAttribute.cs) and [Durian.Configuration.DefaultParamScopedConfigurationAttribute](../Durian.Core/Configuration/_attr/DefaultParamScopedConfigurationAttribute.cs) define a *bool* *ApplyNewModifierWhenPossible* property. Thanks to this property, the user can specify whether to apply the *new* modifier whenever it is pleasable. By default, this property is set to *true*.

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

**Note**: This behaviour is never *true* for *virtual* or *abstract* methods (see: [Inheritance](#inheritance)).

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

## Inheritance

*DefaultParam* offers support for *virtual* and *abstract* methods, exluding those declared as part of an interface. However, overriding methods must meet some requirements in order to be valid:

- Overriding methods cannot add a [Durian.DefaultParamAttribute](../Durian.Core/_attr/DefaultParamAttribute.cs) on a type parameter that previously didn't have one.
- Value of [Durian.DefaultParamAttribute](../Durian.Core/_attr/DefaultParamAttribute.cs) must be exactly the same as for the base method.
- Methods that were generated by the *DefaultParam* generator cannot be overriden directly; original method should be overriden instead.

**Note**: It is possible for an overriding method to not have a [Durian.DefaultParamAttribute](../Durian.Core/_attr/DefaultParamAttribute.cs) on a type parameter that in the base method had one. Such situations should be discouraged, however, as it leads to unnecessary confusion. For this reason an appropriate warning will be provided (see: [DUR0110](../../docs/DefaultParam/DUR0110.md)).

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