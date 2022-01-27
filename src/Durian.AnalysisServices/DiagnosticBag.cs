// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	/// <summary>
	/// A <see cref="IDiagnosticReceiver"/> that holds all the reported diagnostics in a publicly-visible <see cref="List{T}"/>.
	/// </summary>
	public sealed class DiagnosticBag : IDiagnosticReceiverWithBuffer, IDirectDiagnosticReceiver, IEnumerable<Diagnostic>
	{
		private readonly List<Diagnostic> _diagnostics;

		/// <summary>
		/// Number of reported <see cref="Diagnostic"/>s.
		/// </summary>
		public int Count => _diagnostics.Count;

		/// <summary>
		/// Initializes a new instance of the <see cref="DiagnosticBag"/> class.
		/// </summary>
		public DiagnosticBag()
		{
			_diagnostics = new(16);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DiagnosticBag"/> class.
		/// </summary>
		/// <param name="capacity">Capacity of the list of <see cref="_diagnostics"/>.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than <c>0</c>.</exception>
		public DiagnosticBag(int capacity)
		{
			_diagnostics = new(capacity);
		}

		/// <summary>
		/// Clears the bag.
		/// </summary>
		public void Clear()
		{
			_diagnostics.Clear();
		}

		/// <summary>
		/// Returns all <see cref="Diagnostic"/> contained within this <see cref="DiagnosticBag"/>.
		/// </summary>
		public Diagnostic[] GetDiagnostics()
		{
			return _diagnostics.ToArray();
		}

		/// <inheritdoc/>
		public IEnumerator<Diagnostic> GetEnumerator()
		{
			foreach (Diagnostic diag in _diagnostics)
			{
				yield return diag;
			}
		}

		/// <summary>
		/// Reports a diagnostic.
		/// </summary>
		/// <param name="diagnostic"><see cref="Diagnostic"/> to report.</param>
		public void ReportDiagnostic(Diagnostic diagnostic)
		{
			if (diagnostic is null)
			{
				throw new ArgumentNullException(nameof(diagnostic));
			}

			_diagnostics.Add(diagnostic);
		}

		/// <inheritdoc/>
		public void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs)
		{
			if (descriptor is null)
			{
				throw new ArgumentNullException(nameof(descriptor));
			}

			_diagnostics.Add(Diagnostic.Create(descriptor, location, messageArgs));
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		void IDiagnosticReceiverWithBuffer.Push()
		{
			// Do nothing.
		}
	}
}