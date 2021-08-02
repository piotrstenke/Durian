<div align="left">
    <a href="https://www.nuget.org/packages/Durian.Core">
        <img src="https://img.shields.io/nuget/v/Durian.Core?color=seagreen&style=flat-square" alt="Version"/>
    </a>
    <a href="https://www.nuget.org/packages/Durian.Core">
        <img src="https://img.shields.io/nuget/dt/Durian.Core?color=blue&style=flat-square" alt="Downloads"/>
    </a> <br />
</div>

<div align="center">
        <img src="../../img/icons/Durian-256.png" alt="Durian logo"/>
</div>

##

***Durian.Core* is the main package of the Durian framework. It contains all the essential types needed by every given Durian-based generator.**

**In order to use a Durian-based generator, the user's project must reference this package.**

## How does this work?

The *Durian.Core* package is divided into three main systems - *Generator*, *Info* and *Core*.

### Generator
 
The *Generator* system is represented by all the types in the *Durian.Generator* namespace. These types are used internally by the generators to communicate with each other, independently from the user's current compilation preferences. Though most of them are *public*, their direct use is forbidden in order to prevent potential communication errors caused by irresponsible usage.

### Info

However, the remaining two systems can be used freely, with little to no limitations. The *Durian.Info* system provides classes that can inspect and gather information about the current compilation or assembly in the context of the Durian framework. In other words, it is possible to determine at runtime whether a specific assembly uses a given Durian module, what is the module's version, what types, diagnostics or packages does is include, and more.

### Core

The *Core* system is by far the biggest one out of the three. It sort of acts as an interface between the user and the generator - all types that the generator is dependent on are located here. This is true even for generators that are not directly referenced by the current compilation. 

If the user tries to access a type that is not part of any enabled generator, no actual generation will take place. Though not an fatal error in itself, such situation is not desirable, as it leads to unnecessary confusion. In order to prevent this and other similar problems, a separate analyzer package is provided (see: [Durian.Core.Analyzer](../Durian.Core.Analyzer/README.md)) as part of this module.

##

*\(Written by Piotr Stenke\)*