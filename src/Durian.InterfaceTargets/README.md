<div align="left">
    <a href="https://www.nuget.org/packages/Durian.InterfaceTargets">
        <img src="https://img.shields.io/nuget/v/Durian.InterfaceTargets?color=seagreen&style=flat-square" alt="Version"/>
    </a>
    <a href="https://www.nuget.org/packages/Durian.InterfaceTargets">
        <img src="https://img.shields.io/nuget/dt/Durian.InterfaceTargets?color=blue&style=flat-square" alt="Downloads"/>
    </a> <br />
</div>

<div align="center">
        <img src="../../img/icons/Durian-256.png" alt="Durian logo"/>
</div>

##

***Durian.InterfaceTargets*, similar to how the *System.AttributeUsageAttribute* works, allows to specify member kinds an interface can be implemented by.**

1. [Structure](#structure)
2. [Setup](#setup)
3. [Basics](#basics)
4. [Reflection](#configuration)
5. [Possible values](#possible-values) 

## Structure

Packages that are part of the *InterfaceTargets* module:

 - [*Durian.InterfaceTargets*](https://www.nuget.org/packages/Durian.InterfaceTargets/)

*InterfaceTargets* provides 2 types:

 - [Durian.InterfaceTargets](../Durian.InterfaceTargets/InterfaceTargetsProvider.cs)
 - [Durian.InterfaceTargetsAttribute](../Durian.InterfaceTargets/InterfaceTargetsAttributeProvider.cs)

## Setup

To start using *InterfaceTargets*, reference either the [*Durian.InterfaceTargets*](https://www.nuget.org/packages/Durian.InterfaceTargets/) or [*Durian*](https://www.nuget.org/packages/Durian/) package.

**Note**: 
Like with other Durian modules, the target project must reference the [Durian.Core](../Durian.Core/README.md) package as well.

## Basics

The premise of this package is fairly straight-forward and recognizable. Similar to how [System.AttributeUsageAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.attributeusageattribute) works, the [Durian.InterfaceTargetsAttribute](../Durian.InterfaceTargets/InterfaceTargetsAttributeProvider.cs) allows to specify what kind of members the target interface can be implemented by.

```csharp
using Durian;

[InterfaceTargets(InterfaceTargets.Class)]
public interface ITest
{
}

// Success!
// ITest can be implemented, because ClassTest is a class.
public class ClassTest : ITest
{
}

// Error!
// ITest cannot be implemented, because StructTest is a struct, and ITest is valid only for classes.
public struct StructTest : ITest
{
}

```

It is possible to specify more than one member kind.

```csharp
using Durian;

[InterfaceTargets(InterfaceTargets.Class | InterfaceTargets.Struct)]
public interface ITest
{
}

// Success!
// ITest can be implemented, because ClassTest is a class.
public class ClassTest : ITest
{
}

// Success!
// ITest can be implemented, because StructTest is a struct.
public struct StructTest : ITest
{
}

// Error!
// ITest cannot be implemented, because IInterfaceTest is an interface, and ITest is valid only for classes or structs.
public interface IInterfaceTest : ITest
{
}

```

## Reflection

*InterfaceTargets* works only on syntax level - it is possible to implement an interface for a member of forbidden kind through reflection. 

For cases when such behavior is the only intended way, *InterfaceTargets.ReflectionOnly* can be applied.

```csharp
using Durian;

[InterfaceTargets(InterfaceTargets.ReflectionOnly)]
public interface ITest
{
}

// Error!
// ITest can be implemented in code.
public class ClassTest : ITest
{
}

// Error!
// ITest can be implemented in code.
public struct StructTest : ITest
{
}

```

## Possible values

The possible values of the [Durian.InterfaceTargets](../Durian.InterfaceTargets/InterfaceTargetsProvider.cs) enum are:

 - *None* - Interface cannot be implemented in code.
 - *ReflectionOnly* - Interface can only be implemented through reflection; the same as *None*.
 - *Class* - Interface can only be implemented by classes.
 - *Interface* - Interface can only be implemented by other interfaces.
 - *Struct* - Interface can only be implemented by structs.
 - *RecordClass* - Interface can only be implemented by record classes.
 - *RecordStruct* - Interface can only be implemented by record structs.
 - *All* - Interface can be applied to all kinds of members; same as not specifying the [Durian.InterfaceTargetsAttribute](../Durian.InterfaceTargets/InterfaceTargetsAttributeProvider.cs) at all.


##

*\(Written by Piotr Stenke\)*
