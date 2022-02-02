<div align="left">
    <a href="https://www.nuget.org/packages/Durian.FriendClass">
        <img src="https://img.shields.io/nuget/v/Durian.FriendClass?color=seagreen&style=flat-square" alt="Version"/>
    </a>
    <a href="https://www.nuget.org/packages/Durian.FriendClass">
        <img src="https://img.shields.io/nuget/dt/Durian.FriendClass?color=blue&style=flat-square" alt="Downloads"/>
    </a> <br />
</div>

<div align="center">
        <img src="../../img/icons/Durian-256.png" alt="Durian logo"/>
</div>

##

***FriendClass* allows to limit access to 'internal' members by specifying a fixed list of friend types.**

1. [Structure](#structure)
2. [Setup](#setup)
3. [Basics](#basics)
4. [Member rules](#member-rules)
    1. [Accessibility](#accessibility)
    2. [Inner types](#inner-types)
5. [Inheritance](#inheritance)
6. [External assemblies](#external-assemblies)

## Structure

Packages that are part of the *FriendClass* module:

 - [*Durian.FriendClass*](https://www.nuget.org/packages/Durian.FriendClass/)

*FriendClass* provides 2 types:

 - [Durian.FriendClassAttribute](../Durian.FriendClass/FriendClassAttributeProvider.cs)
 - [Durian.Configuration.FriendClassConfigurationAttribute](../Durian.FriendClass/FriendClassConfigurationAttributeProvider.cs)

## Setup

To start using *FriendClass*, reference either the [*Durian.FriendClass*](https://www.nuget.org/packages/Durian.InterfaceTargets/) or [*Durian*](https://www.nuget.org/packages/Durian/) package.

**Note**: 
Like with other Durian modules, the target project must reference the [Durian.Core](../Durian.Core/README.md) package as well.

## Basics

To limit access of 'internal' members, specify a [Durian.FriendClassAttribute](../Durian.FriendClass/FriendClassAttributeProvider.cs) with the target friend type as argument.

```csharp
using Durian;

[FriendClass(typeof(A))]
public class Test
{
    internal static string Key { get; }
}

public class A
{
    public string GetKey()
    {
        // Success!
        // Type 'A' is a friend of 'Test', so it can safely access internal members.
        return Test.Key;
    }
}

public class B
{
    public string GetKey()
    {
        // Error!
        // Type 'B' is not a friend of 'Test', so it cannot access internal members.
        return Test.Key;
    }
}

```

A single type can have multiple friend types.

```csharp
using Durian;

[FriendClass(typeof(A))]
[FriendClass(typeof(B))]
public class Test
{
    internal static string Key { get; }
}

public class A
{
    public string GetKey()
    {
        // Success!
        // Type 'A' is a friend of 'Test', so it can safely access internal members.
        return Test.Key;
    }
}

public class B
{
    public string GetKey()
    {
        // Success!
        // Type 'B' is a friend of 'Test', so it can safely access internal members.
        return Test.Key;
    }
}

```

## Member rules

To ensure proper usage, *FriendClass* enforces a set of limitations on a potential target and its friend types.

### Accessibility

*FriendClass* protects all 'internal' members of the target type, including instance and static members, constructors, inner types etc.

```csharp
using Durian;

[FriendClass(typeof(A))]
public class Test
{
    internal readonly string _key;

    internal Test(string key)
    {
        _key = key;
    }

    internal static Test Default { get; } = new Test("default");

    internal class Inner
    {
    }
}

public class A
{
    public string GetKey()
    {
        // Success!
        // Type 'A' is a friend of 'Test', so it can safely access all available kinds of internal members.

        Test custom = new Test("custom");
        Test def = Test.Default;
        Test.Inner inner = new Test.Inner();

        return def._key;
    }
}

public class B
{
    public string GetKey()
    {
        // Error!
        // Type 'B' is not a friend of 'Test', so it has no access to any of the internal members.

        Test custom = new Test("custom");
        Test def = Test.Default;
        Test.Inner inner = new Test.Inner();

        return def._key;
    }
}

```

Members with the 'protected internal' are also protected, but only against external access - inherited classes still can access them.

```csharp
using Durian;

[FriendClass(typeof(A))]
public class Test
{
    protected internal static string Key { get; }
}

public class A
{
    public string GetKey()
    {
        // Success!
        // Class 'A' is a friend of 'Test'.
        return Test.Key;
    }
}

public class B
{
    public string GetKey()
    {
        // Error!
        // Class 'B' is NOT a friend of 'Test'.
        return Test.Key;
    }
}

public class C : Test
{
    public string GetKey()
    {
        // Success!
        // Class 'A' is a child of 'Test', so Key is not protected.
        return Test.Key;
    }
}

```

### Inner types

Inner types of the target type are implicit friends, meaning they don't have to be specified using the [Durian.FriendClassAttribute](../Durian.FriendClass/FriendClassAttributeProvider.cs). This behavior cannot be changed.

```csharp
using Durian;

// FriendClassAttribute is redundant - Inner1 is an implicit friend type.
[FriendClass(typeof(Inner1))]
public class Test
{
    internal static string Key { get; }

    class Inner1
    {
        public string GetKey()
        {
            // Success!
            // 'Key' can be accessed, because 'Inner1' is an implicit friend type.
            return Key;
        }
    }

    class Inner2
    {
        public string GetKey()
        {
            // Success!
            // 'Key' can be accessed, because 'Inner2' is an implicit friend type.
            return Key;
        }
    }
}

```

The same rule applies to inner types of friends.

```csharp
using Durian;

[FriendClass(typeof(Other))]
public class Test
{
    internal static string Key { get; }
}

public class Other
{
    public string GetKey()
    {
        // Success!
        // Type 'Other' is a friend of 'Test', so it can safely access internal members.
        return Test.Key;
    }

    public class Inner
    {
        public string GetKey()
        {
            // Success!
            // Type 'Inner' is an inner type of friend of 'Test', so it can safely access internal members.
            return Test.Key;
        }
    }
}

```

Inner types can also be friend types.

```csharp
using Durian;

[FriendClass(typeof(Other.Inner))]
public class Test
{
    internal static string Key { get; }
}

public class Other
{
    public string GetKey()
    {
        // Error!
        // Type 'Other' is not a friend of 'Test', so it cannot access internal members.
        return Test.Key;
    }

    public class Inner
    {
        public string GetKey()
        {
            // Success!
            // Type 'Inner' is an inner type of friend of 'Test', so it can safely access internal members.
            return Test.Key;
        }
    }
}

```

## Inheritance

By default, child types of target are not considered friends.

```csharp
using Durian;

[FriendClass(typeof(Other))]
public class Test
{
    internal static string Key { get; }
}

public class Other
{
}

public class Child : Test
{
    public string GetKey()
    {
        // Error!
        // Type 'Child' is not a friend of 'Test', so it cannot access internal members.
        return Key;
    }
}

```

The same rules applies to children of friends.

```csharp
using Durian;

[FriendClass(typeof(Other))]
public class Test
{
    internal static string Key { get; }
}

public class Other
{
}

public class Child : Other
{
    public string GetKey()
    {
        // Error!
        // Type 'Child' is not a friend of 'Test', so it cannot access internal members.
        return Test.Key;
    }
}

```

However, both features can be easily configured - children of target using the *AllowChildren* property of the [Durian.Configuration.FriendClassConfigurationAttribute](../Durian.FriendClass/FriendClassConfigurationAttributeProvider.cs)...

```csharp

using Durian;
using Durian.Configuration;

[FriendClass(typeof(Other))]
[FriendClassConfiguration(AllowChildren = true)]
public class Test
{
    internal static string Key { get; }
}

public class Other
{
}

public class Child : Test
{
    public string GetKey()
    {
        // Success!
        // Type 'Child' is an inner type of friend of 'Test', so it can safely access internal members.
        return Key;
    }
}

```

...and children of friends using the *AllowFriendChildren* property of the [Durian.FriendClassAttribute](../Durian.FriendClass/FriendClassAttributeProvider.cs).

```csharp

using Durian;

[FriendClass(typeof(Other), AllowFriendChildren = true)]
public class Test
{
    internal static string Key { get; }
}

public class Other
{
}

public class Child : Other
{
    public string GetKey()
    {
        // Success!
        // Type 'Child' is an inner type of friend of 'Test', so it can safely access internal members.
        return Test.Key;
    }
}

```

**Note**: Setting the *AllowChildren* property of the [Durian.Configuration.FriendClassConfigurationAttribute](../Durian.FriendClass/FriendClassConfigurationAttributeProvider.cs) to *true* on structs or sealed/static classes is not allowed.


##

*\(Written by Piotr Stenke\)*
