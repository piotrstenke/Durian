using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.Cache;

/// <summary>
/// <see cref="IDurianAnalyzer"/> that caches the result of analysis.
/// </summary>
/// <typeparam name="T">Type of values this analyzer can cache.</typeparam>
public interface ICachedAnalyzer<T> : IDurianAnalyzer
{
	/// <summary>
	/// Registers actions to be performed by the analyzer and caches the result of analysis.
	/// </summary>
	/// <param name="context"><see cref="IDurianAnalysisContext"/> used to register the actions to be performed.</param>
	/// <param name="compilation"><see cref="CSharpCompilation"/> to be used during the analysis.</param>
	/// <param name="cached">A <see cref="ConcurrentDictionary{TKey, TValue}"/> that contains the cached values.</param>
	void Register(IDurianAnalysisContext context, CSharpCompilation compilation, ConcurrentDictionary<FileLinePositionSpan, T> cached);
}
