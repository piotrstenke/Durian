using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace Durian.Tests
{
	/// <summary>
	/// Contains various extension methods for the <see cref="IGeneratorTestResult"/> interface.
	/// </summary>
	public static class GeneratorTestResultExtensions
	{
		private static readonly Regex _diagnosticMessageRegex = new(@"\(\d+,\d+\):.+?: ", RegexOptions.Singleline);

		/// <summary>
		/// Checks if the <see cref="IGeneratorTestResult.IsGenerated"/> property of the <paramref name="result"/> is <c>true</c> and if it contains any diagnostics with the specified <paramref name="ids"/>.
		/// </summary>
		/// <typeparam name="T">Type of <see cref="IGeneratorTestResult"/> to work on.</typeparam>
		/// <param name="result">The <see cref="IGeneratorTestResult"/> to check if has generated any sources and if contains diagnostics with the specified <paramref name="ids"/>.</param>
		/// <param name="ids">Diagnostic IDs to be checked for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="result"/> is <c>null</c>.</exception>
		public static bool HasFailedAndContainsDiagnosticIDs<T>(this T result, params string[]? ids) where T : IGeneratorTestResult
		{
			return ContainsDiagnosticIDs(result, ids) && !result.IsGenerated;
		}

		/// <summary>
		/// Checks if the <paramref name="result"/> contains any diagnostics with the specified <paramref name="ids"/>.
		/// </summary>
		/// <param name="result">The <see cref="IGeneratorTestResult"/> to check if contains diagnostics with the specified <paramref name="ids"/>.</param>
		/// <param name="ids">Diagnostic IDs to be checked for.</param>
		/// <typeparam name="T">Type of <see cref="IGeneratorTestResult"/> to work on.</typeparam>
		/// <exception cref="ArgumentNullException"><paramref name="result"/> is <c>null</c>.</exception>
		public static bool ContainsDiagnosticIDs<T>(this T result, params string[]? ids) where T : IGeneratorTestResult
		{
			if (result is null)
			{
				throw new ArgumentNullException(nameof(result));
			}

			if (ids is null)
			{
				return false;
			}

			if (ids.Length == 0)
			{
				return result.Diagnostics.Length > 0;
			}

			return result.Diagnostics.Length >= ids.Length && result.Diagnostics.All(d => ids.Contains(d.Id));
		}

		/// <summary>
		/// Checks if the specified <paramref name="result"/> contains the specified <paramref name="diagnosticMessages"/>.
		/// </summary>
		/// <param name="result">The <see cref="IGeneratorTestResult"/> to check if contains the specified <paramref name="diagnosticMessages"/>.</param>
		/// <param name="diagnosticMessages">Diagnostic messages to be checked for.</param>
		/// <typeparam name="T">Type of <see cref="IGeneratorTestResult"/> to work on.</typeparam>
		/// <exception cref="ArgumentNullException"><paramref name="result"/> is <c>null</c>.</exception>
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
		/// <exception cref="ArgumentNullException"><paramref name="result"/> is <c>null</c>.</exception>
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

			if (ignoreLocationAndName)
			{
				return result.Diagnostics.All(d =>
				{
					string message = _diagnosticMessageRegex.Replace(d.GetMessage(CultureInfo.InvariantCulture), "");

					return diagnosticMessages.Contains(message);
				});
			}
			else
			{
				return result.Diagnostics.All(d => diagnosticMessages.Contains(d.GetMessage(CultureInfo.InvariantCulture)));
			}
		}
	}
}
