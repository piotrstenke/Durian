using Durian.Analysis;
using Microsoft.CodeAnalysis;

namespace Durian.TestServices;

/// <summary>
/// A delegate that mirrors the signature of the <see cref="DurianBasicGenerator.Execute(GeneratorExecutionContext)"/> method.
/// </summary>
/// <param name="context">The <see cref="GeneratorExecutionContext"/> to be used when performing the action.</param>
public delegate void GeneratorExecute(GeneratorExecutionContext context);
