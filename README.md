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

## Features

### DefaultParam

DefaultParam allows to specify a default type for a generic parameter.

```csharp
using Durian;

public partial class Test<[DefaultParam(typeof(string))]T>
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

## Current state

Right now, Durian is only at the first stage of its evolution. As for its initial release on the 2nd of June 2021, two modules - *Core* and *DefaultParam* - are available, with additional two - *StructInherit* and *GenericSpecialization* - in early development. This does not include two already existing packages that are not part of any module - *Durian.AnalysisServices* and *Durian.TestServices*.

For more information about a specific package or module, go to the *README.md* file in the according project's directory in the *\\src\\* folder.

## What's next?

At the moment, two modules are in experimental stage - *StructInherit* and *GenericSpecialization*, with the latter being further in development. Release dates are yet to be determined.

## History

Durian started as a personal project of a high school student from Gdañsk, Poland - Piotr Stenke, amateur C# programmer and Unity Engine enthusiast. Though the sole idea for a Roslyn-based platform emerged in late 2020, any actual work didn't take place until February 2021. First months of development were especially challenging, with final exams in school, deadly virus roaming all around the globe and adult life slowly, but steadily, approaching. And all of that without even mentioning the worst part - learning from scratch this awful, unintuitive, badly-documented mess of an API that is Roslyn.

At its initial release on the 2nd of June 2021, Durian was meant to be a major card that would get its author into the IT industry. 

##

*\(Written by Piotr Stenke\)*