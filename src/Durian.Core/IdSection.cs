using System;

namespace Durian.Info;

/// <summary>
/// A two-digit number that represents a module or diagnostic id.
/// </summary>
public readonly struct IdSection : IEquatable<IdSection>, IComparable<IdSection>
{
	/// <summary>
	/// Number representing the two digit value of the id.
	/// </summary>
	public int Value { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="IdSection"/> struct.
	/// </summary>
	/// <param name="firstChar">First character of the id.</param>
	/// <param name="secondChar">Second character of the id.</param>
	/// <exception cref="ArgumentException"><paramref name="firstChar"/> is not a digit. -or- <paramref name="secondChar"/> is not a digit.</exception>
	public IdSection(char firstChar, char secondChar)
	{
		if (!char.IsDigit(firstChar))
		{
			throw new ArgumentException($"Character '{firstChar}' is not a digit", nameof(firstChar));
		}

		if (!char.IsDigit(secondChar))
		{
			throw new ArgumentException($"Character '{secondChar}' is not a digit", nameof(secondChar));
		}

		Value = int.Parse(firstChar.ToString() + secondChar.ToString());
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="IdSection"/> struct.
	/// </summary>
	/// <param name="value">Number representing the id. Must be greater than or equal to <c>0</c> and less than <c>100</c>.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> must be greater than or equal to <c>0</c> and less than <c>100</c>.</exception>
	public IdSection(int value)
	{
		if (value < 0 || value > 99)
		{
			throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than or equal to 0 and less than 100");
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
	/// Creates a new instance of the <see cref="IdSection"/> struct with value parsed from the specified <see cref="string"/> value.
	/// </summary>
	/// <exception cref="ArgumentException">
	/// <paramref name="value"/> is <see langword="null"/>. -or-
	/// <paramref name="value"/> does not represent a two-digit number.
	/// </exception>
	/// <exception cref="ArgumentOutOfRangeException">Parsed value is not greater than or equal to <c>0</c>. -or- Parsed value is not less than <c>100</c>.</exception>
	public static IdSection Parse(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			throw new ArgumentException("Value is null or empty", nameof(value));
		}

		if (!int.TryParse(value, out int id))
		{
			throw new ArgumentException("Id does not represent a two-digit number", nameof(value));
		}

		return new IdSection(id);
	}

	/// <summary>
	/// Converts the <see cref="Value"/> two a tuple of two <see cref="char"/>s.
	/// </summary>
	public (char firstChar, char secondChar) AsChars()
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
	public int CompareTo(IdSection other)
	{
		return Value.CompareTo(other.Value);
	}

	/// <inheritdoc/>
	public override bool Equals(object? obj)
	{
		if (obj is not IdSection other)
		{
			return false;
		}

		return this == other;
	}

	/// <inheritdoc/>
	public bool Equals(IdSection other)
	{
		return this == other;
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	/// <inheritdoc/>
	public override string ToString()
	{
		string str = Value.ToString();

		if (Value > 9)
		{
			return str;
		}

		return "0" + str;
	}
}
