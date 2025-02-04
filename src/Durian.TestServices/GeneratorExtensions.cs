using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Durian.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.TestServices;

/// <summary>
/// Contains various extensions methods related to source generation.
/// </summary>
public static class GeneratorExtensions
{
	private static readonly Regex _diagnosticMessageRegex = new(@"\(\d+,\d+\):.+?: ", RegexOptions.Singleline);

	/// <summary>
	/// Checks if the specified <paramref name="result"/> contains the specified <paramref name="diagnosticMessages"/>.
	/// </summary>
	/// <param name="result">The <see cref="IGeneratorTestResult"/> to check if contains the specified <paramref name="diagnosticMessages"/>.</param>
	/// <param name="diagnosticMessages">Diagnostic messages to be checked for.</param>
	/// <typeparam name="T">Type of <see cref="IGeneratorTestResult"/> to work on.</typeparam>
	/// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
	public static bool ContainsDiagnosticMessages<T>(this T result, params string[]? diagnosticMessages) where T : IGeneratorTestResult
	{
		return ContainsDiagnosticMessages(result, true, diagnosticMessages);
	}

	/// <summary>
	/// Checks if the specified <paramref name="result"/> contains the specified <paramref name="diagnosticMessages"/>.
	/// </summary>
	/// <param name="result">The <see cref="IGeneratorTestResult"/> to check if contains the specified <paramref name="diagnosticMessages"/>.</param>
	/// <param name="ignoreLocationAndName">Determines whether to ignore the location of the diagnostic and the name of the symbol it applies to.</param>
	/// <param name="diagnosticMessages">Diagnostic messages to be checked for.</param>
	/// <typeparam name="T">Type of <see cref="IGeneratorTestResult"/> to work on.</typeparam>
	/// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
	public static bool ContainsDiagnosticMessages<T>(this T result, bool ignoreLocationAndName, params string[]? diagnosticMessages) where T : IGeneratorTestResult
	{
		if (result is null)
		{
			throw new ArgumentNullException(nameof(result));
		}

		if (diagnosticMessages is null)
		{
			return false;
		}

		if (result.Diagnostics.Length == 0)
		{
			return false;
		}
		else if (diagnosticMessages.Length == 0)
		{
			return true;
		}

		string[] diagnostics;

		if (ignoreLocationAndName)
		{
			diagnostics = result.Diagnostics.Select(d => _diagnosticMessageRegex.Replace(d.GetMessage(CultureInfo.InvariantCulture), "")).ToArray();
		}
		else
		{
			diagnostics = result.Diagnostics.Select(d => d.GetMessage(CultureInfo.InvariantCulture)).ToArray();
		}

		return diagnosticMessages.All(d => diagnostics.Contains(d));
	}

	/// <summary>
	/// Checks if the <paramref name="result"/> contains any of the specified <paramref name="diagnostics"/>.
	/// </summary>
	/// <param name="result">The <see cref="IGeneratorTestResult"/> to check if contains any of the specified <paramref name="diagnostics"/>.</param>
	/// <param name="diagnostics">Diagnostics to check for.</param>
	/// <typeparam name="T">Type of <see cref="IGeneratorTestResult"/> to work on.</typeparam>
	/// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
	public static bool ContainsDiagnostics<T>(this T result, params DiagnosticDescriptor[]? diagnostics) where T : IGeneratorTestResult
	{
		return result.ContainsDiagnostics(GetIds(diagnostics));
	}

	/// <summary>
	/// Checks if the <paramref name="result"/> contains any diagnostics with the specified <paramref name="ids"/>.
	/// </summary>
	/// <param name="result">The <see cref="IGeneratorTestResult"/> to check if contains diagnostics with the specified <paramref name="ids"/>.</param>
	/// <param name="ids">Diagnostic IDs to check for.</param>
	/// <typeparam name="T">Type of <see cref="IGeneratorTestResult"/> to work on.</typeparam>
	/// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
	public static bool ContainsDiagnostics<T>(this T result, params string[]? ids) where T : IGeneratorTestResult
	{
		if (result is null)
		{
			throw new ArgumentNullException(nameof(result));
		}

		if (ids is null || ids.Length == 0)
		{
			return false;
		}

		string[] diag = new string[result.Diagnostics.Length];

		for (int i = 0; i < diag.Length; i++)
		{
			diag[i] = result.Diagnostics[i].Id;
		}

		return result.Diagnostics.Length >= ids.Length && ids.All(d => Array.IndexOf(diag, d) != -1);
	}

	/// <summary>
	/// Creates a new <see cref="TestableGenerator{TContext}"/> for the specified underlaying <paramref name="generator"/>.
	/// </summary>
	/// <typeparam name="TContext">Type of <see cref="IGeneratorPassContext"/> used to generate sources.</typeparam>
	/// <param name="generator"><see cref="IDurianGenerator"/> that is used to actually generate sources.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
	public static TestableGenerator<TContext> CreateTestable<TContext>(this DurianGeneratorWithContext<TContext> generator) where TContext : GeneratorPassContext
	{
		return new TestableGenerator<TContext>(generator);
	}

