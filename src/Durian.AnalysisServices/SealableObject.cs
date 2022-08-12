// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis
{
	/// <inheritdoc cref="ISealable"/>
	public abstract class SealableObject : ISealable
	{
		/// <inheritdoc/>
		public bool IsSealed { get; private set; }

		/// <inheritdoc/>
		public virtual bool CanBeSealed => !IsSealed;

		/// <inheritdoc/>
		public virtual bool CanBeUnsealed => IsSealed;

		/// <summary>
		/// Initializes a new instance of the <see cref="SealableObject"/> class.
		/// </summary>
		protected SealableObject()
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
				throw new SealedObjectException(this, "Current state of the object does not allow it to be sealed");
			}

			IsSealed = SealCore();
			return IsSealed;
		}

		/// <inheritdoc/>
		public bool Unseal()
		{
			if (!IsSealed)
			{
				return false;
			}

			if (!CanBeUnsealed)
			{
				throw new SealedObjectException(this, "Current state of the object does not allow it to be unsealed");
			}

			bool result = UnsealCore();
			IsSealed = !result;
			return result;
		}

		/// <summary>
		/// Actually seals the object.
		/// </summary>
		protected virtual bool SealCore()
		{
			return true;
		}

		/// <summary>
		/// Actually unseals the object.
		/// </summary>
		protected virtual bool UnsealCore()
		{
			return true;
		}
	}
}
