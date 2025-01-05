using System;
using System.Globalization;
using System.Text;

namespace Durian.Analysis.CodeGeneration;

/// <summary>
/// Writes literal value tokens.
/// </summary>
public sealed class LiteralWriter
{
	/// <inheritdoc cref="CodeBuilder.TextBuilder"/>
	public StringBuilder TextBuilder { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="KeywordWriter"/> class.
	/// </summary>
	/// <param name="builder"><see cref="StringBuilder"/> to write the generated code to.</param>
	/// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
	public LiteralWriter(StringBuilder builder)
	{
		if (builder is null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		TextBuilder = builder;
	}

	/// <summary>
	/// Writes an <see cref="int"/> literal in the binary format.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	/// <param name="upper">Determines whether to write the binary value in upper case (e.g. 0B001 instead of 0b001).</param>
	public LiteralWriter Binary(int value, NumericLiteralSuffix suffix = default, bool upper = false)
	{
		return Binary((long)value, suffix, upper);
	}

	/// <summary>
	/// Writes a <see cref="short"/> literal in the binary format.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	/// <param name="upper">Determines whether to write the binary value in upper case (e.g. 0B001 instead of 0b001).</param>
	public LiteralWriter Binary(short value, NumericLiteralSuffix suffix = default, bool upper = false)
	{
		return Binary((long)value, suffix, upper);
	}

	/// <summary>
	/// Writes a <see cref="sbyte"/> literal in the binary format.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	/// <param name="upper">Determines whether to write the binary value in upper case (e.g. 0B001 instead of 0b001).</param>
	public LiteralWriter Binary(sbyte value, NumericLiteralSuffix suffix = default, bool upper = false)
	{
		return Binary((long)value, suffix, upper);
	}

	/// <summary>
	/// Writes an <see cref="long"/> literal in the binary format.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	/// <param name="upper">Determines whether to write the binary value in upper case (e.g. 0B001 instead of 0b001).</param>
	public LiteralWriter Binary(long value, NumericLiteralSuffix suffix = default, bool upper = false)
	{
		return Binary(ConvertLong(value), suffix, upper);
	}

	/// <summary>
	/// Writes an <see cref="uint"/> literal in the binary format.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	/// <param name="upper">Determines whether to write the binary value in upper case (e.g. 0B001 instead of 0b001).</param>
	public LiteralWriter Binary(uint value, NumericLiteralSuffix suffix = default, bool upper = false)
	{
		return Binary((ulong)value, suffix, upper);
	}

	/// <summary>
	/// Writes an <see cref="ushort"/> literal in the binary format.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	/// <param name="upper">Determines whether to write the binary value in upper case (e.g. 0B001 instead of 0b001).</param>
	public LiteralWriter Binary(ushort value, NumericLiteralSuffix suffix = default, bool upper = false)
	{
		return Binary((ulong)value, suffix, upper);
	}

	/// <summary>
	/// Writes a <see cref="byte"/> literal in the binary format.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	/// <param name="upper">Determines whether to write the binary value in upper case (e.g. 0B001 instead of 0b001).</param>
	public LiteralWriter Binary(byte value, NumericLiteralSuffix suffix = default, bool upper = false)
	{
		return Binary((ulong)value, suffix, upper);
	}

	/// <summary>
	/// Writes an <see cref="ulong"/> literal in the binary format.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	/// <param name="upper">Determines whether to write the binary value in upper case (e.g. 0B001 instead of 0b001).</param>
	public LiteralWriter Binary(ulong value, NumericLiteralSuffix suffix = default, bool upper = false)
	{
		TextBuilder.Append('0');
		TextBuilder.Append(upper ? 'B' : 'b');
		TextBuilder.Append(Convert.ToString((long)value, 2));
		return NumericSuffix(suffix);
	}

	/// <summary>
	/// Writes a <see cref="bool"/> literal.
	/// </summary>
	/// <param name="value">Value to write.</param>
	public LiteralWriter Bool(bool value)
	{
		TextBuilder.Append(value);
		return this;
	}

	/// <summary>
	/// Writes a <see cref="string"/> literal.
	/// </summary>
	/// <param name="value">Value to write.</param>
	public LiteralWriter Char(char value)
	{
		TextBuilder.Append('\'');
		TextBuilder.Append(value);
		TextBuilder.Append('\'');
		return this;
	}

	/// <summary>
	/// Writes a <see cref="float"/> literal.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	/// <param name="exponential">Determines whether or how to write the value using an exponential.</param>
	public LiteralWriter Decimal(float value, DecimalLiteralSuffix suffix = default, ExponentialStyle exponential = default)
	{
		FormatDecimal(value, exponential);
		return NumericSuffix(suffix);
	}

	/// <summary>
	/// Writes a <see cref="double"/> literal.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	/// <param name="exponential">Determines whether or how to write the value using an exponential.</param>
	public LiteralWriter Decimal(double value, DecimalLiteralSuffix suffix = default, ExponentialStyle exponential = default)
	{
		FormatDecimal(value, exponential);
		return NumericSuffix(suffix);
	}

	/// <summary>
	/// Writes a <see cref="decimal"/> literal.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	/// <param name="exponential">Determines whether or how to write the value using an exponential.</param>
	public LiteralWriter Decimal(decimal value, DecimalLiteralSuffix suffix = default, ExponentialStyle exponential = default)
	{
		FormatDecimal(value, exponential);
		return NumericSuffix(suffix);
	}

	/// <summary>
	/// Writes an <see cref="int"/> literal in the hexadecimal format.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	/// <param name="upper">Determines whether to write the hexadecimal value in upper case (e.g. 12FF instead of 12ff).</param>
	public LiteralWriter Hexadecimal(int value, NumericLiteralSuffix suffix = default, bool upper = false)
	{
		return Hexadecimal((long)value, suffix, upper);
	}

	/// <summary>
	/// Writes a <see cref="short"/> literal in the hexadecimal format.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	/// <param name="upper">Determines whether to write the hexadecimal value in upper case (e.g. 12FF instead of 12ff).</param>
	public LiteralWriter Hexadecimal(short value, NumericLiteralSuffix suffix = default, bool upper = false)
	{
		return Hexadecimal((long)value, suffix, upper);
	}

	/// <summary>
	/// Writes a <see cref="long"/> literal in the hexadecimal format.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	/// <param name="upper">Determines whether to write the hexadecimal value in upper case (e.g. 12FF instead of 12ff).</param>
	public LiteralWriter Hexadecimal(long value, NumericLiteralSuffix suffix = default, bool upper = false)
	{
		return Hexadecimal(ConvertLong(value), suffix, upper);
	}

	/// <summary>
	/// Writes a <see cref="sbyte"/> literal in the hexadecimal format.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	/// <param name="upper">Determines whether to write the hexadecimal value in upper case (e.g. 12FF instead of 12ff).</param>
	public LiteralWriter Hexadecimal(sbyte value, NumericLiteralSuffix suffix = default, bool upper = false)
	{
		return Hexadecimal((long)value, suffix, upper);
	}

	/// <summary>
	/// Writes an <see cref="uint"/> literal in the hexadecimal format.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	/// <param name="upper">Determines whether to write the hexadecimal value in upper case (e.g. 12FF instead of 12ff).</param>
	public LiteralWriter Hexadecimal(uint value, NumericLiteralSuffix suffix = default, bool upper = false)
	{
		return Hexadecimal((ulong)value, suffix, upper);
	}

	/// <summary>
	/// Writes an <see cref="ushort"/> literal in the hexadecimal format.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	/// <param name="upper">Determines whether to write the hexadecimal value in upper case (e.g. 12FF instead of 12ff).</param>
	public LiteralWriter Hexadecimal(ushort value, NumericLiteralSuffix suffix = default, bool upper = false)
	{
		return Hexadecimal((ulong)value, suffix, upper);
	}

	/// <summary>
	/// Writes a <see cref="byte"/> literal in the hexadecimal format.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	/// <param name="upper">Determines whether to write the hexadecimal value in upper case (e.g. 12FF instead of 12ff).</param>
	public LiteralWriter Hexadecimal(byte value, NumericLiteralSuffix suffix = default, bool upper = false)
	{
		return Hexadecimal((ulong)value, suffix, upper);
	}

	/// <summary>
	/// Writes an <see cref="ulong"/> literal in the hexadecimal format.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	/// <param name="upper">Determines whether to write the hexadecimal value in upper case (e.g. 12FF instead of 12ff).</param>
	public LiteralWriter Hexadecimal(ulong value, NumericLiteralSuffix suffix = default, bool upper = false)
	{
		string format = upper ? "X" : "x";
		string text = value.ToString(format, CultureInfo.InvariantCulture);
		TextBuilder.Append('0');
		TextBuilder.Append(format);
		TextBuilder.Append(text);
		return NumericSuffix(suffix);
	}

	/// <summary>
	/// Writes an <see cref="int"/> literal.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	public LiteralWriter Integer(int value, NumericLiteralSuffix suffix = default)
	{
		TextBuilder.Append(value);
		return NumericSuffix(suffix);
	}

	/// <summary>
	/// Writes a <see cref="sbyte"/> literal.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	public LiteralWriter Integer(sbyte value, NumericLiteralSuffix suffix = default)
	{
		TextBuilder.Append(value);
		return NumericSuffix(suffix);
	}

	/// <summary>
	/// Writes a <see cref="short"/> literal.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	public LiteralWriter Integer(short value, NumericLiteralSuffix suffix = default)
	{
		TextBuilder.Append(value);
		return NumericSuffix(suffix);
	}

	/// <summary>
	/// Writes a <see cref="long"/> literal.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	public LiteralWriter Integer(long value, NumericLiteralSuffix suffix = default)
	{
		TextBuilder.Append(value);
		return NumericSuffix(suffix);
	}

	/// <summary>
	/// Writes an <see cref="uint"/> literal.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	public LiteralWriter Integer(uint value, NumericLiteralSuffix suffix = default)
	{
		TextBuilder.Append(value);
		return NumericSuffix(suffix);
	}

	/// <summary>
	/// Writes a <see cref="byte"/> literal.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	public LiteralWriter Integer(byte value, NumericLiteralSuffix suffix = default)
	{
		TextBuilder.Append(value);
		return NumericSuffix(suffix);
	}

	/// <summary>
	/// Writes an <see cref="ushort"/> literal.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	public LiteralWriter Integer(ushort value, NumericLiteralSuffix suffix = default)
	{
		TextBuilder.Append(value);
		return NumericSuffix(suffix);
	}

	/// <summary>
	/// Writes an <see cref="ulong"/> literal.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	public LiteralWriter Integer(ulong value, NumericLiteralSuffix suffix = default)
	{
		TextBuilder.Append(value);
		return NumericSuffix(suffix);
	}

	/// <summary>
	/// Writes an <see cref="int"/> literal.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="prefix">Prefix of the numeric value.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	public LiteralWriter Numeric(int value, NumericLiteralPrefix prefix = default, NumericLiteralSuffix suffix = default)
	{
		return Numeric((long)value, prefix, suffix);
	}

	/// <summary>
	/// Writes a <see cref="long"/> literal.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="prefix">Prefix of the numeric value.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	public LiteralWriter Numeric(long value, NumericLiteralPrefix prefix = default, NumericLiteralSuffix suffix = default)
	{
		return Numeric(ConvertLong(value), prefix, suffix);
	}

	/// <summary>
	/// Writes a <see cref="short"/> literal.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="prefix">Prefix of the numeric value.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	public LiteralWriter Numeric(short value, NumericLiteralPrefix prefix = default, NumericLiteralSuffix suffix = default)
	{
		return Numeric((long)value, prefix, suffix);
	}

	/// <summary>
	/// Writes a <see cref="byte"/> literal.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="prefix">Prefix of the numeric value.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	public LiteralWriter Numeric(byte value, NumericLiteralPrefix prefix = default, NumericLiteralSuffix suffix = default)
	{
		return Numeric((ulong)value, prefix, suffix);
	}

	/// <summary>
	/// Writes an <see cref="uint"/> literal.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="prefix">Prefix of the numeric value.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	public LiteralWriter Numeric(uint value, NumericLiteralPrefix prefix = default, NumericLiteralSuffix suffix = default)
	{
		return Numeric((ulong)value, prefix, suffix);
	}

	/// <summary>
	/// Writes an <see cref="ulong"/> literal.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="prefix">Prefix of the numeric value.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	public LiteralWriter Numeric(ulong value, NumericLiteralPrefix prefix = default, NumericLiteralSuffix suffix = default)
	{
		return prefix switch
		{
			NumericLiteralPrefix.None => Integer(value, suffix),
			NumericLiteralPrefix.HexadecimalLower => Hexadecimal(value, suffix, false),
			NumericLiteralPrefix.HexadecimalUpper => Hexadecimal(value, suffix, true),
			NumericLiteralPrefix.BinaryLower => Binary(value, suffix, false),
			NumericLiteralPrefix.BinaryUpper => Binary(value, suffix, true),
			_ => Integer(value, suffix)
		};
	}

	/// <summary>
	/// Writes an <see cref="ushort"/> literal.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="prefix">Prefix of the numeric value.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	public LiteralWriter Numeric(ushort value, NumericLiteralPrefix prefix = default, NumericLiteralSuffix suffix = default)
	{
		return Numeric((ulong)value, prefix, suffix);
	}

	/// <summary>
	/// Writes a <see cref="sbyte"/> literal.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="prefix">Prefix of the numeric value.</param>
	/// <param name="suffix">Suffix of the numeric value.</param>
	public LiteralWriter Numeric(sbyte value, NumericLiteralPrefix prefix = default, NumericLiteralSuffix suffix = default)
	{
		return Numeric((long)value, prefix, suffix);
	}

	/// <summary>
	/// Writes a prefix of a numeric value.
	/// </summary>
	/// <param name="prefix">Prefix of the numeric value.</param>
	public LiteralWriter NumericPrefix(NumericLiteralPrefix prefix)
	{
		if (prefix.GetText() is string value)
		{
			TextBuilder.Append(value);
		}

		return this;
	}

	/// <summary>
	/// Writes a suffix of a numeric value.
	/// </summary>
	/// <param name="suffix">Suffix of the numeric value.</param>
	public LiteralWriter NumericSuffix(NumericLiteralSuffix suffix)
	{
		if (suffix.GetText() is string value)
		{
			TextBuilder.Append(value);
		}

		return this;
	}

	/// <summary>
	/// Writes a suffix of a numeric value.
	/// </summary>
	/// <param name="suffix">Suffix of the numeric value.</param>
	public LiteralWriter NumericSuffix(DecimalLiteralSuffix suffix)
	{
		if (suffix.GetText() is string value)
		{
			TextBuilder.Append(value);
		}

		return this;
	}

	/// <summary>
	/// Writes a <see cref="string"/> literal.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="modifiers">Modifiers of the <see cref="string"/> <paramref name="value"/>.</param>
	public LiteralWriter String(string value, StringModifiers modifiers = default)
	{
		if (modifiers.HasFlag(StringModifiers.Interpolation))
		{
			TextBuilder.Append('$');
		}

		if (modifiers.HasFlag(StringModifiers.Verbatim))
		{
			TextBuilder.Append('@');
		}

		TextBuilder.Append('"');
		TextBuilder.Append(value);
		TextBuilder.Append('"');
		return this;
	}

	private ulong ConvertLong(long value)
	{
		if (value < 0)
		{
			TextBuilder.Append('-');
			return (ulong)(value * 1);
		}

		return (ulong)value;
	}

	private void FormatDecimal<T>(T value, ExponentialStyle exponential) where T : unmanaged, IFormattable
	{
		switch (exponential)
		{
			case ExponentialStyle.Lowercase:
				TextBuilder.Append(value.ToString("e", CultureInfo.InvariantCulture));
				break;

			case ExponentialStyle.Uppercase:
				TextBuilder.Append(value.ToString("E", CultureInfo.InvariantCulture));
				break;

			default:
				TextBuilder.Append(value);
				break;
		}
	}
}