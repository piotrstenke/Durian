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
    1. [Allowed targets](#allowed-targets)
    2. [Unnamed members](#unnamed-members)
    3. [Generic](#generic)
    4. [Multiple CopyFroms](#multiple-copyfroms)
4. [Configuration](#configuration)
    1. [Special members](#special-members)
    2. [Partial parts](#cpartial-parts)
    3. [Adding usings](#adding-usings)
    4. [Additional nodes](#additional-nodes)
5. [Patterns](#patterns)

## Structure

Packages that are part of the *CopyFrom* module:

 - [*Durian.CopyFrom*](https://www.nuget.org/packages/Durian.CopyFrom/)

*CopyFrom* provides 4 types: 

 - [Durian.CopyFromTypeAttribute](../Durian.CopyFrom/CopyFromTypeAttributeProvider.cs)
 - [Durian.CopyFromMethodAttribute](../Durian.CopyFrom/CopyFromMethodAttributeProvider.cs)
 - [Durian.PatternAttribute](../Durian.CopyFrom/PatternAttributeProvider.cs)
- [Durian.Configuration.CopyFromAdditionalNodes](../Durian.CopyFrom/CopyFromAdditionalNodesProvider.cs)

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

**Note**: If the method the [Durian.CopyFromMethodAttribute](../Durian.CopyFrom/CopyFromMethodAttributeProvider.cs) is applied to returns ```void```, the targetted member also has to return ```void```, and vice versa, if the method has a return type, the targetted method also requires a return type.

**Note**: In case of [Durian.CopyFromTypeAttribute](../Durian.CopyFrom/CopyFromTypeAttributeProvider.cs), the target type can be specified with either the ```typeof()``` or a an actual name in form of a string (e.g. through ```nameof()```).

```csharp
using Durian;

/// Using 'typeof' is possib.e
[CopyFromType(typeof(Target))]
public partial class Test1
{
}

/// Using 'nameof' (or any other string) is also possible.
public partial class Test2
{
}

```

 The ```typeof``` approach is should be preferred, thoguh, since it skips the necessary overhead of resolving a string into an actual type.

### Allowed targets

For various reasons, not all member kinds are valid targets for CopyFrom.

For [Durian.CopyFromTypeAttribute](../Durian.CopyFrom/CopyFromTypeAttributeProvider.cs), the valid targets are:

 - Classes.
 - Structs.
 - Interfaces.
 - Records.

For [Durian.CopyFromMethodAttribute](../Durian.CopyFrom/CopyFromMethodAttributeProvider.cs), the valid targets are:

- Non-local methods.
- Non-auto properties.
- Event and property accessors.
- Indexers.
- Constructors.
- Operators.

### Unnamed members

It is possible to target members that have no actual in-code name. This includes indexers, accessors and operators. Rules here are quite similar to C#'s ```<see cref=""/>``` tags.

Targetting indexers is possible by writing ```this``` followed by types of indexer's parameters in square brackets.

```csharp
using Durian;

public partial class Test
{
	[CopyFromMethod("this[int]")]
	public partial int Method();

	public int this[int index] => 1;
}

// Generated

public partial class Test
{
	public partial int Method() => 1;
}

```

Targetting accessors is possible by writing the name of the member followed by '_' and keyword of the accessor.

```csharp
using Durian;

public partial class Test
{
	[CopyFromMethod("Index_get]")]
	public partial int Method();

	public int Index
	{
		get => 1;
		set
		{
		}
	}
}

// Generated

public partial class Test
{
	public partial int Method() => 1;
}

```

The same applies to events...

```csharp
using Durian;

public partial class Test
{
	[CopyFromMethod("Event_add]")]
	public partial void Method();

	public event System.Action Event
	{
		add
		{
			System.Console.WriteLine("Added");
		}
		remove
		{
			System.Console.WriteLine("Removed");
		}
	}
}

// Generated

public partial class Test
{
	public partial void Method()
	{
		System.Console.WriteLine("Added");
	}
}

```

...and indexers.

```csharp
using Durian;

public partial class Test
{
	[CopyFromMethod("this[int]_get")]
	public partial int Method();

	public int this[int index]
	{
		get => 1;
		set
		{
		}
	}
}

// Generated

public partial class Test
{
	public partial int Method() => 1;
}

```

Targetting conversion operators is possible by writing ```implicit/explicit operator``` followed by the return type and source type in parenthesis.

```csharp
using Durian;

public partial class Test
{
	[CopyFromMethod("implicit operator int(Test)")]
	public partial int Method();

	public static implicit operator int(Test test) => 1;
}

// Generated

public partial class Test
{
	public partial int Method() => 1;
}

```

Targetting non-conversion operators is possible by writting the ```operator``` keyword followed by the operator token and list of parameter types.

```csharp
using Durian;

public partial class Test
{
	[CopyFromMethod("operator +(Test, Test)")]
	public partial int Method();

	public static int operator +(Test left, Test right) => 1;
}

// Generated

public partial class Test
{
	public partial int Method() => 1;
}

```

### Generics

It is possible to copy from generic members, event if the marked member itself is not generic.

```csharp
using Durian;

[CopyFromType("Target<T>"))]
public partial class Test
{
}

public class Target<T>
{
	public T Init()
	{
		Console.WriteLine("Init");
		return default(T);
	}
}

// Generated

public partial class Test
{
	public T Init()
	{
		Console.WriteLine("Init");
		return default(T);
	}
}

```

It is possible to copy from a member with substited arguments.

```csharp
using Durian;

[CopyFromType("Target<int>"))]
public partial class Test
{
}

public class Target<T>
{
	public T Init()
	{
		Console.WriteLine("Init");
		return default(T);
	}
}

// Generated

public partial class Test
{
	public int Init()
	{
		Console.WriteLine("Init");
		return default(int);
	}
}

```

### Multiple CopyFroms

A type can be marked with multiple [Durian.CopyFromTypeAttribute](../Durian.CopyFrom/CopyFromTypeAttributeProvider.cs)s.

```csharp
using Durian;

[CopyFromType(typeof(Target))]
[CopyFromType(typeof(Other))]
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

public class Other
{
	public void Method()
	{
		Console.WriteLine("Me");
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

public partial class Test
{
	public void Method()
	{
		Console.WriteLine("Me");
	}
}

```

By default, the attributes are resolved in order as they written in code (top-to-bottom). The ordering can be set statically using the 'Order' property.

```csharp
using Durian;

[CopyFromType(typeof(Target), Order = 2)]
[CopyFromType(typeof(Other), Order = 1)]
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

public class Other
{
	public void Method()
	{
		Console.WriteLine("Me");
	}
}

// Generated

public partial class Test
{
	public void Method()
	{
		Console.WriteLine("Me");
	}
}

public partial class Test
{
	public void Init()
	{
		Console.WriteLine("Init");
	}
}

```

Attribute with lower value of 'Order' will be resolved first. If the 'Order' values are equal, the default ordering rule applies.

At first look, the value of 'Order' does not seem to change anything, but in reality that's not the case. This information will later prove itself crutial when dealing with [Durian.PatternAttribute](../Durian.CopyFrom/PatternAttributeProvider.cs)s.

## Configuration

Certain aspects of how CopyFrom works can be altered by using properties present on the attributes.

### Special members

Some members are automacially modified when copying, to ensure validity of the generated code.

```csharp
using Durian;

[CopyFromType(typeof(Target))]
public partial class Test
{
}

public class Target
{
	public Target()
	{
	}
}

// Generated

public partial class Test
{
	// The type name 'Test' is written instead of the original 'Target'.
	public Test()
	{
	}
}

```

This applies to constructors, destructors and operators.

Such behaviour can be turned off by setting the 'HandleSpecialMembers' property to false.

```csharp
using Durian;

[CopyFromType(typeof(Target), HandleSpecialMembers = false)]
public partial class Test
{
}

public class Target
{
	public Target()
	{
	}
}

// Generated

public partial class Test
{
	// The original 'Target' type name is preserved, leading to invalid code (at least at this step).
	public Target()
	{
	}
}

```

Scenario like this might be desirable when working with the [Durian.PatternAttribute](../Durian.CopyFrom/PatternAttributeProvider.cs) (on that topic later).

### Partial parts

Sometimes it can be preferrable to copy only a part of type's functionality. This can be achieved in multiple ways (e.g. through proper inheritance), but by far the easiest solution is to mark the type with the [Durian.PartialNameAttribute](../Durian.Core/_attr/PartialNameAttribute.cs) and refer to it through the 'PartialPart' property of [Durian.CopyFromTypeAttribute](../Durian.CopyFrom/CopyFromTypeAttributeProvider.cs).

```csharp
using Durian;

[CopyFromType(typeof(Target), PartialPart = "TargetPartial")]
public partial class Test
{
}

public partial class Test
{
	public void Init()
	{
		Console.WriteLine("Init");
	}
}

[PartialName("TargetPartial")]
public partial class Test
{
	public void Other()
	{
		Console.WriteLine("Other");
	}
}

// Generated

public partial class Test
{
	// Only members of the 'TargetPartial' part are copied.
	public void Other()
	{
		Console.WriteLine("Other");
	}
}

```

### Adding usings

It is possible to add a ```using``` directive that is not present in the original code. To do this, use the 'AddUsings' property, present on both [Durian.CopyFromTypeAttribute](../Durian.CopyFrom/CopyFromTypeAttributeProvider.cs) and [Durian.CopyFromMethodAttribute](../Durian.CopyFrom/CopyFromMethodAttributeProvider.cs).

```csharp
using Durian;

[CopyFromType(typeof(Target), AddUsings = new string[] { "System" })]
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

using System;

public partial class Test
{
	public void Init()
	{
		Console.WriteLine("Init");
	}
}

```

This way, it is also possible to add static usings or aliases.

```csharp
using Durian;

[CopyFromType(typeof(Target), AddUsings = new string[] { "System", "static System.Collections.Generic.Enumerable", "Sy = System" })]
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

using System;
using static System.Collections.Generic.Enumerable;
using Sy = System;

public partial class Test
{
	public void Init()
	{
		Console.WriteLine("Init");
	}
}

```

This is especially useful when using a [Durian.PatternAttribute](../Durian.CopyFrom/PatternAttributeProvider.cs).

### Additional nodes

It is possible to also copy nodes that are not normally part of the target member. This includes:

- Using directives.
- Attributes.
- Base type.
- Base interface list.
- Type parameter constraints.
- Documentation comments.

It can be achieved by using the 'AdditionalNodes' property, present on both [Durian.CopyFromTypeAttribute](../Durian.CopyFrom/CopyFromTypeAttributeProvider.cs) and [Durian.CopyFromMethodAttribute](../Durian.CopyFrom/CopyFromMethodAttributeProvider.cs).

```csharp
using Durian;
using Durian.Configuration;

[CopyFromType(typeof(Target), AdditionalNodes = CopyFromAdditionalNodes.BaseType | CopyFromAdditionalNodes.Documentation)]
public partial class Test
{
}

/// <summary>This is a target.</summary>
public class Target : System.Attribute
{
	public void Init()
	{
		Console.WriteLine("Init");
	}
}

// Generated

/// <summary>This is a target.</summary>
public partial class Test : System.Attribute
{
	public void Init()
	{
		Console.WriteLine("Init");
	}
}

```

By default, only CopyFromAdditionalNodes.Usings is specified.

## Patterns

The *CopyFrom* feature would not be as useful as it is without a way do replace parts of the copied code. This is exactly where the [Durian.PatternAttribute](../Durian.CopyFrom/PatternAttributeProvider.cs) comes to help.

```csharp
using Durian;

[CopyFromType(typeof(Target))]
[Pattern("Init", "Method")]
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
	public void Method()
	{
		Console.WriteLine("Method");
	}
}

```

[Durian.PatternAttribute](../Durian.CopyFrom/PatternAttributeProvider.cs) uses regular expressions to get the job done, so opportunities here are quite powerful.

```csharp
using Durian;

[CopyFromType(typeof(Target))]
[Pattern("void (\w+)", "int $1_copied")]
public partial class Test
{
}

public class Target
{
	public void Init1()
	{
		Console.WriteLine("Init1");
	}

	public void Init2()
	{
		Console.WriteLine("Init2");
	}
}

// Generated

public partial class Test
{
	public int Init1_copied()
	{
		Console.WriteLine("Init1");
	}

	public int Init2_copied()
	{
		Console.WriteLine("Init2");
	}
}

```

Patterns can also be chained, with the 'Order' property specifying the actual order.

```csharp
using Durian;

[CopyFromType(typeof(Target))]
[Pattern("Init", "Method", Order = 1)]
[Pattern("Method", "Me", Order = 2)]
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
	public void Me()
	{
		Console.WriteLine("Method");
	}
}

```
##

*\(Written by Piotr Stenke\)*