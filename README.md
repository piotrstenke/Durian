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

Durian is at an early stage of its evolution - many core features are still missing, being either in early development or planning phase. As for now, three fully-fledged modules are completed - *DefaultParam*, *InterfaceTargets* and *FriendClass*.

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

*InterfaceTargets*, similar to how [System.AttributeUsageAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.attributeusageattribute) works, allows to specify what kinds of members an interface can be implemented by.

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

## In Progress

The following modules are still in active development and are yet to be released in a not-yet-specified future.

### [CopyFrom](src/Durian.CopyFrom/README.md)

*CopyFrom* allows to copy implementations of members to other members, without the need for inheritance. A regex pattern can be provided to customize the copied implementation.

```csharp
using Durian;

[CopyFromType(typeof(Other)), Pattern("text", "name")]
public partial class Test
{
}

public class Other
{
    private string _text;

    void Set(string text)
    {
        _text = text;
    }
}

// Generated

partial class Test
{
    private string _name;

    void Set(string name)
    {
        _name = name;
    }
}

```

## Experimental

Experimental stage is a playground of sorts - modules included here are very early in development and there in no guarantee that they will be ever actually released.

### [ConstExpr](src/Durian.ConstExpr/README.md)

*ConstExpr* allows a method to be executed at compile-time, producing actual constants.

```csharp
using Durian;

public static class Utility
{
    [ConstExpr]
    public static int Sum(params int[] values)
    {
        int sum = 0;

        for(int i = 0; i < values.Length; i++)
        {
            sum += values[i];
        }

        return sum;
    }
}

[ConstExprSource("Utility.Sum", "Sum_10", 1, 2, 3, 4, 5, 6, 7, 8, 9, 10)]
public static partial class Constants
{
}

// Generated

public static partial class Constants
{
    public const int Sum_10 = 55;
}

```

### [ConstParam](src/Durian.ConstExpr/README.md)

*ConstParam* allows to specify that a value of a parameter cannot be modified, event if it's a reference type.


```csharp
using Durian;

public class Test
{
    public class Data
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }

    public void Method([ConstParam]Data data)
    {
        // Error! Parameter cannot be modified.
        data.Name = "";
    }
}

```

##

*\(Written by Piotr Stenke\)*
