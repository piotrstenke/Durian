using System;
using System.Collections.Generic;

namespace Durian.Analysis;

/// <summary>
/// <see cref="IDiagnosticReceiver"/> that reports diagnostics for multiple child <see cref="IDiagnosticReceiver"/>s.
/// </summary>
public interface ICompositeDiagnosticReceiver : INodeDiagnosticReceiver, IEnumerable<IDiagnosticReceiver>
{
	/// <summary>
	/// Number of child <see cref="IDiagnosticReceiver"/>s.
	/// </summary>
	int NumReceivers { get; }

	/// <summary>
	/// Adds the specified <paramref name="diagnosticReceiver"/> to the current receiver.
	/// </summary>
	/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to add to the current receiver.</param>
	/// <exception cref="ArgumentNullException"><paramref name="diagnosticReceiver"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException"><paramref name="diagnosticReceiver"/> is already present in the current receiver.</exception>
	void AddReceiver(IDiagnosticReceiver diagnosticReceiver);

	/// <summary>
	/// Adds the specified collection of <see cref="IDiagnosticReceiver"/>s to the current resolver.
	/// </summary>
	/// <param name="diagnosticReceivers">A collection of <see cref="IDiagnosticReceiver"/>s to add to the current resolver.</param>
	/// <exception cref="ArgumentNullException"><paramref name="diagnosticReceivers"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">Collection contains <see langword="null"/> objects. -or- Collection contains <see cref="IDiagnosticReceiver"/>s that are already present in the current receiver.</exception>
	void AddReceivers(IEnumerable<IDiagnosticReceiver> diagnosticReceivers);

	/// <summary>
	/// Determines whether the specified <paramref name="diagnosticReceiver"/> is present in the current receiver.
	/// </summary>
	/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to check whether is present in the current receiver.</param>
	/// <exception cref="ArgumentNullException"><paramref name="diagnosticReceiver"/> is <see langword="null"/>.</exception>
	bool ContainsReceiver(IDiagnosticReceiver diagnosticReceiver);

	/// <summary>
	/// Returns all <see cref="IDiagnosticReceiver"/> added to the current receiver.
	/// </summary>
	IEnumerable<IDiagnosticReceiver> GetReceivers();

	/// <summary>
	/// Removes the specified <paramref name="diagnosticReceiver"/> from the current receiver.
	/// </summary>
	/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to remove from the current receiver.</param>
	/// <exception cref="ArgumentNullException"><paramref name="diagnosticReceiver"/> is <see langword="null"/>.</exception>
	bool RemoveReceiver(IDiagnosticReceiver diagnosticReceiver);

	/// <summary>
	/// Removes all child <see cref="IDiagnosticReceiver"/>s.
	/// </summary>
	void RemoveReceivers();

	/// <summary>
	/// Removes all specified <see cref="IDiagnosticReceiver"/>s from the current resolver.
	/// </summary>
	/// <param name="diagnosticReceivers">A collection of <see cref="IDiagnosticReceiver"/>s to remove from the current resolver.</param>
	/// <exception cref="ArgumentNullException"><paramref name="diagnosticReceivers"/> is <see langword="null"/>.</exception>
	void RemoveReceivers(IEnumerable<IDiagnosticReceiver> diagnosticReceivers);
}
