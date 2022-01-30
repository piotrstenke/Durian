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

The *Durian.Core* package can be divided into two main parts - *Generator* and *Info*.

### Generator
 
The *Generator* part is represented by all the types in the *Durian.Generator* namespace. These types are used internally by the generators to communicate with each other, independently from the user's current compilation preferences. Though most of them are *public*, their direct use is forbidden in order to prevent potential communication errors caused by irresponsible usage.

### Info

The *Info* part is represented by all the types in the *Durian.Info* namespace. It provides classes that can inspect and gather information about the current compilation or assembly in the context of the Durian framework. In other words, it is possible to determine at runtime whether a specific assembly uses a given Durian module, what is the module's version, what types, diagnostics or packages does is include, and more.

##

*\(Written by Piotr Stenke\)*