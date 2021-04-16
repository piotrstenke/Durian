using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Durian
{
	public static partial class DiagnosticReceiver
	{
		/// <summary>
		/// A <see cref="IDiagnosticReceiver"/> that holds all the reported diagnostics in a publicly-visible <see cref="List{T}"/>.
		/// </summary>
		public sealed class Bag : IDiagnosticReceiver
		{
			/// <summary>
			/// A <see cref="List{T}"/> of all reported diagnostics.
			/// </summary>
			public List<Diagnostic> Diagnostics { get; }

			/// <summary>
			/// Initializes a new instance of the <see cref="Bag"/> class.
			/// </summary>
			public Bag()
			{
				Diagnostics = new();
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="Bag"/> class.
			/// </summary>
			/// <param name="capacity">Capacity of the list of <see cref="Diagnostics"/>.</param>
			public Bag(int capacity)
			{
				Diagnostics = new(capacity);
			}

			/// <summary>
			/// Reports a diagnostic.
			/// </summary>
			/// <param name="diagnostic"><see cref="Diagnostic"/> to report.</param>
			public void ReportDiagnostic(Diagnostic diagnostic)
			{
				Diagnostics.Add(diagnostic);
			}

			/// <inheritdoc/>
			public void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs)
			{
				Diagnostics.Add(Diagnostic.Create(descriptor, location, messageArgs));
			}
		}
	}
}
