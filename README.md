<div align="left">
    <a href="https://www.nuget.org/packages/Durian">
        <img src="https://img.shields.io/nuget/v/Durian?color=seagreen&style=flat-square" alt="Version"/>
    </a>
    <a href="https://www.nuget.org/packages/Durian">
        <img src="https://img.shields.io/nuget/dt/Durian?color=blue&style=flat-square" alt="Downloads"/>
    </a> <br />
    <a href="https://github.com/piotrstenke/Durian/actions">
        <img src="https://img.shields.io/github/workflow/status/piotrstenke/Durian/.NET?style=flat-square" alt="Build"/>
    </a>
    <a href="https://github.com//piotrstenke/Durian/blob/master/LICENSE.md">
        <img src="https://img.shields.io/github/license/piotrstenke/Durian?color=orange&style=flat-square" alt="License"/>
    </a>
</div>

<div align="center">
        <img src="img/icons/Durian-256.png" alt="Durian logo"/>
</div>

##

**Durian is a collection of Roslyn-based analyzers, source generators and utility libraries that bring many extensions to C#, with heavy emphasis on features that can be found in other existing languages. It's main goal is to make C# easier and more pleasant to use through reducing necessary boilerplate code, while at the same time providing additional layers of flexibility.**

## Current state

Durian is at an early stage of its evolution - many core features are still missing, being either in early development or planning phase. As for now, only one fully-fledged module is complete - *DefaultParam*, with *InterfaceTargets* and *FriendClass* awaiting in experimental stage for release in the closest future.

## Features

To see more about a specific feature, click on its name.

### [DefaultParam](src/Durian.DefaultParam/README.md)
*DefaultParam* allows to specify a default type for a generic parameter.

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

public class Program
{
    static void Main()
    {
        // Test<T> can be used without type parameters - 'T' defaults to 'string'.
        Test test1 = new Test("");
        
        // Type parameter can be stated explicitly.
        Test<string> test2 = new Test<string>("");
    }
}

```

### [InterfaceTargets](src/Durian.InterfaceTargets/README.md)

*InterfaceTargets*, similar to how [System.AttributeUsageAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.attributeusageattribute?view=net-5.0) works, allows to specify what kinds of members an interface can be implemented by.

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

### [FriendClass](src/Durian.FriendClass/README.md)

*FriendClass* allows to limit access to 'internal' members by specifying a fixed list of friend types.

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

## Experimental

Experimental modules include packages that are almost ready to be released, but still need some more polishing.

Currently, there are no modules in the experimental stage.

##

*\(Written by Piotr Stenke\)*
