// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Durian.TestServices
{
    /// <summary>
    /// Contains various extension methods for the <see cref="IGeneratorTestResult"/> interface.
    /// </summary>
    public static class GeneratorTestResultExtensions
    {
        private static readonly Regex _diagnosticMessageRegex = new(@"\(\d+,\d+\):.+?: ", RegexOptions.Singleline);

        /// <summary>
        /// Checks if the <paramref name="result"/> contains any diagnostics with the specified <paramref name="ids"/>.
        /// </summary>
        /// <param name="result">The <see cref="IGeneratorTestResult"/> to check if contains diagnostics with the specified <paramref name="ids"/>.</param>
        /// <param name="ids">Diagnostic IDs to be checked for.</param>
        /// <typeparam name="T">Type of <see cref="IGeneratorTestResult"/> to work on.</typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
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
        /// Checks if the <paramref name="result"/> failed to generate any code and if it contains <see cref="Diagnostic"/>s with the specified <paramref name="ids"/>.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="IGeneratorTestResult"/> to work on.</typeparam>
        /// <param name="result"><see cref="IGeneratorTestResult"/> to check.</param>
        /// <param name="ids">IDs of <see cref="Diagnostic"/>s to be checked for.</param>
        /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
        public static bool HasFailedAndContainsDiagnosticIDs<T>(this T result, params string[]? ids) where T : IGeneratorTestResult
        {
            return ContainsDiagnosticIDs(result, ids) && !result.IsGenerated;
        }

        /// <summary>
        /// Checks if the <paramref name="result"/> failed to generate any code and if it doesn't contain <see cref="Diagnostic"/>s with the specified <paramref name="ids"/>.
        /// </summary>i
        /// <typeparam name="T">Type of <see cref="IGeneratorTestResult"/> to work on.</typeparam>
        /// <param name="result"><see cref="IGeneratorTestResult"/> to check.</param>
        /// <param name="ids">IDs of <see cref="Diagnostic"/>s that shouldn't be present in the <paramref name="result"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
        public static bool HasFailedAndDoesNotContainDiagnosticIDs<T>(this T result, params string[]? ids) where T : IGeneratorTestResult
        {
            return !ContainsDiagnosticIDs(result, ids) && !result.IsGenerated;
        }

        /// <summary>
        /// Checks if the <paramref name="result"/> has generated code and if it contains <see cref="Diagnostic"/>s with the specified <paramref name="ids"/>.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="IGeneratorTestResult"/> to work on.</typeparam>
        /// <param name="result"><see cref="IGeneratorTestResult"/> to check.</param>
        /// <param name="ids">IDs of <see cref="Diagnostic"/>s to be checked for.</param>
        /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
        public static bool HasSucceededAndContainsDiagnosticIDs<T>(this T result, params string[]? ids) where T : IGeneratorTestResult
        {
            return ContainsDiagnosticIDs(result, ids) && result.IsGenerated;
        }

        /// <summary>
        /// Checks if the <paramref name="result"/> has generated code and if it doesn't contain <see cref="Diagnostic"/>s with the specified <paramref name="ids"/>.
        /// </summary>i
        /// <typeparam name="T">Type of <see cref="IGeneratorTestResult"/> to work on.</typeparam>
        /// <param name="result"><see cref="IGeneratorTestResult"/> to check.</param>
        /// <param name="ids">IDs of <see cref="Diagnostic"/>s that shouldn't be present in the <paramref name="result"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
        public static bool HasSucceededAndDoesNotContainDiagnosticIDs<T>(this T result, params string[]? ids) where T : IGeneratorTestResult
        {
            return !ContainsDiagnosticIDs(result, ids) && result.IsGenerated;
        }
    }
}