	/// <summary>
	/// Creates a new <see cref="TestableGenerator{TContext}"/> for the specified underlaying <paramref name="generator"/>.
	/// </summary>
	/// <typeparam name="TContext">Type of <see cref="IGeneratorPassContext"/> used to generate sources.</typeparam>
	/// <param name="generator"><see cref="IDurianGenerator"/> that is used to actually generate sources.</param>
	/// <param name="testName">Name of the currently executing test.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
	public static TestableGenerator<TContext> CreateTestable<TContext>(this DurianGeneratorWithContext<TContext> generator, string? testName) where TContext : GeneratorPassContext
	{
		return new TestableGenerator<TContext>(generator, testName);
	}

	/// <summary>
	/// Creates a new <see cref="TestableIncrementalGenerator"/> for the specified underlaying <paramref name="generator"/>.
	/// </summary>>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
	public static TestableIncrementalGenerator CreateTestable(this DurianIncrementalGenerator generator)
	{
		return new TestableIncrementalGenerator(generator);
	}

	/// <summary>
	/// Creates a new <see cref="TestableIncrementalGenerator"/> for the specified underlaying <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="IDurianGenerator"/> that is used to actually generate sources.</param>
	/// <param name="testName">Name of the currently executing test.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
	public static TestableIncrementalGenerator CreateTestable(this DurianIncrementalGenerator generator, string testName)
	{
		return new TestableIncrementalGenerator(generator, testName);
	}

	/// <summary>
	/// Checks if the <paramref name="result"/> failed to generate any code and if it contains any of the specified <paramref name="diagnostics"/>.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="IGeneratorTestResult"/> to work on.</typeparam>
	/// <param name="result"><see cref="IGeneratorTestResult"/> to check.</param>
	/// <param name="diagnostics">Diagnostics to check for.</param>
	/// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
	public static bool FailedAndContainsDiagnostics<T>(this T result, params DiagnosticDescriptor[]? diagnostics) where T : IGeneratorTestResult
	{
		return result.FailedAndContainsDiagnostics(GetIds(diagnostics));
	}

	/// <summary>
	/// Checks if the <paramref name="result"/> failed to generate any code and if it contains <see cref="Diagnostic"/>s with the specified <paramref name="ids"/>.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="IGeneratorTestResult"/> to work on.</typeparam>
	/// <param name="result"><see cref="IGeneratorTestResult"/> to check.</param>
	/// <param name="ids">IDs of <see cref="Diagnostic"/>s to check for.</param>
	/// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
	public static bool FailedAndContainsDiagnostics<T>(this T result, params string[]? ids) where T : IGeneratorTestResult
	{
		return ContainsDiagnostics(result, ids) && !result.IsGenerated;
	}

	/// <summary>
	/// Checks if the <paramref name="result"/> failed to generate any code and if it doesn't contain any of the specified <paramref name="diagnostics"/>.
	/// </summary>i
	/// <typeparam name="T">Type of <see cref="IGeneratorTestResult"/> to work on.</typeparam>
	/// <param name="result"><see cref="IGeneratorTestResult"/> to check.</param>
	/// <param name="diagnostics">Diagnostics to check for.</param>
	/// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
	public static bool FailedAndDoesNotContainDiagnostics<T>(this T result, params DiagnosticDescriptor[]? diagnostics) where T : IGeneratorTestResult
	{
		return result.FailedAndDoesNotContainDiagnostics(GetIds(diagnostics));
	}

	/// <summary>
	/// Checks if the <paramref name="result"/> failed to generate any code and if it doesn't contain <see cref="Diagnostic"/>s with the specified <paramref name="ids"/>.
	/// </summary>i
	/// <typeparam name="T">Type of <see cref="IGeneratorTestResult"/> to work on.</typeparam>
	/// <param name="result"><see cref="IGeneratorTestResult"/> to check.</param>
	/// <param name="ids">IDs of <see cref="Diagnostic"/>s that shouldn't be present in the <paramref name="result"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
	public static bool FailedAndDoesNotContainDiagnostics<T>(this T result, params string[]? ids) where T : IGeneratorTestResult
	{
		return !ContainsDiagnostics(result, ids) && !result.IsGenerated;
	}

	/// <summary>
	/// Runs the specified <paramref name="analyzer"/> and checks if it produces <see cref="Diagnostic"/> described by the specified <paramref name="descriptor"/>.
	/// </summary>
	/// <param name="analyzer"><see cref="DiagnosticAnalyzer"/> to run and check.</param>
	/// <param name="compilation"><see cref="Compilation"/> that is the input for the <paramref name="analyzer"/>.</param>
	/// <param name="descriptor"><see cref="DiagnosticDescriptor"/> of the target <see cref="Diagnostic"/>.</param>
	public static Task<bool> ProducesDiagnostic(this DiagnosticAnalyzer analyzer, Compilation compilation, DiagnosticDescriptor descriptor)
	{
		return ProducesDiagnostic(analyzer, compilation, descriptor.Id);
	}

