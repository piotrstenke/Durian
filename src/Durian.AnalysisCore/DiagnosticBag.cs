using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Durian
{
	/// <summary>
	/// A <see cref="IDiagnosticReceiver"/> that holds all the reported diagnostics in a publicly-visible <see cref="List{T}"/>.
	/// </summary>
	public sealed class DiagnosticBag : IDiagnosticReceiver
	{
		/// <summary>
		/// A <see cref="List{T}"/> of all reported diagnostics.
		/// </summary>
		public List<Diagnostic> Diagnostics { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DiagnosticBag"/> class.
		/// </summary>
		public DiagnosticBag()
		{
			Diagnostics = new();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DiagnosticBag"/> class.
		/// </summary>
		/// <param name="capacity">Capacity of the list of <see cref="Diagnostics"/>.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than <c>0</c>.</exception>
		public DiagnosticBag(int capacity)
		{
			Diagnostics = new(capacity);
		}

		/// <summary>
		/// Reports a diagnostic.
		/// </summary>
		/// <param name="diagnostic"><see cref="Diagnostic"/> to report.</param>
		public void ReportDiagnostic(Diagnostic diagnostic)
		{
			if (diagnostic is null)
			{
				return;
			}

			Diagnostics.Add(diagnostic);
		}

		/// <inheritdoc/>
		public void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs)
		{
			if (descriptor is null)
			{
				return;
			}

			Diagnostics.Add(Diagnostic.Create(descriptor, location, messageArgs));
		}
	}
}
