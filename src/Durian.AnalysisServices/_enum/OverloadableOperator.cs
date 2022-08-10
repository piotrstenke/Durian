// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis
{
	/// <summary>
	/// Defines all overloadable operators.
	/// </summary>
	public enum OverloadableOperator
	{
		/// <summary>
		/// Not an operator.
		/// </summary>
		None = 0,

		/// <summary>
		/// Represents the unary plus operator: +x.
		/// </summary>
		UnaryPlus = 1,

		/// <summary>
		/// Represents the unary minus operator: -x.
		/// </summary>
		UnaryMinus = 2,

		/// <summary>
		/// Represents the negation operator: !x.
		/// </summary>
		Negation = 3,

		/// <summary>
		/// Represents the bitwise complement operator: ~x.
		/// </summary>
		Complement = 4,

		/// <summary>
		/// Represents the increment operator: ++x or x++.
		/// </summary>
		Increment = 5,

		/// <summary>
		/// Represents the decrement operator: --x or x--.
		/// </summary>
		Decrement = 6,

		/// <summary>
		/// Represents the <see langword="true"/> operator: if(x).
		/// </summary>
		True = 7,

		/// <summary>
		/// Represents the <see langword="false"/> operator: if(x).
		/// </summary>
		False = 8,

		/// <summary>
		/// Represents the addition operator: x + y.
		/// </summary>
		Addition = 9,

		/// <summary>
		/// Represents the subtraction operator: x - y.
		/// </summary>
		Subtraction = 10,

		/// <summary>
		/// Represents the multiplication operator: x * y.
		/// </summary>
		Multiplication = 11,

		/// <summary>
		/// Represents the division operator: x / y.
		/// </summary>
		Division = 12,

		/// <summary>
		/// Represents the remainder operator: x % y.
		/// </summary>
		Remainder = 13,

		/// <summary>
		/// Represents the logical <see langword="and"/> operator: x &amp; y.
		/// </summary>
		LogicalAnd = 14,

		/// <summary>
		/// Represents the logical <see langword="or"/> operator: x | y.
		/// </summary>
		LogicalOr = 15,

		/// <summary>
		/// Represents the logical <see langword="xor"/> operator: x ^ y.
		/// </summary>
		LogicalXor = 16,

		/// <summary>
		/// Represents the left shift operator: x &lt;&lt; y.
		/// </summary>
		LeftShift = 17,

		/// <summary>
		/// Represents the right shift operator: x &gt;&gt; y.
		/// </summary>
		RightShift = 18,

		/// <summary>
		/// Represents the equality operator: x == y.
		/// </summary>
		Equality = 19,

		/// <summary>
		/// Represents the inequality operator: x != y.
		/// </summary>
		Inequality = 20,

		/// <summary>
		/// Represents the less than operator: x &lt; y.
		/// </summary>
		LessThan = 21,

		/// <summary>
		/// Represents the greater than operator: x &gt; y.
		/// </summary>
		GreaterThan = 22,

		/// <summary>
		/// Represents the less than or equal operator: x &lt;= y.
		/// </summary>
		LessThanOrEqual = 23,

		/// <summary>
		/// Represents the greater than or equal operator: x &gt;= y.
		/// </summary>
		GreaterThanOrEqual = 24
	}
}
