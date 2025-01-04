namespace Durian.Analysis
{
	/// <summary>
	/// Provides a mechanism for prohibiting and permitting modifications of the current object.
	/// </summary>
	public interface ISealable
	{
		/// <summary>
		/// Determines whether the object is not allowed to be modified.
		/// </summary>
		bool IsSealed { get; }

		/// <summary>
		/// Determines whether the current state of the object allows it to be sealed.
		/// </summary>
		bool CanBeSealed { get; }

		/// <summary>
		/// Determines whether the current state of the object allows it to be unsealed.
		/// </summary>
		bool CanBeUnsealed { get; }

		/// <summary>
		/// Prohibits further modification of the object.
		/// </summary>
		/// <returns><see langword="true"/> if successfully sealed, <see langword="false"/> otherwise.</returns>
		/// <exception cref="SealedObjectException">Current state of the object does not allow it to be sealed.</exception>
		bool Seal();

		/// <summary>
		/// Permits this object to be modified.
		/// </summary>
		/// <returns><see langword="true"/> if successfully unsealed, <see langword="false"/> otherwise.</returns>
		/// <exception cref="SealedObjectException">Current state of the object does not allow it to be unsealed.</exception>
		bool Unseal();
	}
}
