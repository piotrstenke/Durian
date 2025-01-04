using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.TestServices
{
	/// <summary>
	/// Creates a new <see cref="IGeneratorTestResult"/> using the specified <paramref name="driver"/> and <paramref name="input"/> and <paramref name="output"/> <see cref="CSharpCompilation"/>s.
	/// </summary>
	/// <param name="driver">A <see cref="CSharpGeneratorDriver"/> that was used to run the <see cref="ISourceGenerator"/> test.</param>
	/// <param name="input">A <see cref="CSharpCompilation"/> that represent an input for the tested <see cref="ISourceGenerator"/>.</param>
	/// <param name="output">A <see cref="CSharpCompilation"/> that was created by the tested <see cref="ISourceGenerator"/>.</param>
	public delegate IGeneratorTestResult GeneratorTestResultProvider(CSharpGeneratorDriver driver, CSharpCompilation input, CSharpCompilation output);

	/// <summary>
	/// Creates a new <see cref="IGeneratorTestResult"/> of type <typeparamref name="TResult"/> using the specified <paramref name="driver"/> and <paramref name="input"/> and <paramref name="output"/> <see cref="CSharpCompilation"/>s.
	/// </summary>
	/// <param name="driver">A <see cref="CSharpGeneratorDriver"/> that was used to run the <see cref="ISourceGenerator"/> test.</param>
	/// <param name="input">A <see cref="CSharpCompilation"/> that represent an input for the tested <see cref="ISourceGenerator"/>.</param>
	/// <param name="output">A <see cref="CSharpCompilation"/> that was created by the tested <see cref="ISourceGenerator"/>.</param>
	/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create.</typeparam>
	public delegate TResult GeneratorTestResultProvider<TResult>(CSharpGeneratorDriver driver, CSharpCompilation input, CSharpCompilation output) where TResult : IGeneratorTestResult;
}
