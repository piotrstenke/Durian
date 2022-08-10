// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis
{
	/// <summary>
	/// <see cref="ISealable"/> that can be sealed only once and cannot be unsealed.
	/// </summary>
	public abstract class PermanentSealableObject : ISealable
	{
		/// <inheritdoc/>
		public bool IsSealed { get; private set; }

		/// <inheritdoc/>
		public virtual bool CanBeSealed => !IsSealed;

		bool ISealable.CanBeUnsealed => false;

		/// <summary>
		/// Initializes a new instance of the <see cref="PermanentSealableObject"/> class.
		/// </summary>
		protected PermanentSealableObject()
		{
		}

		/// <inheritdoc/>
		public bool Seal()
		{
			if (IsSealed)
			{
				return false;
			}

			if (!CanBeSealed)
			{
				throw new SealedObjectException("Current state of the object does not allow it to be sealed");
			}

			IsSealed = SealCore();
			return IsSealed;
		}

		bool ISealable.Unseal()
		{
			return false;
		}

		/// <summary>
		/// Actually seals the object.
		/// </summary>
		protected virtual bool SealCore()
		{
			return true;
		}
	}
}
