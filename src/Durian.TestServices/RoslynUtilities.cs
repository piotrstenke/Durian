using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading;
using Durian.Analysis;
using Durian.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Durian.TestServices
{
	/// <summary>
	/// Contains various utility methods that help with unit testing Roslyn-based projects.
	/// </summary>
	public static class RoslynUtilities
	{
		/// <summary>
		/// Name applied to a created <see cref="Compilation"/> when no other name is specified.
		/// </summary>
		public static string DefaultCompilationName => "TestCompilation";

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains <see cref="MetadataReference"/>s of all the essential .NET assemblies.
		/// </summary>
		/// <param name="includeDurianCore">Determines whether to include the <c>Durian.Core.dll</c> assembly.</param>
		public static CSharpCompilation CreateBaseCompilation(bool includeDurianCore = true)
		{
			MetadataReference[] references = GetBaseReferences(includeDurianCore);

			return CSharpCompilation.Create(
				assemblyName: DefaultCompilationName,
				references: references,
				options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
			);
		}

		/// <inheritdoc cref="CreateCompilation(string?, IEnumerable{Type}?)"/>
		public static CSharpCompilation CreateCompilation(string? source, params Type[]? types)
		{
			return CreateCompilation(source, types as IEnumerable<Type>);
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains a <see cref="SyntaxTree"/> created from the specified <paramref name="source"/>
		/// and <see cref="MetadataReference"/>s to the assemblies that contain the specified <paramref name="types"/>.
		/// </summary>
		/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="SyntaxTree"/> and added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="types">A collection of <see cref="Type"/>s to get the assemblies to reference to.</param>
		public static CSharpCompilation CreateCompilation(string? source, IEnumerable<Type>? types)
		{
			IEnumerable<MetadataReference>? references = types is null ? GetReferences(types) : null;
			return CreateCompilationWithReferences(source, references);
		}

		/// <inheritdoc cref="CreateCompilation(IEnumerable{string}?, IEnumerable{Type}?)"/>
		public static CSharpCompilation CreateCompilation(IEnumerable<string>? sources, params Type[]? types)
		{
			return CreateCompilation(sources, types as IEnumerable<Type>);
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains a <see cref="SyntaxTree"/> created from the specified <paramref name="sources"/>
		/// and <see cref="MetadataReference"/>s to the assemblies that contain the specified <paramref name="types"/>.
		/// </summary>
		/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="SyntaxTree"/>s and added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="types">A collection of <see cref="Type"/>s to get the assemblies to reference to.</param>
		public static CSharpCompilation CreateCompilation(IEnumerable<string>? sources, IEnumerable<Type>? types)
		{
			IEnumerable<MetadataReference> references = GetReferences(types);
			return CreateCompilationWithReferences(sources, references);
		}

		/// <inheritdoc cref="CreateCompilation(SyntaxTree?, IEnumerable{Type}?)"/>
		public static CSharpCompilation CreateCompilation(SyntaxTree? syntaxTree, params Type[]? types)
		{
			return CreateCompilation(syntaxTree, types as IEnumerable<Type>);
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains the specified <see cref="SyntaxTree"/>
		/// and <see cref="MetadataReference"/>s to the assemblies that contain the specified <paramref name="types"/>.
		/// </summary>
		/// <param name="syntaxTree">A <see cref="SyntaxTree"/> that will be added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="types">A collection of <see cref="Type"/>s to get the assemblies to reference to.</param>
		public static CSharpCompilation CreateCompilation(SyntaxTree? syntaxTree, IEnumerable<Type>? types)
		{
			IEnumerable<MetadataReference> references = GetReferences(types);
			return CreateCompilationWithReferences(syntaxTree, references);
		}

		/// <inheritdoc cref="CreateCompilation(IEnumerable{SyntaxTree}?, IEnumerable{Type}?)"/>
		public static CSharpCompilation CreateCompilation(IEnumerable<SyntaxTree>? syntaxTrees, params Type[]? types)
		{
			return CreateCompilation(syntaxTrees, types as IEnumerable<Type>);
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains the specified <see cref="SyntaxTree"/>s
		/// and <see cref="MetadataReference"/>s to the assemblies that contain the specified <paramref name="types"/>.
		/// </summary>
		/// <param name="syntaxTrees">A collection of <see cref="SyntaxTree"/>s that will be added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="types">A collection of <see cref="Type"/>s to get the assemblies to reference to.</param>
		public static CSharpCompilation CreateCompilation(IEnumerable<SyntaxTree>? syntaxTrees, IEnumerable<Type>? types)
		{
			IEnumerable<MetadataReference> references = GetReferences(types);
			return CreateCompilationWithReferences(syntaxTrees, references);
		}

		/// <inheritdoc cref="CreateCompilation(ISourceTextProvider?, IEnumerable{Type}?)"/>
		public static CSharpCompilation CreateCompilation(ISourceTextProvider? sourceText, params Type[]? types)
		{
			return CreateCompilation(sourceText, types as IEnumerable<Type>);
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains the specified <paramref name="sourceText"/>
		/// and <see cref="MetadataReference"/>s to the assemblies that contain the specified <paramref name="types"/>.
		/// </summary>
		/// <param name="sourceText">A <see cref="ISourceTextProvider"/> that creates source text that will be added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="types">A collection of <see cref="Type"/>s to get the assemblies to reference to.</param>
		public static CSharpCompilation CreateCompilation(ISourceTextProvider? sourceText, IEnumerable<Type>? types)
		{
			IEnumerable<MetadataReference> references = GetReferences(types);
			return CreateCompilationWithReferences(sourceText, references);
		}

		/// <inheritdoc cref="CreateCompilation(IEnumerable{ISourceTextProvider}?, IEnumerable{Type}?)"/>
		public static CSharpCompilation CreateCompilation(IEnumerable<ISourceTextProvider>? sourceTexts, params Type[]? types)
		{
			return CreateCompilation(sourceTexts, types as IEnumerable<Type>);
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains the specified <see cref="ISourceTextProvider"/>s
		/// and <see cref="MetadataReference"/>s to the assemblies that contain the specified <paramref name="types"/>.
		/// </summary>
		/// <param name="sourceTexts">A collection of <see cref="ISourceTextProvider"/>s that create source texts that will be added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="types">A collection of <see cref="Type"/>s to get the assemblies to reference to.</param>
		public static CSharpCompilation CreateCompilation(IEnumerable<ISourceTextProvider>? sourceTexts, IEnumerable<Type>? types)
		{
			IEnumerable<MetadataReference> references = GetReferences(types);
			return CreateCompilationWithReferences(sourceTexts, references);
		}

		/// <inheritdoc cref="CreateCompilationWithAssemblies(string?, IEnumerable{Assembly}?)"/>
		public static CSharpCompilation CreateCompilationWithAssemblies(string? source, params Assembly[]? assemblies)
		{
			return CreateCompilationWithAssemblies(source, assemblies as IEnumerable<Assembly>);
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains a <see cref="SyntaxTree"/> created from the specified <paramref name="source"/>
		/// and <see cref="MetadataReference"/>s to the specified <paramref name="assemblies"/>.
		/// </summary>
		/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="SyntaxTree"/> and added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="assemblies">A collection of <see cref="Assembly"/> instances to be referenced by the output <see cref="CSharpCompilation"/>.</param>
		public static CSharpCompilation CreateCompilationWithAssemblies(string? source, IEnumerable<Assembly>? assemblies)
		{
			IEnumerable<MetadataReference> references = GetReferences(assemblies);
			return CreateCompilationWithReferences(source, references);
		}

		/// <inheritdoc cref="CreateCompilationWithAssemblies(IEnumerable{string}?, IEnumerable{Assembly}?)"/>
		public static CSharpCompilation CreateCompilationWithAssemblies(IEnumerable<string>? sources, params Assembly[]? assemblies)
		{
			return CreateCompilationWithAssemblies(sources, assemblies as IEnumerable<Assembly>);
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains <see cref="SyntaxTree"/>s created from the specified <paramref name="sources"/>
		/// and <see cref="MetadataReference"/>s to the specified <paramref name="assemblies"/>.
		/// </summary>
		/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="SyntaxTree"/>s and added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="assemblies">A collection of <see cref="Assembly"/> instances to be referenced by the output <see cref="CSharpCompilation"/>.</param>
		public static CSharpCompilation CreateCompilationWithAssemblies(IEnumerable<string>? sources, IEnumerable<Assembly>? assemblies)
		{
			IEnumerable<MetadataReference> references = GetReferences(assemblies);
			return CreateCompilationWithReferences(sources, references);
		}

		/// <inheritdoc cref="CreateCompilationWithAssemblies(SyntaxTree?, IEnumerable{Assembly}?)"/>
		public static CSharpCompilation CreateCompilationWithAssemblies(SyntaxTree? syntaxTree, params Assembly[]? assemblies)
		{
			return CreateCompilationWithAssemblies(syntaxTree, assemblies as IEnumerable<Assembly>);
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains the specified <see cref="SyntaxTree"/>
		/// and <see cref="MetadataReference"/>s to the specified <paramref name="assemblies"/>.
		/// </summary>
		/// <param name="syntaxTree">A <see cref="SyntaxTree"/> that will be added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="assemblies">A collection of <see cref="Assembly"/> instances to be referenced by the output <see cref="CSharpCompilation"/>.</param>
		public static CSharpCompilation CreateCompilationWithAssemblies(SyntaxTree? syntaxTree, IEnumerable<Assembly>? assemblies)
		{
			IEnumerable<MetadataReference> references = GetReferences(assemblies);
			return CreateCompilationWithReferences(syntaxTree, references);
		}

		/// <inheritdoc cref="CreateCompilationWithAssemblies(IEnumerable{SyntaxTree}?, IEnumerable{Assembly}?)"/>
		public static CSharpCompilation CreateCompilationWithAssemblies(IEnumerable<SyntaxTree>? syntaxTrees, params Assembly[]? assemblies)
		{
			return CreateCompilationWithAssemblies(syntaxTrees, assemblies as IEnumerable<Assembly>);
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains the specified <see cref="SyntaxTree"/>s
		/// and <see cref="MetadataReference"/>s to the specified <paramref name="assemblies"/>.
		/// </summary>
		/// <param name="syntaxTrees">A collection of <see cref="SyntaxTree"/>s that will be added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="assemblies">A collection of <see cref="Assembly"/> instances to be referenced by the output <see cref="CSharpCompilation"/>.</param>
		public static CSharpCompilation CreateCompilationWithAssemblies(IEnumerable<SyntaxTree>? syntaxTrees, IEnumerable<Assembly>? assemblies)
		{
			IEnumerable<MetadataReference> references = GetReferences(assemblies);
			return CreateCompilationWithReferences(syntaxTrees, references);
		}

		/// <inheritdoc cref="CreateCompilationWithAssemblies(ISourceTextProvider?, IEnumerable{Assembly}?)"/>
		public static CSharpCompilation CreateCompilationWithAssemblies(ISourceTextProvider? sourceText, params Assembly[]? assemblies)
		{
			return CreateCompilationWithAssemblies(sourceText, assemblies as IEnumerable<Assembly>);
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains the specified <paramref name="sourceText"/>
		/// and <see cref="MetadataReference"/>s to the specified <paramref name="assemblies"/>.
		/// </summary>
		/// <param name="sourceText">A <see cref="ISourceTextProvider"/> that creates source text that will be added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="assemblies">A collection of <see cref="Assembly"/> instances to be referenced by the output <see cref="CSharpCompilation"/>.</param>
		public static CSharpCompilation CreateCompilationWithAssemblies(ISourceTextProvider? sourceText, IEnumerable<Assembly>? assemblies)
		{
			IEnumerable<MetadataReference> references = GetReferences(assemblies);
			return CreateCompilationWithReferences(sourceText, references);
		}

		/// <inheritdoc cref="CreateCompilationWithAssemblies(IEnumerable{ISourceTextProvider}?, IEnumerable{Assembly}?)"/>
		public static CSharpCompilation CreateCompilationWithAssemblies(IEnumerable<ISourceTextProvider>? sourceTexts, params Assembly[]? assemblies)
		{
			return CreateCompilationWithAssemblies(sourceTexts, assemblies as IEnumerable<Assembly>);
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains the specified <see cref="ISourceTextProvider"/>s
		/// and <see cref="MetadataReference"/>s to the specified <paramref name="assemblies"/>.
		/// </summary>
		/// <param name="sourceTexts">A collection of <see cref="ISourceTextProvider"/>s that create source texts that will be added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="assemblies">A collection of <see cref="Assembly"/> instances to be referenced by the output <see cref="CSharpCompilation"/>.</param>
		public static CSharpCompilation CreateCompilationWithAssemblies(IEnumerable<ISourceTextProvider>? sourceTexts, IEnumerable<Assembly>? assemblies)
		{
			IEnumerable<MetadataReference> references = GetReferences(assemblies);
			return CreateCompilationWithReferences(sourceTexts, references);
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> with the specified <paramref name="input"/> text and a reference compilation containing the given <paramref name="external"/> text.
		/// </summary>
		/// <param name="input">Input text.</param>
		/// <param name="external">Text representing code in an external assembly.</param>
		/// <exception cref="InvalidOperationException">Emit failed.</exception>
		public static CSharpCompilation CreateCompilationWithDependency(string input, string external)
		{
			CSharpCompilation dependency = CreateCompilation(external);

			return CreateCompilationWithDependency(input, dependency);
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> with the specified <paramref name="input"/> text and reference to the given <paramref name="dependency"/>.
		/// </summary>
		/// <param name="input">Input text.</param>
		/// <param name="dependency"><see cref="CSharpCompilation"/> that should be a dependency of a compilation containing the <paramref name="input"/> text.</param>
		/// <exception cref="InvalidOperationException">Emit failed.</exception>
		public static CSharpCompilation CreateCompilationWithDependency(string input, CSharpCompilation dependency)
		{
			MetadataReference reference = CreateReference(dependency);

			return CreateCompilation(input)
				.AddReferences(reference);
		}

		/// <inheritdoc cref="CreateCompilationWithReferences(string?, IEnumerable{MetadataReference}?)"/>
		public static CSharpCompilation CreateCompilationWithReferences(string? source, params MetadataReference[]? references)
		{
			return CreateCompilationWithReferences(source, references as IEnumerable<MetadataReference>);
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains a <see cref="SyntaxTree"/> created from the specified <paramref name="source"/>
		/// and all given <see cref="MetadataReference"/>s.
		/// </summary>
		/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="SyntaxTree"/> and added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="references">A collection of <see cref="MetadataReference"/> to be added to the output <see cref="CSharpCompilation"/>.</param>
		public static CSharpCompilation CreateCompilationWithReferences(string? source, IEnumerable<MetadataReference>? references)
		{
			SyntaxTree? syntaxTree = source is null ? null : CSharpSyntaxTree.ParseText(source, encoding: Encoding.UTF8);

			return CreateCompilationWithReferences(syntaxTree, references);
		}

		/// <inheritdoc cref="CreateCompilationWithReferences(IEnumerable{string}?, IEnumerable{MetadataReference}?)"/>
		public static CSharpCompilation CreateCompilationWithReferences(IEnumerable<string>? sources, params MetadataReference[]? references)
		{
			return CreateCompilationWithReferences(sources, references as IEnumerable<MetadataReference>);
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains <see cref="SyntaxTree"/>s created from the specified <paramref name="sources"/>
		/// and all given <see cref="MetadataReference"/>s.
		/// </summary>
		/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="SyntaxTree"/>s and added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="references">A collection of <see cref="MetadataReference"/> to be added to the output <see cref="CSharpCompilation"/>.</param>
		public static CSharpCompilation CreateCompilationWithReferences(IEnumerable<string>? sources, IEnumerable<MetadataReference>? references)
		{
			IEnumerable<SyntaxTree> syntaxTrees = GetSyntaxTrees(sources);
			return CreateCompilationWithReferences(syntaxTrees, references);
		}

		/// <inheritdoc cref="CreateCompilationWithReferences(SyntaxTree?, IEnumerable{MetadataReference}?)"/>
		public static CSharpCompilation CreateCompilationWithReferences(SyntaxTree? syntaxTree, params MetadataReference[]? references)
		{
			return CreateCompilationWithReferences(syntaxTree, references as IEnumerable<MetadataReference>);
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains the specified <see cref="SyntaxTree"/> and all given <see cref="MetadataReference"/>s.
		/// </summary>
		/// <param name="syntaxTree">A <see cref="SyntaxTree"/> that will be added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="references">A collection of <see cref="MetadataReference"/> to be added to the output <see cref="CSharpCompilation"/>.</param>
		public static CSharpCompilation CreateCompilationWithReferences(SyntaxTree? syntaxTree, IEnumerable<MetadataReference>? references)
		{
			SyntaxTree[]? syntaxTrees = syntaxTree is null ? null : new SyntaxTree[] { syntaxTree };
			return CreateCompilationWithReferences(syntaxTrees, references);
		}

		/// <inheritdoc cref="CreateCompilationWithReferences(IEnumerable{SyntaxTree}?, IEnumerable{MetadataReference}?)"/>
		public static CSharpCompilation CreateCompilationWithReferences(IEnumerable<SyntaxTree>? syntaxTrees, params MetadataReference[]? references)
		{
			return CreateCompilationWithReferences(syntaxTrees, references as IEnumerable<MetadataReference>);
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains the specified <see cref="SyntaxTree"/> and all given <see cref="MetadataReference"/>s.
		/// </summary>
		/// <param name="syntaxTrees">A collection of <see cref="SyntaxTree"/>s that will be added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="references">A collection of <see cref="MetadataReference"/> to be added to the output <see cref="CSharpCompilation"/>.</param>
		public static CSharpCompilation CreateCompilationWithReferences(IEnumerable<SyntaxTree>? syntaxTrees, IEnumerable<MetadataReference>? references)
		{
			CSharpCompilation compilation = CreateBaseCompilation(true);

			if (references is not null)
			{
				compilation = compilation.AddReferences(references);
			}

			if (syntaxTrees is not null)
			{
				compilation = compilation.AddSyntaxTrees(syntaxTrees);
			}

			return compilation;
		}

		/// <inheritdoc cref="CreateCompilationWithReferences(ISourceTextProvider?, IEnumerable{MetadataReference}?)"/>
		public static CSharpCompilation CreateCompilationWithReferences(ISourceTextProvider? sourceText, params MetadataReference[]? references)
		{
			return CreateCompilationWithReferences(sourceText, references as IEnumerable<MetadataReference>);
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains the specified <paramref name="sourceText"/>
		/// and all given <see cref="MetadataReference"/>s.
		/// </summary>
		/// <param name="sourceText">A <see cref="ISourceTextProvider"/> that creates source text that will be added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="references">A collection of <see cref="MetadataReference"/> to be added to the output <see cref="CSharpCompilation"/>.</param>
		public static CSharpCompilation CreateCompilationWithReferences(ISourceTextProvider? sourceText, IEnumerable<MetadataReference>? references)
		{
			SyntaxTree? syntaxTree = sourceText is null ? null : CSharpSyntaxTree.ParseText(sourceText.GetText(), encoding: Encoding.UTF8);

			return CreateCompilationWithReferences(syntaxTree, references);
		}

		/// <inheritdoc cref="CreateCompilationWithReferences(IEnumerable{ISourceTextProvider}?, IEnumerable{MetadataReference}?)"/>
		public static CSharpCompilation CreateCompilationWithReferences(IEnumerable<ISourceTextProvider>? sourceTexts, params MetadataReference[]? references)
		{
			return CreateCompilationWithReferences(sourceTexts, references as IEnumerable<MetadataReference>);
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains the specified <see cref="ISourceTextProvider"/>s
		/// and all given <see cref="MetadataReference"/>s.
		/// </summary>
		/// <param name="sourceTexts">A collection of <see cref="ISourceTextProvider"/>s that create source texts that will be added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="references">A collection of <see cref="MetadataReference"/> to be added to the output <see cref="CSharpCompilation"/>.</param>
		public static CSharpCompilation CreateCompilationWithReferences(IEnumerable<ISourceTextProvider>? sourceTexts, IEnumerable<MetadataReference>? references)
		{
			IEnumerable<SyntaxTree>? syntaxTrees = sourceTexts?.Select(text => CSharpSyntaxTree.ParseText(text.GetText(), encoding: Encoding.UTF8));

			return CreateCompilationWithReferences(syntaxTrees, references);
		}

		/// <summary>
		/// Creates a new, non-defaulted <see cref="GeneratorExecutionContext"/> using a <see cref="CSharpGeneratorDriver"/> and a generator proxy.
		/// </summary>
		public static GeneratorExecutionContext CreateExecutionContext()
		{
			return CreateExecutionContext(CreateBaseCompilation());
		}

		/// <inheritdoc cref="CreateExecutionContext(Compilation, GeneratorInitialize)"/>
		public static GeneratorExecutionContext CreateExecutionContext(Compilation? compilation)
		{
			SourceGeneratorProxy proxy = new();
			CSharpGeneratorDriver driver = CSharpGeneratorDriver.Create(proxy);
			_ = driver.RunGenerators(compilation ?? CreateBaseCompilation());
			return proxy.ExecutionContext;
		}

		/// <summary>
		/// Creates a new, non-defaulted <see cref="GeneratorExecutionContext"/> using a <see cref="CSharpGeneratorDriver"/>.
		/// </summary>
		/// <param name="compilation">A <see cref="Compilation"/> to be assigned to the newly-created <see cref="GeneratorExecutionContext"/>.</param>
		/// <param name="onInitialize">Action to be performed instead of the proper <see cref="ISourceGenerator.Initialize(GeneratorInitializationContext)"/> method.</param>
		/// <remarks>
		/// This method accepts <see cref="Compilation"/> instead of <see cref="CSharpCompilation"/>
		/// to allow the user to intentionally pass invalid <see cref="Compilation"/> to the <see cref="ISourceGenerator.Execute(GeneratorExecutionContext)"/>
		/// to perform proper unit tests regarding validation.
		/// </remarks>
		public static GeneratorExecutionContext CreateExecutionContext(Compilation? compilation, GeneratorInitialize? onInitialize)
		{
			SourceGeneratorProxy proxy = new();

			if (onInitialize is not null)
			{
				proxy.OnInitialize += onInitialize;
			}

			CSharpGeneratorDriver driver = CSharpGeneratorDriver.Create(proxy);
			_ = driver.RunGenerators(compilation ?? CreateBaseCompilation());
			return proxy.ExecutionContext;
		}

		/// <summary>
		/// Creates a new, non-defaulted <see cref="GeneratorInitializationContext"/> using reflection.
		/// </summary>
		public static GeneratorInitializationContext CreateInitializationContext()
		{
			return (GeneratorInitializationContext)Activator.CreateInstance(
				type: typeof(GeneratorInitializationContext),
				bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic,
				binder: null,
				args: new object[] { default(CancellationToken) },
				culture: CultureInfo.InvariantCulture
			)!;
		}

		/// <summary>
		/// Creates a <see cref="MetadataReference"/> to assembly containing the specified <paramref name="input"/> text.
		/// </summary>
		/// <param name="input">Input text.</param>
		/// <exception cref="InvalidOperationException">Emit failed.</exception>
		public static MetadataReference CreateReference(string? input)
		{
			CSharpCompilation compilation = CreateCompilation(input);
			return CreateReference(compilation);
		}

		/// <summary>
		/// Creates a <see cref="MetadataReference"/> to the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to create the <see cref="MetadataReference"/> to.</param>
		/// <exception cref="InvalidOperationException">Emit failed.</exception>
		public static MetadataReference CreateReference(CSharpCompilation compilation)
		{
			using MemoryStream stream = new();

			EmitResult emit = compilation.Emit(stream);

			if (!emit.Success)
			{
				throw new InvalidOperationException("Emit failed!");
			}

			return MetadataReference.CreateFromImage(stream.ToArray());
		}

		/// <summary>
		/// Returns A collection of <see cref="MetadataReference"/>s of all essential .NET assemblies.
		/// </summary>
		/// <param name="includeDurianCore">Determines whether to include the <c>Durian.Core.dll</c> assembly.</param>
		public static MetadataReference[] GetBaseReferences(bool includeDurianCore = true)
		{
			string directory = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

			HashSet<string> locations = new()
			{
				typeof(object).Assembly.Location,
				typeof(File).Assembly.Location,
				typeof(BigInteger).Assembly.Location,
				typeof(Enumerable).Assembly.Location,
				typeof(List<>).Assembly.Location,
				Path.Combine(directory, "System.Runtime.dll"),
				Path.Combine(directory, "netstandard.dll")
			};

			if (includeDurianCore)
			{
				locations.Add(typeof(DurianGeneratedAttribute).Assembly.Location);
			}

			List<MetadataReference> references = new(locations.Count);

			foreach (string location in locations)
			{
				references.Add(MetadataReference.CreateFromFile(location));
			}

			return references.ToArray();
		}

		/// <summary>
		/// Returns a collection of <see cref="MetadataReference"/>s created from the provided <paramref name="types"/>.
		/// </summary>
		/// <param name="types">A collection of <see cref="Type"/>s to get the assemblies to reference to.</param>
		/// <param name="includeBase">Determines whether to automatically include the most essential .NET assemblies.</param>
		public static IEnumerable<MetadataReference> GetReferences(IEnumerable<Type>? types, bool includeBase = true)
		{
			if (includeBase)
			{
				foreach (MetadataReference m in GetBaseReferences())
				{
					yield return m;
				}
			}

			if (types is null)
			{
				yield break;
			}

			foreach (Type type in types)
			{
				if (type is null)
				{
					continue;
				}

				yield return MetadataReference.CreateFromFile(type.Assembly.Location);
			}
		}

		/// <summary>
		/// Returns a collection of <see cref="MetadataReference"/>s created from the provided <paramref name="assemblies"/>.
		/// </summary>
		/// <param name="assemblies">A collection of <see cref="Assembly"/> instances to create the <see cref="MetadataReference"/>s from.</param>
		/// <param name="includeBase">Determines whether to automatically include the most essential .NET assemblies.</param>
		public static IEnumerable<MetadataReference> GetReferences(IEnumerable<Assembly>? assemblies, bool includeBase = true)
		{
			if (includeBase)
			{
				foreach (MetadataReference m in GetBaseReferences())
				{
					yield return m;
				}
			}

			if (assemblies is null)
			{
				yield break;
			}

			foreach (Assembly assembly in assemblies)
			{
				if (assembly is null)
				{
					continue;
				}

				yield return MetadataReference.CreateFromFile(assembly.Location);
			}
		}

		/// <summary>
		/// Returns a collection of <see cref="SyntaxTree"/>s created from the specified <paramref name="sources"/>.
		/// </summary>
		/// <param name="sources">A collection of <see cref="string"/>s that represent the sources to parse and create the <see cref="SyntaxTree"/>s from.</param>
		public static IEnumerable<SyntaxTree> GetSyntaxTrees(IEnumerable<string>? sources)
		{
			if (sources is null)
			{
				yield break;
			}
			else
			{
				foreach (string s in sources)
				{
					if (s is null)
					{
						continue;
					}

					SyntaxTree tree = CSharpSyntaxTree.ParseText(s, encoding: Encoding.UTF8);

					if (tree is SyntaxTree t)
					{
						yield return t;
					}
				}
			}
		}

		/// <summary>
		/// Parses a <see cref="SyntaxTree"/> from the specified <paramref name="source"/> and returns the first <see cref="SyntaxNode"/>  of type <typeparamref name="TNode"/> in that tree.
		/// </summary>
		/// <param name="source">A <see cref="string"/> to be parsed and converted to a <see cref="SyntaxTree"/>.</param>
		/// <typeparam name="TNode">Type of <see cref="SyntaxNode"/> to find and return.</typeparam>
		/// <returns>
		/// The first <see cref="SyntaxNode"/> of type <typeparamref name="TNode"/> found in the parsed <see cref="SyntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="SyntaxNode"/> exists.
		/// </returns>
		public static TNode? ParseNode<TNode>(string? source) where TNode : SyntaxNode
		{
			return ParseNode<TNode>(source, 0);
		}

		/// <summary>
		/// Parses a <see cref="SyntaxTree"/> from the specified <paramref name="source"/> and returns the <see cref="SyntaxNode"/> of type <typeparamref name="TNode"/> at the specified index in that tree.
		/// </summary>
		/// <param name="source">A <see cref="string"/> to be parsed and converted to a <see cref="SyntaxTree"/>.</param>
		/// <param name="index">Index at which the <see cref="SyntaxNode"/> should be returned. Can be thought of as a number of <see cref="SyntaxNode"/> of type <typeparamref name="TNode"/> to skip before returning.</param>
		/// <typeparam name="TNode">Type of <see cref="SyntaxNode"/> to find and return.</typeparam>
		/// <returns>
		/// The <see cref="SyntaxNode"/> of type <typeparamref name="TNode"/> found at the specified index in the parsed <see cref="SyntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="SyntaxNode"/> exists.
		/// </returns>
		public static TNode? ParseNode<TNode>(string? source, int index) where TNode : SyntaxNode
		{
			if (source is null)
			{
				return null;
			}

			return ParseNode<TNode>(CSharpSyntaxTree.ParseText(source, encoding: Encoding.UTF8), index);
		}

		/// <summary>
		/// Returns the first <see cref="SyntaxNode"/> of type <typeparamref name="TNode"/> in the specified <paramref name="tree"/>.
		/// </summary>
		/// <param name="tree"><see cref="SyntaxTree"/> to find the <see cref="SyntaxNode"/> of type <typeparamref name="TNode"/> in.</param>
		/// <typeparam name="TNode">Type of <see cref="SyntaxNode"/> to find and return.</typeparam>
		/// <returns>
		/// The <see cref="SyntaxNode"/> of type <typeparamref name="TNode"/> found at the specified index in the parsed <see cref="SyntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="SyntaxNode"/> exists.
		/// </returns>
		public static TNode? ParseNode<TNode>(SyntaxTree? tree) where TNode : SyntaxNode
		{
			return ParseNode<TNode>(tree, 0);
		}

		/// <summary>
		/// Returns the <see cref="SyntaxNode"/> of type <typeparamref name="TNode"/> at the specified index in the specified <paramref name="syntaxTree"/>.
		/// </summary>
		/// <param name="syntaxTree"><see cref="SyntaxTree"/> to find the <see cref="SyntaxNode"/> of type <typeparamref name="TNode"/> in.</param>
		/// <param name="index">Index at which the <see cref="SyntaxNode"/> should be returned. Can be thought of as a number of <see cref="SyntaxNode"/> of type <typeparamref name="TNode"/> to skip before returning.</param>
		/// <typeparam name="TNode">Type of <see cref="SyntaxNode"/> to find and return.</typeparam>
		/// <returns>
		/// The <see cref="SyntaxNode"/> of type <typeparamref name="TNode"/> found at the specified index in the specified <paramref name="syntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="SyntaxNode"/> exists.
		/// </returns>
		public static TNode? ParseNode<TNode>(SyntaxTree? syntaxTree, int index) where TNode : SyntaxNode
		{
			if (syntaxTree is null)
			{
				return null;
			}

			int count = 0;

			foreach (SyntaxNode node in syntaxTree.GetCompilationUnitRoot().DescendantNodes())
			{
				if (node is TNode n)
				{
					if (count == index)
					{
						return n;
					}

					count++;
				}
			}

			return null;
		}
	}
}
