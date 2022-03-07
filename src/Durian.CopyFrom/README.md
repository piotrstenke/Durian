<div align="left">
    <a href="https://www.nuget.org/packages/Durian.CopyFrom">
        <img src="https://img.shields.io/nuget/v/Durian.CopyFrom?color=seagreen&style=flat-square" alt="Version"/>
    </a>
    <a href="https://www.nuget.org/packages/Durian.CopyFrom">
        <img src="https://img.shields.io/nuget/dt/Durian.CopyFrom?color=blue&style=flat-square" alt="Downloads"/>
    </a> <br />
</div>

<div align="center">
        <img src="../../img/icons/Durian-256.png" alt="Durian logo"/>
</div>

##

**CopyFrom allows to copy implementations of members to other members, without the need for inheritance. A regex pattern can be provided to customize the copied implementation.**

## Table of Contents

1. [Structure](#structure)
2. [Setup](#setup)
3. [Basics](#basics)

## Structure

Packages that are part of the *CopyFrom* module:

 - [*Durian.CopyFrom*](https://www.nuget.org/packages/Durian.CopyFrom/)

*CopyFrom* provides 3 types: 

 - [Durian.CopyFromTypeAttribute](../Durian.CopyFrom/CopyFromTypeAttributeProvider.cs)
 - [Durian.CopyFromMethodAttribute](../Durian.CopyFrom/CopyFromMethodAttributeProvider.cs)
 - [Durian.PatternAttribute](../Durian.CopyFrom/PatternAttributeProvider.cs)

## Setup

To start using *CopyFrom*, reference either the [*Durian.CopyFrom*](https://www.nuget.org/packages/Durian.CopyFrom/) or [*Durian*](https://www.nuget.org/packages/Durian/) package. 

**Note**: 
Like with other Durian modules, the target project must reference the [Durian.Core](../Durian.Core/README.md) package as well.

## Basics

In order to copy from a member, specify a [Durian.CopyFromTypeAttribute](../Durian.CopyFrom/CopyFromTypeAttributeProvider.cs) for a type...

```csharp
using Durian;

[CopyFromType(typeof(Target))]
public partial class Test
{
}

public class Target
{
    public void Init()
    {
        Console.WriteLine("Init");
    }
}

// Generated

public partial class Test
{
    public void Init()
    {
        Console.WriteLine("Init");
    }
}

```

...or a [Durian.CopyFromMethodAttribute](../Durian.CopyFrom/CopyFromMethodAttributeProvider.cs) for a method.

```csharp
using Durian;

public partial class Test
{
    [CopyFromMethod("Init")]
    public partial void Method();

    public void Init()
    {
        Console.WriteLine("Init");
    }
}

// Generated

public partial class Test
{
    public partial void Method()
    {
        Console.WriteLine("Init");
    }
}

```

## Special members

Some members are not copied *exactly* the same. This includes, for obvious reasons, constructors, destructors and operators.

##

*\(Written by Piotr Stenke\)*