	/// <summary>
	/// Runs the specified <paramref name="analyzer"/> and checks if it produces <see cref="Diagnostic"/> with the specified <paramref name="id"/>.
	/// </summary>
	/// <param name="analyzer"><see cref="DiagnosticAnalyzer"/> to run and check.</param>
	/// <param name="compilation"><see cref="Compilation"/> that is the input for the <paramref name="analyzer"/>.</param>
	/// <param name="id">Id of <see cref="Diagnostic"/> to check for.</param>
	public static Task<bool> ProducesDiagnostic(this DiagnosticAnalyzer analyzer, Compilation compilation, string id)
	{
		return RunAnalyzer(analyzer, compilation).ContinueWith(task => task.Result.Any(d => d.Id == id));
	}

	/// <summary>
	/// Runs the specified <paramref name="analyzer"/> and returns an <see cref="ImmutableArray{T}"/> of produced <see cref="Diagnostic"/>s.
	/// </summary>
	/// <param name="analyzer"><see cref="DiagnosticAnalyzer"/> to run.</param>
	/// <param name="compilation"><see cref="Compilation"/> that is the input for the <paramref name="analyzer"/>.</param>
	public static Task<ImmutableArray<Diagnostic>> RunAnalyzer(this DiagnosticAnalyzer analyzer, Compilation compilation)
	{
		return compilation
			.WithAnalyzers(ImmutableArray.Create(analyzer))
			.GetAnalyzerDiagnosticsAsync();
	}

	/// <summary>
	/// Runs the specified <paramref name="analyzer"/> and returns an <see cref="ImmutableArray{T}"/> of produced <see cref="Diagnostic"/>s.
	/// </summary>
	/// <param name="analyzer"><see cref="DiagnosticAnalyzer"/> to run.</param>
	/// <param name="source">A <see cref="string"/> representing a <see cref="SyntaxTree"/> the analysis should be performed on.</param>
	public static Task<ImmutableArray<Diagnostic>> RunAnalyzer(this DiagnosticAnalyzer analyzer, string? source)
	{
		CSharpCompilation compilation = RoslynUtilities.CreateBaseCompilation();

		if (!string.IsNullOrWhiteSpace(source))
		{
			compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(source!));
		}

