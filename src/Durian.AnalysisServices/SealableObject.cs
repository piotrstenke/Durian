// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

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
		public void Seal()
		{
			if (IsSealed)
			{
				return;
			}

			if (!CanBeSealed)
			{
				throw new InvalidOperationException("Current state of the object does not allow it to be sealed");
			}

			IsSealed = SealCore();
		}

		/// <inheritdoc/>
		public void Unseal()
		{
			if (!IsSealed)
			{
				return;
			}

			if (!CanBeUnsealed)
			{
				throw new InvalidOperationException("Current state of the object does not allow it to be unsealed");
			}

			IsSealed = !UnsealCore();
		}

		/// <summary>
		/// Actually seals the object.
		/// </summary>
		protected abstract bool SealCore();

		/// <summary>
		/// Actually unseals the object.
		/// </summary>
		protected abstract bool UnsealCore();
	}
}
