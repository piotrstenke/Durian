This directory contains projects that are not part of any Durian module or package. They exists to be used internally, as a way to speed-up the process of writing repetitive code through automatic generation of the required files. 

 As for now, two projects are located here:
 - **GenerateAnalyzerReleases** - generates the *AnalyzerReleases.Shipped.md* file for the 1.0.0 release of an analyzer.
 - **GenerateModuleRepository** - generates the [Durian.Info.PackageRepository](../src/Durian.Core/.generated/PackageRepository.cs) and [Durian.Info.ModuleRepository](../src/Durian.Core/.generated/ModuleRepository.cs) classes based on attributes from the *Durian.Generator* defined in every project's *_Configuration.cs* file.
##

*\(Written by Piotr Stenke\)*