// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.GenericSpecialization
{
	/// <summary>
	/// Main generator of the <c>GenericSpecialization</c> module. Generates source code of members marked with the <see cref="AllowSpecializationAttribute"/>.
	/// </summary>
#if !MAIN_PACKAGE

	[Generator(LanguageNames.CSharp)]
#endif

	[GeneratorLoggingConfiguration(SupportedLogs = GeneratorLogs.All, LogDirectory = "GenericSpecialization", SupportsDiagnostics = true, RelativeToGlobal = true, EnableExceptions = true)]
	public class GenericSpecializationGenerator //: CachedDurianGenerator<GenSpecCompilationData, GenSpecSyntaxReceiver, GenSpecFilter, GenSpecClassData>
	{
		/// <summary>
		/// Number of trees generated statically by this generator.
		/// </summary>
#if MAIN_PACKAGE
		public const int NumStaticTrees = 0;
#else
		public const int NumStaticTrees = 1;
#endif

		/// <summary>
		/// Name of this source generator, i.e. 'GenericSpecialization'.
		/// </summary>
		public static string GeneratorName => "GenericSpecialization";

		/// <summary>
		/// Version of this source generator.
		/// </summary>
		public static string Version => "1.0.0";

		/// <summary>
		/// Initializes a new instance of the <see cref="GenericSpecializationGenerator"/> class.
		/// </summary>
		public GenericSpecializationGenerator()
		{
		}
	}

	///// <summary>
	///// Filtrates and validates <see cref="ClassDeclarationSyntax"/>es collected by a <see cref="GenSpecSyntaxReceiver"/>.
	///// </summary>
	//public sealed class GenSpecFilter : ICachedGeneratorSyntaxFilterWithDiagnostics<GenSpecClassData>, INodeValidatorWithDiagnostics<GenSpecClassData>, INodeProvider<ClassDeclarationSyntax>
	//{
	//	/// <summary>
	//	/// <see cref="GenericSpecializationGenerator"/> that created this filter.
	//	/// </summary>
	//	public GenericSpecializationGenerator Generator { get; }

	//	/// <summary>
	//	/// <see cref="IHintNameProvider"/> that is used to create a hint name for the generated source.
	//	/// </summary>
	//	public IHintNameProvider HintNameProvider { get; }

	//	/// <inheritdoc/>
	//	public bool IncludeGeneratedSymbols => true;

	//	/// <summary>
	//	/// <see cref="FilterMode"/> of this <see cref="GenSpecFilter"/>.
	//	/// </summary>
	//	public FilterMode Mode => Generator.LoggingConfiguration.CurrentFilterMode;

	//	/// <inheritdoc cref="GenSpecFilter(GenericSpecializationGenerator, IHintNameProvider)"/>
	//	public GenSpecFilter(GenericSpecializationGenerator generator) : this(generator, new SymbolNameToFile())
	//	{
	//	}

	//	/// <summary>
	//	/// Initializes a new instance of the <see cref="GenSpecFilter"/> class.
	//	/// </summary>
	//	/// <param name="generator"><see cref="GenericSpecializationGenerator"/> that created this filter.</param>
	//	/// <param name="hintNameProvider"><see cref="IHintNameProvider"/> that is used to create a hint name for the generated source.</param>
	//	public GenSpecFilter(GenericSpecializationGenerator generator, IHintNameProvider hintNameProvider)
	//	{
	//		Generator = generator;
	//		HintNameProvider = hintNameProvider;
	//	}

	//	IEnumerable<ClassDeclarationSyntax> INodeProvider<ClassDeclarationSyntax>.GetNodes()
	//	{
	//		throw new System.NotImplementedException();
	//	}

	//	IEnumerable<CSharpSyntaxNode> INodeProvider.GetNodes()
	//	{
	//		throw new System.NotImplementedException();
	//	}
	//}
}