		return RunAnalyzer(analyzer, compilation);
	}

	/// <summary>
	/// Runs the specified <paramref name="analyzer"/> and returns an <see cref="ImmutableArray{T}"/> of produced <see cref="Diagnostic"/>s.
	/// </summary>
	/// <param name="analyzer"><see cref="DiagnosticAnalyzer"/> to run.</param>
	/// <param name="tree">A <see cref="SyntaxTree"/> the analysis should be performed on.</param>
	public static Task<ImmutableArray<Diagnostic>> RunAnalyzer(this DiagnosticAnalyzer analyzer, SyntaxTree? tree)
	{
		CSharpCompilation compilation = RoslynUtilities.CreateBaseCompilation();

		if (tree is not null)
		{
			compilation = compilation.AddSyntaxTrees(tree);
		}

		return RunAnalyzer(analyzer, compilation);
	}

	/// <summary>
	/// Performs a test on all source generators registered in the provided <paramref name="generatorDriver"/>.
	/// </summary>
	/// <param name="generatorDriver"><see cref="CSharpGeneratorDriver"/> used to run a tests on all registered source generators.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generatorDriver"/>.</param>
	/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="SyntaxTree"/> and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
	/// <param name="assemblies">An array of <see cref="Assembly"/> instances to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generatorDriver"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static IGeneratorTestResult RunTest(this CSharpGeneratorDriver generatorDriver, GeneratorTestResultProvider resultProvider, string? source, params Assembly[]? assemblies)
	{
		return RunTest(generatorDriver, resultProvider, RoslynUtilities.CreateCompilationWithAssemblies(source, assemblies));
	}

	/// <summary>
	/// Performs a test on all source generators registered in the provided <paramref name="generatorDriver"/>.
	/// </summary>
	/// <param name="generatorDriver"><see cref="CSharpGeneratorDriver"/> used to run a tests on all registered source generators.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generatorDriver"/>.</param>
	/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="SyntaxTree"/> and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generatorDriver"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static IGeneratorTestResult RunTest(this CSharpGeneratorDriver generatorDriver, GeneratorTestResultProvider resultProvider, string? source)
	{
		return RunTest(generatorDriver, resultProvider, source, references: default);
	}

	/// <summary>
	/// Performs a test on all source generators registered in the provided <paramref name="generatorDriver"/>.
	/// </summary>
	/// <param name="generatorDriver"><see cref="CSharpGeneratorDriver"/> used to run a tests on all registered source generators.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generatorDriver"/>.</param>
	/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="SyntaxTree"/> and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
	/// <param name="references">An array of <see cref="MetadataReference"/>s to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generatorDriver"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static IGeneratorTestResult RunTest(this CSharpGeneratorDriver generatorDriver, GeneratorTestResultProvider resultProvider, string? source, params MetadataReference[]? references)
	{
		return RunTest(generatorDriver, resultProvider, RoslynUtilities.CreateCompilationWithReferences(source, references));
	}

	/// <summary>
	/// Performs a test on all source generators registered in the provided <paramref name="generatorDriver"/>.
	/// </summary>
	/// <param name="generatorDriver"><see cref="CSharpGeneratorDriver"/> used to run a tests on all registered source generators.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generatorDriver"/>.</param>
	/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="SyntaxTree"/>s and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
	/// <param name="assemblies">An array of <see cref="Assembly"/> instances to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generatorDriver"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static IGeneratorTestResult RunTest(this CSharpGeneratorDriver generatorDriver, GeneratorTestResultProvider resultProvider, IEnumerable<string>? sources, params Assembly[]? assemblies)
	{
		return RunTest(generatorDriver, resultProvider, RoslynUtilities.CreateCompilationWithAssemblies(sources, assemblies));
	}

	/// <summary>
	/// Performs a test on all source generators registered in the provided <paramref name="generatorDriver"/>.
	/// </summary>
	/// <param name="generatorDriver"><see cref="CSharpGeneratorDriver"/> used to run a tests on all registered source generators.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generatorDriver"/>.</param>
	/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="SyntaxTree"/>s and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generatorDriver"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static IGeneratorTestResult RunTest(this CSharpGeneratorDriver generatorDriver, GeneratorTestResultProvider resultProvider, IEnumerable<string>? sources)
	{
		return RunTest(generatorDriver, resultProvider, sources, references: default);
	}

	/// <summary>
	/// Performs a test on all source generators registered in the provided <paramref name="generatorDriver"/>.
	/// </summary>
	/// <param name="generatorDriver"><see cref="CSharpGeneratorDriver"/> used to run a tests on all registered source generators.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generatorDriver"/>.</param>
	/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="SyntaxTree"/>s and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
	/// <param name="references">An array of <see cref="MetadataReference"/>s to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generatorDriver"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static IGeneratorTestResult RunTest(this CSharpGeneratorDriver generatorDriver, GeneratorTestResultProvider resultProvider, IEnumerable<string>? sources, params MetadataReference[]? references)
	{
		return RunTest(generatorDriver, resultProvider, RoslynUtilities.CreateCompilationWithReferences(sources, references));
	}

	/// <summary>
	/// Performs a test on all source generators registered in the provided <paramref name="generatorDriver"/>.
	/// </summary>
	/// <param name="generatorDriver"><see cref="CSharpGeneratorDriver"/> used to run a tests on all registered source generators.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generatorDriver"/>.</param>
	/// <param name="compilation">A <see cref="CSharpCompilation"/> to be used a input of the tested <paramref name="generatorDriver"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generatorDriver"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
	public static IGeneratorTestResult RunTest(this CSharpGeneratorDriver generatorDriver, GeneratorTestResultProvider resultProvider, CSharpCompilation compilation)
	{
		if (generatorDriver is null)
		{
			throw new ArgumentNullException(nameof(generatorDriver));
		}

		if (resultProvider is null)
		{
			throw new ArgumentNullException(nameof(resultProvider));
		}

		if (compilation is null)
		{
			throw new ArgumentNullException(nameof(compilation));
		}

		CSharpGeneratorDriver driver = generatorDriver;
		driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation outputCompilation, out _);
		return resultProvider(driver, compilation, (CSharpCompilation)outputCompilation);
	}

	/// <summary>
	/// Performs a test on all source generators registered in the provided <paramref name="generatorDriver"/>.
	/// </summary>
	/// <param name="generatorDriver"><see cref="CSharpGeneratorDriver"/> used to run a tests on all registered source generators.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generatorDriver"/>.</param>
	/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="SyntaxTree"/> and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
	/// <param name="assemblies">An array of <see cref="Assembly"/> instances to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
	/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
	/// <exception cref="ArgumentNullException"><paramref name="generatorDriver"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static TResult RunTest<TResult>(this CSharpGeneratorDriver generatorDriver, GeneratorTestResultProvider<TResult> resultProvider, string? source, params Assembly[]? assemblies) where TResult : IGeneratorTestResult
	{
		return RunTest(generatorDriver, resultProvider, RoslynUtilities.CreateCompilationWithAssemblies(source, assemblies));
	}

	/// <summary>
	/// Performs a test on all source generators registered in the provided <paramref name="generatorDriver"/>.
	/// </summary>
	/// <param name="generatorDriver"><see cref="CSharpGeneratorDriver"/> used to run a tests on all registered source generators.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generatorDriver"/>.</param>
	/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="SyntaxTree"/> and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
	/// <param name="references">An array of <see cref="MetadataReference"/>s to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
	/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
	/// <exception cref="ArgumentNullException"><paramref name="generatorDriver"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static TResult RunTest<TResult>(this CSharpGeneratorDriver generatorDriver, GeneratorTestResultProvider<TResult> resultProvider, string? source, params MetadataReference[]? references) where TResult : IGeneratorTestResult
	{
		return RunTest(generatorDriver, resultProvider, RoslynUtilities.CreateCompilationWithReferences(source, references));
	}

	/// <summary>
	/// Performs a test on all source generators registered in the provided <paramref name="generatorDriver"/>.
	/// </summary>
	/// <param name="generatorDriver"><see cref="CSharpGeneratorDriver"/> used to run a tests on all registered source generators.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generatorDriver"/>.</param>
	/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="SyntaxTree"/> and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
	/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
	/// <exception cref="ArgumentNullException"><paramref name="generatorDriver"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static TResult RunTest<TResult>(this CSharpGeneratorDriver generatorDriver, GeneratorTestResultProvider<TResult> resultProvider, string? source) where TResult : IGeneratorTestResult
	{
		return RunTest(generatorDriver, resultProvider, source, references: default);
	}

	/// <summary>
	/// Performs a test on all source generators registered in the provided <paramref name="generatorDriver"/>.
	/// </summary>
	/// <param name="generatorDriver"><see cref="CSharpGeneratorDriver"/> used to run a tests on all registered source generators.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generatorDriver"/>.</param>
	/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="SyntaxTree"/>s and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
	/// <param name="assemblies">An array of <see cref="Assembly"/> instances to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
	/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
	/// <exception cref="ArgumentNullException"><paramref name="generatorDriver"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static TResult RunTest<TResult>(this CSharpGeneratorDriver generatorDriver, GeneratorTestResultProvider<TResult> resultProvider, IEnumerable<string>? sources, params Assembly[]? assemblies) where TResult : IGeneratorTestResult
	{
		return RunTest(generatorDriver, resultProvider, RoslynUtilities.CreateCompilationWithAssemblies(sources, assemblies));
	}

	/// <summary>
	/// Performs a test on all source generators registered in the provided <paramref name="generatorDriver"/>.
	/// </summary>
	/// <param name="generatorDriver"><see cref="CSharpGeneratorDriver"/> used to run a tests on all registered source generators.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generatorDriver"/>.</param>
	/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="SyntaxTree"/>s and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
	/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
	/// <exception cref="ArgumentNullException"><paramref name="generatorDriver"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static TResult RunTest<TResult>(this CSharpGeneratorDriver generatorDriver, GeneratorTestResultProvider<TResult> resultProvider, IEnumerable<string>? sources) where TResult : IGeneratorTestResult
	{
		return RunTest(generatorDriver, resultProvider, sources, references: default);
	}

	/// <summary>
	/// Performs a test on all source generators registered in the provided <paramref name="generatorDriver"/>.
	/// </summary>
	/// <param name="generatorDriver"><see cref="CSharpGeneratorDriver"/> used to run a tests on all registered source generators.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generatorDriver"/>.</param>
	/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="SyntaxTree"/>s and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
	/// <param name="references">An array of <see cref="MetadataReference"/>s to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
	/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
	/// <exception cref="ArgumentNullException"><paramref name="generatorDriver"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static TResult RunTest<TResult>(this CSharpGeneratorDriver generatorDriver, GeneratorTestResultProvider<TResult> resultProvider, IEnumerable<string>? sources, params MetadataReference[]? references) where TResult : IGeneratorTestResult
	{
		return RunTest(generatorDriver, resultProvider, RoslynUtilities.CreateCompilationWithReferences(sources, references));
	}

	/// <summary>
	/// Performs a test on all source generators registered in the provided <paramref name="generatorDriver"/>.
	/// </summary>
	/// <param name="generatorDriver"><see cref="CSharpGeneratorDriver"/> used to run a tests on all registered source generators.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generatorDriver"/>.</param>
	/// <param name="compilation">A <see cref="CSharpCompilation"/> to be used a input of the tested <paramref name="generatorDriver"/>.</param>
	/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
	/// <exception cref="ArgumentNullException"><paramref name="generatorDriver"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
	public static TResult RunTest<TResult>(this CSharpGeneratorDriver generatorDriver, GeneratorTestResultProvider<TResult> resultProvider, CSharpCompilation compilation) where TResult : IGeneratorTestResult
	{
		if (generatorDriver is null)
		{
			throw new ArgumentNullException(nameof(generatorDriver));
		}

		if (resultProvider is null)
		{
			throw new ArgumentNullException(nameof(resultProvider));
		}

		if (compilation is null)
		{
			throw new ArgumentNullException(nameof(compilation));
		}

		CSharpGeneratorDriver driver = generatorDriver;
		driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation outputCompilation, out _);
		return resultProvider(driver, compilation, (CSharpCompilation)outputCompilation);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="ISourceGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="SyntaxTree"/> and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <param name="assemblies">An array of <see cref="Assembly"/> instances to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static IGeneratorTestResult RunTest(this ISourceGenerator generator, GeneratorTestResultProvider resultProvider, string? source, params Assembly[]? assemblies)
	{
		if (generator is null)
		{
			throw new ArgumentNullException(nameof(generator));
		}

		CSharpCompilation compilation = RoslynUtilities.CreateCompilationWithAssemblies(source, assemblies);
		return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="ISourceGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="SyntaxTree"/> and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static IGeneratorTestResult RunTest(this ISourceGenerator generator, GeneratorTestResultProvider resultProvider, string? source)
	{
		return RunTest(generator, resultProvider, source, references: default);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="ISourceGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="SyntaxTree"/> and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <param name="references">An array of <see cref="MetadataReference"/>s to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static IGeneratorTestResult RunTest(this ISourceGenerator generator, GeneratorTestResultProvider resultProvider, string? source, params MetadataReference[]? references)
	{
		if (generator is null)
		{
			throw new ArgumentNullException(nameof(generator));
		}

		CSharpCompilation compilation = RoslynUtilities.CreateCompilationWithReferences(source, references);
		return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="ISourceGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="SyntaxTree"/>s and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <param name="assemblies">An array of <see cref="Assembly"/> instances to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static IGeneratorTestResult RunTest(this ISourceGenerator generator, GeneratorTestResultProvider resultProvider, IEnumerable<string>? sources, params Assembly[]? assemblies)
	{
		if (generator is null)
		{
			throw new ArgumentNullException(nameof(generator));
		}

		CSharpCompilation compilation = RoslynUtilities.CreateCompilationWithAssemblies(sources, assemblies);
		return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="ISourceGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="SyntaxTree"/>s and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static IGeneratorTestResult RunTest(this ISourceGenerator generator, GeneratorTestResultProvider resultProvider, IEnumerable<string>? sources)
	{
		return RunTest(generator, resultProvider, sources, references: default);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="ISourceGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="SyntaxTree"/>s and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <param name="references">An array of <see cref="MetadataReference"/>s to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static IGeneratorTestResult RunTest(this ISourceGenerator generator, GeneratorTestResultProvider resultProvider, IEnumerable<string>? sources, params MetadataReference[]? references)
	{
		if (generator is null)
		{
			throw new ArgumentNullException(nameof(generator));
		}

		CSharpCompilation compilation = RoslynUtilities.CreateCompilationWithReferences(sources, references);
		return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="ISourceGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="compilation">A <see cref="CSharpCompilation"/> to be used a input of the tested <paramref name="generator"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
	public static IGeneratorTestResult RunTest(this ISourceGenerator generator, GeneratorTestResultProvider resultProvider, CSharpCompilation compilation)
	{
		if (generator is null)
		{
			throw new ArgumentNullException(nameof(generator));
		}

		return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="ISourceGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="SyntaxTree"/> and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <param name="assemblies">An array of <see cref="Assembly"/> instances to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static TResult RunTest<TResult>(this ISourceGenerator generator, GeneratorTestResultProvider<TResult> resultProvider, string? source, params Assembly[]? assemblies) where TResult : IGeneratorTestResult
	{
		if (generator is null)
		{
			throw new ArgumentNullException(nameof(generator));
		}

		CSharpCompilation compilation = RoslynUtilities.CreateCompilationWithAssemblies(source, assemblies);
		return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="ISourceGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="SyntaxTree"/> and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <param name="references">An array of <see cref="MetadataReference"/>s to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static TResult RunTest<TResult>(this ISourceGenerator generator, GeneratorTestResultProvider<TResult> resultProvider, string? source, params MetadataReference[]? references) where TResult : IGeneratorTestResult
	{
		if (generator is null)
		{
			throw new ArgumentNullException(nameof(generator));
		}

		CSharpCompilation compilation = RoslynUtilities.CreateCompilationWithReferences(source, references);
		return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="ISourceGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="SyntaxTree"/> and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static TResult RunTest<TResult>(this ISourceGenerator generator, GeneratorTestResultProvider<TResult> resultProvider, string? source) where TResult : IGeneratorTestResult
	{
		return RunTest(generator, resultProvider, source, references: default);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="ISourceGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> of type <typeparamref name="TResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="SyntaxTree"/>s and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <param name="assemblies">An array of <see cref="Assembly"/> instances to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static TResult RunTest<TResult>(this ISourceGenerator generator, GeneratorTestResultProvider<TResult> resultProvider, IEnumerable<string>? sources, params Assembly[]? assemblies) where TResult : IGeneratorTestResult
	{
		if (generator is null)
		{
			throw new ArgumentNullException(nameof(generator));
		}

		CSharpCompilation compilation = RoslynUtilities.CreateCompilationWithAssemblies(sources, assemblies);
		return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="ISourceGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> of type <typeparamref name="TResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="SyntaxTree"/>s and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static TResult RunTest<TResult>(this ISourceGenerator generator, GeneratorTestResultProvider<TResult> resultProvider, IEnumerable<string>? sources) where TResult : IGeneratorTestResult
	{
		return RunTest(generator, resultProvider, sources, references: default);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="ISourceGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> of type <typeparamref name="TResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="SyntaxTree"/>s and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <param name="references">An array of <see cref="MetadataReference"/>s to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static TResult RunTest<TResult>(this ISourceGenerator generator, GeneratorTestResultProvider<TResult> resultProvider, IEnumerable<string>? sources, params MetadataReference[]? references) where TResult : IGeneratorTestResult
	{
		if (generator is null)
		{
			throw new ArgumentNullException(nameof(generator));
		}

		CSharpCompilation compilation = RoslynUtilities.CreateCompilationWithReferences(sources, references);
		return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="ISourceGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> of type <typeparamref name="TResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="compilation">A <see cref="CSharpCompilation"/> to be used a input of the tested <paramref name="generator"/>.</param>
	/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
	public static TResult RunTest<TResult>(this ISourceGenerator generator, GeneratorTestResultProvider<TResult> resultProvider, CSharpCompilation compilation) where TResult : IGeneratorTestResult
	{
		if (generator is null)
		{
			throw new ArgumentNullException(nameof(generator));
		}

		return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="IIncrementalGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="SyntaxTree"/> and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <param name="assemblies">An array of <see cref="Assembly"/> instances to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static IGeneratorTestResult RunTest(this IIncrementalGenerator generator, GeneratorTestResultProvider resultProvider, string? source, params Assembly[]? assemblies)
	{
		if (generator is null)
		{
			throw new ArgumentNullException(nameof(generator));
		}

		CSharpCompilation compilation = RoslynUtilities.CreateCompilationWithAssemblies(source, assemblies);
		return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="IIncrementalGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="SyntaxTree"/> and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static IGeneratorTestResult RunTest(this IIncrementalGenerator generator, GeneratorTestResultProvider resultProvider, string? source)
	{
		return RunTest(generator, resultProvider, source, references: default);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="IIncrementalGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="SyntaxTree"/> and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <param name="references">An array of <see cref="MetadataReference"/>s to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static IGeneratorTestResult RunTest(this IIncrementalGenerator generator, GeneratorTestResultProvider resultProvider, string? source, params MetadataReference[]? references)
	{
		if (generator is null)
		{
			throw new ArgumentNullException(nameof(generator));
		}

		CSharpCompilation compilation = RoslynUtilities.CreateCompilationWithReferences(source, references);
		return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="IIncrementalGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="SyntaxTree"/>s and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <param name="assemblies">An array of <see cref="Assembly"/> instances to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static IGeneratorTestResult RunTest(this IIncrementalGenerator generator, GeneratorTestResultProvider resultProvider, IEnumerable<string>? sources, params Assembly[]? assemblies)
	{
		if (generator is null)
		{
			throw new ArgumentNullException(nameof(generator));
		}

		CSharpCompilation compilation = RoslynUtilities.CreateCompilationWithAssemblies(sources, assemblies);
		return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="IIncrementalGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="SyntaxTree"/>s and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static IGeneratorTestResult RunTest(this IIncrementalGenerator generator, GeneratorTestResultProvider resultProvider, IEnumerable<string>? sources)
	{
		return RunTest(generator, resultProvider, sources, references: default);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="IIncrementalGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="SyntaxTree"/>s and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <param name="references">An array of <see cref="MetadataReference"/>s to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static IGeneratorTestResult RunTest(this IIncrementalGenerator generator, GeneratorTestResultProvider resultProvider, IEnumerable<string>? sources, params MetadataReference[]? references)
	{
		if (generator is null)
		{
			throw new ArgumentNullException(nameof(generator));
		}

		CSharpCompilation compilation = RoslynUtilities.CreateCompilationWithReferences(sources, references);
		return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="IIncrementalGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="compilation">A <see cref="CSharpCompilation"/> to be used a input of the tested <paramref name="generator"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
	public static IGeneratorTestResult RunTest(this IIncrementalGenerator generator, GeneratorTestResultProvider resultProvider, CSharpCompilation compilation)
	{
		if (generator is null)
		{
			throw new ArgumentNullException(nameof(generator));
		}

		return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="IIncrementalGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="SyntaxTree"/> and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <param name="assemblies">An array of <see cref="Assembly"/> instances to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static TResult RunTest<TResult>(this IIncrementalGenerator generator, GeneratorTestResultProvider<TResult> resultProvider, string? source, params Assembly[]? assemblies) where TResult : IGeneratorTestResult
	{
		if (generator is null)
		{
			throw new ArgumentNullException(nameof(generator));
		}

		CSharpCompilation compilation = RoslynUtilities.CreateCompilationWithAssemblies(source, assemblies);
		return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="IIncrementalGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="SyntaxTree"/> and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <param name="references">An array of <see cref="MetadataReference"/>s to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static TResult RunTest<TResult>(this IIncrementalGenerator generator, GeneratorTestResultProvider<TResult> resultProvider, string? source, params MetadataReference[]? references) where TResult : IGeneratorTestResult
	{
		if (generator is null)
		{
			throw new ArgumentNullException(nameof(generator));
		}

		CSharpCompilation compilation = RoslynUtilities.CreateCompilationWithReferences(source, references);
		return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="IIncrementalGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="SyntaxTree"/> and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static TResult RunTest<TResult>(this IIncrementalGenerator generator, GeneratorTestResultProvider<TResult> resultProvider, string? source) where TResult : IGeneratorTestResult
	{
		return RunTest(generator, resultProvider, source, references: default);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="IIncrementalGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> of type <typeparamref name="TResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="SyntaxTree"/>s and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <param name="assemblies">An array of <see cref="Assembly"/> instances to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static TResult RunTest<TResult>(this IIncrementalGenerator generator, GeneratorTestResultProvider<TResult> resultProvider, IEnumerable<string>? sources, params Assembly[]? assemblies) where TResult : IGeneratorTestResult
	{
		if (generator is null)
		{
			throw new ArgumentNullException(nameof(generator));
		}

		CSharpCompilation compilation = RoslynUtilities.CreateCompilationWithAssemblies(sources, assemblies);
		return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="IIncrementalGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> of type <typeparamref name="TResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="SyntaxTree"/>s and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static TResult RunTest<TResult>(this IIncrementalGenerator generator, GeneratorTestResultProvider<TResult> resultProvider, IEnumerable<string>? sources) where TResult : IGeneratorTestResult
	{
		return RunTest(generator, resultProvider, sources, references: default);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="IIncrementalGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> of type <typeparamref name="TResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="SyntaxTree"/>s and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <param name="references">An array of <see cref="MetadataReference"/>s to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
	/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
	public static TResult RunTest<TResult>(this IIncrementalGenerator generator, GeneratorTestResultProvider<TResult> resultProvider, IEnumerable<string>? sources, params MetadataReference[]? references) where TResult : IGeneratorTestResult
	{
		if (generator is null)
		{
			throw new ArgumentNullException(nameof(generator));
		}

		CSharpCompilation compilation = RoslynUtilities.CreateCompilationWithReferences(sources, references);
		return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
	}

	/// <summary>
	/// Performs a test of the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="IIncrementalGenerator"/> to run a test on.</param>
	/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> of type <typeparamref name="TResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
	/// <param name="compilation">A <see cref="CSharpCompilation"/> to be used a input of the tested <paramref name="generator"/>.</param>
	/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
	public static TResult RunTest<TResult>(this IIncrementalGenerator generator, GeneratorTestResultProvider<TResult> resultProvider, CSharpCompilation compilation) where TResult : IGeneratorTestResult
	{
		if (generator is null)
		{
			throw new ArgumentNullException(nameof(generator));
		}

		return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
	}

	/// <summary>
	/// Checks if the <paramref name="result"/> has generated code and if it contains any of the specified <paramref name="diagnostics"/>.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="IGeneratorTestResult"/> to work on.</typeparam>
	/// <param name="result"><see cref="IGeneratorTestResult"/> to check.</param>
	/// <param name="diagnostics">Diagnostics to check for.</param>
	/// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
	public static bool SucceededAndContainsDiagnostics<T>(this T result, params DiagnosticDescriptor[]? diagnostics) where T : IGeneratorTestResult
	{
		return result.SucceededAndContainsDiagnostics(GetIds(diagnostics));
	}

	/// <summary>
	/// Checks if the <paramref name="result"/> has generated code and if it contains <see cref="Diagnostic"/>s with the specified <paramref name="ids"/>.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="IGeneratorTestResult"/> to work on.</typeparam>
	/// <param name="result"><see cref="IGeneratorTestResult"/> to check.</param>
	/// <param name="ids">IDs of <see cref="Diagnostic"/>s to be checked for.</param>
	/// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
	public static bool SucceededAndContainsDiagnostics<T>(this T result, params string[]? ids) where T : IGeneratorTestResult
	{
		return ContainsDiagnostics(result, ids) && result.IsGenerated;
	}

	/// <summary>
	/// Checks if the <paramref name="result"/> has generated code and if it doesn't contain any of the specified <paramref name="diagnostics"/>.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="IGeneratorTestResult"/> to work on.</typeparam>
	/// <param name="result"><see cref="IGeneratorTestResult"/> to check.</param>
	/// <param name="diagnostics">Diagnostics to check for.</param>
	/// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
	public static bool SucceededAndDoesNotContainDiagnostics<T>(this T result, params DiagnosticDescriptor[]? diagnostics) where T : IGeneratorTestResult
	{
		return result.SucceededAndDoesNotContainDiagnostics(GetIds(diagnostics));
	}

	/// <summary>
	/// Checks if the <paramref name="result"/> has generated code and if it doesn't contain <see cref="Diagnostic"/>s with the specified <paramref name="ids"/>.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="IGeneratorTestResult"/> to work on.</typeparam>
	/// <param name="result"><see cref="IGeneratorTestResult"/> to check.</param>
	/// <param name="ids">IDs of <see cref="Diagnostic"/>s that shouldn't be present in the <paramref name="result"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
	public static bool SucceededAndDoesNotContainDiagnostics<T>(this T result, params string[]? ids) where T : IGeneratorTestResult
	{
		return !ContainsDiagnostics(result, ids) && result.IsGenerated;
	}

	private static string[]? GetIds(DiagnosticDescriptor[]? diagnostics)
	{
		return diagnostics?.Select(d => d.Id).ToArray();
	}
}
