<div align="left">
	<a href="https://www.nuget.org/packages/Durian">
		<img src="https://img.shields.io/nuget/v/Durian?color=seagreen&style=flat-square" alt="Version"/>
	</a>
	<a href="https://www.nuget.org/packages/Durian">
		<img src="https://img.shields.io/nuget/dt/Durian?color=blue&style=flat-square" alt="Downloads"/>
	</a> <br />
	<a href="https://github.com/piotrstenke/Durian/actions">
		<img src="https://img.shields.io/github/actions/workflow/status/piotrstenke/Durian/dotnet.yml" alt="Build"/>
	</a>
	<a href="https://github.com//piotrstenke/Durian/blob/master/LICENSE.md">
		<img src="https://img.shields.io/github/license/piotrstenke/Durian?color=orange&style=flat-square" alt="License"/>
	</a>
</div>

<div align="center">
		<img src="img/icons/Durian-256.png" alt="Durian logo"/>
</div>

##

**Durian is a collection of Roslyn-based analyzers, source generators and utility libraries that greatly extend the default capabilities of C# by bringing new features found in other existing programing languages, such as Kotlin, Swift, Java, C++, and many more.**

## Table of Contents

1. [Current State](#current-state)
2. [Features](#features)
	1. [DefaultParam](#defaultparam)
	2. [InterfaceTargets](#interfacetargets)
	3. [FriendClass](#friendclass)
	4. [CopyFrom](#copyfrom)
3. [In Progress](#in-progress)
4. [Experimental](#experimental) 

## Current State

Durian is still very much in development - many planned features are still not implemented or implemented only partially. Features that are production-ready are listed in the [Features](#Features) section below.

## Features

If you seek more information about a specific feature, click on its name below.

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

*InterfaceTargets*, similar to how [System.AttributeUsageAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.attributeusageattribute) works, allows to specify what kinds of types an interface can be implemented by.

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

## In Progress

The following modules are still in active development and are yet to be released in a not-yet-specified future.

## Experimental

Experimental stage is a playground of sorts - modules included here are very early in development and there in no guarantee that they will be ever actually released.

##

*\(Written by Piotr Stenke\)*
