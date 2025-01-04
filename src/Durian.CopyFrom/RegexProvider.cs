using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Durian.Analysis.CopyFrom;

/// <summary>
/// Provides <see cref="Regex"/> for a given pattern.
/// </summary>
public sealed class RegexProvider
{
	private const int _maxCacheSize = 32;
	private readonly Dictionary<string, Regex> _regexCache;
	private readonly Queue<string> _regexQueue;

	/// <summary>
	/// Initializes a new instance of the <see cref="RegexProvider"/> class.
	/// </summary>
	public RegexProvider()
	{
		_regexCache = new(_maxCacheSize);
		_regexQueue = new(_maxCacheSize);
	}

	/// <summary>
	/// Returns a <see cref="Regex"/> for the specified <paramref name="pattern"/>.
	/// </summary>
	/// <param name="pattern">Pattern for the <see cref="Regex"/>/</param>
	public Regex GetRegex(string pattern)
	{
		if (!_regexCache.TryGetValue(pattern, out Regex? regex))
		{
			regex = new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.Singleline);

			if (_regexCache.Count >= _maxCacheSize)
			{
				string oldestPattern = _regexQueue.Dequeue();

				_regexCache.Remove(oldestPattern);
			}

			_regexCache[pattern] = regex;
			_regexQueue.Enqueue(pattern);
		}

		return regex;
	}
}
