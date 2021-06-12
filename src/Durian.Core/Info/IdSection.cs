// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Info
{
	/// <summary>
	/// A two-digit number that represents a module or diagnostic id.
	/// </summary>
	public readonly struct IdSection : IEquatable<IdSection>, IComparable<IdSection>
	{
		/// <summary>
		/// Returns an <see cref="int"/> representing the value of the id.
		/// </summary>
		public readonly int Value { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="IdSection"/> struct.
		/// </summary>
		/// <param name="id">A <see cref="string"/> that represents a two-digit number.</param>
		/// <exception cref="ArgumentException"><paramref name="id"/> does not represent a two-digit number.</exception>
		public IdSection(string id)
		{
			if (!int.TryParse(id, out int value))
			{
				throw new ArgumentException("Id does not represent a two-digit number!", nameof(id));
			}

			Value = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IdSection"/> struct.
		/// </summary>
		/// <param name="firstChar">First character of the id.</param>
		/// <param name="secondChar">Second character of the id.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="firstChar"/> is not a digit. -or- <paramref name="secondChar"/> is not a digit.</exception>
		public IdSection(char firstChar, char secondChar)
		{
			if (!char.IsDigit(firstChar))
			{
				throw new ArgumentOutOfRangeException(nameof(firstChar), "Id character must be a digit!");
			}

			if (!char.IsDigit(secondChar))
			{
				throw new ArgumentOutOfRangeException(nameof(secondChar), "Id character must be a digit!");
			}

			Value = int.Parse(firstChar.ToString() + secondChar.ToString());
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IdSection"/> struct.
		/// </summary>
		/// <param name="value">Number representing the id. Must be greater than or equal to <c>0</c> and less than <c>100</c>.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not greater than or equal to <c>0</c>. -or- <paramref name="value"/> is not less than <c>100</c>.</exception>
		public IdSection(int value)
		{
			if (value < 0 || value > 99)
			{
				throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than or equal to 0 and less than 100!");
			}

			Value = value;
		}

		/// <inheritdoc/>
		public static explicit operator IdSection(int value)
		{
			return new IdSection(value);
		}

		/// <inheritdoc/>
		public static implicit operator int(IdSection id)
		{
			return id.Value;
		}

		/// <inheritdoc/>
		public static bool operator !=(IdSection left, IdSection right)
		{
			return !(left == right);
		}

		/// <inheritdoc/>
		public static bool operator <(IdSection left, IdSection right)
		{
			return left.Value < right.Value;
		}

		/// <inheritdoc/>
		public static bool operator <=(IdSection left, IdSection right)
		{
			return left.Value <= right.Value;
		}

		/// <inheritdoc/>
		public static bool operator ==(IdSection left, IdSection right)
		{
			return left.Value == right.Value;
		}

		/// <inheritdoc/>
		public static bool operator >(IdSection left, IdSection right)
		{
			return left.Value > right.Value;
		}

		/// <inheritdoc/>
		public static bool operator >=(IdSection left, IdSection right)
		{
			return left.Value >= right.Value;
		}

		/// <summary>
		/// Converts the <see cref="Value"/> two a tuple of two <see cref="char"/>s.
		/// </summary>
		public readonly (char firstChar, char secondChar) AsChars()
		{
			if (Value == 0)
			{
				return ('0', '0');
			}

			int first = Value / 10;
			int last = Value % 10;

			return ((char)(first + 48), (char)(last + 48));
		}

		/// <inheritdoc/>
		public readonly int CompareTo(IdSection other)
		{
			return Value.CompareTo(other.Value);
		}

		/// <inheritdoc/>
		public override readonly bool Equals(object obj)
		{
			if (obj is not IdSection other)
			{
				return false;
			}

			return this == other;
		}

		/// <inheritdoc/>
		public readonly bool Equals(IdSection other)
		{
			return this == other;
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			return Value.GetHashCode();
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			string str = Value.ToString();

			if (Value > 9)
			{
				return str;
			}

			return str + str;
		}
	}
}
